using System;
using System.Collections.Generic;

namespace Data.Editor.EDbEquipment
{
    [Serializable]
    public class EDbAccessoryAwakening
    {
        public int Id;
        public List<int> Levels;
        public List<StatType> Options;
        public List<int> Stats;
    }
}