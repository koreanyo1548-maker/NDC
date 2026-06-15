using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_8: SkillEffect
    {
        [SerializeField] private ParticleSystem _punch;
        [SerializeField] private ParticleSystem _slam;
        
        [SerializeField] private float _speed = 5;

        private Vector3 _missileStartPosition;
        private Vector3 _missileEndPosition;

        private Action _toDo;

        private void Awake()
        {
            effectId = 8;
            _missileStartPosition = _missileEndPosition = _punch.transform.localPosition;
            _missileStartPosition.y += 3f;
            
            _slam.gameObject.SetActive(false);
        }


        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        private Vector3 _shake = new(0, 0.2f, 0);
        
        IEnumerator<float> _DoSkill()
        {
            
            _punch.transform.localPosition = _missileStartPosition;
            _punch.Simulate( 0.0f, true, true );
            _punch.Play();
            _punch.transform.DOLocalMove(_missileEndPosition, 1 / _speed).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _slam.gameObject.SetActive(true);
                _slam.Simulate( 0.0f, true, true );
                _slam.Play();
                _slam.transform.DOShakePosition(0.3f, _shake, 30, 0);
                _toDo();
            });

            yield return Timing.WaitForSeconds(1);
            SkillEnd();
        }

        public override void SetAction(Action toDo)
        {
            _toDo = toDo;
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _punch.transform.DOKill();
            _slam.transform.DOKill();
        }
    }
}