using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_2: SkillEffect
    {
        [SerializeField] private ParticleSystem _missile;
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private float _speed = 2;

        private Vector3 _missileStartPosition;

        private Transform _target;
        private Action _toDo;
        
        private void Awake()
        {
            effectId = 2;
            _missileStartPosition = _missile.transform.localPosition;
            
            _missile.gameObject.SetActive(false);
            _explosion.gameObject.SetActive(false);
            
        }

        public override void Skill()
        {
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        public override void SetTarget(Transform target, Action toDo)
        {
            _target = target;
            _toDo = toDo;
        }
        
        IEnumerator<float> _DoSkill()
        {
            _missile.transform.localPosition = _missileStartPosition;
            _missile.gameObject.SetActive(true);
            _missile.Simulate(0.0f, true, true);
            _missile.Play();
            _missile.transform.DOMove(_target.position, 1 / _speed).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _missile.gameObject.SetActive(false);
                _explosion.transform.SetParent(Manager.EffectParent);
                _explosion.transform.position = _missile.transform.position;
                _explosion.gameObject.SetActive(true);
                _explosion.Simulate(0.0f, true, true);
                _explosion.Play();
                _toDo();
            });

            yield return Timing.WaitForSeconds(1 + 1 / _speed);
            _explosion.gameObject.SetActive(false);
            _explosion.transform.SetParent(transform);
            SkillEnd();
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _missile.transform.DOKill();
        }
    }
}