using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_14: SkillEffect
    {
        [SerializeField] private Transform _electric;
        
        [SerializeField] private float _speed = 2;

        private Vector3[] _electricPositions;

        private void Awake()
        {
            effectId = 14;
            _electricPositions =  new Vector3[4];
            _electricPositions[0] = _electricPositions[1] =
                _electricPositions[2] = _electricPositions[3] = _electric.localPosition;
            _electricPositions[0].x -= 1;
            _electricPositions[1].y += 0.5f;
            _electricPositions[2].x += 1;
            _electricPositions[3].y -= 0.5f;
            
        }

        private int positionIdx;

        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            _electric.localPosition = _electricPositions[positionIdx];
            positionIdx++;
            if (positionIdx == 4) positionIdx = 0;
            _electric.DOLocalMove(_electricPositions[positionIdx], 1 / _speed).SetEase(Ease.Linear).OnComplete(Skill);
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _electric.DOKill();
        }
    }
}