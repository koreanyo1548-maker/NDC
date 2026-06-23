using System;
using System.Collections.Generic;
using System.Numerics;
using Cameras;
using Controller;
using Controller.Play;
using Data;
using Data.DbStage;
using DG.Tweening;
using Fight.Logics;
using Fight.Logics.Actors;
using Fight.Logics.TargetSelectors;
using Fight.Stats;
using Managers;
using Managers.Base;
using MEC;
using UnityEditor;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Units
{
    public class Monster: MonoBehaviour, IUpdateable, IFightUnit
    {
        public float speed = 0.5f;

        
        #region Status

        public int id { get; private set; }

        public bool IsDead => _state == MonsterState.Die;
        private MonsterState _state = MonsterState.Idle;
        public bool IsPlayerSide() => false;
        public bool IsValid() => gameObject.IsValid() && !IsDead;
        
        public MonsterStat stat { get; private set; }
        public bool isBoss { get; private set; }
        public bool IsBoss() => isBoss;
        
        private FieldType _field;
        private int _stage;
        private bool _isTraining;


        #endregion
        

        #region Utils

        private Vector3 _lookLeft;
        private Vector3 _lookRight;
        private string _killWhenDieTag;
        private Vector3 _originalPosition;
        private Vector3 _scaleVector = new();
        private SFXType _deadSound;
        private bool _dieCallbackFired;
        private bool _hitFired;
        private const float HIT_NORMALIZED_TIME = 0.5f;

        #endregion

        
        #region Components


        public Vector3 Position() => root.position;

        public Transform targetingPosition { get; private set; }
        public Transform root { get; private set; }
        private Transform _damagePosition;
        public Animator animator { get; private set; }
        private SpriteMaterialSetter _materialSetter;

        #endregion


        #region Tools

        private ITargetSelector _targetSelector;
        private IActor _actor;
        public IFightUnit target { get; private set; }

        #endregion
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(root.position, $"{_state}");
            // if (target != null && target.IsValid())
            // {
            //     Gizmos.DrawLine(root.position, target.Position());
            // }
        }
        #endif

        public enum MonsterState
        {
            Moving,
            Idle,
            Die,
            Action,
            Stun
        }

        private void Awake()
        {
            root = transform.parent;
            animator = transform.GetComponent<Animator>();
            targetingPosition = root.Find("TargetingPosition");
            _damagePosition = root.Find("DamagePosition");
            var scale = root.localScale.x;
            _lookLeft = Define.LookLeft * scale;
            _lookRight = Define.LookRight * scale;
            _originalPosition = transform.localPosition;
            
            stat = new MonsterStat(this);
            _materialSetter = root.gameObject.GetOrAddComponent<SpriteMaterialSetter>();
        }
        
        // #if UNITY_EDITOR
        // private void OnDrawGizmos()
        // {
        //     Handles.Label(_root.position, $"{_id} {_state}");
        // }
        // #endif
        
        public void Init(int id, DbMonster monster, bool isBoss)
        {
            Manager.Updates.Add(this);
            _targetSelector = FightSelector.GetTargetSelector(Manager.Field.CurField.Value, true);
            _actor = FightSelector.GetActor(Manager.Field.CurField.Value);
            _actor.SetMonster(this);
            target = _targetSelector.GetTarget();
            _state = MonsterState.Idle;
            _hitFired = false;
            stat.SetMonster(monster, isBoss);
            this.id = id;
            _killWhenDieTag = "killWhenDieMonster" + id;
            _materialSetter.ChangeColor(Color.black);
            _deadSound = monster.Sound;
            this.isBoss = isBoss;
            _field = Manager.Field.CurField.Value;
            _stage = Manager.Field.CurStage;
            _isTraining = _field == FieldType.Training;
            _dieCallbackFired = false;
            ResetAnimatorToIdle();
        }
        
        public void OnUpdate()
        {
            switch (_state)
            {
                case MonsterState.Die:
                    break;
                case MonsterState.Idle:
                    _actor.UpdateIdle();
                    break;
                case MonsterState.Moving:
                    _actor.UpdateMoving();
                    break;
                case MonsterState.Action:
                    _UpdateAttackEvents();
                    break;
                case MonsterState.Stun:
                    break;
            }
        }

        private void _UpdateAttackEvents()
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (!_hitFired && info.normalizedTime >= HIT_NORMALIZED_TIME)
            {
                _hitFired = true;
                OnAttackHit();
            }
            if (info.normalizedTime >= 1f)
            {
                _hitFired = false;
                OnAttackDone();
            }
        }
        public void Spawn(float posX, float posY)
        {
            root.position = new Vector3(posX, posY);
            transform.localPosition = _originalPosition;
        }
    
        public void Clear()
        {
            Manager.Resource.Destroy(root.gameObject);
            Manager.Updates.Remove(this);
            Timing.KillCoroutines(_killWhenDieTag);
            animator.speed = 1;
            root.DOKill();
            _materialSetter.ChangeColor(Color.black);
        }

        public void Attack()
        {
            if (IsDead) return;

            _hitFired = false;
            animator.Play("attack", 0, 0);
            if (isBoss)
            {
                if (target != null && target.IsValid())
                    LookAt((target.Position() - root.position).x > 0);
            }
            _state = MonsterState.Action;
        }

        public void Idle()
        {
            animator.Play("walk", 0, 0);
            _state = MonsterState.Idle;
        }

        public void Move()
        {
            if (id == 0) Debug.Log(".");
            animator.Play("walk", 0, 0);
            _state = MonsterState.Moving;
        }
        
        private Vector3 _hitPosition;
        public void Attacked(BigInteger attack, AttackType attackType)
        {
            if (_state == MonsterState.Die) return;

            var hit = Manager.Resource.InstantiateParticle(_isTraining ? "Particles/TrainingShield" 
                : "Particles/MonsterHitEffect", 10, Manager.EffectParent);
            if (hit != null)
            {
                if (!_isTraining)
                {
                    hit.transform.position = targetingPosition.position;
                    var randomScale = Random.Range(2f, 4f);
                    (_scaleVector.x, _scaleVector.y, _scaleVector.z) = (randomScale, randomScale, randomScale);
                    hit.transform.localScale = _scaleVector;
                    transform.localPosition = _originalPosition;
                    transform.DOKill();
                    transform.DOShakePosition(0.2f, 0.2f, 20);
                
                    Timing.RunCoroutine(_BlinkRoutine(), _killWhenDieTag);  
                }
                else
                {
                    _hitPosition = transform.position;
                    _hitPosition.y += 0.921f;
                    hit.transform.position = _hitPosition;
                }
                
                if (attackType != AttackType.Normal) CameraController.I.Shake();
                var dm = Manager.Pool.damage.Spawn(_damagePosition.position, attack);
                dm.SetSize(Define.DamageSize(true));
                dm.SetGradient(Define.DamageColor(true, attackType));
            }
           
            _actor.WithDamage(attack);
            Manager.Sound.PlaySFX(SFXType.Attacked);
            // Manager.UI.GetSceneUI<UI_MainSkill>().AddDamage(attack);
            var isDead = stat.Attacked(attack);
            if (isDead)
            {
                Die();
            }
        }

        private void Die()
        {
            if (!_actor.CanDie()) return;
            if (_state == MonsterState.Die || Manager.Field.IsGameOver) return;
            
            Manager.Sound.PlaySFX(_deadSound);
            animator.speed = 1;
            _state = MonsterState.Die;
            animator.Play("die");

            Timing.RunCoroutine(_FadeRoutine(), _killWhenDieTag);
        }

        private void WhenDieAnimationDone()
        {
            if (_dieCallbackFired) return;
            _dieCallbackFired = true;
            Manager.Field.MonsterDie(id);
            PlayController.I.KillMonster(_field, _stage);
            Clear();
        }

        private IEnumerator<float> _BlinkRoutine()
        {
            var time = 0f;
            var multiply = 1 / 0.15f;
            while (time < 0.15f)
            {
                time += Timing.DeltaTime;
                _materialSetter.ChangeColor(Color.Lerp(Color.black, Define.AttackedColor, time * multiply));
                yield return Timing.DeltaTime;
            }

            time = 0.15f;
            while (time > 0)
            {   
                time -= Timing.DeltaTime;
                _materialSetter.ChangeColor(Color.Lerp(Color.black,Define.AttackedColor, time * multiply));
                yield return Timing.DeltaTime;
            }
            _materialSetter.ChangeColor(Color.black);
        }
        
        private IEnumerator<float> _FadeRoutine()
        {
            var time = 0f;
            while (time < 1)
            {
                time += Timing.DeltaTime;
                _materialSetter.ChangeColor(Color.Lerp(Color.black, Define.BlackClear, time));
                yield return Timing.DeltaTime;
            }
            WhenDieAnimationDone();
        }

        public void Stun(float time)
        {
            _state = MonsterState.Stun;
            animator.speed = 0;
            Timing.RunCoroutine(_StunRoutine(time), _killWhenDieTag);
        }
        
        private IEnumerator<float> _StunRoutine(float time)
        {
            yield return Timing.WaitForSeconds(time);
            animator.speed = 1;
            if (_state == MonsterState.Die) yield break;
            Idle();
        }

        public void Pushed(float distance)
        {
            _actor.Pushed(distance);
        }

        public void Gather(Vector3 position)
        {
            _actor.Gather(position);
        }
        private void OnAttackDone()
        {
            if (_state == MonsterState.Die) return;
            if (target == null || !target.IsValid()) { Move(); return; }
            if ((target.Position() - root.position).sqrMagnitude < stat.SqrAttackRange) Attack();
            else Move();
        }

        private void OnAttackHit()
        {
            if (_state == MonsterState.Die) return;
            if (target == null || !target.IsValid()) return;
            target.Attacked(stat.Attack);
        }

        public void LookAt(bool lookLeft)
        {
            root.localScale = lookLeft ? _lookLeft : _lookRight;
            stat.Flip(lookLeft);
        }

        public void PlayAnimation(string animationName)
        {
            animator.Play(animationName, 0, 0);
        }

        private void OnEnable()
        {
            stat.OnEnable();
            ResetAnimatorToIdle();
        }

        private void ResetAnimatorToIdle()
        {
            if (animator == null) return;
            
            animator.Rebind();
            animator.Play("idle", 0, 0);
            animator.Update(0f);
        }

    }
}
