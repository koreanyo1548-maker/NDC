using System;
using UnityEngine;
using System.Collections.Generic;
using Data.Stores;
using MEC;
using DG.Tweening;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace SkillEffects
{
    public class Skill_23: SkillEffect
    {
        [SerializeField] private ParticleSystem[] _explosions;
        [SerializeField] private ParticleSystem _enchant;
        [SerializeField] private ParticleSystem _stun;
        [SerializeField] private ParticleSystem _aura;
        [SerializeField] private float _speed = 4;

        private bool _isFirst = true;
        private Action _attack;
        private Action _push;
        
        private void Awake()
        {
            effectId = 23;
            foreach (var hit in _explosions)
            {
                hit.gameObject.SetActive(false);
            }
            _enchant.gameObject.SetActive(false);
            _stun.gameObject.SetActive(false);
            _aura.gameObject.SetActive(false);

        }

        public override void Skill()
        {
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
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

        private IEnumerator<float> _DoSkill()
        {
            _enchant.gameObject.SetActive(true);
            _enchant.Simulate(0.0f, true, true);
            _enchant.Play();

            yield return Timing.WaitForSeconds(0.8f);
            
            _stun.gameObject.SetActive(true);
            _stun.Simulate(0.0f, true, true);
            _stun.Play();
            _push();
            
            yield return Timing.WaitForSeconds(0.5f);
            _aura.gameObject.SetActive(true);
            _aura.Simulate(0.0f, true, true);
            _aura.Play();
            yield return Timing.WaitForSeconds(0.3f);
            
            Timing.RunCoroutine(_Explosions().CancelWith(gameObject));

        }

        private IEnumerator<float> _Explosions()
        {
            for (var idx = 0; idx < _explosions.Length; ++idx)
            {
                var hit = _explosions[idx];
                hit.gameObject.SetActive(true);
                hit.Simulate(0.0f, true, true);
                hit.Play();
                yield return Timing.WaitForSeconds(Random.Range(0.08f, 0.15f));
            }

            Timing.RunCoroutine(_Explosions().CancelWith(gameObject));
        }
    }
}