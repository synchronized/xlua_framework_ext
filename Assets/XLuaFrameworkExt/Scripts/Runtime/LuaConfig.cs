using System.IO;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace XLuaFrameworkExt
{
    /// <summary>
    /// 项目配置
    /// </summary>
    public static class LuaConfig
    {

        public readonly static float origScreenHeigth = 750;

        public readonly static string frameworkRoot = Application.dataPath + "/XLuaFrameworkExt";

        [RootPath]
        public readonly static string toluaRootPath = $"{frameworkRoot}/XLua";

        /// <summary>
        /// 开发专用目录（Lua脚本和预制体，声音所在目录）
        /// </summary>
        public readonly static string LuaDevPath = "LuaDev";

        public readonly static string GameResPath = $"Assets/{LuaDevPath}/GameRes";

        [AddLuaPath]
        public static IEnumerable<string> AddLuaPath {
            get {
                return new List<string>() {
                    $"{LuaConfig.frameworkRoot}/Lua",
                    $"{Application.dataPath}/{LuaConfig.LuaDevPath}/Lua",
                };
            }
        }
    }
}
