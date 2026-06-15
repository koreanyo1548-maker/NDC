using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbEvent
{
    [Serializable]
    public class DbSeasonPassQuest : DbModel<DbSeasonPassQuest, QuestType>
    {
        public int Goal;
        public int Point;
        public int NameId;

        public override void Load()
        {
            fileName = "SeasonPassQuest";
            if (Application.isPlaying) Init();
        }
    }
}