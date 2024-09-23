using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;
using XLua;

namespace XLuaFrameworkExt
{
    public static class ResManager
    {
        class AssetHandlerUpdater {

            static readonly List<ResAsyncHandler> m_handlerList = new();
            static readonly List<ResAsyncHandler> m_handlerRemoveList = new();

            public static void AddHandler(ResAsyncHandler handler) {
                m_handlerList.Add(handler);
            }

            public static void RemoveHandler(ResAsyncHandler handler) {
                m_handlerRemoveList.Add(handler);
            }

            internal static void Update()
            {
                foreach (var handler in m_handlerList)
                {
                    handler.Update();
                }
                foreach (var handler in m_handlerRemoveList)
                {
                    m_handlerList.Remove(handler);
                }
                m_handlerRemoveList.Clear();
            }
        }

        public abstract class ResAsyncHandler : IEnumerator, IDisposable
        {

            public object Current => null;

            public Action<float> OnProgress;

            public abstract bool IsDone { get; }

            public abstract float Progress { get; }

            public abstract string Error { get; }

            public abstract void Dispose();

            internal void Update() {
                if (IsDone) {
                    OnProgress?.Invoke(1.0f);
                    OnProgress = null;

                    AssetHandlerUpdater.RemoveHandler(this);
                    this.Dispose();
                } else {
                    OnProgress?.Invoke(Progress);
                }
            }

            public bool MoveNext() { return !IsDone; }

            public void Reset() { }
        }

        public class YooAssetAsyncHandler : ResAsyncHandler
        {
            static Queue<YooAssetAsyncHandler> pool = new Queue<YooAssetAsyncHandler>();

            public static YooAssetAsyncHandler Get()
            {
                if (pool.Count > 0) return pool.Dequeue();
                else return new YooAssetAsyncHandler();
            }

            public static void Recycle(YooAssetAsyncHandler loader)
            {
                pool.Enqueue(loader);
            }

            private AssetHandle assetHandle;


            private YooAssetAsyncHandler() { }

            public void Init(AssetHandle assetHandle)
            {
                this.assetHandle = assetHandle;
            }

            public UnityEngine.Object AssetObject { get {
                return assetHandle.AssetObject;
            } }

            public override bool IsDone
            {
                get { return assetHandle.Status != EOperationStatus.Processing; }
            }

            public override float Progress
            {
                get { return assetHandle.Progress;}
            }

            public override string Error
            {
                get { return assetHandle.Status == EOperationStatus.Failed ? assetHandle.LastError : null;}
            }


            public override void Dispose() {
                assetHandle.Dispose();
                assetHandle = null;
                Recycle(this);
            }
        }

        public class YooAssetListAsyncHandler : ResAsyncHandler
        {
            static Queue<YooAssetListAsyncHandler> pool = new Queue<YooAssetListAsyncHandler>();

            public static YooAssetListAsyncHandler Get()
            {
                YooAssetListAsyncHandler result;
                if (pool.Count > 0) result = pool.Dequeue();
                else result = new YooAssetListAsyncHandler();
                return result;
            }

            public static void Recycle(YooAssetListAsyncHandler loader)
            {
                pool.Enqueue(loader);
            }

            private List<AssetHandle> m_assetHandleLsit;
            private int m_doneNum;
            private string m_lastError;

            private YooAssetListAsyncHandler() { }

            public void Init()
            {
                m_doneNum = 0;
                m_lastError = null;
                m_assetHandleLsit ??= new List<AssetHandle>();
            }

            public override bool IsDone
            {
                get { return m_doneNum == m_assetHandleLsit.Count; }
            }

            public override float Progress
            {
                get {
                    if (m_assetHandleLsit.Count == 0 ) return 0;
                    if (m_assetHandleLsit.Count == 1 ) return m_assetHandleLsit[0].Progress;
                    m_doneNum = 0;
                    foreach (var assetHandle in m_assetHandleLsit) {
                        if (assetHandle.IsDone) m_doneNum++;
                    }
                    return 1.0f *m_doneNum / m_assetHandleLsit.Count;
                }
            }

            public override string Error
            {
                get {
                    if (m_lastError != null) return m_lastError;
                    foreach (var assetHandle in m_assetHandleLsit) {
                        if (assetHandle.IsDone && assetHandle.Status != EOperationStatus.Succeed) {
                            m_lastError = assetHandle.LastError;
                        }
                    }
                    return null;
                }
            }

