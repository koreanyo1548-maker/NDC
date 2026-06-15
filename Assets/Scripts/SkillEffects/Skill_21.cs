using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Managers;
using MEC;
using Utils;

namespace SkillEffects
{
    public class Skill_21: SkillEffect
    {
        [SerializeField] private Transform _orbitalT;
        [SerializeField] private Transform _explosionT;
        
        [SerializeField] private float _upSpeed = 2;
        [SerializeField] private float _shootSpeed = 2;

        private ParticleSystem _orbital;
        private ParticleSystem _explosion;
        
        private Vector3 _startPosition;
        private Transform _target;

        private Action _toDo;
        private void Awake()
        {
            effectId = 21;
            _startPosition = _orbitalT.localPosition;

            _orbital = _orbitalT.GetComponent<ParticleSystem>();
            _explosion = _explosionT.GetComponent<ParticleSystem>();
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
            if (_target == default) Debug.LogError("스킬 사용 전 타겟 포지션 지정 필요");
            
            _orbitalT.localPosition = _startPosition;
            
            _orbital.gameObject.SetActive(true);
            _explosion.gameObject.SetActive(false);
            _orbital.Simulate( 0.0f, true, true );
            _orbital.Play();
            yield return Timing.WaitForSeconds(0.5f);
            _orbitalT.DOLocalMoveY(1.143f, 1 / _upSpeed).OnComplete(() =>
            {
                _orbitalT.DOMove(_target.position, 1 / _shootSpeed).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    _orbitalT.gameObject.SetActive(false);
                    _explosionT.position = _target.position;
                    _explosion.gameObject.SetActive(true);
                    _explosion.Simulate( 0.0f, true, true );
                    _explosion.Play();
                    _toDo();
                    
                });
            });

            yield return Timing.WaitForSeconds(1 + 1/ _upSpeed + 1/_shootSpeed);
            SkillEnd();
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _orbitalT.DOKill();
        }
    }
}