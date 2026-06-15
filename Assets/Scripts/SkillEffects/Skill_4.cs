using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Managers;
using MEC;

namespace SkillEffects
{
    public class Skill_4: SkillEffect
    {
        [SerializeField] private ParticleSystem _charge;
        [SerializeField] private ParticleSystem _chargeEnd;
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private float _speed = 2;

        private Transform _target;
        private Action _stun;
        private Action _attack;
        
        private void Awake()
        {
            effectId = 4;
            _charge.gameObject.SetActive(false);
            _chargeEnd.gameObject.SetActive(false);
            _explosion.gameObject.SetActive(false);
        }

        public override void SetTarget(Transform target, Action toDo)
        {
            _target = target;
            _stun = toDo;
        }

        public override void SetAction(Action toDo)
        {
            _attack = toDo;
        }

        public override void Skill()
        {
            transform.rotation = Quaternion.Euler(0, Manager.Player.IsLookingLeft ? 180 : 0, 0);
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        IEnumerator<float> _DoSkill()
        {
            _charge.gameObject.SetActive(true);
            _charge.Simulate(0.0f, true, true);
            _charge.Play();
            _charge.transform.DOLocalRotate(new Vector3(0, 0, 500), 1 / _speed, RotateMode.FastBeyond360)
                //.SetLoops(2, LoopType.Restart)
                .SetEase(Ease.Linear).OnComplete(() =>
            {
                _chargeEnd.gameObject.SetActive(true);
                _chargeEnd.Simulate(0.0f, true, true);
                _chargeEnd.Play();
                _stun();
            });
            
            yield return Timing.WaitForSeconds(1/_speed + 0.2f);
                
            _charge.gameObject.SetActive(false);
            _explosion.transform.position = _target.position;
            _explosion.gameObject.SetActive(true);
            _attack();
            _explosion.Simulate(0.0f, true, true);
            _explosion.Play();
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            _charge.transform.DOKill();
            base.WhenSkillEnd(reverseCall);
        }
    }
}