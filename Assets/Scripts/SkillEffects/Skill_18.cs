using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using UnityEngine;

namespace SkillEffects
{
    public class Skill_18: SkillEffect
    {
        [SerializeField] private ParticleSystem _effect;
        [SerializeField] private List<Transform> _bubbles;
        
        private List<ParticleSystem> _drops;
        private List<ParticleSystem> _explosions;

        private Action _attack;
        private List<Action> _stuns;
        private List<Transform> _targets;
        
        private int _curCount = 0;
        
        private void Awake()
        {
            effectId = 18;
            _drops = new();
            _explosions = new();
            _targets = new();
            _stuns = new();
            foreach (var bubble in _bubbles)
            {
                SetEffect(bubble);
                bubble.gameObject.SetActive(false);
            }
            
            _effect.gameObject.SetActive(false);
        }

        private void SetEffect(Transform effect)
        {
            _drops.Add(effect.Find("BubbleMissile").GetComponent<ParticleSystem>());
            _explosions.Add(effect.Find("BubbleExplosion").GetComponent<ParticleSystem>());
            effect.gameObject.SetActive(false);
        }

        public override void SetAction(Action toDo)
        {
            _attack = toDo;
        }

        public override void Skill()
        {
            transform.SetParent(Manager.EffectParent);
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        public override void SetTarget(Transform target, Action toDo)
        {
            _targets.Add(target);
            _stuns.Add(toDo);
            _curCount++;
            if (_targets.Count > _bubbles.Count)
            {
                var newBubble = Instantiate(_bubbles[0].gameObject, transform).transform;
                _bubbles.Add(newBubble);
                SetEffect(newBubble);
            }
        }

        IEnumerator<float> _DoSkill()
        {
            _effect.gameObject.SetActive(true);
            _effect.Simulate( 0.0f, true, true );
            _effect.Play();

            yield return Timing.WaitForSeconds(1);
            
            var idx = 0;
            while (idx < _curCount)
            {
                _bubbles[idx].gameObject.SetActive(true);
                var startPos = _targets[idx].position.y;
                //_targets[idx].DOMoveY( startPos + 0.3f, 0.3f);

                var drop = _drops[idx];
                drop.transform.position = _targets[idx].position;
                drop.gameObject.SetActive(true);
                drop.Simulate(0.0f, true, true);
                drop.Play();
                _stuns[idx]();
                //drop.transform.DOMoveY(startPos + 0.3f, 0.3f);
                idx++;
            }
                
            yield return Timing.WaitForSeconds(1.7f);

            foreach (var drop in _drops)
            {
                drop.gameObject.SetActive(false);
            }

            idx = 0;
            _attack();
            while (idx < _curCount)
            {
                //_targets[idx].DOMoveY( _targets[idx].position.y - 0.3f, 0.3f);
                var explosion = _explosions[idx];
                explosion.transform.position = _drops[idx].transform.position;
                explosion.gameObject.SetActive(true);
                explosion.Simulate(0.0f, true, true);
                explosion.Play();
                idx++;
            }
            
            yield return Timing.WaitForSeconds(1);
            
            foreach (var explosion in _explosions)
            {
                explosion.gameObject.SetActive(false);
            }
            SkillEnd();
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            foreach (var target in _targets)
            {
                target.DOKill();
            }
            _targets.Clear();
            _stuns.Clear();
            foreach (var effect in _bubbles)
            {
                effect.gameObject.SetActive(false);
            }
            foreach (var effect in _drops)
            {
                effect.gameObject.SetActive(false);
            }
            _curCount = 0;
            base.WhenSkillEnd(reverseCall);
        }
    }
}