using System;
using DG.Tweening;
using UIBases;
using UnityEngine.UI;

namespace UIs.Etc.Warning
{
    public class UI_NetworkError: UI_Scene
    {
        private Image _img;

        void Awake()
        {
            _img = transform.GetComponentInChildren<Image>();
            Init();
        }
        
        private void OnEnable()
        {
            _img.DOFade(0.157f, 0.7f).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            _img.DOKill();
        }

        public override bool NeedRaycast()
        {
            return false;
        }
    }
}