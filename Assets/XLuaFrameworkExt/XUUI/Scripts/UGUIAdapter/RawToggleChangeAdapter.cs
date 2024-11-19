using UnityEngine.UI;
using System;

namespace XUUI.UGUIAdapter
{
    public class RawToggleChangeAdapter : RawAdapterBase, EventEmitter
    {
        private Toggle target;

        public Action OnAction { get; set; } // InputField发生变化需要调用OnValueChange

        public RawToggleChangeAdapter(Toggle toggle, string bindTo)
        {
            target = toggle;
            BindTo = bindTo;

            target.onValueChanged.AddListener((val) =>
            {
                if (OnAction != null)
                {
                    OnAction();
                }
            });
        }
    }
}
