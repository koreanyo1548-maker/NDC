using System;
using System.Collections.Generic;
using System.Numerics;
using Data.DbCommon;
using Data.DbStage;
using Data.Utils;
using Exceptions;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.DbDungeon
{
    [Serializable]
    public class DbSkillGrowthDungeonReward : DbModel<DbSkillGrowthDungeonReward, int>, IDbDungeonReward
    {
        public List<DbReward> Rewards;
        public List<DbReward> FirstRewards;
        public List<string> FirstClearReward;
        public List<string> Reward;

        public override void Load()
        {
            fileName = "SkillGrowthDungeonReward";
            if (Application.isPlaying) Init();
            ForEach(q =>
            {
                q.FirstRewards = new List<DbReward>();
                foreach (var t in q.FirstClearReward) q.FirstRewards.Add(new DbReward(t));
                q.FirstClearReward.Clear();
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
            throw new NotDefinedValueException("RewardCounts of DbSkillGrowthDungeonReward");
        }

        public List<DbProbability<CurrencyType>> GetProbabilities()
        {
            throw new NotDefinedValueException("Probabilities of DbSkillGrowthDungeonReward");
        }

        public List<DbReward> GetFirstClearReward()
        {
            return FirstRewards;
        }

        public int GetMonsterReward()
        {
            throw new NotDefinedValueException("Monster Reward of SkillGrowthDungeonReward");
        }
    }
}