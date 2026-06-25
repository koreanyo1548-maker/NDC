using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Fight.Logics;
using Fight.Logics.TargetSelectors;
using Fight.Stats;
using Managers;
using Managers.Base;
using Managers.Game;
using MEC;
using SkillEffects;
using TMPro;
using UnityEditor;
using UnityEngine;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Units
{
    public class Player: MonoBehaviour, IUpdateable, IFightUnit
    {

        #region Status

        public float SqrAttackRange => _stat.SqrAttackRange;
        public bool IsPlayerSide() => true;
        public bool IsValid() => _state != PlayerState.Die;
        private PlayerState _state = PlayerState.Idle;
        private PlayerStat _stat;
        private bool _canMove;
        private int _attackCount;
        private Action _hitAction;
        private bool _hitFired;
        private const float HIT_NORMALIZED_TIME = 0.5f;
        private const float NORMAL_ATTACK_POSITION_SQR_EPSILON = 0.01f;
        public bool IsBoss() => false;

        public int attackBuff;

        #endregion


        #region Utils

        private Vector3 _dragPos;
        private Vector3 _curPos;

        #endregion


        #region Components

        public bool IsLookingLeft => _positioner.localScale == Define.LookRight;

        public Vector3 Position() => _root.position;
        public Transform Root => _root;
        public Transform WeaponEffectParent => _weaponEffectParent;
        public Transform SkillEffectParent => _skillEffectParent;
        public SimpleSpineSkinAssigner SkinAssigner => _skinAssigner;
        
        private Transform _root;
        private Transform _positioner;
        private Transform _damagePosition;
        private Transform _weaponEffectParent;
        private Transform _skillEffectParent;
        private Animator _animator;

        private GameObject _bubble;
        private TextMeshProUGUI _bubbleText;

        private Transform[] _petPositions;
        public Transform GetPetPosition(int idx) => _petPositions[idx];

        private GameObject[] _dashes;

        private SimpleSpineSkinAssigner _skinAssigner;
        private bool _skillAnimAlt;
        

        #endregion
        

        #region Tools

        private IFightUnit _target;
        private ITargetSelector _targetSelector;
        

        #endregion




        public enum PlayerState
        {
            Die,
            Action,
            Moving,
            Idle,
            Skill,
            //SkillTime,
            Wait
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(_root.position, $"{_state} {_canMove}\n{_stat.Hp}");
            if (_target != null && _target.IsValid())
            {
                Gizmos.DrawLine(_root.position, _target.Position());
            }
        }
        #endif

        private void Awake()
        {
            _root = transform.parent.parent;
            _positioner = transform.parent;
            _animator = GetComponentInChildren<Animator>();
            _damagePosition = _root.Find("DamagePosition");
            _stat = new PlayerStat(this);
            _weaponEffectParent = transform;//Util.FindChild<Transform>(gameObject, "WeaponEffectPos", true);
            _skillEffectParent = Util.FindChild<Transform>(_root.gameObject, "Effect", true);
            _dashMagnitude = DbPlay.Get(PlayType.DashRange).Value;

            _bubble = _root.Find("Bubble").gameObject;
            _bubble.GetComponent<Canvas>().worldCamera = Camera.main;
            _bubbleText = _bubble.transform.Find("T_Text").GetComponent<TextMeshProUGUI>();
            _bubble.SetActive(false);

            var positionParent = _positioner.Find("PetPositions");
            _petPositions = new Transform[4];
            for (var idx = 0; idx < 4; ++idx) _petPositions[idx] = positionParent.GetChild(idx);
            
            var dashParent = _root.Find("DashBar");
            _dashes = new GameObject[4];
            for (var idx = 0; idx < 4; ++idx) _dashes[idx] = dashParent.GetChild(idx).gameObject;
            
            _skinAssigner = GetComponentInChildren<SimpleSpineSkinAssigner>();

            Manager.Player = this;
            TotalStatController.I.Init();

            TotalStatController.I.AttackSpeed.ValueChanged += WhenAttackSpeedChanged;
            WhenAttackSpeedChanged(null, null);
        }

        public void OnUpdate()
        {
            switch (_state)
            {
                case PlayerState.Die:
                    break;
                case PlayerState.Action:
                    UpdateAttackEvents();
                    break;
                case PlayerState.Moving:
                    UpdateMoving();
                    break;
                case PlayerState.Idle:
                    UpdateIdle();
                    break;
                case PlayerState.Skill:
                    UpdateSkillEvents();
                    break;
            }
        }

        private void Start()
        {   
            new PetManager();
        }

        public void Spawn(float posX, float posY)
        {
            _targetSelector = FightSelector.GetTargetSelector(Manager.Field.CurField.Value, false);
            Idle();
            _stat.ApplyMaxHp(TotalStatController.I.Hp, true);
            _stat.ResetAttackRange();
            Manager.Updates.Add(this);
            _root.position = new Vector3(posX, posY);
        }
        
        private void UpdateIdle()
        {
            SetTarget();
            if (_target == null) return;
            Move();
        }
        
        public void SetTarget()
        {
            _target = _targetSelector.GetTarget();
        }
        
        private float _dashMagnitude;
        private const float DASH_DURATION = 0.3f;
        
        private void UpdateMoving()
        {
            if (!_canMove) return;
            if (!_target.IsValid())
            {
                Idle();
                return;
            }
        
            if (_target.IsPlayerSide())
            {
                SetTarget();
            }
        
            var isTargetPlayerSide = _target.IsPlayerSide();
            var targetPosition = GetAttackTargetPosition();
            var dir = isTargetPlayerSide ? targetPosition - _root.position + Define.SlightDown : targetPosition - _root.position;
            var magnitude = dir.sqrMagnitude;
            LookAt(dir.x < 0);
            if (!isTargetPlayerSide && magnitude < NORMAL_ATTACK_POSITION_SQR_EPSILON)
            {
                Attack();
            }
            else
            {
                var isDash = !isTargetPlayerSide && !_target.IsBoss() && magnitude <= _dashMagnitude;
                var moveDist = Mathf.Clamp(TotalStatController.I.GetMoveSpeed(isDash) * Time.deltaTime, 0, dir.magnitude);
                _animator.Play(moveDist == 0 ? "Wait1" : "Walk1", 0);
                _root.position += dir.normalized * moveDist;
            }
        }

        private void Attack()
        {
            LookAtAttackTarget();
            _hitFired = false;
            if (_attackCount < 4) _dashes[_attackCount].SetActive(false);
            _attackCount++;

            _animator.Play(GetAttackAnimation(), 0, 0);
            _state = PlayerState.Action;
        }

        private void Idle()
        {
            _animator.speed = 1f;
            _animator.Play("Wait1", 0, 0);
            _state = PlayerState.Idle;
        }

        private void Die()
        {
            Manager.Sound.PlaySFX(SFXType.Crow_voice3);
            _hitAction = null;
            if (_state == PlayerState.Skill)
            {
                SkillDone();
            }
            _animator.Play("Die", 0, 0);
            _state = PlayerState.Die;

            Timing.KillCoroutines(Define.KillWhenPlayerDieTag);
            _bubble.SetActive(false);
            Manager.Updates.Remove(this);
            var curField = Manager.Field.CurField.Value;
            Manager.Field.GameOver(curField == FieldType.Stage ? GameOverType.StageFail : GameOverType.DungeonFail, 3.25f, curField);
            QuestController.I.DoQuests(QuestType.CharacterDeath);

            var effects = _root.GetComponentsInChildren<SkillEffect>();
            foreach (var effect in effects)
            {
                effect.WhenSkillEnd(true);
            }
        }

        private void Move()
        {
            _canMove = true;
            _animator.Play("Walk1", 0, 0);
            _state = PlayerState.Moving;
        }
        
        public void Attacked(BigInteger attack, AttackType attackType)
        {
            if (_state == PlayerState.Die) return;

            // Manager.UI.GetSceneUI<UI_MainSkill>().AddHit();
            var dm = Manager.Pool.damage.Spawn(_damagePosition.position, attack);
            dm.SetSize(Define.DamageSize(false));
            dm.SetGradient(Define.DamageColor(false));
            var isDead = _stat.Attacked(attack);
            if (isDead)
            {
                Die();
            }
        }

        private CoroutineHandle _dashRoutine;
        private float GetClipLength(string clipName)
        {
            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
                if (clip.name == clipName) return clip.length;
            return 1f;
        }

        private void Dash(float distance, Effect_Dash effect)
        {
            Manager.Sound.PlaySFX(SFXType.Skill1);
            _animator.speed = GetClipLength("Run1") / DASH_DURATION;
            _animator.Play("Run1", 0);
            var nearest = Manager.Field.GetNearestMonster(Position(), 0.1f);
            if (nearest == null)
            {
                Idle();
                return;
            }
            var monsterPosition = nearest.Position();
            var diff = new Vector3(monsterPosition.x - Position().x, monsterPosition.y - Position().y, 0);
            diff.Normalize();
            diff *= distance;
            diff = Manager.Field.GetInnerDiff(diff, Position());
            if (nearest.IsBoss()) diff.y = 0;
            if (effect != null)
            {
                effect.SetPosition(Position(), Position() + diff);
                effect.Skill();
            }
            DashAttack();
            Timing.KillCoroutines(_dashRoutine);
            _dashRoutine = Timing.RunCoroutine(_DashRoutine(diff), Define.KillWhenPlayerDieTag);
        }

        private void DashAttack()
        {
            Manager.Field.ForEachMonstersInRange(_stat.SqrHitRange, TargetBaseType.Player, -1,
                monster =>
                {
                    var isCritical = TotalStatController.I.IsCritical();
                    var damage = TotalStatController.I.GetDashAttack(isCritical, monster.isBoss, attackBuff);
                    monster.Attacked(damage, isCritical ? AttackType.Critical : AttackType.Normal);
                });
        }

        IEnumerator<float> _DashRoutine(Vector3 diff)
        {
            _canMove = false;
            var deltaTime = Time.deltaTime;
            var count = DASH_DURATION / deltaTime;
            diff /= count;
            while (count-- > 0)
            {
                _root.position += diff;
                yield return Timing.WaitForSeconds(deltaTime);
            }
            _canMove = true;
            Idle();
            
            ResetDash();
        }
        
        private void UpdateAttackEvents()
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (!IsAttackAnimationName(info)) return;
            float t = info.normalizedTime % 1f;
            if (!_hitFired && t >= HIT_NORMALIZED_TIME)
            {
                _hitFired = true;
                OnAttackHit();
            }
            if (t >= 0.95f)
            {
                _hitFired = false;
                OnAttackDone();
            }
        }

        private void UpdateSkillEvents()
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (!IsSkillAnimationName(info)) return;
            if (info.normalizedTime % 1f >= 0.95f)
                SkillDone();
        }

        private void OnAttackDone()
        {
            //if (_state == PlayerState.SkillTime) return;
            if (_state == PlayerState.Wait) return;

            if (_state == PlayerState.Skill)
            {
                SkillDone();
                return;
            }

            if (_canMove && _attackCount >= 4)
            {
                _attackCount = 0;
                var particle = Manager.Resource.InstantiateParticle("Particles/Effect_Dash", 1, Manager.EffectParent);
                Dash(2, particle == null ? null : particle.GetOrAddComponent<Effect_Dash>());
                return;
            }
            
            if (_target != null && _target.IsValid())
            {
                if ((GetAttackTargetPosition() - _root.position).sqrMagnitude < NORMAL_ATTACK_POSITION_SQR_EPSILON) Attack();
                else Move();
            }
            else Idle();
        }

        private void ResetDash()
        {
            foreach (var dash in _dashes)
            {
                dash.SetActive(true);
            }
        }

        public void SetHitAction(Action action)
        {
            _hitAction += action;
        }

        private void OnAttackHit()
        {
            if (Manager.Field.IsGameOver) return;
            if (_hitAction != null)
            {
                _hitAction();
                _hitAction = null;
            }
            Manager.Sound.PlaySFX(SFXType.Attack);
            if (_state == PlayerState.Skill) return;// || _state == PlayerState.SkillTime) return;

            if (_target == null) return;
            Manager.Field.ForEachMonstersInNormalAttackRange(Position(), !IsLookingLeft,
                _stat.SqrHitRange,
                monster =>
                {
                    var isCritical = TotalStatController.I.IsCritical();
                    var damage = TotalStatController.I.GetAttack(isCritical, monster.isBoss, attackBuff);
                    monster.Attacked(damage, isCritical ? AttackType.Critical : AttackType.Normal);
                });
        }

        private Vector3 GetAttackTargetPosition()
        {
            return _target is Monster monster ? monster.GetNormalAttackPosition(Position()) : _target.Position();
        }

        private void LookAtAttackTarget()
        {
            if (_target == null) return;
            LookAt(_target is Monster monster
                ? monster.ShouldAttackerLookLeft(Position())
                : _target.Position().x < Position().x);
        }

        public void DoSkill(string skillAnimation)
        {
            _state = PlayerState.Skill;
            _animator.Play(GetSkillAnimation(), 0, 0);
        }

        public void SkillDone()
        {
            Idle();
        }

        public bool GameOver()
        {
            var isDie = _state == PlayerState.Die;
            if (_state == PlayerState.Skill)
            {
                SkillDone();
            }
            if (_state != PlayerState.Die)
            {
                _animator.Play("Wait1");
            }
            _state = PlayerState.Die;
            Manager.Updates.Remove(this);
            _attackCount = 0;
            ResetDash();
            return isDie;
        }
    
        public void HpRecoveryBuff(int percent)
        {
            // var effectObj = Manager.Resource.InstantiateParticle("Particles/Effect_Heal", 1, Manager.EffectParent);
            // if (effectObj != null)
            // {
            //     var effect = effectObj.GetOrAddComponent<SkillEffect>();
            //     effect.SetPosition(Position());
            //     effect.Skill();
            // }
            _stat.Recovery(TotalStatController.I.Hp * percent / 1000);
            // Timing.RunCoroutine(_HpRecoveryRoutine(dotPerMille, interval, time), Define.KillWhenPlayerDieTag);
        }

        // IEnumerator<float> _HpRecoveryRoutine(int perMille, float interval, float time)
        // {
        //     var useTime = 0f;
        //     while (useTime < time)
        //     {
        //         useTime += interval;
        //         yield return Timing.WaitForSeconds(interval);
        //     }
        // }
        
        public void AttackBuff(int percent)
        {
            attackBuff = percent;
            // var effectObj = Manager.Resource.InstantiateParticle("Particles/Effect_AttackBuff", 1, Manager.EffectParent);
            // if (effectObj != null)
            // {
            //     var effect = effectObj.GetOrAddComponent<SkillEffect>();
            //     effect.SetPosition(Position());
            //     effect.Skill();
            // }
            // Timing.RunCoroutine(_AttackBuffRoutine(percent, time), Define.KillWhenPlayerDieTag);
        }

        // IEnumerator<float> _AttackBuffRoutine(int percent, int time)
        // {
        //     attackBuff = percent;
        //     yield return Timing.WaitForSeconds(time);
        //     attackBuff = 0;
        // }
        
        private string GetAttackAnimation()
        {
            var suffix = (_attackCount % 2 == 1) ? "1" : "2";
            if (_skinAssigner == null) return $"Attack_OneHand{suffix}";

            var right = _skinAssigner.rightHandWeaponSkin ?? "";
            var left  = _skinAssigner.leftHandWeaponSkin  ?? "";

            if (right.Contains("Bow_"))                                    return "Shoot1";
            if (right.Contains("Scepter_") || right.Contains("Staff_"))   return $"Spell{suffix}";
            if (right.Contains("TwoHand"))                                 return $"Attack_TwoHand{suffix}";

            var leftIsWeapon = left.Contains("Dagger") || left.Contains("Sword") || left.Contains("Spear");
            return leftIsWeapon ? $"Attack_DualHand{suffix}" : $"Attack_OneHand{suffix}";
        }

        private string GetSkillAnimation()
        {
            _skillAnimAlt = !_skillAnimAlt;
            var suffix = _skillAnimAlt ? "2" : "1";
            if (_skinAssigner == null) return $"Cast{suffix}";

            var right = _skinAssigner.rightHandWeaponSkin ?? "";
            if (right.Contains("Bow_"))                                  return "Shoot1";
            if (right.Contains("Scepter_") || right.Contains("Staff_")) return $"Spell{suffix}";
            return $"Cast{suffix}";
        }

        private static bool IsAttackAnimationName(AnimatorStateInfo info)
        {
            return info.IsName("Attack_OneHand1")  || info.IsName("Attack_OneHand2")  ||
                   info.IsName("Attack_DualHand1") || info.IsName("Attack_DualHand2") ||
                   info.IsName("Attack_TwoHand1")  || info.IsName("Attack_TwoHand2")  ||
                   info.IsName("Shoot1") || info.IsName("Spell1") || info.IsName("Spell2");
        }

        private static bool IsSkillAnimationName(AnimatorStateInfo info)
        {
            return info.IsName("Cast1")  || info.IsName("Cast2")  ||
                   info.IsName("Spell1") || info.IsName("Spell2") ||
                   info.IsName("Shoot1");
        }

        private void WhenAttackSpeedChanged(object sender, EventArgs eventArgs)
        {
            _animator.SetFloat("AttackSpeed", TotalStatController.I.AttackSpeed.Value);

        }

        public void LookAt(bool lookLeft)
        {
            _positioner.localScale = lookLeft ? Define.LookRight : Define.LookLeft;
        }

        public void ShowBubble(string text)
        {
            _bubbleText.text = text;
            _bubble.SetActive(true);
            Timing.RunCoroutine(_BubbleRoutine(), Define.KillWhenPlayerDieTag);
        }

        private IEnumerator<float> _BubbleRoutine()
        {
            yield return Timing.WaitForSeconds(1);
            _bubble.SetActive(false);
        }

        public void SetWait()
        {
            _state = PlayerState.Wait;
        }
    }
}
