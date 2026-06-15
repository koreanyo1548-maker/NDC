

using DG.Tweening;
using Managers;
using UnityEngine;
using Utils;

namespace SkillEffects
{
    public class Skill_25: SkillEffect
    {
        [SerializeField] private Transform _effect;
        private void Awake()
        {
            effectId = 25;
        }

        private Vector3 _endScale = new Vector3(3, 3, 3);
        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            _effect.localScale = Define.One;
            _effect.DOScale(_endScale, 3.5f).SetEase(Ease.Linear);
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _effect.DOKill();
        }
    }
}