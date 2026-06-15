using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbEvent
{
    [Serializable]
    public class DbFriendReward : DbModel<DbFriendReward, int>
    {
        public int NeedCount;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;
        
        public override void Load()
        {
            fileName = "FriendReward";
            if (Application.isPlaying) Init();
        }
    }
}