using System;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using Utils;

namespace Cameras
{
    public class DieEffect: MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _fade;
        [SerializeField] private Transform _unmask;
        [SerializeField] private GameObject _black;
        [SerializeField] private Ease _ease = Ease.InOutCubic;
        [SerializeField] private float _speed = 1;
        [SerializeField] private float _stay = 0.5f;

        private Vector3 _largeScale = new(8, 8, 8);
        
        
        public void Effect(bool isDie)
        {
            if (!isDie)
            {
                _black.SetActive(false);
                gameObject.SetActive(true);

                _fade.DOColor(Color.black, 1).OnComplete(() =>
                {
                    _fade.DOColor(Color.clear, 1).OnComplete(() => gameObject.SetActive(false));
                });
            }
            else
            {
                _unmask.localScale = _largeScale;
                var playerPos = Manager.Player.Position();
                var startPos = playerPos;
                startPos.y = 0;
                playerPos.y += 0.2f;
                _unmask.position = startPos;
                _black.SetActive(true);
                gameObject.SetActive(true);
                _unmask.DOMove(playerPos, 1 / _speed).SetEase(_ease);
                _unmask.DOScale(Define.One, 1 / _speed).SetEase(_ease).OnComplete(() =>
                {// 0.25 + 1 + 1 //+ 1
                    Timing.CallDelayed(_stay, () =>
                    {
                        _fade.DOColor(Color.black, 1).OnComplete(() =>
                        {
                            _black.SetActive(false);
                            _fade.DOColor(Color.clear, 1).OnComplete(() => gameObject.SetActive(false));
                        });
                    });
                });
            }
        }
    }
}