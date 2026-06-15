using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbGrowthMaterial : DbModel<DbGrowthMaterial, int>
    {
        public List<long> Stones;

        public override void Load()
        {
            fileName = "GrowthMaterial";
            if (Application.isPlaying) Init();
        }

        public long Get(FullGradeType grade)
        {
            return Stones[grade - FullGradeType.Normal1];
        }
    }
}