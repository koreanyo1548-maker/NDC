using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Utils;
using Data;
using Data.DbRecord;
using Data.DbUser.Progress;
using Data.Utils;
using Utils;

namespace Controller.Infos
{
    public class NewbieQuestController: Singleton<NewbieQuestController>, IDayDiffChecker
    {
        public Dictionary<int, ControllerField<bool>> IsAnyRewardInDay = new();
        public ControllerField<bool> IsAllCleared = new(false);
        public DbFieldWithoutParent<int> CurDay = new (0, 0);

        public void Init()
        {
            for (var day = 1; day <= 7; ++day) IsAnyRewardInDay.Add(day, new(false));
            var isAllCleared = true;
            DbUserNewbieQuest.ForEach(q =>
            {
                var initialCount = QuestController.I.GetInitQuestCount(q.Meta.ToDo);
                if (initialCount > 0 && q.Count.Value != initialCount) q.Count.Value = initialCount;
                
                if (isAllCleared && !q.IsRewarded.Value) isAllCleared = false;
                if (q.CanRewarded) IsAnyRewardInDay[q.Meta.Day].Value = true;
            });
            IsAllCleared.Value = isAllCleared;
        }
        
        private void SetRewardStatus(int day)
        {
            var isAllCleared = true;
            var isAnyReward = false;
            DbUserNewbieQuest.ForEach(q =>
            {
                if (isAllCleared && !q.IsRewarded.Value) isAllCleared = false;
                if (!isAnyReward && q.Meta.Day == day && q.CanRewarded) isAnyReward = true;
            });
            IsAllCleared.Value = isAllCleared;
            IsAnyRewardInDay[day].Value = isAnyReward;
        }
        
        public void SetCurDay(int day)
        {
            CurDay.Value = day;
        }

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff > 0 && CurDay.Value < 7) CurDay.Value++;
            if (CurDay.Value == 0) CurDay.Value = 1;
        }

        public void DoQuest(QuestType quest, int count)
        {
            DbUserNewbieQuest.ForEach(q => 
                (q.Meta.Day <= CurDay.Value || q.Meta.Continuous) && !q.IsRewarded.Value && q.Meta.ToDo == quest, q =>
            {
                q.Count.Value += count;
                if (q.CanRewarded) IsAnyRewardInDay[q.Meta.Day].Value = true; 
            });
        }

        public void SetQuests(QuestType quest, int count)
        {
            DbUserNewbieQuest.ForEach(q => 
                (q.Meta.Day <= CurDay.Value || q.Meta.Continuous) && !q.IsRewarded.Value && q.Meta.ToDo == quest, q =>
            {
                q.Count.Value = count;
                if (q.CanRewarded) IsAnyRewardInDay[q.Meta.Day].Value = true; 
            });
        }

        public Tuple<CurrencyType, int, int> GetReward(DbUserNewbieQuest quest)
        {
            quest.IsRewarded.Value = true;
            var meta = DbNewbieQuest.Get(quest.Id);

            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            CurrencyController.I.GetReward(meta.RewardType, meta.RewardCount, meta.RewardId);
            if (meta.RewardType == CurrencyType.Dia) 
                CurrencyController.I.SetDiaLog($"신규유저퀘스트 {quest.Id} 보상", meta.RewardCount, prev);
            
            QuestController.I.DoQuests(QuestType.NewbieDayQuestClear);
            SetRewardStatus(quest.Meta.Day);

            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Quest);
            return new Tuple<CurrencyType, int, int>(meta.RewardType, meta.RewardCount, meta.RewardId);
        }
        
        
        public Dictionary<CurrencyType, Dictionary<int, int>> GetAllRewards(int day)
        {
            var rewards = new Dictionary<CurrencyType, Dictionary<int, int>>();
            DbUserNewbieQuest.ForEach(q => q.Meta.Day == day, quest =>
            {
                if (!quest.CanRewarded) return;
                var reward = GetReward(quest);
                if (rewards.ContainsKey(reward.Item1))
                {
                    if (rewards[reward.Item1].ContainsKey(reward.Item3)) rewards[reward.Item1][reward.Item3] += reward.Item2;
                    else rewards[reward.Item1].Add(reward.Item3, reward.Item2);
                }
                else rewards.Add(reward.Item1, new (){{reward.Item3, reward.Item2}});
            });
            SetRewardStatus(day);
            return rewards;
        }

        public bool CanGetReward(int day)
        {
            if (day == 0)
                return (CurDay.Value >= 1 && IsAnyRewardInDay[1].Value) || (CurDay.Value >= 2 && IsAnyRewardInDay[2].Value) || 
                       (CurDay.Value >= 3 && IsAnyRewardInDay[3].Value) || (CurDay.Value >=4 && IsAnyRewardInDay[4].Value) ||
                       (CurDay.Value >= 5 && IsAnyRewardInDay[5].Value) || (CurDay.Value >= 6 && IsAnyRewardInDay[6].Value) ||
                       (CurDay.Value >= 7 && IsAnyRewardInDay[7].Value);
            return IsAnyRewardInDay[day].Value;
        }
    }
}