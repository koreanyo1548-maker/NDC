namespace Data.DbShop
{
    public interface IDbPass
    {
        public int GetId();
        public PassType GetPassType();
        public CurrencyType GetSpecificPassType();
        public int GetSpecificPassTypeIdx();
        public int GetGoal();
        public CurrencyType GetFreeRewardType();
        public int GetFreeRewardValue();
        public int GetFreeRewardCounts();
        public CurrencyType GetPremiumRewardType();
        public int GetPremiumRewardValue();
        public int GetPremiumRewardCounts();
        public int GetNextId();
    }
}