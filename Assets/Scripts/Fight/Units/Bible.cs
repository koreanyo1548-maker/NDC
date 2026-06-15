using System.Numerics;
using Data;
using DG.Tweening;
using Fight.Stats;
using Managers;
using UnityEngine;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Units
{
    public class Bible: MonoBehaviour, IFightUnit
    {
        public BigInteger GetMaxHp() => _stat.MaxHp;
        public bool IsPlayerSide() => true;
        public bool IsValid() => _state != BibleState.Die;
        public Vector3 Position() => transform.position;
        public bool IsBoss() => false;

        private BibleStat _stat;
        private BibleState _state;
        private Transform _damagePosition;

        private Vector3 _originalPosition;
        
        private enum BibleState
        {
            Die,
            Idle
        }

        private void Awake()
        {
            Manager.Bible = this;

            _stat = new BibleStat(this);
            _damagePosition = transform.Find("DamagePosition");
            Manager.Resource.Destroy(gameObject);
        }


        public void Spawn(float posX, float posY)
        {
            _state = BibleState.Idle;
            _originalPosition = new Vector3(posX, posY);
            transform.position = _originalPosition;
        }

        public void Attacked(BigInteger attack, AttackType attackType = AttackType.Normal)
        {
            if (_state == BibleState.Die) return;

            var hit = Manager.Resource.InstantiateParticle("Particles/MonsterHitEffect", 10, Manager.EffectParent);
            if (hit != null)
            {
                hit.transform.position = Position() + Define.HitParticlePositionDiff;
                var dm = Manager.Pool.damage.Spawn(_damagePosition.position, attack);
                dm.SetSize(Define.DamageSize(false));
                dm.SetGradient(Define.DamageColor(false));
                transform.DOKill();
                transform.position = _originalPosition;
                transform.DOShakePosition(0.2f, 0.2f, 20);
            }
            var isDead = _stat.Attacked(attack);
            if (isDead)
            {
                Die();
            }
        }

        private void Die()
        {
            _state = BibleState.Die;

            Manager.Field.GameOver(GameOverType.DungeonFail, 3.25f, FieldType.Pet);
            UseDone();
        }

        public void UseDone()
        {
            Manager.Resource.Destroy(gameObject);
        }
        private void OnEnable()
        {
            _stat.OnEnable();
        }

        private void OnDisable()
        {
            _stat.OnDisable();
        }
    }
}