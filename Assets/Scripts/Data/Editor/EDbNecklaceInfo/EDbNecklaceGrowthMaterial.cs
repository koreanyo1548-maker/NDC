using System;
using System.Collections.Generic;

namespace Data.Editor.EDbNecklaceInfo
{
    [Serializable]
    public class EDbNecklaceGrowthMaterial
    {
        public GradeType Id;
        public List<int> Counts;
        public int MergeCount;

    }
}