using System;
using System.Collections.Generic;
using Cameras;
using Controller;
using Controller.Play;
using Data;
using Data.DbEquipment;

using Data.DbUser.Equipment;
using Fight.Units;
using MEC;
using SkillEffects;
using UIs.FieldMain;
using UIs.FieldMain.MainSkill;
using UnityEngine;
using Utils;

namespace Managers.Game
{
    public class SkillManager
    {
        private Dictionary<int, SkillEffect> _effect = new();
        private bool _isSkillDelay;

        private Vector3 _randomPosition;
        private bool _shouldClear = false;

        public bool CanUseSkill => !_isSkillDelay;
        private UI_MainSkill _mainSkill;


        public void Init()
        {
            _mainSkill = Manager.UI.GetSceneUI<UI_MainSkill>();
        }
        
        public void ClearAll()
        {
            _shouldClear = true;
            foreach (var effect in _effect)
            {
                effect.Value.WhenSkillEnd(false);
            }
            _effect.Clear();
            _isSkillDelay = false;
        }

        public void RemoveMe(int skillId)
        {
            if (_effect.ContainsKey(skillId)) _effect.Remove(skillId);
        }

        private IEnumerator<float> _SkillDelayRoutine(float time)
        {
            _isSkillDelay = true;
            yield return Timing.WaitForSeconds(time);
            _isSkillDelay = false;
            _mainSkill.ASkillUseDone();
        }
        
        /// <returns> <사용했으면 True, 다음 타이머 시작해야하면 True> </returns>
        public Tuple<bool, bool> UseSkill(int skillId)
        {
            var skill = DbSkill.Get(skillId);
            if (!HaveMonster(skill.Target)) return new Tuple<bool, bool>(false, false);

            Timing.RunCoroutine(_SkillDelayRoutine(skill.Delay), Define.KillWhenPlayerDieTag);
            if (skill.Grade >= GradeType.Legendary)
            {
                Manager.Field.Map.FadeToColor(Define.Color808080, Color.white, 0.5f, 0.5f);
                Manager.Player.ShowBubble(LocalString.Get(skill.NameId));
            }
            
            _shouldClear = false;
            Manager.Sound.PlaySFX(skill.Sound);
            
            var skillEffect = MakeSkillEffect(skill.Id, skill.EffectPrefab);
            
            Manager.Player.DoSkill(skill.Animation);
            
            Handle(skill.ToDos, skill.Target, skill.EffectPosition, 
                skillEffect, DbUserSkill.Get(skillId).Bonus, skill.ShakeOnHit);

            return new Tuple<bool, bool>(true, true);

            // bool IsLoopSkill()
            // {
            //     return skill.ToDos.Exists(s => s.type == SkillType.DotAttack);
            // }
        }

        private SkillEffect MakeSkillEffect(int skillId, string effectPrefab)
        {
            var skillEffect = Manager.Resource.Instantiate("Particles/" + effectPrefab, 1, Manager.EffectParent).GetOrAddComponent<SkillEffect>();
            if (_effect.TryGetValue(skillId, out var skill)) skill.WhenSkillEnd(true);
            _effect.Add(skillId, skillEffect);
            return skillEffect;
        }

