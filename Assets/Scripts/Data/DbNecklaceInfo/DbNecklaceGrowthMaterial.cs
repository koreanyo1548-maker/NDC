using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbNecklaceInfo
{
    [Serializable]
    public class DbNecklaceGrowthMaterial : DbModel<DbNecklaceGrowthMaterial, GradeType>
    {
        public List<int> Counts;
        public int MergeCount;
        
        public override void Load()
        {
            fileName = "NecklaceGrowthMaterial";
            if (Application.isPlaying) Init();
        }
    }
}