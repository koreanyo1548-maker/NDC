using System;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbBlackMarket
    {
        public int Id;
        public string Resource;
        public RenewalType RenewalInterval;
        public int BuyLimit;
        public int Price;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;
    }
}