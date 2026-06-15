using System.Numerics;
using Controller;
using Controller.Play;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Fight.Stats
{
    public class HpStat
    {
        public BigInteger Hp { get; protected set; }

        public BigInteger MaxHp { get; protected set; }
        
        protected Transform hpBar;
        protected Transform hpBarParent;

        private Vector3 _hpScale = Vector3.one;
        protected Vector3 _hpLeft;
        protected Vector3 _hpRight;

        protected bool _isBoss;

        
        public void Flip(bool isLeft)
        {
            hpBarParent.localScale = isLeft ? _hpLeft : _hpRight;
        }
        
        public bool Attacked(BigInteger minus)
        {
            Hp -= minus;
            
            if (Hp < 0) Hp = 0;

            SetHpBar();
            return Hp == 0;
        }

        private void SetHpBar()
        {
            _hpScale.x = Hp == 0 ? 0 : (float) Hp / (float)MaxHp;
            hpBar.localScale = _hpScale;
            
            if (_isBoss) PlayController.I.SetRedProgress(Hp);
        }

        public void Recovery(BigInteger recoveryHp)
        {
            Hp += recoveryHp;
            if (Hp >= MaxHp) Hp = MaxHp;
            SetHpBar();
        }

        public void ApplyMaxHp(BigInteger maxHp, bool needReset)
        {
            var diff = maxHp - this.MaxHp;
            this.MaxHp = maxHp;
            if (needReset || Hp == 0) Hp = maxHp;
            else if (diff > 0) Hp += diff;
            SetHpBar();
        }
    }
}