using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using Exceptions;
using UnityEngine;

namespace Data.DbDungeon
{
    [Serializable]
    public class DbBlackMarketDungeonReward: DbModel<DbBlackMarketDungeonReward, int>, IDbDungeonReward
    {
        public List<DbReward> Rewards;
        public List<string> Reward;
        public int MonsterReward;

        public override void Load()
        {
            fileName = "BlackMarketDungeonReward";
            if (Application.isPlaying) Init();
            ForEach(q =>
            {
                q.Rewards = new List<DbReward>();
                foreach (var t in q.Reward) q.Rewards.Add(new DbReward(t));
                q.Reward.Clear();
            });
            
        }

        public bool IsRewardStatic()
        {
            return true;
        }

        public List<DbReward> GetRewards()
        {
            return Rewards;
        }

        public int GetRewardCounts()
        {
            throw new NotDefinedValueException("RewardCounts of DbBlackMarketReward");
        }

        public List<DbProbability<CurrencyType>> GetProbabilities()
        {
            throw new NotDefinedValueException("Probabilities of DbBlackMarketDungeonReward");
        }

        public List<DbReward> GetFirstClearReward()
        {
            return new();
        }

        public int GetMonsterReward()
        {
            return MonsterReward;
        }
    }
}