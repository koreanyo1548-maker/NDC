using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using UnityEngine;
using Utils;

namespace Data.DbShop
{
    [Serializable]
    public class DbInGameShop : DbModel<DbInGameShop, int>, IDbShop
    {
        public int NameId;
        public ShopCategoryType Category;
        public string Resource;
        public List<string> Rewards;
        public RenewalType RenewalInterval;
        public int BuyLimit;
        public CurrencyType PriceType;
        public int Price;
        public int IncreasePrice;
        public string StartDate;
        public DateTime StartDateCal;
        public int Duration;
        public List<DbReward> Reward;

        public override void Load()
        {
            fileName = "InGameShop";
            if (Application.isPlaying) Init();
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
            });
        }

        public string GetProductId()
        {
            throw new Exception();
        }

        public int GetId()
        {
            return Id;
        }

        public bool IsInApp()
        {
            return false;
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
            return string.Empty;
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
            return PriceType;
        }

        public int GetPrice()
        {
            return Price;
        }

        public int GetIncreasePrice()
        {
            return IncreasePrice;
        }

        public string GetDisplayPrice()
        {
            return string.Empty;
        }

        public int GetMileage()
        {
            return 0;
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
            return false;
        }

        public bool GetIsNew()
        {
            return false;
        }

        public bool GetIsLimited()
        {
            return false;
        }

        public float GetValue()
        {
            return 0;
        }

        public List<DbReward> GetRewards()
        {
            return Reward;
        }
    }
}