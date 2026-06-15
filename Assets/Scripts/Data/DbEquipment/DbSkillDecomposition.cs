using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbSkillDecomposition: DbModel<DbSkillDecomposition, GradeType>
    {
        public int SkillGrowthStone;
        
        public override void Load()
        {
            fileName = "SkillDecomposition";
            if (Application.isPlaying) Init();
        }
    }
}