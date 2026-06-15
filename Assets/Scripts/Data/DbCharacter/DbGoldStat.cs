using System;
using System.Numerics;
using Data.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.DbCharacter
{
    [Serializable]
    public class DbGoldStat: DbModel<DbGoldStat, StatType>, IDbStatCondition
    {
        public StatConditionType Condition;
        public int Goal;
        public int Index;
        public int DescriptionId;
        
        public override string ToString()
        {
            return $"{Index}, {Goal}, {Condition}, {DescriptionId}";
        }
        public override void Load()
        {
            fileName = "GoldStat";
            if (Application.isPlaying) Init();
        }

        public int GetMaxLevel()
        {
            switch (Id)
            {
                case StatType.Attack: return DbAttackLevel.Count;
                case StatType.Hp: return DbHpLevel.Count;
                case StatType.CriticalProbability: return DbCriticalProbabilityLevel.Count;
                case StatType.CriticalAttackBonus: return DbCriticalAttackBonusLevel.Count;
                case StatType.AttackBonus: return DbAttackBonusLevel.Count -1;
                case StatType.HpBonus: return DbHpBonusLevel.Count -1;
                case StatType.BossAttackBonus: return DbBossAttackBonusLevel.Count -1;
            }

            throw new Exception($"{Id} is not defined as gold upgrading stat");
        }

        public StatConditionType GetCondition()
        {
            return Condition;
        }

        public int GetGoal()
        {
            return Goal;
        }

        public int GetDescriptionId()
        {
            return DescriptionId;
        }
    }
}