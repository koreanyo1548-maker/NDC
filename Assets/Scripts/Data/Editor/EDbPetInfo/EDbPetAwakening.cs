using System;
using System.Collections.Generic;

namespace Data.Editor.EDbPetInfo
{
    [Serializable]
    public class EDbPetAwakening
    {
        public int Id;
        public StatType Option;
        public string SkillOption;
        public List<int> Stats;
        public List<int> Levels;
        public int DescriptionId;
    }
}