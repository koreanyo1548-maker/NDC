using System;

namespace Data.Editor.EDbDungeon
{
    [Serializable]
    public class EDbTrainingGroundReward
    {
        public int Id;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;
    }
}