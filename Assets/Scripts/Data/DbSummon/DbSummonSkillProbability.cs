using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbSummon
{
    [Serializable]
    public class DbSummonSkillProbability : DbModel<DbSummonSkillProbability, int>, IDbSummonProbability
    {
        public int Normal;
        public int Magic;
        public int Rare;
        public int Unique;
        public int Heroic;
        public int Legendary;
        public int Mythic;

        public override void Load()
        {
            fileName = "SummonSkillProbability";
            if (Application.isPlaying) Init();
        }

        public int GetPr(int idx)
        {
            switch (idx)
            {
                case 0: return Normal;
                case 1: return Magic; 
                case 2: return Rare;
                case 3: return Unique;
                case 4: return Heroic;
                case 5: return Legendary;
                case 6: return Mythic;
                default: return 0;
            }
        }

        public int GetPr(GradeType grade)
        {
            switch (grade)
            {
                case GradeType.Normal:
                    return Normal;
                case GradeType.Magic:
                    return Magic;
                case GradeType.Rare:
                    return Rare;
                case GradeType.Unique:
                    return Unique;
                case GradeType.Heroic:
                    return Heroic;
                case GradeType.Legendary:
                    return Legendary;
                case GradeType.Mythic:
                    return Mythic;
                default:
                    return 0;
            }
        }
    }
}