        private void Handle(List<SkillToDo> toDos, SkillTarget target, 
            EffectPositionType effectPosition, SkillEffect effect, int bonus, bool isShakeOnHit)
        {
            var isEffectPositionSet = false;
            for (var idx = 0; idx < toDos.Count; ++idx)
            {
                HandleIt(toDos[idx]);
            }

            void HandleIt(SkillToDo toDo)
            {
                SetEffectPosition();
                switch (toDo.type)
                {
                    case SkillType.Attack:
                        effect.SetAction(() => DoToMonster(target, Attack, effect));
                        effect.Skill();
                        // effect.SetHitAction(() => DoToMonster(target, Attack, effect));
                        break;
                    case SkillType.TargetAttack:
                        DoToMonster(target, monster =>
                            {
                                effect.SetTarget(monster.targetingPosition, () => Attack(monster));
                            }, effect);
                        effect.Skill();
                        // effect.WhenSkillEnd(true);
                        break;
                    case SkillType.Gather:
                        DoToMonster(target, monster => Gather(monster, _randomPosition), effect);
                        break;
                    case SkillType.Push:
                        effect.SetAction(() => Timing.RunCoroutine(_PushRoutine(), Define.KillWhenPlayerDieTag));
                        break;
                    case SkillType.Stun:
                        if (effect.effectId == 18)
                        {
                            DoToMonster(target, monster =>
                            {
                                effect.SetTarget(monster.targetingPosition, () => Stun(monster));
                            }, effect);
                        }
                        else
                        {
                            effect.SetTarget(Manager.Field.GetNearestMonster(Manager.Player.Position(), 0.1f).transform, () =>
                            {
                                DoToMonster(target, Stun, effect);
                            });
                        }
                        break;
                    case SkillType.DotAttack:
                        effect.Skill();
                        
                        if (_shouldClear)
                        {
                            effect.WhenSkillEnd(false);
                            return;
                        }
                        if (effectPosition == EffectPositionType.PlayerDirection)
                        {
                            Timing.RunCoroutine(_DotAttackRoutine(_randomPosition), Define.KillWhenPlayerDieTag);
                        }
                        else
                        {
                            Timing.RunCoroutine(_DotAttackRoutine(), Define.KillWhenPlayerDieTag);
                        }
                        break;
                }
                
                void Attack(Monster monster)
                {
                    var isCritical = TotalStatController.I.IsCritical();
                    var damage = TotalStatController.I.GetSkillAttack(isCritical, toDo.attack * 10 + bonus, Manager.Player.attackBuff);
                    monster.Attacked(damage, isCritical ? AttackType.SkillCritical : AttackType.Skill);
                    if (isShakeOnHit) CameraController.I.Shake(3);
                }
                void Stun(Monster monster)
                {
                    monster.Stun(toDo.time);
                }
                void Push(Monster monster)
                {
                    monster.Pushed(toDo.distance);
                }
                void Gather(Monster monster, Vector3 position)
                {
                    monster.Gather(position);
                    monster.Stun(toDo.time);
                }
                
                IEnumerator<float> _DotAttackRoutine(Vector3 reference = default)
                {
                    var useTime = 0f;
                    var time = toDo.time;
                    while (useTime < time)
                    {
                        useTime += toDo.interval;
                        DoToMonster(target, Attack, effect, reference);
                        if (useTime > time) continue;
                        yield return Timing.WaitForSeconds(toDo.interval);
                    }

                    Manager.Player.SkillDone();
                    effect.WhenSkillEnd(true);
                }

                IEnumerator<float> _PushRoutine()
                {
                    var useTime = 0f;
                    yield return Timing.WaitForSeconds(toDo.delay);
                    while (useTime < toDo.time)
                    {
                        useTime += toDo.interval;
                        DoToMonster(target, Push, effect);
                        yield return Timing.WaitForSeconds(toDo.interval);
                    }
                }

                void SetEffectPosition()
                {
                    if (isEffectPositionSet) return;
                    isEffectPositionSet = true;
                    
                    if (effectPosition == EffectPositionType.Player)
                    {
                        effect.SetStartPosition();
                    }
                    else if (effectPosition == EffectPositionType.Target)
                    {
                        var monsterTarget = Manager.Field.GetNearestMonster(Manager.Player.Position(), 0.1f);
                        if (monsterTarget == default) effect.WhenSkillEnd(true);
                        effect.SetStartPosition(monsterTarget.targetingPosition.position);
                    }
                    else if (effectPosition == EffectPositionType.PlayerDirection)
                    {
                        var monster = Manager.Field.GetNearestMonster(Manager.Player.Position(), 0.2f);
                        _randomPosition = monster.targetingPosition.position;
                        effect.SetStartPosition();
                        effect.SetDirection(_randomPosition);
                    }
                    else if (effectPosition == EffectPositionType.Random)
                    {
                        _randomPosition = GetRandomPosition(2f);
                        effect.SetStartPosition(_randomPosition);
                    }
                    else if (effectPosition == EffectPositionType.Center)
                    {
                        var center = CameraController.I.Center;
                        center.z = 0;
                        effect.SetStartPosition(center);
                    }
                    else if (effectPosition == EffectPositionType.MoveRandom)
                    {
                        effect.SetStartPosition(GetRandomPosition(2f), true);
                    }
                }
            }
        }

        private Vector3 GetRandomPosition(float range)
        {
            var playerPosition = Manager.Player.Position();
            var monster = Manager.Field.GetNearestMonster(playerPosition, 0.1f);
            var monsterPosition = monster.targetingPosition.position;
            var diff = new Vector3(monsterPosition.x - playerPosition.x,  monsterPosition.y - playerPosition.y, 0);
            diff.Normalize();
            diff *= range;
            diff = Manager.Field.GetInnerDiff(diff, playerPosition);
            return diff + playerPosition;
        }

        private bool HaveMonster(SkillTarget target)
        {
            if (target.targetRange == TargetRangeType.Range)
                return Manager.Field.HaveMonsterInRange(target.targetBase == TargetBaseType.Player
                    ? target.rangeFromBase
                    : target.rangeFromPlayer);
            if (target.targetRange == TargetRangeType.Straight) return Manager.Field.HaveMonsterInStraight(target.rangeFromPlayerExtra, target.rangeFromBase);
            if (target.targetRange == TargetRangeType.Number) return Manager.Field.HaveMonsterInRange(target.rangeFromPlayer);
            return true;
        }

        private Vector3 DoToMonster(SkillTarget target, Action<Monster> toDo, SkillEffect effect, Vector3 reference = default)
        {
            if (Manager.Field.IsGameOver) return default;
            
            Vector3 position = default;
            
            if (target.targetRange == TargetRangeType.Range)
            {
                if (target.targetBase == TargetBaseType.Effect)
                {
                    position = Manager.Field.ForEachMonstersInRange(target.rangeFromBase, effect.transform, target.targetCount, toDo);
                }
                else
                {
                    position = Manager.Field.ForEachMonstersInRange(target.rangeFromBase, target.targetBase, target.targetCount, toDo);
                }
            }
            else if (target.targetRange == TargetRangeType.Straight)
            {
                position = Manager.Field.ForEachMonstersInStraight(target.rangeFromPlayer, target.rangeFromBase, reference, toDo);
            }
            else if (target.targetRange == TargetRangeType.Number)
            {
                position = Manager.Field.ForEachNearestMonsters(target.targetCount, toDo);
            }

            return position;
        }
    }
}