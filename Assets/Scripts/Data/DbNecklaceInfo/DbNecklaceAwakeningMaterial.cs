using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbNecklaceInfo
{
    [Serializable]
    public class DbNecklaceAwakeningMaterial : DbModel<DbNecklaceAwakeningMaterial, GradeType>
    {
        public List<int> Counts;
        public List<int> Stones;
        
        public override void Load()
        {
            fileName = "NecklaceAwakeningMaterial";
            if (Application.isPlaying) Init();
        }
        
        public int GetStone(int awakening)
        {
            return Stones[awakening - 1];
        }
        public int GetEquipment(int awakening)
        {
            return Counts[awakening - 1];
        }
    }
}