﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XLua;

namespace XLuaFrameworkExt.Config
{
    public static class GenConfig
    {
        /***************如果你全lua编程，可以参考这份自动化配置***************/
        //--------------begin 纯lua编程配置参考----------------------------
        static List<string> exclude = new List<string> {
            "HideInInspector", "ExecuteInEditMode",
            "AddComponentMenu", "ContextMenu",
            "RequireComponent", "DisallowMultipleComponent",
            "SerializeField", "AssemblyIsEditorAssembly",
            "Attribute", "Types",
            "UnitySurrogateSelector", "TrackedReference",
            "TypeInferenceRules", "FFTWindow",
            "RPC", "Network", "MasterServer",
            "BitStream", "HostData",
            "ConnectionTesterStatus", "GUI", "EventType",
            "EventModifiers", "FontStyle", "TextAlignment",
            "TextEditor", "TextEditorDblClickSnapping",
            "TextGenerator", "TextClipping", "Gizmos",
            "ADBannerView", "ADInterstitialAd",
            "Android", "Tizen", "jvalue",
            "iPhone", "iOS", "Windows", "CalendarIdentifier",
            "CalendarUnit", "CalendarUnit",
            "ClusterInput", "FullScreenMovieControlMode",
            "FullScreenMovieScalingMode", "Handheld",
            "LocalNotification", "NotificationServices",
            "RemoteNotificationType", "RemoteNotification",
            "SamsungTV", "TextureCompressionQuality",
            "TouchScreenKeyboardType", "TouchScreenKeyboard",
            "MovieTexture", "UnityEngineInternal",
            "Terrain", "Tree", "SplatPrototype",
            "DetailPrototype", "DetailRenderMode",
            "MeshSubsetCombineUtility", "AOT", "Social", "Enumerator",
            "SendMouseEvents", "Cursor", "Flash", "ActionScript",
            "OnRequestRebuild", "Ping",
            "ShaderVariantCollection", "SimpleJson.Reflection",
            "CoroutineTween", "GraphicRebuildTracker",
            "Advertisements", "UnityEditor", "WSA",
            "EventProvider", "Apple",
            "ClusterInput", "Motion",
            "UnityEngine.UI.ReflectionMethodsCache", "NativeLeakDetection",
            "NativeLeakDetectionMode", "WWWAudioExtensions", "UnityEngine.Experimental",

            "UnityEngine.InputSystem.LowLevel",
            "UnityEngine.InputSystem.InputControlExtensions",
        };

        static bool isExcluded(Type type)
        {
            var fullName = type.FullName;
            for (int i = 0; i < exclude.Count; i++)
            {
                if (fullName.Contains(exclude[i]))
                {
                    return true;
                }
            }
            return false;
        }

        [LuaCallCSharp]
        public static List<Type> dotween_lua_call_cs_list = new List<Type>()
        {
            typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),
            typeof(DG.Tweening.Core.TweenerCore<Color, Color, DG.Tweening.Plugins.Options.ColorOptions>),
            //typeof(System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>),
        };

        static List<string> userExclude = new List<string> {
            "BundleStream",
        };
        static bool isUserExcluded(Type type)
        {
            var fullName = type.FullName;
            foreach (var exclude in userExclude)
            {
                if (fullName.Contains(exclude))
                {
                    return true;
                }
            }
            return false;
        }