            public override void Dispose() {
                foreach (var assetHandle in m_assetHandleLsit) {
                    assetHandle.Dispose();
                }
                m_doneNum = 0;
                m_lastError = null;
                m_assetHandleLsit.Clear();
                Recycle(this);
            }

            public void AddAssetHandler(AssetHandle assetHandle) {
                m_assetHandleLsit.Add(assetHandle);
            }
        }

        public class YooAssetAsyncSceneHandler : ResAsyncHandler
        {
            static Queue<YooAssetAsyncSceneHandler> pool = new Queue<YooAssetAsyncSceneHandler>();

            public static YooAssetAsyncSceneHandler Get()
            {
                if (pool.Count > 0) return pool.Dequeue();
                else return new YooAssetAsyncSceneHandler();
            }

            public static void Recycle(YooAssetAsyncSceneHandler loader)
            {
                pool.Enqueue(loader);
            }

            private SceneHandle assetHandle;


            private YooAssetAsyncSceneHandler() { }

            public void Init(SceneHandle assetHandle)
            {
                this.assetHandle = assetHandle;
            }

            public UnityEngine.SceneManagement.Scene Scene { get {
                return assetHandle.SceneObject;
            } }

            public override bool IsDone
            {
                get { return assetHandle.Status != EOperationStatus.Processing; }
            }

            public override float Progress
            {
                get { return assetHandle.Progress;}
            }

            public override string Error
            {
                get { return assetHandle.Status == EOperationStatus.Failed ? assetHandle.LastError : null;}
            }


            public override void Dispose() {
                assetHandle = null;
                Recycle(this);
            }
        }

        static string m_resPathPrefix;

        public static void Initalize() {
            m_resPathPrefix = LuaConfig.GameResPath;
        }

        /// <summary>
        /// lua调用，获取二进制数据
        /// </summary>
        public static byte[] LLoadBinaryAssetSyn(string assetPath)
        {
            TextAsset asset = LoadAssetSync<TextAsset>(assetPath);
            if (asset == null) return null;
            return asset.bytes;
        }

        public static void LLoadSceneAsync(string scenePath, LuaFunction onComplete, LuaFunction onProgress)
        {

            async UniTask WaitAsync()
            {
                var handler = LoadScene(scenePath);
                handler.OnProgress += (float progress) => { onProgress?.Action(progress); };
                AssetHandlerUpdater.AddHandler(handler);
                await handler;
                if (handler.Error != null) {
                    onComplete.Action(handler.Error, handler.Scene);
                } else {
                    onComplete.Action<string, UnityEngine.SceneManagement.Scene>(null, handler.Scene);
                }
                onComplete.Dispose();
                onComplete = null;
                onProgress?.Dispose();
                onProgress = null;
            }
            WaitAsync().Forget();
        }


        public static T LoadAssetSync<T>(string assetPath) where T : UnityEngine.Object
        {
            return LoadAssetSync(typeof(T), assetPath) as T;
        }

        /// <summary>
        /// 从AssetBundle里异步获取资源
        /// </summary>
        public static UnityEngine.Object LoadAssetSync(Type type, string assetPath)
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetFullPath = $"{m_resPathPrefix}/{assetPath}";
            if (!package.CheckLocationValid(assetFullPath)) {
                throw new XLuaFrameworkExtException(
                    $"无效的资源地址 path:{assetFullPath}");
            }
            AssetHandle assetHandle = package.LoadAssetSync(assetFullPath, type);
            if (assetHandle.Status == EOperationStatus.Failed) {
                throw new XLuaFrameworkExtException(
                    $"加载资源失败 err:{assetHandle.LastError} path:{assetFullPath}");
            }
            return assetHandle.AssetObject;
        }

        /// <summary>
        /// 异步获取资源(assetPath不带后缀名)
        /// </summary>
        public static void LoadAssetAsync<T>(string assetPath, Action<Exception, T> onComplete, Action<float> onProgress) where T : UnityEngine.Object
        {
            async UniTask WaitAsync()
            {
                var handler = LoadAsset<T>(assetPath);
                handler.OnProgress += onProgress;
                AssetHandlerUpdater.AddHandler(handler);
                await handler;
                if (handler.Error != null) {
                    var e = new XLuaFrameworkExtException(handler.Error);
                    onComplete(e, null);
                    return;
                }
                onComplete(null, handler.AssetObject as T);
            }
            WaitAsync().Forget();
        }

