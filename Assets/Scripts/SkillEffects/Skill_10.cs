using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_10: SkillEffect
    {
        [SerializeField] private ParticleSystem[] _lights;
        [SerializeField] private ParticleSystem _explosion;

        private Vector3[] _startPositions;
        private Vector3 _endPosition;

        private Action _stun;
        private Action _attack;
        
        private void Awake()
        {
            effectId = 10;
            _endPosition = _explosion.transform.position;

            _startPositions = new Vector3[_lights.Length];
            for (var idx = 0; idx < _lights.Length; ++idx)
            {
                var light = _lights[idx];
                _startPositions[idx] = light.transform.localPosition;
                light.gameObject.SetActive(false);
            }
        }

        public override void SetTarget(Transform target, Action toDo)
        {
            _stun = toDo;
        }

        public override void SetAction(Action toDo)
        {
            _attack = toDo;
        }

        public override void Skill()
        {
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        IEnumerator<float> _DoSkill()
        {
            for (var idx = 0; idx < _lights.Length; ++idx)
            {
                var light = _lights[idx];
                light.transform.localPosition = _startPositions[idx];
                light.gameObject.SetActive(true);
                light.Simulate(0.0f, true, true);
                light.Play();
                light.transform.DOLocalMove(_endPosition, 0.3f + (_lights.Length - idx) * 0.05f).SetEase(Ease.InSine);
                yield return Timing.WaitForSeconds(0.05f);
            }

            yield return Timing.WaitForSeconds(0.3f);
            foreach (var light in _lights) light.gameObject.SetActive(false);
            
            _explosion.Simulate( 0.0f, true, true );
            _explosion.Play();
            _stun();
            _attack();
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            foreach (var light in _lights) light.transform.DOKill();
            base.WhenSkillEnd(reverseCall);
        }
    }
}