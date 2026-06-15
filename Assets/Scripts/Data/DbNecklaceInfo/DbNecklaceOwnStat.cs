using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbNecklaceInfo
{
    [Serializable]
    public class DbNecklaceOwnStat : DbModel<DbNecklaceOwnStat, int>
    {
        public List<int> Stats;
        
        public override void Load()
        {
            fileName = "NecklaceOwnStat";
            if (Application.isPlaying) Init();
        }
    }
}