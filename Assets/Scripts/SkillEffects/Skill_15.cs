using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace SkillEffects
{
    public class Skill_15: SkillEffect
    {
        [SerializeField] private ParticleSystem _move;
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private float _risingSpeed = 1;
        [SerializeField] private float _fallingSpeed = 2;

        private Vector3 _startPosition;
        private Vector3 _highPosition;

        private Transform _target;
        private Action _stun;
        private Action _attack;
        
        
        private void Awake()
        {
            effectId = 15;
            _startPosition = _highPosition =_move.transform.localPosition;
            _highPosition.y += 3f;
            _move.gameObject.SetActive(false);
            _explosion.gameObject.SetActive(false);
        }

        public override void SetTarget(Transform target, Action toDo)
        {
            _target = target;
            _stun = toDo;
        }

        public override void SetAction(Action toDo)
        {
            _attack = toDo;
        }

        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            transform.rotation = Quaternion.Euler(0, Manager.Player.IsLookingLeft ? 180 : 0, 0);
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        private Vector3 _startScale = new(0.5f, 0.5f, 0.5f);
        private Vector3 _highScale = new(1.2f, 1.2f, 1.2f);
        IEnumerator<float> _DoSkill()
        {
            _move.transform.localPosition = _startPosition;
            _move.transform.localScale = _startScale;
            _move.gameObject.SetActive(true);
            _move.Simulate(0.0f, true, true);
            _move.Play();
            
            _move.transform.DOScale(_highScale, 1 / _risingSpeed).SetEase(Ease.InQuint);
            _move.transform.DOLocalMove(_highPosition, 1 / _risingSpeed).SetEase(Ease.Linear).OnComplete(() =>
            {
                _move.transform.DOMove(_target.position, 1 / _fallingSpeed).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    _stun();
                    _move.gameObject.SetActive(false);
                    _explosion.transform.position = _target.position;
                    _explosion.gameObject.SetActive(true);
                    _explosion.Simulate(0.0f, true, true);
                    _explosion.Play();
                    _attack();
                });
            });
            yield return Timing.WaitForSeconds(3);
            SkillEnd();
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            _move.transform.DOKill();
            base.WhenSkillEnd(reverseCall);
        }
    }
}