        public static void LLoadAssetAsync(Type tp, string assetPath, LuaFunction onComplete, LuaFunction onProgress)
        {
            async UniTask WaitAsync()
            {
                var handler = LoadAsset(tp, assetPath);
                handler.OnProgress += (float progress) => { onProgress?.Action(progress); };
                AssetHandlerUpdater.AddHandler(handler);
                await handler;
                if (handler.Error != null) {
                    onComplete.Action<string, UnityEngine.Object>(handler.Error, null);
                } else {
                    onComplete.Action<string, UnityEngine.Object>(null, handler.AssetObject);
                }
                onComplete.Dispose();
                onComplete = null;
                onProgress?.Dispose();
                onProgress = null;
            }
            WaitAsync().Forget();
        }

        public static void LLoadAssetListAsync(string[] assetPaths, LuaFunction onComplete, LuaFunction onProgress)
        {
            async UniTask WaitAsync()
            {
                var handler = LoadAssetList(assetPaths);
                handler.OnProgress += (float progress) => { onProgress?.Action(progress); };
                AssetHandlerUpdater.AddHandler(handler);
                await handler;
                if (handler.Error != null) {
                    onComplete.Action(handler.Error);
                } else {
                    onComplete.Action<string>(null);
                }
                onComplete.Dispose();
                onComplete = null;
                onProgress?.Dispose();
                onProgress = null;
            }
            WaitAsync().Forget();
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory()
        {
            //Debug.LogError("主动GC");
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        /// <summary>
        /// 从AssetBundle里异步加载场景
        /// </summary>
        public static YooAssetAsyncSceneHandler LoadScene(string assetPath)
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetFullPath = $"{m_resPathPrefix}/{assetPath}";
            if (!package.CheckLocationValid(assetFullPath)) {
                throw new XLuaFrameworkExtException(
                    $"无效的资源地址 path:{assetFullPath}");
            }
            SceneHandle sceneHandle = package.LoadSceneAsync(assetFullPath);
            var hander = YooAssetAsyncSceneHandler.Get();
            hander.Init(sceneHandle);
            return hander;
        }

        /// <summary>
        /// 从AssetBundle里异步获取资源
        /// </summary>
        public static YooAssetAsyncHandler LoadAsset(Type type, string assetPath)
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetFullPath = $"{m_resPathPrefix}/{assetPath}";
            if (!package.CheckLocationValid(assetFullPath)) {
                throw new XLuaFrameworkExtException(
                    $"无效的资源地址 path:{assetFullPath}");
            }
            AssetHandle assetHandle = package.LoadAssetAsync(assetFullPath, type);
            var hander = YooAssetAsyncHandler.Get();
            hander.Init(assetHandle);
            return hander;
        }

        /// <summary>
        /// 从AssetBundle里异步获取资源
        /// </summary>
        public static YooAssetAsyncHandler LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            return LoadAsset(typeof(T), assetPath);
        }

        public static ResAsyncHandler LoadAssetList(string[] assetPaths)
        {
            var handler = YooAssetListAsyncHandler.Get();
            handler.Init();
            if (assetPaths.Length == 0) {
                return handler;
            }
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetInfos = new AssetInfo[assetPaths.Length];
            for (int i=0; i<assetPaths.Length; i++) {
                var assetPath = assetPaths[i];
                var assetFullPath = $"{m_resPathPrefix}/{assetPath}";
                if (!package.CheckLocationValid(assetFullPath)) {
                    throw new XLuaFrameworkExtException(
                        $"无效的资源地址 path:{assetFullPath}");
                }
                assetPaths[i] = assetFullPath;
                assetInfos[i] = package.GetAssetInfo(assetFullPath);
            }

            for (var i=0; i<assetInfos.Length; i++) {
                var assetInfo = assetInfos[i];
                var assetHandle = package.LoadAssetAsync(assetPaths[i], assetInfo.AssetType);
                handler.AddAssetHandler(assetHandle);
            }
            return handler;
        }

        /// <summary>
        /// 网络管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal static void Update(float elapseSeconds, float realElapseSeconds)
        {
            AssetHandlerUpdater.Update();
        }
    }
}
