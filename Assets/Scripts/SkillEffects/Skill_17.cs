using System;
using System.Collections.Generic;
using Managers;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_17: SkillEffect
    {
        [SerializeField] private ParticleSystem _swordEffect;
        [SerializeField] private ParticleSystem _explosionEffect;

        private Action _attack;
        private void Awake()
        {
            effectId = 17;
            _swordEffect.gameObject.SetActive(false);
            _explosionEffect.gameObject.SetActive(false);
        }

        public override void Skill()
        {
            transform.rotation = Quaternion.Euler(0, Manager.Player.IsLookingLeft ? 180 : 0, 0);
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        public override void SetAction(Action toDo)
        {
            _attack = toDo;
        }

        IEnumerator<float> _DoSkill()
        {
            _swordEffect.gameObject.SetActive(true);
            _swordEffect.Simulate(0.0f, true, true);
            _swordEffect.Play();

            yield return Timing.WaitForSeconds(0.12f);
            // _explosionEffect.transform.position = monster.position;
            _explosionEffect.gameObject.SetActive(true);
            _explosionEffect.Simulate(0.0f, true, true);
            _explosionEffect.Play();
            _attack();

            yield return Timing.WaitForSeconds(0.7f);
            SkillEnd();
        }
    }
}