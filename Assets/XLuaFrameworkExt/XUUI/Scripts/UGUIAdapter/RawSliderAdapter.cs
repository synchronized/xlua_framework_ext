using UnityEngine.UI;

namespace XUUI.UGUIAdapter
{
    public class RawSliderAdapter : RawAdapterBase, DataConsumer<float>
    {
        private Slider target;

        public float Value
        {
            set
            {
                target.value = value;
            }
        }

        public RawSliderAdapter(Slider slider, string bindTo)
        {
            target = slider;
            BindTo = bindTo;
        }
    }
}
