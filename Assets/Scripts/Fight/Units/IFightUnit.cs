using System.Numerics;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Units
{
    public interface IFightUnit
    {
        public bool IsBoss();
        public Vector3 Position();
        public void Attacked(BigInteger attack, AttackType attackType = AttackType.Normal);
        public bool IsValid();
        public bool IsPlayerSide();
    }
}