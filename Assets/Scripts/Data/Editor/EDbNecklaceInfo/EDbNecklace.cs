using System;

namespace Data.Editor.EDbNecklaceInfo
{
    [Serializable]
    public class EDbNecklace
    {
        public int Id;
        public int NameId;
        public GradeType Grade;
        public StatType EquipStat;
        public string Resource;
        public int PrevId;
        public int NextId;
        public int EquipIdx;
        public int OwnIdx;
    }
}