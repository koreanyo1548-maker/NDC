namespace Data.DbEquipment
{
    public interface IDbEquipment: IHaveGrade
    {
        public EquipmentType GetEquipmentType();
        public int GetId();
        public int GetNameId();
        public FullGradeType GetFullGrade();
        public int GetEquipStat();
        public StatType GetEquipStatType();
        public int GetEquipGrowthStat();
        public StatType GetOwnStatType();
        public int GetOwnStat();
        public int GetOwnGrowthStat();
        public string GetResource();
        public int GetCount();
        public int GetPrev();
        public int GetNext();

        public IDbAwakeningMaterial GetAwakeningMaterial();
        public IDbEquipmentAwakening GetAwakening();
        public string GetOwnDescription(int level, bool isIncreasing);
    }
}