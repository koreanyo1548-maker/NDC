using System;
using System.Collections.Generic;
 
using Data.Utils;
using UnityEngine;
using Utils;
using static Utils.LocalString;

namespace Data.DbRecord
{
    [Serializable]
    public class DbTitle: DbModel<DbTitle, int>
    {
        public QuestType ToDo;
        public List<int> Goal;
        public bool IsSecret;
        public StatType Option;
        public int Value;
        public int MaxLevel;
        public int NameId;
        public int GoalId;
        public string ColorCode;

        public override void Load()
        {
            fileName = "Title";
            if (Application.isPlaying) Init();
        }

        public string GetNameWithColor()
        {
            return $"<color={ColorCode}>{LocalString.Get(NameId)}</color>";
        }
    }
}