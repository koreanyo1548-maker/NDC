using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbStagePass: DbModel<DbStagePass, int>, IDbPass
    {
        public CurrencyType PassType;
        public int NeedStage;
        public CurrencyType FreeRewardType;
        public int FreeRewardValue;
        public int FreeRewardCounts;
        public CurrencyType PremiumRewardType;
        public int PremiumRewardValue;
        public int PremiumRewardCounts;
        public int NextId;

        public override void Load()
        {
            fileName = "StagePass";
            if (Application.isPlaying) Init();
        }

        public static DbStagePass GetFirstLarger(int stage)
        {
            foreach (var meta in Meta)
            {
                if (meta.Value.NeedStage <= stage) continue;
                return meta.Value;
            }

            return null;
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
            return Data.PassType.StagePass;
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
                case CurrencyType.StagePass1: return 0;
                case CurrencyType.StagePass2: return 1;
                case CurrencyType.StagePass3: return 2;
                case CurrencyType.StagePass4: return 3;
                case CurrencyType.StagePass5: return 4;
            }

            return 0;
        }

        public int GetGoal()
        {
            return NeedStage;
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