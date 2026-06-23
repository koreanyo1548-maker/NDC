using System.Numerics;
using Controller.Play;
using DG.Tweening;
using Fight.Units;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Logics.Actors
{
    public class TrainingMonsterActor: IActor
    {
        private Monster _monster;
        private static readonly int attacked = Animator.StringToHash("Attacked");
        // private static readonly int attackedVersion = Animator.StringToHash("AttackedVersion");
        private bool _attackedVersion;

        public void SetMonster(Monster monster)
        {
            _monster = monster;
            PlayController.I.damage.Value = 0;
        }

        public bool CanDie()
        {
            return false;
        }

        public void WithDamage(BigInteger damage)
        {
            // _monster.animator.SetTrigger(attacked);
            // _monster.animator.SetBool(attackedVersion, _attackedVersion);
            _attackedVersion = !_attackedVersion;
            PlayController.I.damage.Value += damage;
        }

        public void UpdateMoving()
        {
            if (_monster.target == null || !_monster.target.IsValid()) return;
            _monster.LookAt(_monster.target.Position().x > _monster.root.position.x);
        }

        public void UpdateIdle()
        {
            if (_monster.target == null || !_monster.target.IsValid()) return;
            _monster.LookAt(_monster.target.Position().x > _monster.root.position.x);
        }

        public void Pushed(float distance)
        {
            
        }

        public void Gather(Vector3 position)
        {
        }
    }
}