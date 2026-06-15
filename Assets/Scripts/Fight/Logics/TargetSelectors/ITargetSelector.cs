using Fight.Units;

namespace Fight.Logics.TargetSelectors
{
    public interface ITargetSelector
    {
        public IFightUnit GetTarget();
    }
}