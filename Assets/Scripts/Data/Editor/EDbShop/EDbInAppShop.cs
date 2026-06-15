using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbInAppShop
    {
        public int Id;
        public int NameId;
        public ShopCategoryType Category;
        public string Resource;
        public string BackgroundResource;
        public List<string> Rewards;
        public RenewalType RenewalInterval;
        public int BuyLimit;
        public string ProductId;
        public int Mileage;
        public int UnlockStage;
        public PlatformType Platform;
        public string StartDate;
        public int Duration;
        public bool IsBest;
        public bool IsNew;
        public float Value;
        public bool IsLimited;
    }
}