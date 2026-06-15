using Data;

namespace UIs.Guide
{
    public class UI_Guide_20: UI_Guide_StatLevel
    {
        public override bool Init()
        {
            if (!base.Init()) return false;

            _statName = StatType.CriticalProbability.ToString();

            return true;
        }
    }
}