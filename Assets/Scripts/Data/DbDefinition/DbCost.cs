using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbCost : DbModel<DbCost, CostType>
    {
        public int Cost;

        public override void Load()
        {
            fileName = "Cost";
            if (Application.isPlaying) Init();
        }
    }
}