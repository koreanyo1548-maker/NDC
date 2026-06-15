using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.DbRelicInfo
{
    [Serializable]
    public class DbRelicGrowthProbability: DbModel<DbRelicGrowthProbability, int>
    {
        public List<int> Probability;

        public override void Load()
        {
            fileName = "RelicGrowthProbability";
            if (Application.isPlaying) Init();
        }

        public int GetPr(int id)
        {
            return Probability[id - 1];
        }
    }
}