using System;
using System.Collections.Generic;
using MEC;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace SkillEffects
{
    public class Skill_7: SkillEffect
    {
        [SerializeField] private ParticleSystem _effect;
        [SerializeField] private List<Transform> _missiles;

        private List<ParticleSystem> _missileEffects;
        private List<ParticleSystem> _explosionEffects;

        private Vector3 _missileStartPosition;
        private Vector3 _missileEndPosition;

        private List<Action> _toDos;
        private List<Transform> _targets;

        private int _curCount = 0;
        
        private void Awake()
        {
            effectId = 7;
            _missileEffects = new();
            _explosionEffects = new();
            _targets = new();
            _toDos = new();

            SetEffect(_missiles[0]);
            
            _missileStartPosition = _missileEndPosition = _missileEffects[0].transform.localPosition;
            _missileEndPosition.y = _explosionEffects[0].transform.localPosition.y;
            _effect.gameObject.SetActive(false);
            
        }

        private void SetEffect(Transform effect)
        {
            _missileEffects.Add(effect.Find("EnergyMissilePink").GetComponent<ParticleSystem>());
            _explosionEffects.Add(effect.Find("MagicPillarBlastBlue").GetComponent<ParticleSystem>());
            effect.gameObject.SetActive(false);
        }
        

        public override void Skill()
        {
            Timing.RunCoroutine(_DoSkill().CancelWith(gameObject));
        }

        public override void SetTarget(Transform target, Action toDo)
        {
            _targets.Add(target);
            _toDos.Add(toDo);
            _curCount++;
            if (_targets.Count > _missiles.Count)
            {
                var newMissile = Instantiate(_missiles[0].gameObject, transform).transform;
                _missiles.Add(newMissile);
                SetEffect(newMissile);
            }
        }

        IEnumerator<float> _DoSkill()
        {
            _effect.gameObject.SetActive(true);
            _effect.Simulate( 0.0f, true, true );
            _effect.Play();
            yield return Timing.WaitForSeconds(0.2f);
            var idx = 0;
            while (idx < _curCount)
            {
                var effect = _missiles[idx];
                var missile = _missileEffects[idx];
                var position = _targets[idx].position;
                position.x += Random.Range(-0.3f, 0.3f);
                effect.position = position;
                missile.transform.localPosition = _missileStartPosition;
                effect.gameObject.SetActive(true);
                missile.gameObject.SetActive(true);
                missile.Simulate( 0.0f, true, true );
                missile.Play();
                var curIdx = idx;
                missile.transform.DOLocalMove(_missileEndPosition, 0.3f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    missile.gameObject.SetActive(false);
                    var explosion = _explosionEffects[curIdx];
                    explosion.Simulate( 0.0f, true, true );
                    explosion.Play();
                    _toDos[curIdx]();
                });
                idx++;
                yield return Timing.WaitForSeconds(0.01f);//UnityEngine.Random.Range(0.01f, 0.05f));
            }

            yield return Timing.WaitForSeconds(1f);
            SkillEnd();
        }

        public override void WhenSkillEnd(bool reverseCall)
        {
            _targets.Clear();
            _toDos.Clear();
            foreach (var effect in _missiles)
            {
                effect.transform.DOKill();
                effect.gameObject.SetActive(false);
            }
            _curCount = 0;
            base.WhenSkillEnd(reverseCall);
        }
    }
}