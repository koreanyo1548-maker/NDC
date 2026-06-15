using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbRecord
{
    [Serializable]
    public class EDbQuest
    {
        public int Id;
        public QuestCycleType Cycle;
        public int NameId;
        public int Goal;
        public CurrencyType Reward;
        public int RewardCount;
        public QuestType ToDo;
    }
}