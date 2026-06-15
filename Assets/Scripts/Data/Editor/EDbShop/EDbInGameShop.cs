using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbInGameShop
    {
        public int Id;
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
        public int Duration;
    }
}