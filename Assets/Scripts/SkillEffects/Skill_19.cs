using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace SkillEffects
{
    public class Skill_19: SkillEffect
    {
        [SerializeField] private ParticleSystem[] _hits;
        
        private void Awake()
        {
            effectId = 19;
            foreach (var hit in _hits)
            {
                hit.gameObject.SetActive(false);
            }
        }
        
        public override void Skill()
        {
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }
        
        private IEnumerator<float> _DoSkill()
        {
            for (var idx = 0; idx < _hits.Length; ++idx)
            {
                var hit = _hits[idx];
                hit.gameObject.SetActive(true);
                hit.Simulate(0.0f, true, true);
                hit.Play();
                yield return Timing.WaitForSeconds(Random.Range(0.08f, 0.15f));
            }

            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

    }
}