using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;

namespace SkillEffects
{
    public class Skill_1: SkillEffect
    {
        [SerializeField] private ParticleSystem[] _fires;
        
        
        private void Awake()
        {
            effectId = 1;
        }

        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        private IEnumerator<float> _DoSkill()
        {
            for (var idx = 0; idx < _fires.Length; ++idx)
            {
                var fire = _fires[idx];
                fire.gameObject.SetActive(true);
                fire.Simulate(0.0f, true, true);
                fire.Play();
                yield return Timing.WaitForSeconds(Random.Range(0.05f, 0.15f));
            }

            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }
    }
}