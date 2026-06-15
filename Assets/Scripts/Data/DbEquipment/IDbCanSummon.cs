namespace Data.DbEquipment
{
    public interface IDbCanSummon
    {
        public int GetId();
        public GradeType GetGrade();
        public string GetResource();
    }
}