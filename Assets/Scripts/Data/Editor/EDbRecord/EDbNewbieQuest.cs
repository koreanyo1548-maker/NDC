using System;

namespace Data.Editor.EDbRecord
{
    [Serializable]
    public class EDbNewbieQuest
    {
        public int Id;
        public int Day;
        public int NameId;
        public int Goal;
        public CurrencyType RewardType;
        public int RewardId;
        public int RewardCount;
        public QuestType ToDo;
        public bool Continuous;
    }
}