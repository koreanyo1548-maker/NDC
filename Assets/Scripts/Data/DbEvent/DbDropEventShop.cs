using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using UnityEngine;

namespace Data.DbEvent
{
    [Serializable]
    public class DbDropEventShop : DbModel<DbDropEventShop, int>
    {
        public string Resource;
        public RenewalType RenewalInterval;
        public int BuyLimit;
        public int Price;
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;

        public override void Load()
        {
            fileName = "DropEventShop";
            if (Application.isPlaying) Init();
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
    }
}