using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbNecklaceInfo
{
    [Serializable]
    public class DbNecklaceEquipStat : DbModel<DbNecklaceEquipStat, int>
    {
        public List<int> Stats;
        
        public override void Load()
        {
            fileName = "NecklaceEquipStat";
            if (Application.isPlaying) Init();
        }
    }
}