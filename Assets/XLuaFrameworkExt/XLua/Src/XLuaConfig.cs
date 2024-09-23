using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XLua
{
    public static class XLuaConfig
    {

        public static string RootPath { get; private set; }

        public static string GenCSPath { get; private set; }

        public static string GenLuaPath { get; private set; }

        public static string AssetGenLuaPath { get; private set; }

        //默认lua搜索目录
        private static List<string> luaSearchPaths = new();

        static XLuaConfig()
        {
            InitRootPath();
            InitGenCSPath();
            InitGenLuaPath();
            InitLuaSearchPath();
        }

        private static void InitRootPath()
        {
#if !XLUA_HAS_GEN_CONFIG
            RootPath = $"{Application.dataPath}/XLua";
            foreach(var type in (from type in XLua.Utils.GetAllTypes()
            where type.IsAbstract && type.IsSealed
            select type))
            {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.IsDefined(typeof(RootPathAttribute), false)) 
                    {
                        if (field.FieldType == typeof(string)) {
                            RootPath = field.GetValue(null) as string;
                        }
                        else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string", 
                                type.Name, field.Name, field.FieldType.Name));
                        }
                    }
                }

                foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (prop.IsDefined(typeof(RootPathAttribute), false))
                    {
                        if (prop.PropertyType == typeof(string)) {
                            RootPath = prop.GetValue(null, null) as string;
                        }
                        else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string", 
                                type.Name, prop.Name, prop.PropertyType.Name));
                        }
                    }
                }
            }
            RootPath = LuaTools.GetRegularPath(RootPath);

            //去掉最后的斜杠
            if (RootPath.EndsWith("/")) RootPath = RootPath.Substring(0, RootPath.Length-1);
#else
            RootPath = CSObjectWrap.Config.XLuaAutoGenConfig.RootPath;
#endif
        }

        private static void InitGenCSPath()
        {
#if !XLUA_HAS_GEN_CONFIG
            GenCSPath = $"{RootPath}/Gen/CS"; //default csharp path
            foreach(var type in (from type in Utils.GetAllTypes()
            where type.IsAbstract && type.IsSealed
            select type))
            {
                var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (field.IsDefined(typeof(GenCSPathAttribute), false))
                    {
                        if (field.FieldType == typeof(string)) {
                            GenCSPath = field.GetValue(null) as string;
                        }
                        else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string", 
                                type.Name, field.Name, field.FieldType.Name));
                        }
                    }
                }

                var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var prop in props)
                {
                    if (prop.IsDefined(typeof(GenCSPathAttribute), false))
                    {
                        if (prop.PropertyType == typeof(string)) {
                            GenCSPath = prop.GetValue(null, null) as string;
                        }
                        else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string", 
                                type.Name, prop.Name, prop.PropertyType.Name));
                        }
                    }
                }
            }
            GenCSPath = LuaTools.GetRegularPath(GenCSPath);
            //去掉最后的斜杠
            if (GenCSPath.EndsWith("/")) GenCSPath = GenCSPath.Substring(0, GenCSPath.Length-1);
#else
            GenCSPath = CSObjectWrap.Config.XLuaAutoGenConfig.GenCSPath;
#endif
        }

        private static void InitGenLuaPath()
        {
#if !XLUA_HAS_GEN_CONFIG
            GenLuaPath = $"{RootPath}/Gen/Lua"; //default path
            foreach(var type in (from type in Utils.GetAllTypes()
            where type.IsAbstract && type.IsSealed
            select type))
            {
                var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (field.IsDefined(typeof(GenLuaPathAttribute), false))
                    {
                        if (field.FieldType == typeof(string)) {
                            GenLuaPath = field.GetValue(null) as string;
                        }
                        else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string", 
                                type.Name, field.Name, field.FieldType.Name));
                        }
                    }
                }

                var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var prop in props)
                {
                    if (prop.IsDefined(typeof(GenLuaPathAttribute), false))
                    {
                        if (prop.PropertyType == typeof(string)) {
                            GenLuaPath = prop.GetValue(null, null) as string;
                        }
                        else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string", 
                                type.Name, prop.Name, prop.PropertyType.Name));
                        }
                    }
                }
            }
            GenLuaPath = LuaTools.GetRegularPath(GenLuaPath);
            //去掉最后的斜杠
            if (GenLuaPath.EndsWith("/")) GenLuaPath = GenLuaPath.Substring(0, GenLuaPath.Length-1);
#else
            GenLuaPath = CSObjectWrap.Config.XLuaAutoGenConfig.GenLuaPath;
#endif

            AssetGenLuaPath = LuaTools.AbsolutePathToAssetPath(GenLuaPath);
        }

        private static void InitLuaSearchPath()
        {
#if UNITY_EDITOR
            ClearLuaSearchPaths();

            foreach(var type in (from type in Utils.GetAllTypes()
            where type.IsAbstract && type.IsSealed
            select type))
            {
                var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (field.IsDefined(typeof(AddLuaPathAttribute), false))
                    {
                        var obj = field.GetValue(null);
                        if (obj is string) {
                            AddLuaSearchPath(obj as string);
                        } else if (obj is IEnumerable<string>) {
                            AddRangeLuaSearchPath(obj as IEnumerable<string>);
                        } else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string or IEnumerable<string>", type.Name, field.Name, field.FieldType.Name));
                        }
                    }
                }

                var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var prop in props)
                {
                    if (prop.IsDefined(typeof(AddLuaPathAttribute), false))
                    {
                        var obj = prop.GetValue(null, null);
                        if (obj is string) {
                            AddLuaSearchPath(obj as string);
                        } else if (obj is IEnumerable<string>) {
                            AddRangeLuaSearchPath(obj as IEnumerable<string>);
                        } else {
                            UnityEngine.Debug.LogWarning(String.Format("{0}.{1} type is {2} expect string or IEnumerable<string>", type.Name, prop.Name, prop.PropertyType.Name));
                        }
                    }
                }
            }
#endif
        }

        public static void ClearLuaSearchPaths()
        {
            luaSearchPaths.Clear();
        }

        public static void AddLuaSearchPath(string luaSearchPath)
        {
            luaSearchPaths.Add(luaSearchPath);
        }

        public static void AddRangeLuaSearchPath(IEnumerable<string> luaSearchPath)
        {
            luaSearchPaths.AddRange(luaSearchPath);
        }

        public static string[] GetLuaSearchPaths()
        {
            return luaSearchPaths.ToArray();
        }
    }

    public class RootPathAttribute : Attribute { }

    public class GenCSPathAttribute : Attribute { }

    public class GenLuaPathAttribute : Attribute { }

    public class AddLuaPathAttribute : Attribute { }
}
