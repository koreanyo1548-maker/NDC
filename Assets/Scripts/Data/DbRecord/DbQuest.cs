using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbRecord
{
    [Serializable]
    public class DbQuest : DbModel<DbQuest, int>
    {
        public QuestCycleType Cycle;
        public int NameId;
        public int Goal;
        public CurrencyType Reward;
        public int RewardCount;
        public QuestType ToDo;

        public override void Load()
        {
            fileName = "Quest";
            if (Application.isPlaying) Init();
        }
    }
}