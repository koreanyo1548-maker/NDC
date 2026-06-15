using System.Collections.Generic;
using Data;
using Data.DbPetInfo;

using Data.DbUser.Equipment;
using Exceptions;
using Managers;
using Managers.Game;
using MEC;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Fight.Units
{
    public class Pet: MonoBehaviour, IUpdateable
    {
        private DbPetAwakening _petMeta;
        private DbUserPet _pet;
        
        private Animator _animator;
        private Transform _followTarget;
        
        private bool _curLookLeft = true;
        private Vector3 _position = new(0, 0, 0);
        private CoroutineHandle _petSkillRoutine;

        private EventsManager _stageChanged;

        // private TweenerCore<Vector3, Vector3, VectorOptions> _following;

        private void Awake()
        {
            _animator = transform.GetComponent<Animator>();
        }

        public void Set(int id, Transform follow)
        {
            _followTarget = follow;
            _animator.Play("Idle");
            transform.position = follow.position;
            Manager.Updates.Add(this);
            _position.x = transform.position.x;
            _position.y =  transform.position.y;
            LookAt(false);

            if (id == -1) return;
            
            _petMeta = DbPetAwakening.Get(id);
            _pet = DbUserPet.Get(id);

            if (_petMeta.Option == StatType.Skill)
            {
                if (_petMeta.Skill.type == SkillType.AttackBuff)
                {
                    Manager.Player.AttackBuff(_petMeta.GetStat(_pet.Awakening.Value));
                }
                else
                {
                    if (!Manager.Field.IsGameOver)
                    {
                        Timing.KillCoroutines(_petSkillRoutine);
                        _petSkillRoutine = Timing.RunCoroutine(_PetSkillRoutine(), Define.KillWhenPlayerDieTag);
                    }
                    _stageChanged = new EventsManager(this, new EventsManager.Config
                    {
                        updatedUI = new [] {Manager.Field.StageChanged},
                        handler = () =>
                        {
                            if (Manager.Field.StageMeta.GetStageType() == StageType.Sequence && Manager.Field.CurStage > 1)
                            {
                                return;
                            }
                            Timing.KillCoroutines(_petSkillRoutine);
                            _petSkillRoutine = Timing.RunCoroutine(_PetSkillRoutine(), Define.KillWhenPlayerDieTag);
                        } 
                    });
                }
            }
        }

        IEnumerator<float> _PetSkillRoutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(_petMeta.Skill.coolTime);
                
                _animator.Play("Skill");
                
                switch (_petMeta.Skill.type)
                {
                    case SkillType.Heal:
                        Manager.Player.HpRecoveryBuff(_petMeta.GetStat(_pet.Awakening.Value));
                        break;
                    default: 
                        throw new NotDefinedValueException($"{_petMeta.Skill.type} of PetSkill");
                }
            }
        }

        public void Remove()
        {
            _stageChanged?.Dispose();
            
            if (_petMeta != null && _petMeta.Skill != null && _petMeta.Skill.type == SkillType.AttackBuff)
            {
                Manager.Player.AttackBuff(0);
            }
            Timing.KillCoroutines(_petSkillRoutine);
            Manager.Updates.Remove(this);
            Manager.Resource.Destroy(gameObject);
        }

        public void OnUpdate()
        {
            var direction = _followTarget.position - transform.position;
            var velocity = direction.magnitude * Time.deltaTime * Random.Range(1f, 3f);
            var normalize = direction.normalized;
            _position.x += velocity * normalize.x;
            _position.y += velocity * normalize.y;
            
            LookAt(_position.x < transform.position.x);
            transform.position = Vector3.Lerp(transform.position, _position, 0.15f);
        }

        private void LookAt(bool lookLeft)
        {
            if (_curLookLeft == lookLeft) return;

            _curLookLeft = lookLeft;
            transform.rotation = Quaternion.Euler(0, lookLeft ? 180 : 0, 0);
        }
    }
}