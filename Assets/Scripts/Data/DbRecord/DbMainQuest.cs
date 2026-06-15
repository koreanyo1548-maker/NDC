using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.DbRecord
{
    [Serializable]
    public class DbMainQuest : DbModel<DbMainQuest, int>
    {
        public QuestType ToDo;
        public int NameId;
        public int Goal;
        public int IncreasingGoal;
        public bool HaveEnd;
        public CurrencyType RewardType;
        public int RewardCounts;
        public Dictionary<int, int> GoToGuide;
        public List<string> GoTo;

        
        public override void Load()
        {
            fileName = "MainQuest";
            if (Application.isPlaying) Init();
            ForEach(q =>
            {
                q.GoToGuide = new Dictionary<int, int>();
                foreach (var gt in q.GoTo)
                {
                    var splits = gt.Split(":");
                    q.GoToGuide.Add(int.Parse(splits[0])-1, int.Parse(splits[1]));
                }
            });
        }
    }
}