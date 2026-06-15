using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbBlackMarket : DbModel<DbBlackMarket, int>, IDbShop
    {
        public string Resource;
        public RenewalType RenewalInterval;
        public int BuyLimit;
        public int Price;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;

        private List<DbReward> _rewards;
        public override void Load()
        {
            fileName = "BlackMarket";
            if (Application.isPlaying) Init();
            
            ForEach(s =>
            {
                s._rewards = new();
                s._rewards.Add(new (s.RewardType, s.RewardCount, s.RewardId));
            });
        }

        public static int GetMinCost()
        {
            var cost = int.MaxValue;
            ForEach(s =>
            {
                if (s.Price < cost) cost = s.Price;
            });
            return cost;
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
            throw new Exception();
        }

        public ShopCategoryType GetCategory()
        {
            return ShopCategoryType.Normal;
        }

        public string GetResource()
        {
            return Resource;
        }

        public string GetBackgroundResource()
        {
            throw new Exception();
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
            return CurrencyType.BlackMarketCoin;
        }

        public int GetPrice()
        {
            return Price;
        }

        public int GetIncreasePrice()
        {
            return 0;
        }

        public string GetDisplayPrice()
        {
            throw new Exception();
        }

        public int GetMileage()
        {
            throw new Exception();
        }

        public int GetDuration()
        {
            throw new Exception();
        }

        public DateTime GetStartTime()
        {
            throw new Exception();
        }

        public bool GetIsBest()
        {
            throw new Exception();
        }

        public bool GetIsNew()
        {
            throw new Exception();
        }

        public bool GetIsLimited()
        {
            throw new Exception();
        }

        public float GetValue()
        {
            throw new Exception();
        }

        public List<DbReward> GetRewards()
        {
            return _rewards;
        }
    }
}