using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbRecord
{
    [Serializable]
    public class EDbMainQuest
    {
        public int Id;
        public QuestType ToDo;
        public int NameId;
        public int Goal;
        public int IncreasingGoal;
        public bool HaveEnd;
        public CurrencyType RewardType;
        public int RewardCounts;
        public List<string> GoTo;
    }
}