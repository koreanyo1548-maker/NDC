using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbNecklaceInfo
{
    [Serializable]
    public class DbNecklaceAwakening : DbModel<DbNecklaceAwakening, int>
    {
        public List<int> Levels;
        public List<StatType> Options;
        public List<int> Stats;
        
        public override void Load()
        {
            fileName = "NecklaceAwakening";
            if (Application.isPlaying) Init();
        }

        public int GetLevel(int level)
        {
            return Levels[level];
        }

        public StatType GetOption(int level)
        {
            return Options[level];
        }

        public int GetStat(int level)
        {
            return Stats[level];
        }
    }
}