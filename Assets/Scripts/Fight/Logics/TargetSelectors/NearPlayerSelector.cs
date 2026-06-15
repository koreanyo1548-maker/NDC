using Fight.Units;
using Managers;

namespace Fight.Logics.TargetSelectors
{
    public class NearPlayerSelector: ITargetSelector
    {
        public IFightUnit GetTarget()
        {
            var player = Manager.Player;
            return Manager.Field.GetNearestMonster(player.Position(), player.SqrAttackRange);
        }
    }
}