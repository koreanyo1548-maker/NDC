using System;

namespace Data.Editor.EDbCharacter
{
    [Serializable]
    public class EDbLevelPoint
    {
        public StatType Id;
        public int Value;
        public int MaxLevel;
        public StatConditionType Condition;
        public int Goal;
        public int DescriptionId;
    }
}