using System.Numerics;
namespace Fight.Stats
{
    public abstract class Stat: HpStat
    {
        public BigInteger Attack { get; protected set; }
        protected float _attackRange;

        public float SqrAttackRange { get; protected set; }

    }
}