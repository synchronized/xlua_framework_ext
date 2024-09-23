using UnityEngine.UI;
using XLua;

namespace XLuaFrameworkExt
{
    [LuaCallCSharp]
    public static class ToggleExtend
    {
        public static void OnValueChanged(this Toggle toggle, LuaFunction onValueChanged)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((bool b) =>
            {
                onValueChanged.Call(b);
            });
        }

    }
}