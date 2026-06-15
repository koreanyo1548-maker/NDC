using Data;

namespace UIs.Guide
{
    public class UI_Guide_17: UI_Guide_StatLevel
    {
        public override bool Init()
        {
            if (!base.Init()) return false;

            _statName = StatType.Attack.ToString();

            return true;
        }
    }
}