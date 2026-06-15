using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Managers;
using MEC;
using Random = UnityEngine.Random;

namespace SkillEffects
{
    public class Skill_24: SkillEffect
    {
        [SerializeField] private ParticleSystem _effect;
        [SerializeField] private ParticleSystem[] _fires;
        [SerializeField] private ParticleSystem[] _explosions;
        [SerializeField] private Transform[] _positions;

        private Vector3[] _startPositions;
        private Action _toDo;
        
        private void Awake()
        {
            effectId = 24;
            var idx = 0;
            _startPositions = new Vector3[_fires.Length];
            foreach (var fire in _fires)
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
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        public override void SetAction(Action toDo)
        {
            _toDo = toDo;
        }

        IEnumerator<float> _DoSkill()
        {
            _effect.gameObject.SetActive(false);
            
            var shortest = 100f;
            for (var idx = 0; idx < _fires.Length; ++idx)
            {
                var time = Random.Range(0.3f, 0.5f);
                if (time < shortest) shortest = time;
                var fire = _fires[idx];
                var explosion = _explosions[idx];
                fire.transform.localPosition = _startPositions[idx];
                fire.gameObject.SetActive(true);
                fire.Simulate( 0.0f, true, true );
                fire.Play();
                fire.transform.DOMove(_positions[idx].position, time).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    fire.gameObject.SetActive(false);
                    explosion.transform.position = fire.transform.position;
                    explosion.gameObject.SetActive(true);
                    explosion.Simulate( 0.0f, true, true );
                    explosion.Play();
                });
            }

            yield return Timing.WaitForSeconds(shortest);
            _effect.gameObject.SetActive(true); 
            _toDo();
            _effect.Simulate( 0.0f, true, true );
            _effect.Play();

            yield return Timing.WaitForSeconds(5);
            SkillEnd();
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            foreach (var fire in _fires) fire.transform.DOKill();
            foreach (var explosion in _explosions)
            {
                explosion.gameObject.SetActive(false);
            }
        }
    }
}