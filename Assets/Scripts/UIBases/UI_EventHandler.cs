using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UIBases
{
    public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action<PointerEventData> OnClickHandler = null;
        public Action<PointerEventData> OnPointerDownHandler = null;
        public Action<PointerEventData> OnPointerUpHandler = null;
        public Action<PointerEventData> OnLongClickHandler = null;

        public Func<UIEffectType> DownEffect;
        public Func<UIEffectType> UpEffect;
        public Func<UIEffectType> LongClickEffect;
        
        private bool _isPointDown;
        private bool _isLongTimeStarted;
        private bool _isLockClickBlocked = false;
        private float _longClickTriggerTime = 0.5f;
        private float _longClickedDistance = 0.1f;
        

        private static bool _isSomethingClicked = false;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isLongTimeStarted)
            {
                _isLongTimeStarted = false;
                _isSomethingClicked = false;
            }
            else
            {
                if (_isSomethingClicked) return;
                var click = OnClickHandler != null;
                if (click) OnClickHandler?.Invoke(eventData);
                else OnLongClickHandler?.Invoke(eventData);
            }
            _isLockClickBlocked = false;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isSomethingClicked) return;

            _isPointDown = true;
            _isSomethingClicked = true;
            OnPointerDownHandler?.Invoke(eventData);

            DoEffect(DownEffect(), true);
            if (OnLongClickHandler == null) return;
            
            Timing.RunCoroutine(_LongClickRoutine(eventData));

        }

        private IEnumerator<float> _LongClickRoutine(PointerEventData eventData)
        {
            _isPointDown = true;
            yield return Timing.WaitForSeconds(_longClickTriggerTime);
            if (!_isPointDown) yield break;
            
            _isLongTimeStarted = true;
            _longClickedDistance = 0.3f;
            
            while (_isLongTimeStarted && !_isLockClickBlocked)
            {
                OnLongClickHandler?.Invoke(eventData);
                yield return Timing.WaitForSeconds(_longClickedDistance);
                if (_longClickedDistance >= 0.2f) _longClickedDistance -= 0.1f;
                DoEffect(LongClickEffect());
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // if (_isLongTimeStarted)
            // {
            //     _isLongTimeStarted = false;
            // }
            DoEffect(UpEffect(), false);
            OnPointerUpHandler?.Invoke(eventData);
            
            if (_isPointDown) _isSomethingClicked = false;
            _isPointDown = false;
        }

        private void DoEffect(UIEffectType effect, bool isDown = false)
        {
            switch (effect)
            {
                case UIEffectType.None: return;
                case UIEffectType.BiBounce:
                    transform.DOKill(true);
                    transform.DOScale(Define.Shrink, 0.05f).SetEase(Ease.InQuart).OnComplete(() =>
                        transform.DOScale(Define.One, 0.05f).SetEase(Ease.OutBounce)
                        );
                    break;
                case UIEffectType.Bounce:
                    if (isDown)
                    {
                        transform.DOKill(true);
                        transform.DOScale(Define.Shrink, 0.2f).SetEase(Ease.InQuart);
                    }
                    else
                    {
                        transform.DOKill(true);
                        transform.DOScale(Define.One, 0.2f).SetEase(Ease.OutBounce);
                    }
                    break;
            }
        }

        private void OnDisable()
        {
            if (_isPointDown || _isLongTimeStarted) _isSomethingClicked = false;
        }

        public void StopLongClick()
        {
            _isLockClickBlocked = true;
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                _isPointDown = false;
                _isLongTimeStarted = false;
                _isSomethingClicked = false;
            }
        }
    }
}