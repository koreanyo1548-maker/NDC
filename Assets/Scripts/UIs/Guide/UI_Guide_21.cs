using Data;

namespace UIs.Guide
{
    public class UI_Guide_21: UI_Guide_StatLevel
    {
        public override bool Init()
        {
            if (!base.Init()) return false;

            _statName = StatType.CriticalAttackBonus.ToString();

            return true;
        }
    }
}