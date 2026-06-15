using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Etc
{
    public class UI_FadeOnScene: MonoBehaviour
    {

        private Image _fade;
        
        private void Init()
        {
            _fade = transform.GetComponent<Image>();
            GetComponent<Canvas>().sortingOrder = 200;
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
            _fade.DOFade(0, 0.5f).OnComplete(() => Destroy(gameObject));
        }
    }
}