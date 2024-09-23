using UnityEngine;
using System.Collections.Generic;
using XLua;
using System.IO;
using System.Text;
using System.Linq;
using CSObjectWrapEditor;

namespace XLua.Editor
{
    public class AutoGenConfig : ScriptableObject
    {
        public TextAsset Template;

        public static IEnumerable<CustomGenTask> GetTasks(LuaEnv lua_env, UserConfig user_cfg)
        {
            LuaTable data = lua_env.NewTable();
            data.Set("RootPath", LuaTools.AbsolutePathToAssetPath(XLuaConfig.RootPath, false));
            data.Set("GenCSPath", LuaTools.AbsolutePathToAssetPath(XLuaConfig.GenCSPath, false));
            data.Set("GenLuaPath", LuaTools.AbsolutePathToAssetPath(XLuaConfig.GenLuaPath, false));

            yield return new CustomGenTask
            {
                Data = data,
                Output = new StreamWriter(GeneratorConfig.common_path + "/XLuaAutoGenConfig.cs", false, Encoding.UTF8)
            };
        }

        [GenCodeMenu]//加到Generate Code菜单里头
        public static void GenAutoGenConfig()
        {
            Generator.CustomGen(ScriptableObject.CreateInstance<AutoGenConfig>().Template.text, GetTasks);
        }
    }
}