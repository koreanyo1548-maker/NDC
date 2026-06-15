using System;

namespace Data.Editor.EDbEvent
{
    [Serializable]
    public class EDbFriendReward
    {
        public int Id;
        public int NeedCount;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;
        
    }
}