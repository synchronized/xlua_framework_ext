/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if HOTFIX_ENABLE
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;
using System.Diagnostics;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif //UNITY_2019_1_OR_NEWER

namespace XLua
{
    public static class HotfixConfig
    {
        //返回-1表示没有标签
        static int getHotfixType(MemberInfo memberInfo)
        {
            try
            {
                foreach (var ca in memberInfo.GetCustomAttributes(false))
                {
                    var ca_type = ca.GetType();
                    if (ca_type.ToString() == "XLua.HotfixAttribute")
                    {
                        return (int)(ca_type.GetProperty("Flag").GetValue(ca, null));
                    }
                }
            }
            catch { }
            return -1;
        }

        static void mergeConfig(MemberInfo test, Type cfg_type, Func<IEnumerable<Type>> get_cfg, Action<Type, int> on_cfg)
        {
            int hotfixType = getHotfixType(test);
            if (-1 == hotfixType || !typeof(IEnumerable<Type>).IsAssignableFrom(cfg_type))
            {
                return;
            }

            foreach (var type in get_cfg())
            {
                if (!type.IsDefined(typeof(ObsoleteAttribute), false)
                    && !type.IsEnum && !typeof(Delegate).IsAssignableFrom(type)
                    && (!type.IsGenericType || type.IsGenericTypeDefinition))
                {
                    if ((type.Namespace == null || (type.Namespace != "XLua" && !type.Namespace.StartsWith("XLua."))))
                    {
                        on_cfg(type, hotfixType);
                    }
                }
            }
        }

        public static void GetConfig(Dictionary<string, int> hotfixCfg, IEnumerable<Type> cfg_check_types)
        {
            if (cfg_check_types != null)
            {
                Action<Type, int> on_cfg = (type, hotfixType) =>
                {
                    string key = type.FullName.Replace('+', '/');
                    if (!hotfixCfg.ContainsKey(key))
                    {
                        hotfixCfg.Add(key, hotfixType);
                    }
                };
                foreach (var type in cfg_check_types.Where(t => !t.IsGenericTypeDefinition && t.IsAbstract && t.IsSealed))
                {
                    var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                    foreach (var field in type.GetFields(flags))
                    {
                        mergeConfig(field, field.FieldType, () => field.GetValue(null) as IEnumerable<Type>, on_cfg);
                    }
                    foreach (var prop in type.GetProperties(flags))
                    {
                        mergeConfig(prop, prop.PropertyType, () => prop.GetValue(null, null) as IEnumerable<Type>, on_cfg);
                    }
                }
            }
        }

        public static List<Assembly> GetHotfixAssembly()
        {
            var projectPath = Assembly.Load("Assembly-CSharp").ManifestModule.FullyQualifiedName;
            Regex rgx = new Regex(@"^(.*)[\\/]Library[\\/]ScriptAssemblies[\\/]Assembly-CSharp.dll$");
            MatchCollection matches = rgx.Matches(projectPath);
            projectPath = matches[0].Groups[1].Value;

            List<Type> types = new List<Type>();
            Action<Type, int> on_cfg = (type, hotfixType) =>
            {
                types.Add(type);
            };

            foreach (var assmbly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in (from type in assmbly.GetTypes()
                                          where !type.IsGenericTypeDefinition
                                          select type))
                    {
                        if (getHotfixType(type) != -1)
                        {
                            types.Add(type);
                        }
                        else
                        {
                            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                            foreach (var field in type.GetFields(flags))
                            {
                                mergeConfig(field, field.FieldType, () => field.GetValue(null) as IEnumerable<Type>, on_cfg);
                            }
                            foreach (var prop in type.GetProperties(flags))
                            {
                                mergeConfig(prop, prop.PropertyType, () => prop.GetValue(null, null) as IEnumerable<Type>, on_cfg);
                            }
                        }
                    }
                }
                catch { } // 防止有的工程有非法的dll导致中断
            }
            return types.Select(t => t.Assembly).Distinct()
                .Where(a => a.ManifestModule.FullyQualifiedName.IndexOf(projectPath) == 0)
                .ToList();
        }

        public static List<string> GetHotfixAssemblyPaths()
        {
            return GetHotfixAssembly().Select(a => a.ManifestModule.FullyQualifiedName).Distinct().ToList();
        }
    }
}

namespace XLua
{
#if UNITY_2019_1_OR_NEWER
    class MyCustomBuildProcessor : IPostBuildPlayerScriptDLLs
    {
        public int callbackOrder { get { return 0; } }
        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            var dir = Path.GetDirectoryName(report.files.Single(file => file.path.EndsWith("Assembly-CSharp.dll")).path);
            Hotfix.HotfixInject(dir);
        }
    }
#endif //UNITY_2019_1_OR_NEWER

    public static class Hotfix
    {
        static bool ContainNotAsciiChar(string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] > 127)
                {
                    return true;
                }
            }
            return false;
        }

#if !UNITY_2019_1_OR_NEWER
        [PostProcessScene]
