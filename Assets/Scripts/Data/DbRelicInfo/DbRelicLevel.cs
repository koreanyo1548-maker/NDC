using System;
using System.Collections.Generic;
using Data.DbAbility;
using Data.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.DbRelicInfo
{
    [Serializable]
    public class DbRelicLevel: DbModel<DbRelicLevel, int>
    {
        public List<int> Stat;
        
        public override void Load()
        {
            fileName = "RelicLevel";
            if (Application.isPlaying) Init();
        }

        public int GetStat(int id)
        {
            return Stat[id - 1];
        }
    }
}