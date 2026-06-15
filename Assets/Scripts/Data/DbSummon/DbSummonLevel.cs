using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbSummon
{
    [Serializable]
    public class DbSummonLevel : DbModel<DbSummonLevel, int>
    {
        public int NeedExp;
        public int SkillNeedExp;
        public int NecklaceNeedExp;
        public int WeaponId;
        public long WeaponCount;
        public int AccessoryId;
        public long AccessoryCount;
        public int SkillId;
        public int SkillCount;

        public override void Load()
        {
            fileName = "SummonLevel";
            if (Application.isPlaying) Init();
        }
    }
}