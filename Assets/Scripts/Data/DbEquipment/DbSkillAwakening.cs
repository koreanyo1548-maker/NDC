using System;
using System.Collections.Generic;
using System.Linq;
using Data.Utils;
using Exceptions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbSkillAwakening : DbModel<DbSkillAwakening, int>, IDbEquipmentAwakening
    {
        public List<int> Levels;
        public List<StatType> Options;
        public List<int> Stats;
        
        public override void Load()
        {
            fileName = "SkillAwakening";
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

        public int GetUpgradeValue(int awakening)
        {
            return Stats[awakening];
        }

        public int GetStat(int level)
        {
            return Stats[level];
        }

        public int GetMaxAwakening()
        {
            return 5;
        }
        public string GetDescription()
        {
            throw new NotDefinedValueException("Description of Skill Awakening");
        }
    }
}