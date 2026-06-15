using System;
using System.Collections.Generic;
using Data.DbCommon;
using Data.Utils;
using UnityEngine;

namespace Data.DbRecord
{
    [Serializable]
    public class DbGuideQuest : DbModel<DbGuideQuest, int>
    {
        public QuestType ToDo;
        public int NameId;
        public int Goal;
        public List<DbReward> Rewards;
        public int EffectId;
        public int GoTo;

        public List<string> QuestRewards;
        public override void Load()
        {
            fileName = "GuideQuest";
            if (Application.isPlaying) Init();
            ForEach(q =>
            {
                q.Rewards = new List<DbReward>();
                foreach (var t in q.QuestRewards)
                {
                    q.Rewards.Add(new DbReward(t));
                }
            });
        }
        
        
    }
}