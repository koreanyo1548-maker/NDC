using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbStagePass
    {
        public int Id;
        public CurrencyType PassType;
        public int NeedStage;
        public CurrencyType FreeRewardType;
        public int FreeRewardValue;
        public int FreeRewardCounts;
        public CurrencyType PremiumRewardType;
        public int PremiumRewardValue;
        public int PremiumRewardCounts;
        public int NextId;
    }
}