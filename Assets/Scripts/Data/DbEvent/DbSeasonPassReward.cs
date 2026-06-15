using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbEvent
{
    [Serializable]
    public class DbSeasonPassReward : DbModel<DbSeasonPassReward, int>
    {
        public int NeedPoint;
        public bool IsFree;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;
        
        
        public override void Load()
        {
            fileName = "SeasonPassReward";
            if (Application.isPlaying) Init();
        }
    }
}