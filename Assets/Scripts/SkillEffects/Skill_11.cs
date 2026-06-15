using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;

namespace SkillEffects
{
    public class Skill_11: SkillEffect
    {
        [SerializeField] private Transform[] _effects;
        
        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private void Awake()
        {
            effectId = 11;
            _endPosition = _startPosition = _effects[0].localPosition;
            _endPosition.x += 9;
        }


        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            Timing.RunCoroutine(_DoSkill(0).CancelWith(gameObject));
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

        public override void WhenSkillEnd(bool reverseCall)
        {
            foreach (var effect in _effects)
            {
                effect.DOKill();
            }
            base.WhenSkillEnd(reverseCall);
        }
    }
}