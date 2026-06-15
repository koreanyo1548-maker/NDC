using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_28: SkillEffect
    {
        [SerializeField] private ParticleSystem _effect;
        [SerializeField] private ParticleSystem[] _rains;
        [SerializeField] private ParticleSystem[] _explosions;
        
        public Ease ease = Ease.InCubic;

        private Vector3[] _startPositions;
        
        private CoroutineHandle _rainLoop;
        
        private void Awake()
        {
            effectId = 28;
            var idx = 0;
            _startPositions = new Vector3[_rains.Length];
            foreach (var fire in _rains)
            {
                _startPositions[idx++] = fire.transform.localPosition;
                fire.gameObject.SetActive(false);
            }

            foreach (var explosion in _explosions)
            {
                explosion.gameObject.SetActive(false);
            }
            
            _effect.gameObject.SetActive(false);

        }

        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            _effect.gameObject.SetActive(true); 
            _effect.Simulate( 0.0f, true, true );
            _effect.Play();
            
            Timing.RunCoroutine(_RainLoops().CancelWith(gameObject));
        }
        
        private IEnumerator<float> _RainLoops()
        {
            for (var idx = 0; idx < _rains.Length; ++idx)
            {
                var rain = _rains[idx];
                var explosion = _explosions[idx];
                rain.transform.localPosition = _startPositions[idx];
                rain.gameObject.SetActive(true);
                rain.Simulate( 0.0f, true, true );
                rain.Play();
                var endPosition = _startPositions[idx];
                endPosition.y -= Random.Range(4f, 9f);
                rain.transform.DOLocalMove(endPosition, Random.Range(0.3f, 0.5f)).SetEase(ease).OnComplete(() =>
                {
                    rain.gameObject.SetActive(false);
                    explosion.transform.position = rain.transform.position;
                    explosion.gameObject.SetActive(true);
                    explosion.Simulate( 0.0f, true, true );
                    explosion.Play();
                });
                yield return Timing.WaitForSeconds(Random.Range(0.05f, 0.1f));
            }

            Timing.RunCoroutine(_RainLoops().CancelWith(gameObject));
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            foreach (var rain in _rains) rain.transform.DOKill();
        }
    }
}