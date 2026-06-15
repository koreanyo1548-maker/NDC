using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbSummon
{
    [Serializable]
    public class EDbSummonLevel
    {
        public int Id;
        public int NeedExp;
        public int SkillNeedExp;
        public int NecklaceNeedExp;
        public int WeaponId;
        public long WeaponCount;
        public int AccessoryId;
        public long AccessoryCount;
        public int SkillId;
        public int SkillCount;
    }
}