using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using UnityEngine;

namespace Data.DbEvent
{
    [Serializable]
    public class DbAttendReward: DbModel<DbAttendReward, int>
    {
        public List<string> Rewards;
        public List<DbReward> Items;

        public override void Load()
        {
            fileName = "AttendReward";
            if (Application.isPlaying) Init();

            ForEach(i =>
            {
                i.Items = new List<DbReward>();
                foreach (var reward in i.Rewards)
                {
                    i.Items.Add(new DbReward(reward));
                }
            });
        }
    }
}