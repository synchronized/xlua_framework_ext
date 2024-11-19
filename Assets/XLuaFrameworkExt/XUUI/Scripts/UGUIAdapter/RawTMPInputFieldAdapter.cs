using UnityEngine.UI;
using System;

namespace XUUI.UGUIAdapter
{
    public class RawTMPInputFieldAdapter : RawAdapterBase, DataConsumer<string>, DataProducer<string>
    {
        private TMPro.TMP_InputField target;

        public Action<string> OnValueChange { get; set; } // InputField发生变化需要调用OnValueChange

        public string Value // VM发生变化，会调用到该Setter，需要同步给InputField
        {
            set
            {
                target.text = value == null ? "" : value;
            }
        }

        public RawTMPInputFieldAdapter(TMPro.TMP_InputField input, string bindTo)
        {
            target = input;
            BindTo = bindTo;

            target.onValueChanged.AddListener((val) =>
            {
                if (OnValueChange != null)
                {
                    OnValueChange(val);
                }
            });
        }
    }
}
