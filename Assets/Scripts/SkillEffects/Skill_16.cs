using System.Collections.Generic;
using Cameras;
using Managers;
using MEC;
using UnityEngine;

namespace SkillEffects
{
    public class Skill_16: SkillEffect
    {
        [SerializeField] private ParticleSystem _snow;
        [SerializeField] private ParticleSystem[] _explosions;
        
        private void Awake()
        {
            effectId = 16;
        }
        
        private Vector3 _center;
        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            _center = transform.position;
            _snow.gameObject.SetActive(true);
            _snow.Simulate(0.0f, true, true);
            _snow.Play();

            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        IEnumerator<float> _DoSkill()
        {
            for (var idx = 0; idx < _explosions.Length; ++idx)
            {
                var explosion = _explosions[idx];
                var newPos = _center;
                newPos.x += Random.Range(-2, 2);
                newPos.y += Random.Range(-3, 4);
                explosion.transform.position = newPos;
                explosion.gameObject.SetActive(true);
                explosion.Simulate(0.0f, true, true);
                explosion.Play();
                yield return Timing.WaitForSeconds(Random.Range(0.08f, 0.15f));
            }

            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }
    }
}