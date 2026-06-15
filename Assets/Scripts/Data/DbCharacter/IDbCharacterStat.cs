using System.Numerics;

namespace Data.DbCharacter
{
    public interface IDbCharacterStat
    {
        public long GetValue();
        public BigInteger GetSpendCount();
        public int GetMaxLevel();
    }
}