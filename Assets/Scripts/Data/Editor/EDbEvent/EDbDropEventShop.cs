using System;
using System.Collections.Generic;

namespace Data.Editor.EDbEvent
{
    [Serializable]
    public class EDbDropEventShop
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