using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using Utils;

namespace SkillEffects
{
    public class Skill_9: SkillEffect
    {
        [SerializeField] private ParticleSystem _missile;
        [SerializeField] private ParticleSystem _magic;
        [SerializeField] private ParticleSystem _explosion;
        
        [SerializeField] private float _missileSpeed = 2;
        [SerializeField] private float _magicSpeed = 2;

        private Vector3 _missileStartPosition;

        private Transform _target;
        private Action _toDo;
        
        private void Awake()
        {
            effectId = 9;
            _missileStartPosition = _missile.transform.localPosition;
            
            _missile.gameObject.SetActive(false);
            _magic.gameObject.SetActive(false);
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

        private Vector3 _magicStartScale = new(0.5f, 0.5f, 0.5f);
        private Vector3 _magicEndScale = new(2f, 2f, 2f);
        
        IEnumerator<float> _DoSkill()
        {
            if (_target == default) Debug.LogError("스킬 사용 전 타겟 포지션 지정 필요");
            
            _missile.transform.localPosition = _missileStartPosition;
            _missile.gameObject.SetActive(true);
            transform.SetParent(Manager.EffectParent);
            _missile.Simulate( 0.0f, true, true );
            _missile.Play();
            _missile.transform.DOMove(_target.position, 1 / _missileSpeed).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                _missile.gameObject.SetActive(false);
                _magic.transform.localScale = _magicStartScale;
                _magic.transform.position = _target.position;
                _magic.gameObject.SetActive(true);
                _magic.Simulate( 0.0f, true, true );
                _magic.Play();
                _magic.transform.DOScale(_magicEndScale, 1 / _magicSpeed).SetEase(Ease.Linear).OnUpdate(() =>
                {
                    _magic.transform.position = _target.position;
                }).OnComplete(() =>
                {
                    _magic.gameObject.SetActive(false);
                    _explosion.transform.position = _target.position;
                    _explosion.gameObject.SetActive(true);
                    _explosion.Simulate( 0.0f, true, true );
                    _explosion.Play();
                    _toDo();
                });
            });

            yield return Timing.WaitForSeconds(1 + 1 / _missileSpeed + 1 / _magicSpeed);
            _explosion.gameObject.SetActive(false);
            SkillEnd();
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _missile.transform.DOKill();
            _magic.transform.DOKill();
        }
    }
}