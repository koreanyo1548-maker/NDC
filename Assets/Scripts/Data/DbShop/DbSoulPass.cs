using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbSoulPass: DbModel<DbSoulPass, int>, IDbPass
    {
        public int NeedPoints;
        public CurrencyType FreeRewardType;
        public int FreeRewardValue;
        public int FreeRewardCounts;
        public CurrencyType PremiumRewardType;
        public int PremiumRewardValue;
        public int PremiumRewardCounts;
        public int NextId;

        public override void Load()
        {
            fileName = "SoulPass";
            if (Application.isPlaying) Init();
        }

        public static DbSoulPass GetFirstLarger(int stage)
        {
            foreach (var meta in Meta)
            {
                if (meta.Value.NeedPoints <= stage) continue;
                return meta.Value;
            }

            return null;
        }


        public PassType GetPassType()
        {
            return PassType.SoulPass;
        }
        public int GetId()
        {
            return Id;
        }

        public CurrencyType GetSpecificPassType()
        {
            return CurrencyType.SoulPass;
        }

        public int GetSpecificPassTypeIdx()
        {
            return 0;
        }

        public int GetGoal()
        {
            return NeedPoints;
        }

        public CurrencyType GetFreeRewardType()
        {
            return FreeRewardType;
        }

        public int GetFreeRewardValue()
        {
            return FreeRewardValue;
        }

        public int GetFreeRewardCounts()
        {
            return FreeRewardCounts;
        }

        public CurrencyType GetPremiumRewardType()
        {
            return PremiumRewardType;
        }

        public int GetPremiumRewardValue()
        {
            return PremiumRewardValue;
        }

        public int GetPremiumRewardCounts()
        {
            return PremiumRewardCounts;
        }

        public int GetNextId()
        {
            return NextId;
        }
    }
}