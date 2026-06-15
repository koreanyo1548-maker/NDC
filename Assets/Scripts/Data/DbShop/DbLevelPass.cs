using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbLevelPass: DbModel<DbLevelPass, int>, IDbPass
    {
        public CurrencyType PassType;
        public int NeedLevel;
        public CurrencyType FreeRewardType;
        public int FreeRewardValue;
        public int FreeRewardCounts;
        public CurrencyType PremiumRewardType;
        public int PremiumRewardValue;
        public int PremiumRewardCounts;
        public int NextId;

        public override void Load()
        {
            fileName = "LevelPass";
            if (Application.isPlaying) Init();
        }

        public static DbLevelPass GetFirstLarger(int level)
        {
            foreach (var meta in Meta)
            {
                if (meta.Value.NeedLevel <= level) continue;
                return meta.Value;
            }

            var emptyLarge = new DbLevelPass();
            var last = Meta[Meta.Count-1];
            emptyLarge.Id = last.Id + 1;
            return emptyLarge;
        }

        public static int GetFirstId(CurrencyType type)
        {
            foreach (var meta in Meta)
            {
                if (meta.Value.PassType == type) return meta.Value.Id;
            }

            return 0;
        }

        public PassType GetPassType()
        {
            return Data.PassType.LevelPass;
        }
        public int GetId()
        {
            return Id;
        }
        public CurrencyType GetSpecificPassType()
        {
            return PassType;
        }

        public int GetSpecificPassTypeIdx()
        {
            switch (PassType)
            {
                case CurrencyType.LevelPass1: return 0;
                case CurrencyType.LevelPass2: return 1;
                case CurrencyType.LevelPass3: return 2;
                case CurrencyType.LevelPass4: return 3;
                case CurrencyType.LevelPass5: return 4;
            }

            return 0;
        }

        public int GetGoal()
        {
            return NeedLevel;
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