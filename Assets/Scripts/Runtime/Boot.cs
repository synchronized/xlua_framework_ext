using UnityEngine;
using UniFramework.Event;
using YooAsset;
using XLuaFrameworkExt;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
//using Mono.Cecil;
using UnityEngine.SceneManagement;
using System;

namespace GameLogic.BootLogic
{

    public class Boot : MonoBehaviour
    {
        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

        private EDefaultBuildPipeline BuildPipeline = EDefaultBuildPipeline.ScriptableBuildPipeline;

        [HideInInspector]
        public static Boot Instance;

        void Awake()
        {
            Instance = this;

            GlobalManager.ResLoadMode = PlayMode == EPlayMode.EditorSimulateMode
                    ? ResLoadMode.SimulateMode : ResLoadMode.NormalMode;
            GlobalManager.Behaviour = this;
            GlobalManager.MainCanvas = transform.Find("MainCanvas");
            GlobalManager.MainCamera = transform.Find("MainCamera");
            GlobalManager.DefaultPackage = "DefaultPackage";

            Debug.Log($"资源加载模式运行模式：{PlayMode}");
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
        }
        async UniTask Start()
        {

            PatchManager.Initalize(this);

            // 初始化事件系统
            UniEvent.Initalize();

            // 初始化资源系统
            YooAssets.Initialize();

            ResManager.Initalize();

            SoundManager.Initalize();

            LuaManager.Instance.Initalize(this);

            //初始化lua文件加载器
            LuaManager.Instance.InitLoader();

            //创建更新界面
            PatchWindow.CreateWindow();

            // 开始补丁更新流程
            PatchOperation operation = new PatchOperation(GlobalManager.DefaultPackage, BuildPipeline.ToString(), PlayMode);
            YooAssets.StartOperation(operation);
            await operation;

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            YooAssets.SetDefaultPackage(gamePackage);

            //添加启动完成后的清理逻辑
            LuaManager.Instance.OnStartComplete += () => {
                PatchWindow.DestroyWindow();
            };
        }

        void Update()
        {
            NetManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
            ResManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
            LuaManager.Instance.Update();
        }

        void OnDestroy()
        {
            NetManager.Shutdown();
            //NOTE 还未修复LuaEnv销毁提示引用未释放的问题 
            //LuaManager.Instance.Dispose();
        }

        //进入更新流程
        public void EnterPatchFlow()
        {
            async UniTask WaitAsync()
            {
                //创建更新界面
                PatchWindow.CreateWindow();

                Resources.Load<GameObject>("Blank");

                // 开始补丁更新流程
                PatchOperation operation = new PatchOperation(GlobalManager.DefaultPackage, BuildPipeline.ToString(), PlayMode, false);
                YooAssets.StartOperation(operation);
                await operation;

                // 设置默认的资源包
                var gamePackage = YooAssets.GetPackage(GlobalManager.DefaultPackage);
                YooAssets.SetDefaultPackage(gamePackage);

                //添加启动完成后的清理逻辑
                LuaManager.Instance.OnStartComplete += () => {
                    PatchWindow.DestroyWindow();
                };

                LuaManager.Instance.OnApplicationQuit();

                await UniTask.NextFrame();

                LuaManager.Instance.Restart();
            }
            WaitAsync().Forget();
        }
    }
}
