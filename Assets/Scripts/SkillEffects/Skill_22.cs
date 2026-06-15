using System.Collections.Generic;
using Data.Stores;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;

namespace SkillEffects
{
    public class Skill_22: SkillEffect
    {
        [SerializeField] private Transform[] _effects;
        [SerializeField] private ParticleSystem[] _explosions;
        
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private void Awake()
        {
            effectId = 22;
            _startPosition = _endPosition = _effects[0].localPosition;
            _endPosition.x += 9;
        }

        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            Timing.RunCoroutine(_DoSkill(0).CancelWith(gameObject));
            Timing.RunCoroutine(_Explosioning().CancelWith(gameObject));
        }
        
        IEnumerator<float> _DoSkill(int idx)
        {
            _effects[idx].localPosition = _startPosition;
            var particle = _effects[idx].GetComponent<ParticleSystem>();
            particle.Simulate( 0.0f, true, true );
            particle.Play();
            _effects[idx].DOLocalMove(_endPosition, 0.8f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);

            if (idx == 0)
            {
                yield return Timing.WaitForSeconds(0.25f);
                Timing.RunCoroutine(_DoSkill(1).CancelWith(gameObject));
            }
        }
        
        private IEnumerator<float> _Explosioning()
        {
            for (var idx = 0; idx < _explosions.Length; ++idx)
            {
                var explosion = _explosions[idx];
                explosion.gameObject.SetActive(true);
                explosion.Simulate(0.0f, true, true);
                explosion.Play();
                yield return Timing.WaitForSeconds(Random.Range(0.15f, 0.3f));
            }

            Timing.RunCoroutine(_Explosioning().CancelWith(gameObject));
        }
    }
}