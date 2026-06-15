using System;
using System.Collections.Generic;
using Data.Stores;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_27: SkillEffect
    {
        [SerializeField] private ParticleSystem _effect;
        [SerializeField] private List<ParticleSystem> _lightnings;

        private List<Action> _toDos;
        private List<Transform> _targets;
        
        private int _curCount = 0;
        
        private void Awake()
        {
            effectId = 27;
            _targets = new();
            _toDos = new();

            foreach (var effect in _lightnings)
            {
                effect.gameObject.SetActive(false);
            }
            _effect.gameObject.SetActive(false);
        }

        public override void Skill()
        {
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        public override void SetTarget(Transform target, Action toDo)
        {
            _targets.Add(target);
            _toDos.Add(toDo);
            _curCount++;
            if (_targets.Count > _lightnings.Count)
            {
                _lightnings.Add(Instantiate(_lightnings[0].gameObject, transform).GetComponent<ParticleSystem>());
            }
        }
        IEnumerator<float> _DoSkill()
        {
            _effect.gameObject.SetActive(true);
            _effect.Simulate( 0.0f, true, true );
            _effect.Play();
            yield return Timing.WaitForSeconds(0.2f);
            var idx = 0;
            var count = 0;
            while (idx < _curCount)
            {
                var effect = _lightnings[idx];
                effect.transform.position = _targets[idx].position;
                effect.gameObject.SetActive(true);
                effect.Simulate( 0.0f, true, true );
                effect.Play();
                _toDos[idx]();
                idx++;

                if (++count == 3)
                {
                    count = 0;
                    yield return Timing.WaitForSeconds(0.01f);
                }
            }
        
            yield return Timing.WaitForSeconds(1f);
            SkillEnd();
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            _targets.Clear();
            _toDos.Clear();
            foreach (var effect in _lightnings)
            {
                effect.gameObject.SetActive(false);
            }
            _curCount = 0;
            base.WhenSkillEnd(reverseCall);
        }
    }
}