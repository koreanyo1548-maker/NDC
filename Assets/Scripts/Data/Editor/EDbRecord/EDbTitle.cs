using System;
using System.Collections.Generic;

namespace Data.Editor.EDbRecord
{
    [Serializable]
    public class EDbTitle
    {
        public int Id;
        public QuestType ToDo;
        public List<int> Goal;
        public bool IsSecret;
        public StatType Option;
        public int Value;
        public int MaxLevel;
        public int NameId;
        public int GoalId;
        public string ColorCode;
    }
}