using UnityEngine;
using XLua;

namespace XLuaFrameworkExt
{

    [LuaCallCSharp]
    public static class LButtonExtend
    {

        private static T GetOrAddComponent<T>(this Transform transform) where T : Component {
            T component = transform.GetComponent<T>();
            if (!component)
            {
                component = transform.gameObject.AddComponent<T>();
            }
            return component;
        }

        public static LButton OnPointerClick(this Transform btn, LuaFunction clickEvent, LuaTable self = null)
        {
            LButton bButton = GetOrAddComponent<LButton>(btn);
            bButton.onPointerClick = clickEvent;
            bButton.self = self;
            return bButton;
        }

        public static LButton OnPointerDown(this Transform btn, LuaFunction downEvent, LuaTable self = null)
        {
            LButton bButton = GetOrAddComponent<LButton>(btn);
            bButton.onPointerDown = downEvent;
            bButton.self = self;
            return bButton;
        }

        public static LButton OnPointerUp(this Transform btn, LuaFunction upEvent, LuaTable self = null)
        {
            LButton bButton = GetOrAddComponent<LButton>(btn);
            bButton.onPointerUp = upEvent;
            bButton.self = self;
            return bButton;
        }

        public static LButton OnDrag(this Transform btn, LuaFunction dragEvent, LuaTable self = null)
        {
            LButton bButton = GetOrAddComponent<LButton>(btn);
            bButton.onDrag = dragEvent;
            bButton.self = self;
            return bButton;
        }

        public static LButton ClearEvent(this Transform btn)
        {
            LButton bButton = btn.GetComponent<LButton>();
            if (bButton)
            {
                GameObject.Destroy(bButton);
            }
            return bButton;
        }

    }
}