using Fight.Units;
using Managers;

namespace Fight.Logics.TargetSelectors
{
    public class PlayerSelector: ITargetSelector
    {
        public IFightUnit GetTarget()
        {
            return Manager.Player;
        }
    }
}