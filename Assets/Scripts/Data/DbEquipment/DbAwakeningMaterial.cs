using System;
using System.Collections.Generic;
using Data.Stores;
using Data.Utils;
using UnityEngine;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbAwakeningMaterial : DbModel<DbAwakeningMaterial, int>, IDbAwakeningMaterial
    {
        public List<int> EquipmentCounts;
        public List<int> Stones;
        
        public override void Load()
        {
            fileName = "AwakeningMaterial";
            if (Application.isPlaying) Init();
        }

        public int GetStone(int awakening)
        {
            return Stones[awakening - 1];
        }
        public int GetEquipment(int awakening)
        {
            return EquipmentCounts[awakening - 1];
        }
    }
}