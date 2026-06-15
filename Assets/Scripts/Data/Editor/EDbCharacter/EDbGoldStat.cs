using System;

namespace Data.Editor.EDbCharacter
{
    [Serializable]
    public class EDbGoldStat
    {
        public StatType Id;
        public StatConditionType Condition;
        public int Goal;
        public int Index;
        public int DescriptionId;
    }
}