using System;

namespace Data.Editor.EDbEvent
{
    [Serializable]
    public class EDbSeasonPassReward
    {
        public int Id;
        public int NeedPoint;
        public bool IsFree;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;
    }
}