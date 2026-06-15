using System.Collections.Generic;

namespace Data.DbEquipment
{
    public interface IDbEquipmentAwakening
    {
        public int GetLevel(int level);
        public StatType GetOption(int level);
        public int GetStat(int level);
        public int GetMaxAwakening();
        public string GetDescription();
    }
}