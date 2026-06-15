using System;
using Controller.Currency;
using Controller.Play;
using Data;
using Data.DbCharacter;
using Data.DbUser.Progress;
using Data.Utils;
using Utils;

namespace Controller.Infos
{
    public class LevelPointController: Singleton<LevelPointController>
    {
        private static DbUserLevelPoint data = DbUserLevelPoint.Get(0);

        public static LevelPointLevel attack = new(DbLevelPoint.Get(StatType.Attack));
        public static LevelPointLevel hp = new(DbLevelPoint.Get(StatType.Hp));
        public static LevelPointLevel criticalProbability = new(DbLevelPoint.Get(StatType.CriticalProbability));
        public static LevelPointLevel criticalAttackBonus = new(DbLevelPoint.Get(StatType.CriticalAttackBonus));
        public static LevelPointLevel stageExpEarn = new(DbLevelPoint.Get(StatType.StageExpEarn));
        public static LevelPointLevel stageGoldEarn = new(DbLevelPoint.Get(StatType.StageGoldEarn));
        public static LevelPointLevel dashAttackBonus = new(DbLevelPoint.Get(StatType.DashAttackBonus));
        public static LevelPointLevel debuffMonsterAttack = new(DbLevelPoint.Get(StatType.DebuffMonsterAttack));
        public static LevelPointLevel debuffMonsterHp = new(DbLevelPoint.Get(StatType.DebuffMonsterHp));

        // public int GetPower()
        // {
        //     return attack.Power.Value + hp.Power.Value + criticalProbability.Power.Value +
        //            criticalAttackBonus.Power.Value;
        // }
        
        public LevelPointLevel Get(DbLevelPoint stat)
        {
            switch (stat.Id)
            {
                case StatType.Attack:
                    return attack;

                case StatType.Hp:
                    return hp;

                case StatType.CriticalProbability:
                    return criticalProbability;

                case StatType.CriticalAttackBonus:
                    return criticalAttackBonus;
                
                case StatType.StageExpEarn:
                    return stageExpEarn;
                
                case StatType.StageGoldEarn:
                    return stageGoldEarn;

                case StatType.DashAttackBonus:
                    return dashAttackBonus;
                
                case StatType.DebuffMonsterAttack:
                    return debuffMonsterAttack;
                
                case StatType.DebuffMonsterHp:
                    return debuffMonsterHp;
            }

            throw new Exception("존재하지 않는 스탯 타입");
        }
        
        public class LevelPointLevel
        {
            public long value;
            public readonly float multiply;
            public readonly string form;
            public readonly DbLevelPoint meta;
            public DbField<int> Level { get; private set; }
            // public DbField<int> Power { get; private set; }

            // private PowerType powerType;
            
            public LevelPointLevel(DbLevelPoint levelPoint)
            {
                meta = levelPoint;
                switch (meta.Id)
                {
                    case StatType.Attack :
                        Level = data.AttackLevel;
                        form = "N0";
                        multiply = 1.0f;
                        break;
                    
                    case StatType.Hp :
                        Level = data.HpLevel;
                        form = "N0";
                        multiply = 1.0f;
                        break;
                    
                    case StatType.CriticalProbability :
                        Level = data.CriticalProbabilityLevel;
                        form = "P2";
                        multiply = 0.001f;
                        break;
                    
                    case StatType.CriticalAttackBonus :
                        Level = data.CriticalAttackBonusLevel;
                        form = "P1";
                        multiply = 0.001f;
                        break;
                        
                    case StatType.StageExpEarn :
                        Level = data.ExpBonusRateLevel;
                        form = "P0";
                        multiply = 0.0001f;
                        break;
                    
                    case StatType.StageGoldEarn :
                        Level = data.GoldBonusRateLevel;
                        form = "P0";
                        multiply = 0.0001f;
                        break;
                    
                    case StatType.DashAttackBonus :
                        Level = data.DashAttackBonusLevel;
                        form = "P0";
                        multiply = 0.01f;
                        break;
                        
                    case StatType.DebuffMonsterAttack :
                        Level = data.DebuffMonsterAttackLevel;
                        form = "P1";
                        multiply = 0.001f;
                        break;
                    
                    case StatType.DebuffMonsterHp :
                        Level = data.DebuffMonsterHpLevel;
                        form = "P1";
                        multiply = 0.001f;
                        break;
                }
                SetValueByLevel();
            }

            private void SetValueByLevel()
            {
                value = Level.Value * meta.Value;
                // if (powerType != PowerType.None) Power.Value = Level.Value * DbPower.Get(powerType).Value;
            }

            public void LevelUp(int add)
            {
                Level.Value += add;
                SetValueByLevel();
                TotalStatController.I.Apply(meta.Id);
                
                QuestController.I.SetQuest(QuestType.CheckLevelPoint,LevelController.I.GetTotalLevelPointUse());
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
            }

            public int CanLevelUp(int count = 1, bool checkPoint = false)
            {
                if (Level.Value == meta.MaxLevel) return 0;
                if (count == -1) count = CurrencyController.I.GetEtcModel(CurrencyType.LevelPoint).Value;
                var canLevelUp = Math.Min(count, meta.MaxLevel - Level.Value);
                if (checkPoint && CurrencyController.I.GetEtcModel(CurrencyType.LevelPoint).Value < canLevelUp) return 0;
                return canLevelUp;
            }

            public int Reset()
            {
                var returnPoint = Level.Value;
                Level.Value = 0;
                SetValueByLevel();
                TotalStatController.I.Apply(meta.Id);
                return returnPoint;
            }

            public bool CanReset()
            {
                if (Level.Value == 0) return false;
                return true;
            }
        }
    }
}
