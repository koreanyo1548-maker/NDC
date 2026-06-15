using System;
using Data.Stores;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEffects
{
    public class Skill_5: SkillEffect
    {
        public float speed;
        [SerializeField] private Transform _wave;
        private void Awake()
        {
            effectId = 5;
        }

        public override void Skill()
        {
            DoSkill();
        }

        private Vector3[] _rotate = new[] {new Vector3(0, 0, 180), new Vector3(0, 0, 360)};
        private bool _first;
        private void DoSkill()
        {
            _wave.DORotate(_rotate[_first ? 0 : 1], 1 / speed, RotateMode.FastBeyond360).SetEase(Ease.Linear).OnComplete(() =>
            {
                _first = !_first;
                DoSkill();
            });
        }
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _wave.DOKill();
        }
    }
}