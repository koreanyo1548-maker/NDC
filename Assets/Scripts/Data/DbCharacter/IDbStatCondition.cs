namespace Data.DbCharacter
{
    public interface IDbStatCondition
    {
        public StatConditionType GetCondition();
        public int GetGoal();
        public int GetDescriptionId();
    }
}