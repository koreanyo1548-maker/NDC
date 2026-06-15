using System;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace UIs.Utils
{
    public class CanvasFadeInOut: MonoBehaviour
    {
        [Tooltip("알파값이 0 > 1 되는 시간(초)")] public float FadeInTime;
        [Tooltip("알파값이 1 > 0 되는 시간(초)")] public float FadeOutTime;
        [Tooltip("알파값이 1에서 머무는 시간(초)")] public float OnTime;
        [Tooltip("루프면 체크")] public bool IsLoop;   
        
        private CanvasGroup _canvas;
        private Sequence _sequence;

        private bool _isInitialized;
        // private bool _isFirstLoad = true;
        
        private void OnEnable()
        {
            // if (_isFirstLoad)
            // {
            //     _isFirstLoad = false;
            //     return;
            // }
            if (!_isInitialized)
            {
                _canvas = gameObject.GetOrAddComponent<CanvasGroup>();
                _isInitialized = true;
            
                _sequence = DOTween.Sequence();
                _sequence.Append(_canvas.DOFade(1, FadeInTime));
                _sequence.AppendInterval(OnTime);
                _sequence.Append(_canvas.DOFade(0, FadeOutTime));

                if (IsLoop) _sequence.SetLoops(-1);
                else _sequence.OnComplete(() => gameObject.SetActive(false));
            }

            _canvas.alpha = 0;
            if (gameObject.activeSelf) _sequence.Play();
        }
    }
}