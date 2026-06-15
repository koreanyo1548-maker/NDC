using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbStat: DbModel<DbStat, StatType>
    {
        public int NameId;
        public int StaticNameId;
        public float ShowMultiply;
        public bool IsPercent;
        
        public override void Load()
        {
            fileName = "Stat";
            if (Application.isPlaying) Init();
        }
    }
}