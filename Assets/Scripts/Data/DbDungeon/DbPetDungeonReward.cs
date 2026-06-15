using System;
using System.Collections.Generic;
using System.Linq;
using Data.DbCommon;
using Data.Utils;
using Exceptions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.DbDungeon
{
    [Serializable]
    public class DbPetDungeonReward: DbModel<DbPetDungeonReward, int>, IDbDungeonReward
    {
        public int Counts;
        public List<DbProbability<CurrencyType>> Probabilities;
        public List<DbReward> Rewards;
        public List<DbReward> FirstRewards;
        public List<string> FirstClearReward;
        public List<string> Probability;

        public override void Load()
        {
            fileName = "PetDungeonReward";
            if (Application.isPlaying) Init();
            ForEach(q =>
            {
                q.Rewards = new();
                q.FirstRewards = new List<DbReward>();
                foreach (var t in q.FirstClearReward) q.FirstRewards.Add(new DbReward(t));
                q.FirstClearReward.Clear();
                
                q.Probabilities = new List<DbProbability<CurrencyType>>();
                foreach (var t in q.Probability) q.Probabilities.Add(new DbProbability<CurrencyType>(t));
                q.Probability.Clear();
            });
        }

        public bool IsRewardStatic()
        {
            return false;
        }

        public List<DbReward> GetRewards()
        {
            Rewards.Clear();
            var prCount = Probabilities.Count;
            var getCount = 0;
            while (getCount < Counts)
            {
                var ran = Random.Range(0, 100);
                var add = 0;
                for (var idx = 0; idx < prCount; ++idx)
                {
                    add += Probabilities[idx].probability;
                    if (ran < add)
                    {
                        getCount++;
                        var reward = Rewards.Find(r => r.currencyType == Probabilities[idx].category);
                        if (reward == null)
                        {
                            Rewards.Add(new DbReward(Probabilities[idx].category, 1, 0));
                        }
                        else
                        {
                            reward.count++;
                        }
                        break;
                    }
                }
            }

            return Rewards;
        }

        public int GetRewardCounts()
        {
            return Counts;
        }

        public List<DbProbability<CurrencyType>> GetProbabilities()
        {
            return Probabilities;
        }

        public List<DbReward> GetFirstClearReward()
        {
            return FirstRewards;
        }

        public int GetMonsterReward()
        {
            throw new NotDefinedValueException("Monster Reward of PetDungeonReward");
        }
    }
}