using System.Collections.Generic;
using System;
using XLua;
using UnityEngine.UI;

namespace XUUI.UGUIAdapter
{

    public static class AdapterGenConfig
    {
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp = new List<Type>()
        {
            typeof(AdapterBase),
            typeof(RawAdapterBase),
            typeof(RawTextAdapter),
            typeof(RawTMPTextAdapter),
            typeof(RawInputFieldAdapter),
            typeof(RawTMPInputFieldAdapter),
            typeof(RawDropdownAdapter),
            typeof(RawButtonAdapter),
            typeof(RawSliderAdapter),
            typeof(RawToggleAdapter),
        };

        [CSharpCallLua]
        public static List<Type> CSharpCallLua = new List<Type>()
        {
            typeof(Action),
            typeof(Action<string>),
            typeof(Action<int>),
            typeof(Action<bool>),
        };
    }
}
