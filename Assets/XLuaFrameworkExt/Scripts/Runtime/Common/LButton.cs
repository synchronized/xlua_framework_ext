using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace XLuaFrameworkExt
{
    public class LButton : MonoBehaviour , 
        IPointerClickHandler, 
        IPointerDownHandler, IPointerUpHandler, 
        IDragHandler
    {
        public LuaTable self;
        public float canTriggerInterval = 0.5f;

        #region 集成IPointerClickHandler
        public LuaFunction onPointerClick;

        public void OnPointerClick(PointerEventData eventData) {
            if (!enabled) return;
            if (onPointerClick != null) {
                if (self == null) {
                    onPointerClick.Call(eventData);
                } else {
                    onPointerClick.Call(self, eventData);
                }
            }
        }

        public LuaFunction onPointerEnter;

        public void OnPointerEnter(PointerEventData eventData) {
            if (!enabled) return;
            if (onPointerEnter != null) {
                if (self == null) {
                    onPointerEnter.Call(eventData);
                } else {
                    onPointerEnter.Call(self, eventData);
                }
            }
        }

        public LuaFunction onPointerExit;

        public void OnPointerExit(PointerEventData eventData) {
            if (!enabled) return;
            if (onPointerExit != null) {
                if (self == null) {
                    onPointerExit.Call(eventData);
                } else {
                    onPointerExit.Call(self, eventData);
                }
            }
        }
        #endregion

        #region 集成IPointerDownHandler, IPointerUpHandler
        public LuaFunction onPointerDown;
        public void OnPointerDown(PointerEventData eventData) {
            if (!enabled) return;
            if (onPointerDown != null) {
                if (self == null) {
                    onPointerDown.Call(eventData);
                } else {
                    onPointerDown.Call(self, eventData);
                }
            }
        }

        public LuaFunction onPointerUp;
        public void OnPointerUp(PointerEventData eventData) {
            if (!enabled) return;
            if (onPointerUp != null) {
                if (self == null) {
                    onPointerUp.Call(eventData);
                } else {
                    onPointerUp.Call(self, eventData);
                }
            }
        }
        #endregion

        #region 集成IDragHandler
        public LuaFunction onDrag;

        public void OnDrag(PointerEventData eventData)
        {
            if (!enabled) return;
            if (onDrag != null) {
                if (self == null) {
                    onDrag.Call(eventData);
                } else {
                    onDrag.Call(self, eventData);
                }
            }
        }
        #endregion
    }
}