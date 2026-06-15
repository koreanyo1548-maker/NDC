using System;
using System.Numerics;
using Newtonsoft.Json;

namespace Data.DbCommon
{
    [Serializable]
    public class DbRewardBig
    {
        public CurrencyType currencyType;
        public int id;
        public BigInteger count;

        [JsonConstructor]
        public DbRewardBig(CurrencyType currencyType, BigInteger count, int id = 0)
        {
            this.currencyType = currencyType;
            this.count = count;
            this.id = id;
        }

        public DbRewardBig(string reward)
        {
            var splits = reward.Split(":");
            if (splits.Length == 2)
            {
                currencyType = Enum.Parse<CurrencyType>(splits[0]);
                count = int.Parse(splits[1]);
            }
            else if (splits.Length == 3)
            {
                currencyType = Enum.Parse<CurrencyType>(splits[0]);
                count = int.Parse(splits[2]);
                id = int.Parse(splits[1]);
            }
        }
    }
}