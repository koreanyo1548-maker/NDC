using System;
using System.Collections.Generic;
using Data.Utils;
using Exceptions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbAccessoryAwakening : DbModel<DbAccessoryAwakening, int>, IDbEquipmentAwakening
    {
        public List<int> Levels;
        public List<StatType> Options;
        public List<int> Stats;

        public override void Load()
        {
            fileName = "AccessoryAwakening";
            if (Application.isPlaying) Init();
        }

        public int GetLevel(int level)
        {
            return Levels[level];
        }

        public StatType GetOption(int level)
        {
            return Options[level];
        }

        public int GetStat(int level)
        {
            return Stats[level];
        }

        public int GetMaxAwakening()
        {
            return 7;
        }
        
        public string GetDescription()
        {
            throw new NotDefinedValueException("Description of Accessory Awakening");
        }
    }
}