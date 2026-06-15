using System;
using System.Numerics;
using Controller.Currency;
using Controller.Play;
using Data;
using Data.DbCharacter;
using Data.DbDefinition;
using Data.DbUser.Progress;
using Data.Utils;
using UnityEngine;
using Utils;

namespace Controller.Infos
{
    public class StatController: Singleton<StatController>
    {
        private static DbUserStat data = DbUserStat.Get(0);

        public static StatLevel attack = new(DbStat.Get(StatType.Attack));
        public static StatLevel hp = new(DbStat.Get(StatType.Hp));
        public static StatLevel criticalProbability = new(DbStat.Get(StatType.CriticalProbability));
        public static StatLevel criticalAttackBonus = new(DbStat.Get(StatType.CriticalAttackBonus));
        public static StatLevel attackBonus = new(DbStat.Get(StatType.AttackBonus));
        public static StatLevel hpBonus = new(DbStat.Get(StatType.HpBonus));
        public static StatLevel bossAttackBonus = new(DbStat.Get(StatType.BossAttackBonus));

        public StatLevel Get(StatType stat)
        {
            switch (stat)
            {
                case StatType.Attack:
                    return attack;
                case StatType.Hp:
                    return hp;
                case StatType.CriticalProbability:
                    return criticalProbability;
                case StatType.CriticalAttackBonus:
                    return criticalAttackBonus;
                case StatType.AttackBonus:
                    return attackBonus;
                case StatType.HpBonus:
                    return hpBonus;
                case StatType.BossAttackBonus:
                    return bossAttackBonus;
            }

            throw new Exception("존재하지 않는 스탯 타입");
        }

        // public long GetPower()
        // {
        //     return attack.Power.Value + hp.Power.Value + criticalProbability.Power.Value +
        //            criticalAttackBonus.Power.Value;
        // }
        
        public class StatLevel
        {
            public readonly DbStat statMeta;
            
            public long value;
            
            public readonly float multiply;
            public readonly string form;
            public DbField<int> Level { get; private set; }
            // public DbField<long> Power { get; private set; }
            public StatType StatType => statMeta.Id;
            private QuestType questType;
            
            public StatLevel(DbStat stat)
            {
                statMeta = stat;
                switch (statMeta.Id)
                {
                    case StatType.Attack :
                        Level = data.AttackLevel;
                        form = "N0";
                        multiply = 1.0f;
                        questType = QuestType.CheckAttackLevel;
                        break;
                    case StatType.Hp :
                        Level = data.HpLevel;
                        form = "N0";
                        multiply = 1.0f;
                        questType = QuestType.CheckHpLevel;
                        break;
                    
                    case StatType.CriticalProbability :
                        Level = data.CriticalProbabilityLevel;
                        form = "P1";
                        multiply = 0.001f;
                        questType = QuestType.CheckCriticalProbabilityLevel;
                        break;
                    
                    case StatType.CriticalAttackBonus :
                        Level = data.CriticalAttackBonusLevel;
                        form = "P1";
                        multiply = 0.001f;
                        questType = QuestType.CheckCriticalAttackBonusLevel;
                        break;
                    
                    case StatType.AttackBonus :
                        Level = data.AttackBonusLevel;
                        form = "P0";
                        multiply = 0.01f;
                        questType = QuestType.None;
                        break;
                    
                    case StatType.HpBonus :
                        Level = data.HpBonusLevel;
                        form = "P0";
                        multiply = 0.01f;
                        questType = QuestType.None;
                        break;
                    
                    case StatType.BossAttackBonus :
                        Level = data.BossAttackBonusLevel;
                        form = "P1";
                        multiply = 0.001f;
                        questType = QuestType.None;
                        break;
                    
                }

                SetValueByLevel();
            }

            private void SetValueByLevel()
            {
                value =  DbSelector.GetCharacterStat(statMeta.Id, Level.Value).GetValue();
            }

            private BigInteger GetCostForLevel(int level)
            {
                return DbSelector.GetCharacterStat(statMeta.Id, level).GetSpendCount();
            }
            
            public void LevelUp(int levelUpCount)
            {
                Level.Value += levelUpCount;
                SetValueByLevel();
                TotalStatController.I.Apply(statMeta.Id);
                if (questType != QuestType.None) QuestController.I.SetQuest(questType, Level.Value);
                QuestController.I.DoQuests(QuestType.GoldStatLevelUp, levelUpCount);
                BadgeController.I.OnStatUpdated(null, null);
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
            }

            public int CanLevelUp(int count = 1, bool checkGold = false)
            {
                var meta = DbGoldStat.Get(statMeta.Id);
                var maxLevel = meta.GetMaxLevel();
                
                // 최대 레벨이면 레벨업 불가능
                if (Level.Value == maxLevel) return 0;
                
                // 잠겨있으면 레벨업 불가능
                if (LevelController.I.CheckIsLocked(meta.GetCondition(), meta.GetGoal())) return 0;
                
                // MAX count를 해야할 경우
                if (count == -1) count = GetMaxLevelUpAffordable(maxLevel);
                
                // 최대 레벨에 도달하는것보다 항상 작거나 같아야함
                var canLevelUp = Mathf.Min(count, maxLevel - Level.Value);
                
                // 골드 체크해야하면 골드 부족하면 0으로
                if (checkGold && GetMaxLevelUpAffordable(Math.Min(Level.Value + count, maxLevel)) < canLevelUp) return 0;
                return canLevelUp;
                
                int GetMaxLevelUpAffordable(int toGo)
                {
                    BigInteger have = CurrencyController.I.GetMoneyModel(CurrencyType.Gold).Value;
                    var maxLevel = Level.Value;
                    for (var levelToGo = Level.Value+1; levelToGo <= toGo; ++levelToGo)
                    {
                        have -= GetCostForLevel(levelToGo);
                        if (have < 0) break;
                        maxLevel = levelToGo;
                    }

                    return maxLevel - Level.Value;
                }
            }

            public BigInteger GetNeedGoldForLevelUp(int levelUpCount)
            {
                var cost = new BigInteger(0);
                for (var level = Level.Value + 1; level <= Level.Value + levelUpCount; ++level)
                {
                    cost += GetCostForLevel(level);
                }

                return cost;
            }

            public float GetStatValueMultiplied(int level)
            {
                return DbSelector.GetCharacterStat(statMeta.Id, level).GetValue() * multiply;
            }
        }
    }
}