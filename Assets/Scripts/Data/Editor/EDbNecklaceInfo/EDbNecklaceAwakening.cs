using System;
using System.Collections.Generic;

namespace Data.Editor.EDbNecklaceInfo
{
    [Serializable]
    public class EDbNecklaceAwakening
    {
        public int Id;
        public List<int> Levels;
        public List<StatType> Options;
        public List<int> Stats;
        
    }
}