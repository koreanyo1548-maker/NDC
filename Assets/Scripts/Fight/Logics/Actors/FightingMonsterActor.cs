using System.Numerics;
using Cameras;
using DG.Tweening;
using Fight.Units;
using Managers;
using Managers.Base;
using MEC;
using UnityEngine;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Logics.Actors
{
    public class FightingMonsterActor: IActor
    {
        private Monster _monster;
        

        public void SetMonster(Monster monster)
        {
            _monster = monster;
        }
        
        public bool CanDie()
        {
            return true;
        }

        public void WithDamage(BigInteger damage)
        {
            
        }
        
        public void UpdateMoving()
        {
            var dir = _monster.target.Position() - _monster.root.position;

            var sqrDistance = dir.sqrMagnitude;
            if (sqrDistance < _monster.stat.SqrAttackRange)
            {
                _monster.LookAt(dir.x > 0);
                _monster.Attack();
            }
            else
            {
                if (sqrDistance < 0.0001f)
                {
                    _monster.Idle();
                }
                else
                {
                    var moveDist = Mathf.Clamp(_monster.speed * Time.deltaTime, 0, dir.magnitude);
                    _monster.root.position += dir.normalized * moveDist;
                }
                
                _monster.LookAt(dir.x > 0);
            }
        }

        public void UpdateIdle()
        {
            if (_monster.id == 0) Debug.Log(".");
            _monster.Move();
        }
        
        public void Gather(Vector3 gatherPosition)
        {
            gatherPosition.x += Random.Range(-0.5f, 0.5f);
            gatherPosition.y += Random.Range(-0.75f, 0f);
            var diff = gatherPosition - _monster.Position();
            var time = 2.2f * diff.magnitude;
            _monster.root.DOMove(_monster.root.position + diff, time).SetEase(Ease.OutCubic);
            Timing.CallDelayed(2.2f, () =>
            {   
                if (!_monster.IsDead)
                {
                    _monster.root.DOMove(new Vector3(_monster.root.position.x + Random.Range(-2f, 2f), 
                            _monster.root.position.y + Random.Range(-2f, 2f), 0),
                        0.2f);
                }
            });
        }
        
        public void Pushed(float distance)
        {
            var playerPosition = Manager.Player.Position();
            var diff = new Vector3(_monster.Position().x - playerPosition.x, _monster.Position().y - playerPosition.y, 0);
            diff.Normalize();
            diff *= distance;
            diff = Manager.Field.GetInnerDiff(diff, _monster.Position());
            _monster.root.DOMove(_monster.root.position + diff, 0.5f);
        }
    }
}