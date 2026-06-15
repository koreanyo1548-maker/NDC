using System.Collections.Generic;
using MEC;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace SkillEffects
{
    public class Skill_3: SkillEffect
    {
        [SerializeField] private ParticleSystem[] _hits;
        [SerializeField] private Transform _smoke;
        [SerializeField] private float _speed = 4;
        
        private Vector3[] _smokePosition;

        private void Awake()
        {
            effectId = 3;
            _smokePosition =  new Vector3[4];
            _smokePosition[0] = _smokePosition[1] =
                _smokePosition[2] = _smokePosition[3] = _smoke.localPosition;
            _smokePosition[0].x -= 0.3f;
            _smokePosition[1].y += 0.3f;
            _smokePosition[2].x += 0.3f;
            _smokePosition[3].y -= 0.3f;
            
            foreach (var hit in _hits)
            {
                hit.gameObject.SetActive(false);
            }
        }


        public override void Skill()
        {
            DoSkill();
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        
        private int positionIdx;
        private void DoSkill()
        {
            _smoke.localPosition = _smokePosition[positionIdx];
            positionIdx++;
            if (positionIdx == 4) positionIdx = 0;
            _smoke.DOLocalMove(_smokePosition[positionIdx], 1 / _speed).SetEase(Ease.Linear).OnComplete(DoSkill);
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
        
        public override void WhenSkillEnd(bool reverseCall)
        {
            base.WhenSkillEnd(reverseCall);
            _smoke.DOKill();
        }
    }
}