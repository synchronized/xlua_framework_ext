using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace XUUI.UGUIAdapter
{
    public enum ComponentType
    {
        Text,
        TMP_Text,
        InputField,
        TMP_InputField,
        Button,
        Dropdown,
        Slider,
        Toggle,
        ToggleChange,
    }

    [Serializable]
    public struct Binding
    {
        [SerializeField]
        public ComponentType Type;

        [SerializeField]
        public Component Component;

        [SerializeField]
        public string BindTo;

        [SerializeField]
        public bool MultiFields;
    }

    public class ViewBinding : MonoBehaviour
    {
        [SerializeField]
        public List<Binding> Bindings;

        [NonSerialized]
        private object[][] cacheAdapters = null;

        private RawAdapterBase createAdapter(Binding binding)
        {
            switch (binding.Type) {
            case ComponentType.Text:
                var text = binding.Component as Text;
                if (text != null)
                {
                    return new RawTextAdapter(text, binding.BindTo);
                }
                break;
            case ComponentType.TMP_Text:
                var tmp_text = binding.Component as TMPro.TMP_Text;
                if (tmp_text != null)
                {
                    return new RawTMPTextAdapter(tmp_text, binding.BindTo);
                }
                break;
            case ComponentType.InputField:
                var inputField = binding.Component as InputField;
                if (inputField != null)
                {
                    return new RawInputFieldAdapter(inputField, binding.BindTo);
                }
                break;
            case ComponentType.TMP_InputField:
                var tmp_inputField = binding.Component as TMPro.TMP_InputField;
                if (tmp_inputField != null)
                {
                    return new RawTMPInputFieldAdapter(tmp_inputField, binding.BindTo);
                }
                break;
            case ComponentType.Button:
                var button = binding.Component as Button;
                if (button != null)
                {
                    return new RawButtonAdapter(button, binding.BindTo);
                }
                break;
            case ComponentType.Dropdown:
                var dropdown = binding.Component as Dropdown;
                if (dropdown != null)
                {
                    return new RawDropdownAdapter(dropdown, binding.BindTo);
                }
                break;
            case ComponentType.Slider:
                var slider = binding.Component as Slider;
                if (slider != null)
                {
                    return new RawSliderAdapter(slider, binding.BindTo);
                }
                break;
            case ComponentType.Toggle:
                var toggle = binding.Component as Toggle;
                if (toggle != null)
                {
                    return new RawToggleAdapter(toggle, binding.BindTo);
                }
                break;
            case ComponentType.ToggleChange:
                var toggleChange = binding.Component as Toggle;
                if (toggleChange != null)
                {
                    return new RawToggleChangeAdapter(toggleChange, binding.BindTo);
                }
                break;
            }

            return null;
        }

        public object[][] GetAdapters()
        {
            if (cacheAdapters == null)
            {
                if (Bindings == null || Bindings.Count == 0)
                {
                    cacheAdapters = new object[][] { };
                }
                else
                {
                    var dataConsumers = new List<object>();
                    var dataProducers = new List<object>();
                    var eventEmitters = new List<object>();

                    for (int i = 0; i < Bindings.Count; i++)
                    {
                        var binding = Bindings[i];
                        RawAdapterBase adapter = createAdapter(binding);
                        if (adapter == null)
                        {
                            throw new InvalidOperationException("no adatper for " + binding.Component);
                        }
                        if (adapter is DataConsumer)
                        {
                            dataConsumers.Add(adapter);
                        }
                        if (adapter is DataProducer)
                        {
                            dataProducers.Add(adapter);
                        }
                        if (adapter is EventEmitter)
                        {
                            eventEmitters.Add(adapter);
                        }
                    }

                    cacheAdapters = new object[][] { dataConsumers.ToArray(), dataProducers.ToArray(), eventEmitters.ToArray() };
                }
            }
            return cacheAdapters;
        }

    }
}
