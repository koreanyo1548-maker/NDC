using System;
using System.Collections.Generic;

namespace Data.Editor.EDbRelicInfo
{
    [Serializable]
    public class EDbRelic
    {
        public int Id;
        public GradeType Grade;
        public int NameId;
        public StatType StatType;
        public string Resource;
    }
}