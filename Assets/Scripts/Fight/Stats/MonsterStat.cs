using System.Numerics;
using Controller;
using Controller.Play;
using Data;
using Data.DbStage;
using Managers;
using UnityEngine;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Stats
{
    public class MonsterStat: Stat
    {
        private float _sightRange = 3;
        public float SqrSightRange { get; private set; }

        private DbMonster _monster;

        public MonsterStat(MonoBehaviour owner)
        {
            hpBar = Util.FindChild(owner.transform.parent.gameObject, "Hp", true).transform;
            hpBarParent = hpBar.parent;
            _hpRight = hpBarParent.localScale;
            _hpLeft = new Vector3(-_hpRight.x, _hpRight.y);
            SqrSightRange = _sightRange * _sightRange;
        }

        public void SetMonster(DbMonster monster, bool isBoss)
        {
            _monster = monster;
            _isBoss = isBoss;
            SetStat();
        }

        private void SetStat()
        {
            Attack = Manager.Field.StageMeta.GetMonsterAttack() * (1000 - TotalStatController.I.GetStat(StatType.DebuffMonsterAttack)) / 1000;
            ApplyMaxHp(Manager.Field.StageMeta.GetMonsterHp() * (1000 - TotalStatController.I.GetStat(StatType.DebuffMonsterHp)) / 1000, true);
            
            _attackRange = _monster.AttackRange;
            SqrAttackRange = _attackRange * _attackRange;
        }

        public void OnEnable()
        {
            if (_monster != null)
            {
                SetStat();
            }
        }
    }
}