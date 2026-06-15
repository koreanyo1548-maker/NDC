using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using Exceptions;
using ThirdParty;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbInAppShop : DbModel<DbInAppShop, int>, IDbShop
    {
        public int NameId;
        public ShopCategoryType Category;
        public string Resource;
        public string BackgroundResource;
        public List<string> Rewards;
        public RenewalType RenewalInterval;
        public int BuyLimit;
        public string ProductId;
        public string DisplayPrice;
        public int Mileage;
        public int UnlockStage;
        public PlatformType Platform;
        public string StartDate;
        public DateTime StartDateCal;
        public int Duration;
        public bool IsBest;
        public bool IsNew;
        public float Value;
        public bool IsLimited;
        public List<DbReward> Reward;
        
        public override void Load()
        {
            fileName = "InAppShop";
            if (Application.isPlaying) Init();
            var removed = new List<int>();
            ForEach(s =>
            {
                s.Reward = new List<DbReward>();
                foreach (var reward in s.Rewards)
                {
                    s.Reward.Add(new DbReward(reward));
                }
                if (s.Duration > 0)
                {
                    var date = s.StartDate.Split(".");
                    s.StartDateCal = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));
                }
                #if ONESTORE
                if (s.Platform is not PlatformType.OneStore and not PlatformType.All)
                {
                    removed.Add(s.Id);
                }
                #elif APPSTORE
                if (s.Platform is not PlatformType.AppStore and not PlatformType.All and not PlatformType.AppStoreReview)
                {
                    removed.Add(s.Id);
                }
                #elif GOOGLEPLAY
                if (s.Platform is not PlatformType.GooglePlay and not PlatformType.All)
                {
                    removed.Add(s.Id);
                }
                #endif
            });
            for (var idx = 0; idx < removed.Count; ++idx) Meta.Remove(removed[idx]);
        }

        public static void Remove(List<int> toRemove)
        {
            for (var idx = 0; idx < toRemove.Count; ++idx) Meta.Remove(toRemove[idx]);
        }

        public string GetProductId()
        {
            return ProductId;
        }

        public int GetId()
        {
            return Id;
        }

        public bool IsInApp()
        {
            return true;
        }

        public int GetNameId()
        {
            return NameId;
        }

        public ShopCategoryType GetCategory()
        {
            return Category;
        }

        public string GetResource()
        {
            return Resource;
        }

        public string GetBackgroundResource()
        {
            return BackgroundResource;
        }

        public RenewalType GetRenewalInterval()
        {
            return RenewalInterval;
        }

        public int GetBuyLimit()
        {
            return BuyLimit;
        }

        public CurrencyType GetPriceType()
        {
            throw new NotDefinedValueException("PriceType of InAppShop");
        }

        public int GetPrice()
        {
            return 0;
        }

        public int GetIncreasePrice()
        {
            return 0;
        }

        public string GetDisplayPrice()
        {
            return DisplayPrice;
        }

        public int GetMileage()
        {
            return Mileage;
        }

        public int GetDuration()
        {
            return Duration;
        }

        public DateTime GetStartTime()
        {
            return StartDateCal;
        }

        public bool GetIsBest()
        {
            return IsBest;
        }

        public bool GetIsNew()
        {
            return IsNew;
        }

        public bool GetIsLimited()
        {
            return IsLimited;
        }

        public float GetValue()
        {
            return Value;
        }

        public List<DbReward> GetRewards()
        {
            return Reward;
        }
    }
}