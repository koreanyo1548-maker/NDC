using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbSoulPass
    {
        public int Id;
        public int NeedPoints;
        public CurrencyType FreeRewardType;
        public int FreeRewardValue;
        public int FreeRewardCounts;
        public CurrencyType PremiumRewardType;
        public int PremiumRewardValue;
        public int PremiumRewardCounts;
        public int NextId;
    }
}