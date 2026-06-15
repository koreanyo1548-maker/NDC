using System;
using Data.Utils;
using UnityEngine;
using Utils;

namespace Data.DbEvent
{
    [Serializable]
    public class DbDropEvent : DbModel<DbDropEvent, int>
    {
        public string StartDate;
        public DateTime StartDateCal;
        public int DropDuration;
        public int ShopDuration;

        public override void Load()
        {
            fileName = "DropEvent";
            if (Application.isPlaying) Init();

            ForEach(p =>
            {
                var date = p.StartDate.Split(".");
                // p.StartDateCal = DateTime.Now.Subtract(DateTime.Now.TimeOfDay);
                p.StartDateCal = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));
            });
        }
        
        public string GetLeftTime()
        {
            var cur = DateTime.UtcNow.AddHours(9);
            var endTime = StartDateCal.AddDays(ShopDuration);
            var diff = endTime - cur;
            return StringMaker.GetTimeString(diff);
        }
        
        public int GetNextUpdateSeconds()
        {
            var cur = DateTime.UtcNow.AddHours(9);
            var endTime = StartDateCal.AddDays(ShopDuration);
            var diff = endTime - cur;
            if (diff.Days > 0) return diff.Minutes * 60;
            return diff.Seconds;
        }
    }
}