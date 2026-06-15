using System;
using Controller;
using Controller.Currency;
using Controller.Utils;
using Data.DbShop;
using Data.Utils;
using ThirdParty;
using Utils;

namespace Data.DbUser.Currency
{
    [Serializable]
    public class DbUserPass
    {
        public PassType PassType;
        public int LastFreeRewarded;
        public int[] LastPremiumRewarded;
        public int Progress;
        public ControllerField<int> Season;

        public ControllerField<bool>[] CanGetNormal;
        public ControllerField<bool>[] CanGetPremium;

        public DbUserPass(PassType passType, int lastFreeRewarded, int[] lastPremiumRewarded, int progress, int season)
        {
            PassType = passType;
            LastFreeRewarded = lastFreeRewarded;
            LastPremiumRewarded = lastPremiumRewarded;
            Progress = progress;
            Season = new ControllerField<int>(season);
            
            var count = Define.passToCurrency[passType].Count;
            CanGetNormal = new ControllerField<bool>[count];
            CanGetPremium = new ControllerField<bool>[count];
            for (var idx = 0; idx < count; ++idx)
            {
                CanGetNormal[idx] = new ControllerField<bool>(true);
                CanGetPremium[idx] = new ControllerField<bool>(true);
            }
        }

        public void Reset(int season)
        {
            LastFreeRewarded = -1;
            LastPremiumRewarded = new[]{-1, -1, -1, -1, -1};
            Progress = 0;
            Season.Value = season;
            SetGetCondition();
            PlayFabManager.Store.ForceSave();
        }

        public void SetGetCondition()
        {
            var count = Define.passToCurrency[PassType].Count;
            var nextNormalMeta = DbSelector.GetPass(PassType, LastFreeRewarded + 1);
            for (var idx = 0; idx < count; ++idx)
            {
                var nextPremiumMeta = DbSelector.GetPass(PassType, LastPremiumRewarded[idx]+1);
                var specific = Define.passToCurrency[PassType][idx];
                if (nextPremiumMeta != null && nextPremiumMeta.GetSpecificPassType() < specific)
                {
                    nextPremiumMeta = DbSelector.GetPass(PassType, DbSelector.GetFirstId(PassType, specific));
                }
                CanGetNormal[idx].Value = nextNormalMeta != null && Progress >= nextNormalMeta.GetGoal() && nextNormalMeta.GetSpecificPassType() == specific;
                CanGetPremium[idx].Value = nextPremiumMeta != null
                                           && CurrencyController.I.Have(Define.passToCurrency[PassType][idx])
                                           && Progress >= nextPremiumMeta.GetGoal()
                                           && nextPremiumMeta.GetSpecificPassType() == specific;
            }
        }
    }
}
