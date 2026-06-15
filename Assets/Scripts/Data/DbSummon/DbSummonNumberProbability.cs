using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbSummon
{
    [Serializable]
    public class DbSummonNumberProbability : DbModel<DbSummonNumberProbability, GradeType>
    {
        public List<int> Probability;

        public override void Load()
        {
            fileName = "SummonNumberProbability";
            if (Application.isPlaying) Init();
        }
    }
}