using Data;
using Data.DbDefinition;
using Fight.Units;
using Managers;

namespace Fight.Logics.TargetSelectors
{
    public class NearBibleSelector: ITargetSelector
    {
        public IFightUnit GetTarget()
        {
            var bible = Manager.Bible;
            var player = Manager.Player;
            var monster = Manager.Field.GetNearestMonster(bible.Position(), player.SqrAttackRange, DbPlay.Get(PlayType.BibleDefenseRange).Value);
            if (monster != null) return monster;
            monster = Manager.Field.GetNearestMonster(player.Position(), player.SqrAttackRange);
            if (monster != null) return monster;
            return bible;
        }
    }
}