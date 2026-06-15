using System;
using System.Collections.Generic;

namespace Data.Editor.EDbAbility
{
    [Serializable]
    public class EDbAbilityRune
    {
        public StatType Id;
        public List<int> Value;
        public int NameId;
        public string Resource;
    }
}