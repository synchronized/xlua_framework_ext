using System;
using System.IO;
using UnityEngine;
using XLua;
using YooAsset;

namespace XLuaFrameworkExt
{
    public class LuaManager: IDisposable
    {
        private static LuaManager m_Instance;

        public static LuaManager Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = new LuaManager();
                }
                return m_Instance;
            }
        }

        const string hotfixMainScriptName = "XLua.HotfixMain";

        private MonoBehaviour behaviour;
        private LuaEnv luaEnv = null;
        private LuaUpdater luaUpdater = null;

        internal Action OnStartComplete;

        private LuaManager()
        {
        }

        internal void Initalize(MonoBehaviour behaviour){
            this.behaviour = behaviour;
            InitLuaEnv();
        }

        //被Lua调用
        public static void StartComplete() {
            Instance.OnStartComplete?.Invoke();
            Instance.OnStartComplete = null;
        }

        public bool HasGameStart
        {
            get;
            protected set;
        }

        public LuaEnv GetLuaEnv()
        {
            return luaEnv;
        }

        void InitLuaEnv()
        {
            luaEnv = new LuaEnv();
#if UNITY_EDITOR
            luaEnv.translator.debugDelegateBridgeRelease = true;
#endif //UNITY_EDITOR
            HasGameStart = false;
            if (luaEnv != null)
            {
                luaEnv.AddBuildin("cjson", XLua.LuaDLL.Lua.LoadCjson);
                luaEnv.AddBuildin("cjson.safe", XLua.LuaDLL.Lua.LoadCjsonSafe);

                luaEnv.AddBuildin("lpeg", XLua.LuaDLL.Lua.LoadLpeg);

                luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadPb);
                luaEnv.AddBuildin("pb.io", XLua.LuaDLL.Lua.LoadPbIo);
                luaEnv.AddBuildin("pb.conv", XLua.LuaDLL.Lua.LoadPbConv);
                luaEnv.AddBuildin("pb.buffer", XLua.LuaDLL.Lua.LoadPbBuffer);
                luaEnv.AddBuildin("pb.slice", XLua.LuaDLL.Lua.LoadPbSlice);
                luaEnv.AddBuildin("pb.unsafe", XLua.LuaDLL.Lua.LoadPbUnsafe);

                luaEnv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
                luaEnv.AddBuildin("crypt", XLua.LuaDLL.Lua.LoadCrypt);
                luaEnv.AddBuildin("sproto.core", XLua.LuaDLL.Lua.LoadSprotoCore);
            }
            else
            {
                Debug.LogError("InitLuaEnv null!!!");
            }
        }

        void InitLuaLoader()
        {
            if (luaEnv != null)
            {

#if UNITY_EDITOR
                if (GlobalManager.ResLoadMode == ResLoadMode.SimulateMode) {
                    foreach (var addLuaPath in XLuaConfig.GetLuaSearchPaths()) {
                        luaEnv.AddLoader(new PathLuaLoader(addLuaPath));
                    }
                }
                else 
#endif //UNITY_EDITOR
                {
                    luaEnv.AddLoader(new YooAssetLuaLoader());
                }
            }
        }

        /// <summary>
        /// 启动Lua框架
        /// 这里必须要等待资源管理模块加载Lua AB包以后才能初始化
        /// </summary>
        void InitLuaUpdater()
        {
            if (luaEnv != null)
            {
                //因为LuaUpdater会读取lua中全局的Update, LateUpdate, FixedUpdate 函数
                //所以需要先加载文件
                LoadScript("Main"); 
                luaUpdater = behaviour.gameObject.GetComponent<LuaUpdater>();
                if (luaUpdater == null)
                {
                    luaUpdater = behaviour.gameObject.AddComponent<LuaUpdater>();
                }
                luaUpdater.OnInit(luaEnv);
            }
        }

        internal void InitLoader()
        {
            InitLuaLoader();
            InitLuaUpdater();
        }

        internal void StartGame()
        {
            if (luaEnv != null)
            {
                SafeDoString("Main()");
                HasGameStart = true;
            }
        }

        // 重启虚拟机：热更资源以后被加载的lua脚本可能已经过时，需要重新加载
        // 最简单和安全的方式是另外创建一个虚拟器，所有东西一概重启
        internal void Restart()
        {
            StopHotfix();
            Dispose();
            InitLuaEnv();
            InitLoader();
        }

        public void SafeDoString(string scriptContent)
        {
            if (luaEnv != null)
            {
                try
                {
                    luaEnv.DoString(scriptContent);
                }
                catch (System.Exception ex)
                {
                    string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                    UnityEngine.Debug.LogError(msg, null);
                }
            }
        }

        public void StartHotfix(bool restart = false)
        {
            if (luaEnv == null)
            {
                return;
            }

            if (restart)
            {
                StopHotfix();
                ReloadScript(hotfixMainScriptName);
            }
            else
            {
                LoadScript(hotfixMainScriptName);
            }
            SafeDoString("HotfixMain.Start()");
        }

        public void StopHotfix()
        {
            SafeDoString("HotfixMain.Stop()");
        }
        
        public void ReloadScript(string scriptName)
        {
            SafeDoString(string.Format("package.loaded['{0}'] = nil", scriptName));
            LoadScript(scriptName);
        }

        void LoadScript(string scriptName)
        {
            SafeDoString(string.Format("require('{0}')", scriptName));
        }

        internal void Update()
        {
            if (luaEnv != null)
            {
                luaEnv.Tick();

                if (Time.frameCount % 100 == 0)
                {
                    luaEnv.FullGc();
                }
            }
        }

        public void Dispose()
        {
            if (luaUpdater != null)
            {
                luaUpdater.OnDispose();
            }
            if (luaEnv != null)
            {
                try
                {
                    luaEnv.Dispose();
                    luaEnv = null;
                }
                catch (System.Exception ex)
                {
                    string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                    UnityEngine.Debug.LogError(msg, null);
                }
            }
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName)
        {
            using var luaFunction = GetFunction(funcName);
            luaFunction.Call();
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param)
        {
            using var luaFunction = GetFunction(funcName);
            luaFunction.Call(param);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2)
        {
            using var luaFunction = GetFunction(funcName);
            luaFunction.Call(param1, param2);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2, object param3)
        {
            using var luaFunction = GetFunction(funcName);
            luaFunction.Call(param1, param2, param3);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2, object param3, object param4)
        {
            using var luaFunction = GetFunction(funcName);
            luaFunction.Call(param1, param2, param3, param4);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2, object param3, object param4, object param5)
        {
            using var luaFunction = GetFunction(funcName);
            luaFunction.Call(param1, param2, param3, param4, param5);
        }

        /// <summary>
        /// 获取Lua全局方法
        /// </summary>
        public LuaFunction GetFunction(string funcName)
        {
            return luaEnv.Global.Get<LuaFunction>(funcName);
        }

    }
}
