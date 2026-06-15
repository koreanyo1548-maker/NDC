using System;
using System.Collections.Generic;
using Features;
using JetBrains.Annotations;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Object = UnityEngine.Object;
using Text = TMPro.TextMeshProUGUI;

namespace UIBases
{
    public abstract class UI_Base : MonoBehaviour
    {
        protected bool _isInit;
        private Dictionary<Type, Object[]> _objects = new();

        public virtual bool Init()
        {
            if (_isInit) return false;
            _isInit = true;
            return true;
        }

        protected void ManualAdd(Type type, int idx, Object obj)
        {
            _objects[type][idx] = obj;
        }
        
        public static void BindEvent(GameObject go, Func<Boolean> condition, Action<PointerEventData> action, 
        UIEffectType effect = UIEffectType.None, UIEvent type = UIEvent.Click)
        {
            var evt = Util.GetOrAddComponent<UI_EventHandler>(go);

            Action<PointerEventData> newAction = ev =>
            {
                if (condition()) action(ev);
            };
            
            UIEffectType NewEffect() => condition() ? effect : UIEffectType.None;
            UIEffectType NewLongEffect() => condition() ? effect + 1 : UIEffectType.None;
            switch (type)
            {
                case UIEvent.Click:
                    evt.OnClickHandler -= newAction;
                    evt.OnClickHandler += newAction;
                    evt.DownEffect = NewEffect;
                    evt.UpEffect = NewEffect;
                    break;
                case UIEvent.Down:
                    evt.OnPointerDownHandler -= newAction;
                    evt.OnPointerDownHandler += newAction;
                    evt.DownEffect = NewEffect;
                    break;
                case UIEvent.Up:
                    evt.OnPointerUpHandler -= newAction;
                    evt.OnPointerUpHandler += newAction;
                    evt.UpEffect = NewEffect;
                    break;
                case UIEvent.LongClick:
                    evt.OnLongClickHandler -= newAction;
                    evt.OnLongClickHandler += newAction;
                    evt.DownEffect = NewEffect;
                    evt.UpEffect = () => effect;
                    evt.LongClickEffect = NewLongEffect;
                    break;
            }
        }
        
        public static void UnbindEvent(GameObject go)
        {
            var evt = Util.GetOrAddComponent<UI_EventHandler>(go);

            evt.OnClickHandler = null;
            evt.OnPointerDownHandler = null;
            evt.OnPointerUpHandler = null;
            evt.OnLongClickHandler = null;
        }
        protected void Bind<T>(Type type) where T : Object
        {
            var names = Enum.GetNames(type);
            var objects = new Object[names.Length];
            _objects.Add(typeof(T), objects);

            for (var idx = 0; idx < names.Length; ++idx)
            {
                if (typeof(T) == typeof(GameObject))
                    objects[idx] = Util.FindChild(gameObject, names[idx], true);
                else
                {
                    objects[idx] = Util.FindChild<T>(gameObject, names[idx], true);
                }

                if (objects[idx] == null)
                {
                    //Debug.Log($"Failed to bind {names[idx]}");
                }
            }
        }

        protected T Get<T>(int idx) where T : Object
        {
            Object[] objects = null;
            if (_objects.TryGetValue(typeof(T), out objects) == false)
                return null;
            return objects[idx] as T;
        }
        

        public T Get<T>(string tName) where T: Object
        {
            return Util.FindChild<T>(gameObject, tName, true);
        }

        public T Get<T>(string tName, int siblingIdx) where T : Object
        {
            return (Get<T>(tName) as Transform).parent.GetChild(siblingIdx).GetComponent<T>();
        }
        private void Awake()
        {
            gameObject.GetOrAddComponent<Poolable>();
            
            // var texts = transform.GetComponentsInChildren<Text>(true);
            // foreach (var text in texts)
            // {
            //     var prevMaterial = text.fontMaterial.name;
            //     text.font = Manager.Font;
            //     if (prevMaterial.Contains("Thin"))
            //     {
            //         text.fontMaterial = Manager.ThinFont;
            //     }
            //     else if (prevMaterial.Contains("Outline"))
            //     {
            //         text.fontMaterial = Manager.OutlineFont;
            //     }
            //     else if (prevMaterial.Contains("Shadow"))
            //     {
            //         text.fontMaterial = Manager.ShadowFont;
            //     }
            //     else
            //     {
            //         text.fontMaterial = Manager.NormalFont;
            //     }
            // }
        }
    }
}