        [LuaCallCSharp]
        public static IEnumerable<Type> LuaCallCSharp
        {
            get
            {
                List<string> namespaces = new List<string>() // 在这里添加名字空间
                {
                    "UnityEngine",
                    "UnityEngine.UI",
                    "UnityEngine.InputSystem",
                    "DG.Tweening",
                };
                var unityTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                                from type in assembly.GetExportedTypes()
                                where type.Namespace != null 
                                where namespaces.Contains(type.Namespace) 
                                where !isExcluded(type)
                                where type.BaseType != typeof(MulticastDelegate) 
                                where !type.IsInterface 
                                where !type.IsEnum
                                select type);

                string[] customAssemblys = new string[] {
                    "Assembly-CSharp",
                };
                var customTypes = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
                                from type in assembly.GetExportedTypes()
                                where type.Namespace == null || !type.Namespace.StartsWith("XLua")
                                where !isUserExcluded(type)
                                where type.BaseType != typeof(MulticastDelegate) 
                                where !type.IsInterface 
                                where !type.IsEnum
                                select type);
                return unityTypes.Concat(customTypes);
            }
        }

        static List<string> delegateExcludeList = new List<string>() {
            "UnityEngine.InputSystem.LowLevel",
        };

        static bool isDelegateExclude(Type delegateType)
        {
            var fullName = delegateType.FullName;
            foreach (var exclude in  delegateExcludeList)
            {
                if (fullName.Contains(exclude))
                {
                    return true;
                }
            }
            return false;
        }

        //自动把LuaCallCSharp涉及到的delegate加到CSharpCallLua列表，后续可以直接用lua函数做callback
        [CSharpCallLua]
        public static List<Type> CSharpCallLua
        {
            get
            {
                var lua_call_csharp = LuaCallCSharp;
                var delegate_types = new List<Type>();
                var flag = BindingFlags.Public 
                    | BindingFlags.Instance
                    | BindingFlags.Static 
                    | BindingFlags.IgnoreCase 
                    | BindingFlags.DeclaredOnly;

                foreach (var field in lua_call_csharp.SelectMany(type => type.GetFields(flag)))
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        delegate_types.Add(field.FieldType);
                    }
                }

                foreach (var method in lua_call_csharp.SelectMany(type => type.GetMethods(flag)))
                {
                    if (typeof(Delegate).IsAssignableFrom(method.ReturnType))
                    {
                        delegate_types.Add(method.ReturnType);
                    }
                    foreach (var param in method.GetParameters())
                    {
                        var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                        if (typeof(Delegate).IsAssignableFrom(paramType))
                        {
                            delegate_types.Add(paramType);
                        }
                    }
                }
                return delegate_types
                    .Where(t => t.BaseType == typeof(MulticastDelegate))
                    .Where(t => !hasGenericParameter(t))
                    .Where(t => !delegateHasEditorRef(t))
                    .Where(t => !isDelegateExclude(t))
                    .Distinct()
                    .ToList();
            }
        }
        //--------------end 纯lua编程配置参考----------------------------

        //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
        [CSharpCallLua]
        public static List<Type> CSharpCallLuaCustom = new List<Type>() {
            // unity
            typeof(System.Action),
            typeof(System.Action<int>),
            typeof(System.Collections.IEnumerator),
            typeof(UnityEngine.Event),
            typeof(UnityEngine.Events.UnityAction),
            typeof(UnityEngine.Events.UnityAction<Vector2>),
        };

        //黑名单
        [BlackList]
        public static List<List<string>> BlackList = new List<List<string>>()  {

            new List<string>(){"System.IO.File", "GetAccessControl"},
            new List<string>(){"System.IO.File", "SetAccessControl"},
            new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
            new List<string>(){"System.IO.Directory", "SetAccessControl"},
            new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},

            // unity
            new List<string>(){"UnityEngine.AnimationClip", "averageDuration"},
            new List<string>(){"UnityEngine.AnimationClip", "averageAngularSpeed"},
            new List<string>(){"UnityEngine.AnimationClip", "averageSpeed"},
            new List<string>(){"UnityEngine.AnimationClip", "apparentSpeed"},
            new List<string>(){"UnityEngine.AnimationClip", "isLooping"},
            new List<string>(){"UnityEngine.AnimationClip", "isAnimatorMotion"},
            new List<string>(){"UnityEngine.AnimationClip", "isHumanMotion"},
            new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
            new List<string>(){"UnityEngine.AnimatorControllerParameter", "name"},
            new List<string>(){"UnityEngine.Application", "ExternalEval"},
            new List<string>(){"UnityEngine.Caching", "SetNoBackupFlag"},
            new List<string>(){"UnityEngine.Caching", "ResetNoBackupFlag"},
            new List<string>(){"UnityEngine.CanvasRenderer", "OnRequestRebuild"},
            new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
            new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
            new List<string>(){"UnityEngine.Graphic", "OnRebuildRequested"},
            new List<string>(){"UnityEngine.Light", "areaSize"},
            new List<string>(){"UnityEngine.Light", "lightmappingMode"},
            new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
            new List<string>(){"UnityEngine.Light", "shadowAngle"},
            new List<string>(){"UnityEngine.Light", "shadowRadius"},
            new List<string>(){"UnityEngine.Light", "SetLightDirty"},

            new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
            new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
            new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
            new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
            new List<string>(){"UnityEngine.WebCamTexture", "MarkNonReadable"},
            new List<string>(){"UnityEngine.WebCamTexture", "isReadable"},
            new List<string>(){"UnityEngine.WWW", "movie"},
            new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},

            new List<string>(){"UnityEngine.InputSystem.InputSystem", "onDeviceCommand"},
        };

        static bool hasGenericParameter(Type type)
        {
            if (type.IsGenericTypeDefinition) return true;
            if (type.IsGenericParameter) return true;
            if (type.IsByRef || type.IsArray)
            {
                return hasGenericParameter(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (hasGenericParameter(typeArg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool typeHasEditorRef(Type type)
        {
            if (type.Namespace != null && (type.Namespace == "UnityEditor" || type.Namespace.StartsWith("UnityEditor.")))
            {
                return true;
            }
            if (type.IsNested)
            {
                return typeHasEditorRef(type.DeclaringType);
            }
            if (type.IsByRef || type.IsArray)
            {
                return typeHasEditorRef(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (typeArg.IsGenericParameter) {
                        //skip unsigned type parameter
                        continue;
                    } 
                    if (typeHasEditorRef(typeArg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool delegateHasEditorRef(Type delegateType)
        {
            if (typeHasEditorRef(delegateType)) return true;
            var method = delegateType.GetMethod("Invoke");
            if (method == null)
            {
                return false;
            }
            if (typeHasEditorRef(method.ReturnType)) return true;
            return method.GetParameters().Any(pinfo => typeHasEditorRef(pinfo.ParameterType));
        }

    }

}
