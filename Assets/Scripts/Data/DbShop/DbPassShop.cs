using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Stores;
using Data.Utils;
using ThirdParty;
using UnityEngine;
using Utils;

namespace Data.DbShop
{
    [Serializable]
    public class DbPassShop : DbModel<DbPassShop, int>
    {
        public int NameId;
        public CurrencyType PassType;
        public string DisplayPrice;
        public string Resource;
        public List<string> Rewards;
        public List<DbReward> Reward;
        public int BuyLimit;
        public string ProductId;

        public PassCondition Condition;
        public int ResetDay;

        public int Mileage;

        public override void Load()
        {
            fileName = "PassShop";
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

        public int GetNextUpdateSeconds()
        {
            var cur = DateTime.UtcNow.AddHours(9);
            var endTime = UserInfo.saved.info.createdDate.Subtract(UserInfo.saved.info.createdDate.TimeOfDay).AddDays(GetCurrentSeason(cur) * ResetDay);
            var diff = endTime - cur;
            if (diff.Days > 0) return diff.Minutes * 60;
            return diff.Seconds;
        }
        
        public string GetLeftTime()
        {
            var cur = DateTime.UtcNow.AddHours(9);
            var endTime = UserInfo.saved.info.createdDate.Subtract(UserInfo.saved.info.createdDate.TimeOfDay).AddDays(GetCurrentSeason(cur) * ResetDay);
            var diff = endTime - cur;
            return StringMaker.GetTimeString(diff);
        }

        public bool CanAdd(DateTime now)
        {
            if (Condition == PassCondition.Open) return true;
            if (Condition == PassCondition.Closed) return false;
            if (Condition == PassCondition.CheckDate) return true;
            return false;
            // {
            //     if (!CheckStart()) return false;
            //     return CheckEnd();
            // }
            // return false; 
            //
            // bool CheckStart()
            // {
            //     if (StartDate.Equals("0")) return true;
            //     var date = StartDate.Split(".");
            //     if (date.Length != 3) return false;
            //     return now >= new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));
            // }
            //
            // bool CheckEnd()
            // {
            //     if (ResetDay.Equals("0")) return true;
            //     var date = ResetDay.Split(".");
            //     if (date.Length != 3) return false;
            //     return now <= new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));
            // }
        }

        public int GetCurrentSeason(DateTime now)
        {
            var dayDiff = Define.GetDayDiff(UserInfo.saved.info.createdDate, now);
            return dayDiff / ResetDay + 1;
        }
    }
}