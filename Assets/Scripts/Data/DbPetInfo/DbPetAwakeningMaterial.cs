using System;
using System.Collections.Generic;
using Data.DbEquipment;
using Data.Utils;
using UnityEngine;

namespace Data.DbPetInfo
{
    [Serializable]
    public class DbPetAwakeningMaterial: DbModel<DbPetAwakeningMaterial, int>, IDbAwakeningMaterial
    {
        public List<int> EquipmentCounts;
        public List<int> Stones;
        
        public override void Load()
        {
            fileName = "PetAwakeningMaterial";
            if (Application.isPlaying) Init();
        }

        public int GetStone(int awakening)
        {
            return Stones[awakening -1];
        }

        public int GetEquipment(int awakening)
        {
            return EquipmentCounts[awakening - 1];
        }
    }
}