using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbPower : DbModel<DbPower, PowerType>
    {
        public int Value;

        public override void Load()
        {
            fileName = "Power";
            if (Application.isPlaying) Init();
        }
    }
}