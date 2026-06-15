using System;
using System.Collections.Generic;

namespace Data.Editor.EDbNecklaceInfo
{
    [Serializable]
    public class EDbNecklaceAwakeningMaterial
    {
        public GradeType Id;
        public List<int> Counts;
        public List<int> Stones;

    }
}