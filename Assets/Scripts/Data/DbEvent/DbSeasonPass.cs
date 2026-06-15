using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;
using Utils;

namespace Data.DbEvent
{
    [Serializable]
    public class DbSeasonPass : DbModel<DbSeasonPass, int>
    {
        public string StartDate;
        public DateTime StartDateCal;
        public int Duration;
        public int ShopId;
        public int NameId;
        
        public override void Load()
        {
            fileName = "SeasonPass";
            if (Application.isPlaying) Init();

            ForEach(p =>
            {
                var date = p.StartDate.Split(".");
                //p.StartDateCal = DateTime.Now;
                p.StartDateCal = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));
            });
        }
        
        public string GetLeftTime()
        {
            var cur = DateTime.UtcNow.AddHours(9);
            var endTime = StartDateCal.AddDays(Duration);
            var diff = endTime - cur;
            return StringMaker.GetTimeString(diff);
        }
        
        public int GetNextUpdateSeconds()
        {
            var cur = DateTime.UtcNow.AddHours(9);
            var endTime = StartDateCal.AddDays(Duration);
            var diff = endTime - cur;
            if (diff.Days > 0) return diff.Minutes * 60;
            return diff.Seconds;
        }
    }
}