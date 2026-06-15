using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbCoupon : DbModel<DbCoupon, string>
    {
        public List<string> Rewards;
        public List<DbReward> Reward;

        public override void Load()
        {
            fileName = "Coupon";
            if (Application.isPlaying) Init();

            ForEach(s =>
            {
                s.Reward = new List<DbReward>();
                foreach (var reward in s.Rewards)
                {
                    s.Reward.Add(new DbReward(reward));
                }
            });
        }
    }
}