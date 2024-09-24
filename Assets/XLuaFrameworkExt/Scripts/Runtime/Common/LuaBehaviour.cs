using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace XLuaFrameworkExt
{
    public class LuaBehaviour : MonoBehaviour
    {
        public string prefabPath;
        public bool isUIStack;
        public bool keepActive;
        public bool isFloat;
        LuaTable lua;
        class Functions
        {
            public LuaTable lua;
            public LuaFunction onEnable, start, onDisable, onAppFocus, onDestroy;
            public Functions(LuaTable lua, LuaFunction onEnable, LuaFunction start, LuaFunction onDisable, LuaFunction onAppFocus, LuaFunction onDestroy)
            {
                this.lua = lua;
                this.onEnable = onEnable;
                this.start = start;
                this.onDisable = onDisable;
                this.onAppFocus = onAppFocus;
                this.onDestroy = onDestroy;
            }
        }
        Dictionary<string, Functions> luas = new Dictionary<string, Functions>();

        public bool IsSetedOrder { get; private set; }
        /// <summary>
        /// Lua调用
        /// </summary>
        public void AddLuaClass(LuaTable lua, LuaFunction onEnable, LuaFunction start, LuaFunction onDisable, LuaFunction onAppFocus, LuaFunction onDestroy)
        {
            string className = lua.Get<string>("name");
            if (!luas.ContainsKey(className))
            {
                luas.Add(className, new Functions(lua, onEnable, start, onDisable, onAppFocus, onDestroy));
            }
        }

        /// <summary>
        /// Lua调用
        /// </summary>
        public void RemoveLuaClass(LuaTable lua)
        {
            string className = lua.Get<string>("name");
            if (luas.ContainsKey(className))
            {
                luas.Remove(className);
            }
        }

        protected virtual void Awake()
        {
        }

        protected virtual void OnEnable()
        {
            foreach (var lua in luas.Values)
            {
                lua.onEnable?.Call(lua.lua);
            }
        }

        protected virtual void Start()
        {
            foreach (var lua in luas.Values)
            {
                lua.start?.Call(lua.lua);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var lua in luas.Values)
            {
                lua.onDisable?.Call(lua.lua);
            }
            IsSetedOrder = false;
        }

        protected virtual void OnApplicationFocus(bool isFocus)
        {
            foreach (var lua in luas.Values)
            {
                lua.onAppFocus?.Call(lua.lua, isFocus);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var lua in luas.Values)
            {
                lua.onDestroy?.Call(lua.lua);
            }
        }
    }
}
