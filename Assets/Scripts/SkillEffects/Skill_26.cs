using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;

namespace SkillEffects
{
    public class Skill_26 : SkillEffect 
    {
        [SerializeField] private ParticleSystem _light;
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private float _speed = 1;

        private Action _attack;
        private Vector3 _startPosition;
        
        private void Awake()
        {
            effectId = 26;
            _explosion.gameObject.SetActive(false);

            Skill();
        }
        
        private Vector3 _largeScale = new (-10f, -10f, -10f);
        private Vector3 _smallScale = new (-3f, -3f, -3f);

        public override void SetAction(Action toDo)
        {
            _attack = toDo;
        }

        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            DoSkill();
        }
        
        private void DoSkill()
        {
            _light.transform.localScale = _largeScale;
            _light.gameObject.SetActive(true);
            _light.Simulate(0.0f, true, true);
            _light.Play();
            _light.transform.DOScale(_smallScale, 1 / _speed).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _explosion.gameObject.SetActive(true);
                _explosion.Simulate(0.0f, true, true);
                _explosion.Play();
                _light.transform.DOScale(_largeScale, 0.3f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    _attack();
                    _light.gameObject.SetActive(false);
                    SkillEnd();
                });
            });
            
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            _light.transform.DOKill();
            base.WhenSkillEnd(reverseCall);
        }
    }
}