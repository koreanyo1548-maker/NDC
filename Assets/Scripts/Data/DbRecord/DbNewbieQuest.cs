using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbRecord
{
    [Serializable]
    public class DbNewbieQuest : DbModel<DbNewbieQuest, int>
    {
        public int Day;
        public int Goal;
        public CurrencyType RewardType;
        public int RewardId;
        public int RewardCount;
        public QuestType ToDo;
        public int NameId;
        public bool Continuous;

        
        public override void Load()
        {
            fileName = "NewbieQuest";
            if (Application.isPlaying) Init();
        }
    }
}