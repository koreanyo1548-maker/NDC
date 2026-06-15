using System;

namespace Data.DbCommon
{
    public class DbProbability<T> where T: struct, Enum
    {
        public T category;
        public int probability;

        public DbProbability(string probability)
        {
            var splits = probability.Split(":");
            if (splits.Length == 2)
            {
                category = Enum.Parse<T>(splits[0]);
                this.probability = int.Parse(splits[1]);
            }
        }
    }
}