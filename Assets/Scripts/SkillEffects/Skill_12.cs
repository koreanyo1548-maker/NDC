using System;
using DG.Tweening;
using Managers;
using UnityEngine;
using Utils;

namespace SkillEffects
{
    public class Skill_12: SkillEffect
    {
        [SerializeField] private Transform _effect;
        private void Awake()
        {
            effectId = 12;
        }

        private Vector3 _startScale = new Vector3(0.2f, 0.2f, 0.2f);
        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            _effect.localScale = _startScale;
            _effect.DOScale(Define.One, 3.5f).SetEase(Ease.Linear);
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _effect.DOKill();
        }
    }
}