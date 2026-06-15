using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbPetInfo
{
    [Serializable]
    public class DbPetGrowthMaterial: DbModel<DbPetGrowthMaterial, int>
    {
        public List<int> Stones;
        
        public override void Load()
        {
            fileName = "PetGrowthMaterial";
            if (Application.isPlaying) Init();
        }


        public long Get(GradeType grade)
        {
            return Stones[grade - GradeType.Normal];
        }
    }
}