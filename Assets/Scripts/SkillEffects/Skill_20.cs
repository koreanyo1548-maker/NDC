using System;
using System.Collections.Generic;
using Cameras;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using Utils;

namespace SkillEffects
{
    public class Skill_20 : SkillEffect
    {
        [SerializeField] private ParticleSystem _missile;
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private float _speed = 1;

        private Vector3 _startPosition;
        
        private bool _isFirst = true;
        private Action _attack;
        private Action _push;
        private void Awake()
        {
            effectId = 20;
            _startPosition = _missile.transform.localPosition;
            _missile.gameObject.SetActive(false);
            _explosion.gameObject.SetActive(false);
        }
        
        public override void SetAction(Action toDo)
        {
            if (_isFirst)
            {
                _push = toDo;
                _isFirst = false;
            }
            else _attack = toDo;
        }

        private Vector3 _scale = new (4f, 4f, 4f);

        public override void Skill()
        {
            _missile.transform.localPosition = _startPosition;
            _missile.transform.localScale = Define.One;
            
            transform.SetParent(Manager.EffectParent);
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));

        }
        
        IEnumerator<float> _DoSkill()
        {
            var center = CameraController.I.Center;
            center.z = 0;
            _missile.gameObject.SetActive(true);
            _missile.Simulate(0.0f, true, true);
            _missile.Play();
            _push();
            _missile.transform.DOScale(_scale, 1 / _speed).SetEase(Ease.InQuint);
            _missile.transform.DOMove(center, 1 / _speed).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _missile.gameObject.SetActive(false);
                _explosion.transform.position = center;
                _explosion.gameObject.SetActive(true);
                _explosion.Simulate(0.0f, true, true);
                _explosion.Play();
                _attack();
            });
            yield return Timing.WaitForSeconds(3);
            SkillEnd();
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            _isFirst = true;
            _missile.transform.DOKill();
            base.WhenSkillEnd(reverseCall);
        }
    }
}