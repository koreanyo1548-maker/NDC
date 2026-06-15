using System;
using System.Collections.Generic;
using Managers;
using Managers.Base;
using UIBases;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utils
{
    public static class Extension
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return Util.GetOrAddComponent<T>(go);
        }

        public static void BindEvent(this GameObject go, Func<Boolean> condition, Action<PointerEventData> action,
            UIEffectType effect = UIEffectType.None, bool needSound = true, UIEvent type = UIEvent.Click)
        {
            if (needSound)
                action += _ => Manager.Sound.PlaySFX(SFXType.UI_Button);
            UI_Base.BindEvent(go, condition, action, effect, type);
        }
        public static void UnbindEvent(this GameObject go)
        {
            UI_Base.UnbindEvent(go);
        }

        public static bool IsValid(this GameObject go)
        {
            return go != null && go.activeSelf;
        }

        public static void SetPositionAsZero(this Transform t)
        {
            var rect = t.GetComponent<RectTransform>();
            rect.anchorMax = Define.Zero2;
            rect.anchorMin = Define.Zero2;
            t.localPosition = Define.Zero3;
        }
    }
}