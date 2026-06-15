using System;
using System.Collections.Generic;
using System.Numerics;
using Controller.Currency;
using Controller.Have;
using Controller.Infos;
using Controller.Utils;
using Data;
using Data.DbAbility;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbNecklaceInfo;
using Data.DbPetInfo;
using Data.DbPromote;
using Data.DbRecord;
using Data.DbRelicInfo;
using Data.DbShop;
using Data.DbUser.Equipment;
using Data.DbUser.Progress;
using Data.Stores;
using Managers;
using MEC;
using ThirdParty;
using UIs.Etc;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Controller.Play
{
    public class TotalStatController : Singleton<TotalStatController>
    {
        private Dictionary<StatType, long> _stats;
        private BigInteger _attack;
        public BigInteger Hp { get; private set; }
        public ControllerField<float> AttackSpeed { get; private set; }
        private float _moveSpeed;
        private float _dashSpeed;

        private EventsManager _bibleLockManager;
        private bool _isBibleLocked;

        public static ControllerField<BigInteger> Power = new(0);

        #region Defining Attack & Hp Types, and default values

        private List<StatType> _attackTypes = new()
        {
            StatType.Attack, StatType.AttackBonus, StatType.AwakeningAttackBonus,
            StatType.PetAttackBonus, StatType.FinalAttackBonus, StatType.PotentialAttackBonus,
            StatType.RelicAttackBonus, StatType.NecklaceAttackBonus
        };

        private List<StatType> _hpTypes = new()
        {
            StatType.Hp, StatType.HpBonus, StatType.AwakeningHpBonus, StatType.PetHpBonus,
            StatType.FinalHpBonus, StatType.PotentialHpBonus, StatType.RelicHpBonus,
            StatType.NecklaceHpBonus
        };

        private Dictionary<StatType, long> _defaults = new()
        {
            {StatType.Attack, 0}, {StatType.Hp, 0}, {StatType.CriticalProbability, 0},
            {StatType.CriticalAttackBonus, 1000}, {StatType.StageExpEarn, 10000},
            {StatType.StageGoldEarn, 10000},
            {StatType.MoveSpeedBonus, 1000}, {StatType.AttackSpeedBonus, 1000}, {StatType.StageGrowthEarn, 100},
            {StatType.BlackMarketDungeonEarn, 100},
            {StatType.SkillGrowthDungeonEarn, 100}, {StatType.DiaDungeonEarn, 100},
            {StatType.AwakeningDungeonEarn, 100}, {StatType.BossAttackBonus, 1000},
            {StatType.SpecificSkillAttackBonus, 100}, {StatType.SkillAttackBonus, 1000},
            {StatType.DashAttackBonus, 100}, {StatType.FinalAttackBonus, 1000}, {StatType.FinalHpBonus, 1000},
            {StatType.AwakeningAttackBonus, 100},
            {StatType.AwakeningHpBonus, 100}, {StatType.PetAttackBonus, 1000}, {StatType.PetHpBonus, 1000},
            {StatType.StageItemRate, 100},
            {StatType.DebuffMonsterHp, 0}, {StatType.DebuffMonsterAttack, 0}, {StatType.AttackBonus, 100},
            {StatType.HpBonus, 100},
            {StatType.PotentialAttackBonus, 1000}, {StatType.PotentialHpBonus, 1000}, {StatType.AbilityGoldEarn, 100},
            {StatType.AbilityExpEarn, 100},
            {StatType.RelicAttackBonus, 1000}, {StatType.RelicHpBonus, 1000}, {StatType.NecklaceAttackBonus, 1000},
            {StatType.NecklaceHpBonus, 1000},
            {StatType.NormalAttackBonus, 1000}
        };

        #endregion


        #region GetFinalStats

        public long GetStat(StatType stat)
        {
            return _stats[stat];
        }

        public bool IsCritical()
        {
            return Random.Range(0, 1000) < _stats[StatType.CriticalProbability];
        }

        public BigInteger GetAttack(bool isCritical, bool isBoss, int buff = 0)
        {
            // Debug.Log("기본 : " + (_attack * _stats[StatType.NormalAttackBonus] / 1000).ToString("N0") 
            //                   + " 최종 : " + (_attack * _stats[StatType.NormalAttackBonus] * (1000 + buff) / 1000000).ToString("N0"));
            // NormalAttack 1000, CriticalAttack 1000, BossAttack 1000, buff 1000
            if (isCritical)
            {
                if (isBoss)
                    return _attack * _stats[StatType.NormalAttackBonus] *
                           (_stats[StatType.CriticalAttackBonus] * _stats[StatType.BossAttackBonus]) * (1000 + buff) /
                           1000000000000; // 1,000,000,000,000  
                return _attack * _stats[StatType.NormalAttackBonus] * _stats[StatType.CriticalAttackBonus] *
                    (1000 + buff) / 1000000000; // 1,000,000,000
            }

            if (isBoss)
                return _attack * _stats[StatType.NormalAttackBonus] * _stats[StatType.BossAttackBonus] * (1000 + buff) /
                       1000000000; // 1,000,000,000
            return _attack * _stats[StatType.NormalAttackBonus] * (1000 + buff) / 1000000; // 1,000,000
        }


        public BigInteger GetDashAttack(bool isCritical, bool isBoss, int buff = 0)
        {
            // NormalAttack 1000, CriticalAttack 1000, BossAttack 1000, DashAttack 100, buff 1000
            if (isCritical)
            {
                if (isBoss)
                    return _attack * _stats[StatType.NormalAttackBonus] *
                        (_stats[StatType.CriticalAttackBonus] * _stats[StatType.BossAttackBonus] *
                         _stats[StatType.DashAttackBonus]) * (1000 + buff) / 100000000000000;  // 100,000,000,000,000
                return _attack * _stats[StatType.NormalAttackBonus] *
                       (_stats[StatType.CriticalAttackBonus] * _stats[StatType.DashAttackBonus]) * (1000 + buff) /
                       100000000000; // 100,000,000,000
            }

            if (isBoss)
                return _attack * _stats[StatType.NormalAttackBonus] *
                    (_stats[StatType.BossAttackBonus] * _stats[StatType.DashAttackBonus]) * (1000 + buff) / 100000000000; // 100,000,000,000
            return _attack * _stats[StatType.NormalAttackBonus] * _stats[StatType.DashAttackBonus] * (1000 + buff) /
                   100000000; // 100,000,000
        }

        public BigInteger GetSkillAttack(bool isCritical, int percent, int buff = 0)
        {
            // NormalAttack 1000, CriticalAttack 1000, BossAttack 1000, SkillAttackBonus 1000, DashAttack 100, buff 1000, percent 1000
            if (isCritical)
                return _attack * (_stats[StatType.SkillAttackBonus] * percent * _stats[StatType.CriticalAttackBonus]) *
                    (1000 + buff) / 1000000000000; // 1,000,000,000,000
            return _attack * (_stats[StatType.SkillAttackBonus] * percent) * (1000 + buff) / 1000000000; // 1,000,000,000
        }

        public float GetMoveSpeed(bool isDash)
        {
            if (isDash) return _dashSpeed;
            return _moveSpeed;
        }

        #endregion

        public TotalStatController()
        {
            AttackSpeed = new ControllerField<float>(0);

            _isBibleLocked = LevelController.I.CheckIsLocked(DbLock.Get(LockType.Pet));
            _stats = new();
            _isBibleLocked = LevelController.I.CheckIsLocked(DbLock.Get(LockType.Pet));
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                if (stat == StatType.None || stat == StatType.SpecificSkillAttackBonus ||
                    stat == StatType.Skill) continue;

                _stats.Add(stat, CalculateStat(stat));
            }

            if (_isBibleLocked)
            {
                _bibleLockManager = new EventsManager(Manager.Player, new EventsManager.Config
                {
                    handler = CheckBibleLocked,
                    updatedField = LevelController.I.GetUpdatedFieldForLock(DbLock.Get(LockType.Pet))
                });
            }

            SetAttack();
            SetHp();
            SetMoveSpeed();
            SetAttackSpeed();
        }

        public void Init()
        {
        }

        private void CheckBibleLocked()
        {
            if (!LevelController.I.CheckIsLocked(DbLock.Get(LockType.Pet)))
            {
                _bibleLockManager?.Dispose();
                _isBibleLocked = false;
                Apply(StatType.Attack);
                Apply(StatType.Hp);
            }
        }

        public void Apply(StatType type)
        {
            if (type == StatType.SpecificSkillAttackBonus || type == StatType.Skill || type == StatType.None) return;

            _stats[type] = CalculateStat(type);

            if (_attackTypes.Contains(type)) SetAttack();
            else if (_hpTypes.Contains(type)) SetHp();
            else if (type == StatType.MoveSpeedBonus) SetMoveSpeed();
            else if (type == StatType.AttackSpeedBonus) SetAttackSpeed();
            else if (type is StatType.SkillAttackBonus or StatType.CriticalProbability or StatType.CriticalAttackBonus) CalculatePower();
        }

        #region Additional Calculation for attack, hp, attack speed and move speed

        private void SetAttack()
        {
            var tmpAttack = new BigInteger(1);
            foreach (var stat in _attackTypes)
            {
                tmpAttack *= _stats[stat] == 0 ? 1 : _stats[stat];
            }

            for (var idx = 1; idx < _attackTypes.Count; ++idx)
            {
                tmpAttack /= 100;
            }

            tmpAttack /=
                100000; // PetAttackBonus, FinalAttackBonus, PotentialAttackBonus, RelicAttackBonus, NecklaceAttackBonus

            _attack = tmpAttack;
            CalculatePower();
        }

        private void SetHp()
        {
            var tmpHp = new BigInteger(1);
            foreach (var stat in _hpTypes)
            {
                tmpHp *= _stats[stat];
            }

            for (var idx = 1; idx < _hpTypes.Count; ++idx)
            {
                tmpHp /= 100;
            }

            tmpHp /= 100000; // PetHpBonus, FinalHpBonus, PotentialHpBonus, RelicHpBonus, NecklaceAttackBonus

            Hp = tmpHp;
            CalculatePower();
        }

        private void SetMoveSpeed()
        {
            _moveSpeed = DbPlay.Get(PlayType.DefaultMoveSpeed).Value * _stats[StatType.MoveSpeedBonus] * 0.001f;
            _dashSpeed = DbPlay.Get(PlayType.DefaultDashSpeed).Value * _stats[StatType.MoveSpeedBonus] * 0.001f;
        }

        private void SetAttackSpeed()
        {
            AttackSpeed.Value = DbPlay.Get(PlayType.DefaultAttackSpeed).Value * _stats[StatType.AttackSpeedBonus] *
                                0.001f;
        }

        #endregion

        #region Power

        private int _powerInitiating = 2;
        private bool _isWaitForPowerUpdated;
        private CoroutineHandle _powerToastRoutine;
        private BigInteger _prevPower;

        private void CalculatePower()
        {
            Power.Value = (BigInteger)((double)_attack * 
                ((_stats[StatType.CriticalProbability] * 0.001f * _stats[StatType.CriticalAttackBonus] * 0.001f 
                  + (1 - _stats[StatType.CriticalProbability] * 0.001f)) * 0.8 * (0.25f + _stats[StatType.SkillAttackBonus] * 0.001f))) + Hp / 30;

            if (_powerInitiating > 0)
            {
                _powerInitiating--;
                _prevPower = Power.Value;
                if (_powerInitiating == 0 && LevelController.data.MaxPower.Value == 0)
                {
                    LevelController.data.MaxPower.Value = Power.Value;
                }

                return;
            }

            if (_powerInitiating == 0 && _prevPower < Power.Value)
            {
                if (_isWaitForPowerUpdated)
                {
                    Timing.KillCoroutines(_powerToastRoutine);
                }

                _powerToastRoutine = Timing.RunCoroutine(_PowerToastRoutine(Power.Value - _prevPower));
            }
            else
            {
                _prevPower = Power.Value;
            }
        }

        private IEnumerator<float> _PowerToastRoutine(BigInteger diff)
        {
            _isWaitForPowerUpdated = true;
            yield return Timing.WaitForSeconds(0.5f);
            Manager.UI.ShowSingleUI<UI_PowerToast>().Set(Power.Value, diff);
            if (Power.Value > LevelController.data.MaxPower.Value)
            {
                LevelController.data.MaxPower.Value = Power.Value;
            }

            _prevPower = Power.Value;
            _isWaitForPowerUpdated = false;
        }

        #endregion

        private long CalculateStat(StatType type)
        {
            var stat = _defaults[type];
            switch (type)
            {
                case StatType.Attack:
                    stat += StatController.attack.value + LevelPointController.attack.value + GetBibleAttack();
                    break;
                case StatType.Hp:
                    stat += StatController.hp.value + LevelPointController.hp.value + GetBibleHp();
                    break;
                case StatType.CriticalAttackBonus:
                    stat += StatController.criticalAttackBonus.value + LevelPointController.criticalAttackBonus.value;
                    break;
                case StatType.CriticalProbability:
                    stat += StatController.criticalProbability.value + LevelPointController.criticalProbability.value;
                    break;
                case StatType.BossAttackBonus:
                    stat += StatController.bossAttackBonus.value;
                    break;
                case StatType.StageExpEarn:
                    stat += LevelPointController.stageExpEarn.value;
                    break;
                case StatType.StageGoldEarn:
                    stat += LevelPointController.stageGoldEarn.value;
                    break;
                case StatType.DebuffMonsterAttack:
                    stat += LevelPointController.debuffMonsterAttack.value;
                    break;
                case StatType.DebuffMonsterHp:
                    stat += LevelPointController.debuffMonsterHp.value;
                    break;
                case StatType.DashAttackBonus:
                    stat += LevelPointController.dashAttackBonus.value;
                    break;
                case StatType.AttackBonus:
                    stat += StatController.attackBonus.value + SumOfWeaponOwn() + SumOfWeaponEquip();
                    break;
                case StatType.HpBonus:
                    stat += StatController.hpBonus.value + SumOfAccessoryOwn() + SumOfAccessoryEquip();
                    break;
                case StatType.PetAttackBonus:
                    stat += SumOfPetAttack();
                    break;
                case StatType.PetHpBonus:
                    stat += SumOfPetHp();
                    break;
                case StatType.FinalAttackBonus:
                    stat += GetPromotionAttack();
                    break;
                case StatType.FinalHpBonus:
                    stat += GetPromotionHp();
                    break;
                case StatType.NecklaceHpBonus:
                    stat += SumOfNecklaceOwnHp();
                    break;
            }

            stat += SumOfTitle(type) + SumOfWeaponAwakening(type) +
                    SumOfAccessoryAwakening(type) + SumOfSkillAwakening(type)
                    + SumOfPetEquip(type) + SumOfCostume(type) + SumOfAdBuff(type)
                    + SumOfAbilityOption(type) + SumOfAbilityRune(type) + SumOfRelic(type)
                    + SumOfNecklace(type);


            return stat;
        }


        private long SumOfTitle(StatType type)
        {
            var sum = 0;
            DbUserTitle.ForEach(t =>
            {
                if (t.Level.Value > 0)
                {
                    var title = DbTitle.Get(t.Id);
                    if (title.Option == type)
                    {
                        sum += t.Level.Value * title.Value;
                    }
                }
            });
            return sum;
        }

        private long SumOfWeaponOwn()
        {
            var sum = 0L;
            DbUserWeapon.ForEach(w =>
            {
                if (w.Have.Value)
                {
                    var weapon = DbWeapon.Get(w.Id);
                    sum += weapon.OwnAttack + weapon.OwnGrowthAttack * (w.Growth.Value - 1);
                }
            });
            return sum;
        }

        private long SumOfWeaponEquip()
        {
            var sum = 0L;
            DbUserWeapon.ForEach(w =>
            {
                if (EquipController.I.IsEquipped(w))
                {
                    var weapon = DbWeapon.Get(w.Id);
                    sum += weapon.EquipAttack + weapon.EquipGrowthAttack * (w.Growth.Value - 1);
                }
            });
            return sum;
        }


        private long SumOfAccessoryOwn()
        {
            var sum = 0L;
            DbUserAccessory.ForEach(a =>
            {
                if (a.Have.Value)
                {
                    var accessory = DbAccessory.Get(a.Id);
                    sum += accessory.OwnHp + accessory.OwnGrowthHp * (a.Growth.Value - 1);
                }
            });
            return sum;
        }

        private long SumOfAccessoryEquip()
        {
            var sum = 0L;
            DbUserAccessory.ForEach(a =>
            {
                if (EquipController.I.IsEquipped(a))
                {
                    var accessory = DbAccessory.Get(a.Id);
                    sum += accessory.EquipHp + accessory.EquipGrowthHp * (a.Growth.Value - 1);
                }
            });
            return sum;
        }

        private long SumOfPetAttack()
        {
            var sum = 0L;
            DbUserPet.ForEach(p =>
            {
                if (EquipController.I.IsEquipped(p))
                {
                    var pet = DbPet.Get(p.Id);
                    sum += pet.EquipAttack + pet.EquipGrowthAttack * (p.Growth.Value - 1);
                }
            });
            return sum;
        }

        private long SumOfPetHp()
        {
            var sum = 0L;
            DbUserPet.ForEach(p =>
            {
                if (EquipController.I.IsEquipped(p))
                {
                    var pet = DbPet.Get(p.Id);
                    sum += pet.EquipHp + pet.EquipGrowthHp * (p.Growth.Value - 1);
                }
            });
            return sum;
        }

        private long GetBibleAttack()
        {
            if (_isBibleLocked) return 0;
            return DbBibleLevel.Get(LevelController.data.BibleLevel.Value).Attack;
        }

        private long GetBibleHp()
        {
            if (_isBibleLocked) return 0;
            return DbBibleLevel.Get(LevelController.data.BibleLevel.Value).Hp;
        }

        private long GetPromotionAttack()
        {
            var promotion = LevelController.data.Promotion.Value;
            var attack = 0L;
            for (var idx = 1; idx <= promotion; ++idx)
            {
                attack += DbPromotion.Get(promotion).Attack;
            }

            return attack;
        }

        private long GetPromotionHp()
        {
            var promotion = LevelController.data.Promotion.Value;
            var hp = 0L;
            for (var idx = 1; idx <= promotion; ++idx)
            {
                hp += DbPromotion.Get(promotion).Hp;
            }

            return hp;
        }


        private long SumOfWeaponAwakening(StatType type)
        {
            var sum = 0L;
            DbUserWeapon.ForEach(w =>
            {
                if (w.Awakening.Value > 0)
                {
                    var weapon = DbWeapon.Get(w.Id);
                    var awakening = weapon.GetAwakening();
                    for (var idx = 1; idx <= w.Awakening.Value; ++idx)
                    {
                        if (awakening.GetOption(idx).Equals(type))
                            sum += awakening.GetStat(idx);
                    }
                }
            });
            return sum;
        }

        private long SumOfAccessoryAwakening(StatType type)
        {
            var sum = 0L;
            DbUserAccessory.ForEach(a =>
            {
                if (a.Awakening.Value > 0)
                {
                    var accessory = DbAccessory.Get(a.Id);
                    var awakening = accessory.GetAwakening();
                    for (var idx = 1; idx <= a.Awakening.Value; ++idx)
                    {
                        if (awakening.GetOption(idx).Equals(type))
                            sum += awakening.GetStat(idx);
                    }
                }
            });
            return sum;
        }

        private long SumOfSkillAwakening(StatType type)
        {
            var sum = 0L;
            DbUserSkill.ForEach(s =>
            {
                if (s.Awakening.Value > 0)
                {
                    var skill = DbSkill.Get(s.Id);
                    var awakening = skill.AwakeningMeta;
                    for (var idx = 1; idx <= s.Awakening.Value; ++idx)
                    {
                        if (awakening.GetOption(idx).Equals(type))
                            sum += awakening.GetStat(idx);
                    }
                }
            });
            return sum;
        }

        private long SumOfNecklace(StatType type)
        {
            // 각성, 장착
            var sum = 0L;
            DbUserNecklace.ForEach(n =>
            {
                if (!n.Have.Value) return;
                if (n.Awakening.Value > 0)
                {
                    var necklace = DbNecklaceAwakening.Get(n.Id);
                    for (var idx = 1; idx <= n.Awakening.Value; ++idx)
                    {
                        if (necklace.GetOption(idx).Equals(type)) sum += necklace.GetStat(idx);
                    }
                }

                var meta = n.Meta;
                if (meta.EquipStat == type)
                {
                    if (EquipController.I.IsEquipped(n)) sum += DbNecklaceEquipStat.Get(n.Growth.Value).Stats[meta.EquipIdx];
                }
            });
            return sum;
        }

        private long SumOfNecklaceOwnHp()
        {
            var sum = 0L;
            DbUserNecklace.ForEach(n =>
            {
                if (!n.Have.Value) return;
                sum += DbNecklaceOwnStat.Get(n.Growth.Value).Stats[n.Meta.OwnIdx];
            });
            return sum;
        }
        
        private long SumOfPetEquip(StatType type)
        {
            var sum = 0L;
            DbUserPet.ForEach(p =>
            {
                if (EquipController.I.IsEquipped(p))
                {
                    var pet = DbPet.Get(p.Id);
                    var awakening = DbPetAwakening.Get(pet.Awakening);
                    if (awakening.Option.Equals(type)) sum = (int)awakening.GetStat(p.Awakening.Value);
                }
            });
            return sum;
        }

        private long SumOfCostume(StatType type)
        {
            var sum = 0L;
            CurrencyController.data.Costumes.ForEach(c =>
            {
                var costume = DbCostume.Get(c);
                for (var idx = 0; idx < costume.Options.Count; ++idx)
                {
                    if (costume.Options[idx] == type)
                    {
                        sum += costume.Values[idx];
                    }
                }
            });
            return sum;
        }
        
        private long SumOfAdBuff(StatType type)
        {
            var sum = 0L;
            CurrencyController.data.AdBuff.ForEach(a =>
            {
                if (a.IsUsing.Value && a.BuffType == type)
                {
                    var addBuff = DbAdBuff.Get(a.AdBuffType);
                    sum += addBuff.Buff;
                }
            });
            return sum;
        }

        private long SumOfAbilityOption(StatType type)
        {
            var sum = 0L;
            for (var idx = 0; idx < 5; ++idx)
            {
                var ability = DbUserAbility.Get(AbilityController.I.preset.Value * 5 + idx);
                if (ability.Option.Value == type)
                    sum += DbAbilityOption.Get(type).Value[ability.OptionGrade - GradeType.Normal];
            }
            return sum;
        }
        
        private long SumOfAbilityRune(StatType type)
        {
            if (AbilityController.I.runeLevel.TryGetValue(type, out var rune))
            {
                var level = rune.Value;
                if (level == 0) return 0;
                return DbAbilityRune.Get(type).Value[level-1];
            }

            return 0;
        }
        
        private long SumOfRelic(StatType type)
        {
            var sum = 0L;
            DbUserRelic.ForEach(r =>
            {
                if (r.Level.Value > 0 && DbRelic.Get(r.Id).StatType == type)
                    sum += DbRelicLevel.Get(r.Level.Value).GetStat(r.Id);
            });
            return sum;
        }
    }
}