#endif //UNITY_2019_1_OR_NEWER
        [MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            HotfixInject("./Library/ScriptAssemblies");
        }

        public static void HotfixInject(string assemblyDir)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (EditorApplication.isCompiling)
            {
                UnityEngine.Debug.LogError("You can't inject before the compilation is done");
                return;
            }

#if UNITY_EDITOR_OSX
			var mono_path = Path.Combine(Path.GetDirectoryName(typeof(UnityEngine.Debug).Module.FullyQualifiedName),
				"../MonoBleedingEdge/bin/mono");
			if(!File.Exists(mono_path))
			{
				mono_path = Path.Combine(Path.GetDirectoryName(typeof(UnityEngine.Debug).Module.FullyQualifiedName),
					"../../MonoBleedingEdge/bin/mono");
			}
			if(!File.Exists(mono_path))
			{
				UnityEngine.Debug.LogError("can not find mono!");
			}
#elif UNITY_EDITOR_WIN
            var mono_path = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Data/MonoBleedingEdge/bin/mono.exe");
#endif //UNITY_EDITOR_OSX
            var inject_tool_path = "./Tools/XLuaHotfixInject.exe";
            if (!File.Exists(inject_tool_path))
            {
                UnityEngine.Debug.LogError("please install the Tools");
                return;
            }

            var save_path = CSObjectWrapEditor.GeneratorConfig.common_path;
            var assembly_csharp_path = Path.Combine(assemblyDir, "Assembly-CSharp.dll");
            var id_map_file_path = $"{save_path}/Resources/hotfix_id_map.lua.txt";
            var hotfix_cfg_in_editor = $"{save_path}/hotfix_cfg_in_editor.data";

            Dictionary<string, int> editor_cfg = new Dictionary<string, int>();
            Assembly editor_assembly = typeof(Hotfix).Assembly;
            HotfixConfig.GetConfig(editor_cfg, Utils.GetAllTypes().Where(t => t.Assembly == editor_assembly));

            if (!Directory.Exists(save_path))
            {
                Directory.CreateDirectory(save_path);
            }

            using (BinaryWriter writer = new BinaryWriter(new FileStream(hotfix_cfg_in_editor, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(editor_cfg.Count);
                foreach (var kv in editor_cfg)
                {
                    writer.Write(kv.Key);
                    writer.Write(kv.Value);
                }
            }

#if UNITY_2019_1_OR_NEWER
            List<string> args = new List<string>() { assembly_csharp_path, assembly_csharp_path, id_map_file_path, hotfix_cfg_in_editor };
#else
            List<string> args = new List<string>() { assembly_csharp_path, typeof(LuaEnv).Module.FullyQualifiedName, id_map_file_path, hotfix_cfg_in_editor };
#endif //UNITY_2019_1_OR_NEWER

            foreach (var path in
                (from asm in AppDomain.CurrentDomain.GetAssemblies() select asm.ManifestModule.FullyQualifiedName)
                 .Distinct())
            {
                try
                {
                    args.Add(System.IO.Path.GetDirectoryName(path));
                }
                catch (Exception)
                {

                }
            }
            var injectAssemblyPaths = HotfixConfig.GetHotfixAssemblyPaths();
            var idMapFileNames = new List<string>();
            foreach (var injectAssemblyPath in injectAssemblyPaths)
            {
                args[0] = Path.Combine(assemblyDir, Path.GetFileName(injectAssemblyPath));
                if (ContainNotAsciiChar(args[0]))
                {
                    throw new Exception("project path must contain only ascii characters");
                }

                var injectAssemblyFileName = Path.GetFileName(injectAssemblyPath);
                args[2] = save_path + "/Resources/hotfix_id_map_" + injectAssemblyFileName.Substring(0, injectAssemblyFileName.Length - 4) + ".lua.txt";
                idMapFileNames.Add(args[2]);
                Process hotfix_injection = new Process();
                hotfix_injection.StartInfo.FileName = mono_path;
#if UNITY_5_6_OR_NEWER
                hotfix_injection.StartInfo.Arguments = "--runtime=v4.0.30319 " + inject_tool_path + " \"" + String.Join("\" \"", args.ToArray()) + "\"";
#else
                hotfix_injection.StartInfo.Arguments = inject_tool_path + " \"" + String.Join("\" \"", args.ToArray()) + "\"";
#endif //UNITY_5_6_OR_NEWER
                hotfix_injection.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                hotfix_injection.StartInfo.RedirectStandardOutput = true;
                hotfix_injection.StartInfo.UseShellExecute = false;
                hotfix_injection.StartInfo.CreateNoWindow = true;
                hotfix_injection.Start();
                UnityEngine.Debug.Log(hotfix_injection.StandardOutput.ReadToEnd());
                hotfix_injection.WaitForExit();
            }

            File.Delete(hotfix_cfg_in_editor);
            AssetDatabase.Refresh();
        }
    }
}
#endif //HOTFIX_ENABLE
