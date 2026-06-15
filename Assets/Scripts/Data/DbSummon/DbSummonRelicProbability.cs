using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbSummon
{
    [Serializable]
    public class DbSummonRelicProbability : DbModel<DbSummonRelicProbability, GradeType>
    {
        public int Probability;

        public override void Load()
        {
            fileName = "SummonRelicProbability";
            if (Application.isPlaying) Init();
        }

    }
}