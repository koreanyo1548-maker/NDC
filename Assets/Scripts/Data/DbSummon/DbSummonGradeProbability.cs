using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbSummon
{
    [Serializable]
    public class DbSummonGradeProbability : DbModel<DbSummonGradeProbability, int>, IDbSummonProbability
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
            fileName = "SummonGradeProbability";
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

        public int GetPr(FullGradeType grade)
        {
            switch (grade)
            {
                case FullGradeType.Normal1:
                    return Normal;
                case FullGradeType.Magic1:
                    return Magic;
                case FullGradeType.Rare1:
                    return Rare;
                case FullGradeType.Unique1:
                    return Unique;
                case FullGradeType.Heroic1:
                    return Heroic;
                case FullGradeType.Legendary1:
                    return Legendary;
                case FullGradeType.Mythic1:
                    return Mythic;
                default:
                    return 0;
            }
        }
    }
}