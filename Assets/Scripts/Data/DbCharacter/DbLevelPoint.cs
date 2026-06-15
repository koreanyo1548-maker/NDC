using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbCharacter
{
    [Serializable]
    public class DbLevelPoint: DbModel<DbLevelPoint, StatType>, IDbStatCondition
    {
        public int Value;
        public int MaxLevel;
        public StatConditionType Condition;
        public int Goal;
        public int DescriptionId;
        
        public override void Load()
        {
            fileName = "LevelPoint";
            if (Application.isPlaying) Init();
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