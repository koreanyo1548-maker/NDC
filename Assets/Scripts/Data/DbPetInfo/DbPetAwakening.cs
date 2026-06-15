using System;
using System.Collections.Generic;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.Stores;
using Data.Utils;
using Exceptions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Data.DbPetInfo
{
    [Serializable]
    public class DbPetAwakening: DbModel<DbPetAwakening, int>, IDbEquipmentAwakening
    {
        public StatType Option;
        public PetSkill Skill;
        public List<int> Stats;
        public List<int> Levels;
        public int DescriptionId;
        public string SkillOption;
        
        public override void Load()
        {
            fileName = "PetAwakening";
            if (Application.isPlaying) Init(); 
            ForEach(s =>
            {
                if (s.Option == StatType.Skill) s.Skill = new PetSkill(s.SkillOption);
            });
        }

        public int GetLevel(int level)
        {
            return Levels[level];
        }

        public StatType GetOption(int level)
        {
            return Option;
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
            return string.Empty;
        }

        public string GetDescription(long value)
        {
            var stat = DbStat.Get(Option);
            if (stat != null) return StringMaker.GetFinalString(Option, value);
            return string.Format(LocalString.Get(DescriptionId), value * 0.1f);
        }
    }
    
    [Serializable]
    public class PetSkill
    {
        public SkillType type;
        public int coolTime;
        public int remainTime;
        
        public PetSkill(string str)
        {
            var strs = str.Split(":");
            try
            {
                type = (SkillType)Enum.Parse(typeof(SkillType), strs[0]);
                switch (type)
                {
                    case SkillType.Heal:
                        coolTime = int.Parse(strs[1]);
                        break;
                    case SkillType.AttackBuff:
                        break;
                    default:
                        throw new NotDefinedValueException($"{type} of PetAwakening");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }
    }

}
