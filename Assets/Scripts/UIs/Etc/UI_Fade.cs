using System;
using DG.Tweening;
using Managers;
using UIBases;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Etc
{
    public class UI_Fade: UI_Scene
    {

        private Image _fade;

        public override bool Init()
        {
            if (!base.Init()) return false;
            
            _fade = transform.GetComponent<Image>();
            GetComponent<Canvas>().sortingOrder = 200;
            
            return true;
        }

        public void FadeIn(Action finish)
        {
            Init();
            _fade.color = Color.clear;
            _fade.DOFade(1, 0.5f).OnComplete(() =>
            {
                finish();
            });
        }

        public void FadeOut()
        {
            Init();
            _fade.color = Color.black;
            _fade.DOFade(0, 0.5f).OnComplete(() => Manager.Resource.Destroy(gameObject));
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}