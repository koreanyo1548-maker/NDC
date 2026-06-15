using System;

namespace Data.DbCommon
{
    [Serializable]
    public class DbReward
    {
        public CurrencyType currencyType;
        public int id;
        public long count;

        public DbReward(CurrencyType currencyType, long count, int id = 0)
        {
            this.currencyType = currencyType;
            this.count = count;
            this.id = id;
        }

        public DbReward(string reward)
        {
            var splits = reward.Split(":");
            if (splits.Length == 2)
            {
                currencyType = Enum.Parse<CurrencyType>(splits[0]);
                count = long.Parse(splits[1]);
            }
            else if (splits.Length == 3)
            {
                currencyType = Enum.Parse<CurrencyType>(splits[0]);
                count = long.Parse(splits[2]);
                id = int.Parse(splits[1]);
            }
        }
    }
}