using Fight.Units;
using Managers;

namespace Fight.Logics.TargetSelectors
{
    public class BibleSelector: ITargetSelector
    {
        public IFightUnit GetTarget()
        {
            return Manager.Bible;
        }
    }
}