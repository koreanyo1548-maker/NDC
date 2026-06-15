using System.Numerics;
using Fight.Units;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Logics.Actors
{
    public interface IActor
    {
        public void SetMonster(Monster monster);
        public bool CanDie();
        public void WithDamage(BigInteger damage);
        public void UpdateMoving();
        public void UpdateIdle();
        public void Gather(Vector3 gatherPosition);
        public void Pushed(float distance);
    }
}