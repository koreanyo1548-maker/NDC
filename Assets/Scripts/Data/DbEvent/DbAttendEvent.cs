using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbEvent
{
    [Serializable]
    public class DbAttendEvent : DbModel<DbAttendEvent, int>
    {
        public string StartDate;
        public DateTime StartDateCal;
        public int Duration;
        public List<int> RewardIds;

        public override void Load()
        {
            fileName = "AttendEvent";
            if (Application.isPlaying) Init();

            ForEach(p =>
            {
                var date = p.StartDate.Split(".");
                p.StartDateCal = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));
            });
        }
    }
}