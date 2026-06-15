using System.Collections.Generic;
using System.Numerics;
using Data.DbCommon;

namespace Data.DbDungeon
{
    public interface IDbDungeonReward
    {
        public bool IsRewardStatic();
        public List<DbReward> GetRewards();
        public int GetRewardCounts();
        public List<DbProbability<CurrencyType>> GetProbabilities();
        public List<DbReward> GetFirstClearReward();
        public int GetMonsterReward();
    }
}