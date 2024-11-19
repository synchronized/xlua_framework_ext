using UnityEngine.UI;

namespace XUUI.UGUIAdapter
{
    public class RawTMPTextAdapter : RawAdapterBase, DataConsumer<string>
    {
        private TMPro.TMP_Text target;

        public string Value
        {
            set
            {
                target.text = value;
            }
        }

        public RawTMPTextAdapter(TMPro.TMP_Text text, string bindTo)
        {
            target = text;
            BindTo = bindTo;
        }
    }
}
