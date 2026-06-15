using System;
using System.Collections.Generic;
using Data.DbCommon;

namespace Data.DbShop
{
    public interface IDbShop
    {
        public string GetProductId();
        public int GetId();
        public bool IsInApp();
        public int GetNameId();
        public ShopCategoryType GetCategory();
        public string GetResource();
        public string GetBackgroundResource();
        public RenewalType GetRenewalInterval();
        public int GetBuyLimit();
        public CurrencyType GetPriceType();
        public int GetPrice();
        public int GetIncreasePrice();
        public string GetDisplayPrice();
        public int GetMileage();
        public int GetDuration();
        public DateTime GetStartTime();
        public bool GetIsBest();
        public bool GetIsNew();
        public bool GetIsLimited();
        public float GetValue();
        public List<DbReward> GetRewards();
    }
}