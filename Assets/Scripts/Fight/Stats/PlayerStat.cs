using Data;
using Managers;
using UnityEngine;
using Utils;

namespace Fight.Stats
{
    public class PlayerStat: Stat
    {   
        public float SqrHitRange { get; private set; }
        public PlayerStat(MonoBehaviour owner)
        {
            hpBar = Util.FindChild(owner.transform.parent.parent.gameObject, "Hp", true).transform;
            hpBarParent = hpBar.parent;
            _hpRight = hpBarParent.localScale;
            _hpLeft = new Vector3(-_hpRight.x, _hpRight.y);

            _attackRange = 1;
            SqrAttackRange = _attackRange * _attackRange;
        }

        public void ResetAttackRange()
        {
            var stageType = Manager.Field.StageMeta.GetStageType();
            if (stageType == StageType.Boss || stageType == StageType.Training)
            {
                _attackRange = 1.8f;
                SqrHitRange = _attackRange * _attackRange;
            }
            else
            {
                _attackRange = 1;
                SqrHitRange = 1.5f * 1.5f;
            }
            SqrAttackRange = _attackRange * _attackRange;
        }
    }
}