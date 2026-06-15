using System;
using System.Collections.Generic;
using System.Drawing;
using Controller.Currency;
using Controller.Utils;
using Data;
using Data.DbCommon;
using Data.DbEvent;
using Data.DbUser.Progress;
using Managers;
using UIs.Toast;
using Utils;

namespace Controller.Infos
{
    public class SeasonPassController: Singleton<SeasonPassController>
    {
        public static DbUserSeasonPass data = DbUserSeasonPass.Get(0);
        public ControllerField<bool> CanGetReward = new(false);
        public ControllerField<bool> CanGetPoint = new(false);
        public ControllerField<int> NextRewarded = new(0);

        public void Init()
        {
            var removes = new List<QuestType>();
            foreach (var q in data.Quest)
            {
                if (DbSeasonPassQuest.Get(q.Key) == null)
                {
                    removes.Add(q.Key);
                }
            }

            foreach (var key in removes)
            {
                data.Quest.Remove(key);
            }
            SetNextRewardId();
            CheckCanGetReward();
            CheckCanGetPoint();
        }

        public void WhenBuySeasonPass()
        {
            SetNextRewardId();
            CheckCanGetReward();
        }

        private void CheckCanGetReward()
        {
            if (data.CurrentId.Value == 0)
            {
                CanGetReward.Value = false;
                return;
            }
            var rewardCount = data.Rewarded.Value.Count;
            var lastRewarded = GetLastRewardId();
            var getAll = DbSeasonPassReward.Get(lastRewarded+1) == null;
            if (IsSeasonPassPurchased() && lastRewarded != rewardCount) 
            {
                CanGetReward.Value = true;
                return;
            }
            if (!getAll && data.Point.Value >= DbSeasonPassReward.Get(NextRewarded.Value).NeedPoint)
            {
                CanGetReward.Value = true;
            }
            else
            {
                CanGetReward.Value = false;
            }
        }
        
        public int GetLastRewardId()
        {
            var rewardCount = data.Rewarded.Value.Count;
            if (rewardCount == 0) return 0;
            return data.Rewarded.Value[rewardCount-1];
        }

        public void SetNextRewardId()
        {
            if (data.CurrentId.Value == 0)
            {
                return;
            }
            var prev = NextRewarded.Value;
            var lastRewardId = GetLastRewardId();
            var nextReward = DbSeasonPassReward.Get(lastRewardId + 1);
            if (nextReward == null)
            {
                Set(lastRewardId);
                return ;
            }

            if (IsSeasonPassPurchased())
            {
                Set(nextReward.Id);
                return;
            }

            while (true)
            {
                if (nextReward.IsFree) 
                {
                    Set(nextReward.Id);
                    return;
                }

                if (data.Point.Value < nextReward.NeedPoint)
                {
                    Set(nextReward.Id);
                    return;
                }
                
                lastRewardId = nextReward.Id;
                nextReward = DbSeasonPassReward.Get(lastRewardId + 1);
                if (nextReward == null)
                {
                    Set(lastRewardId);
                    return;
                }
            }

            void Set(int id)
            {
                if (id != prev) NextRewarded.Value = id;
            }
        }

        private void CheckCanGetPoint()
        {
            if (data.CurrentId.Value == 0)
            {
                CanGetPoint.Value = false;
                return;
            }
            foreach (var quest in data.Quest)
            {
                if (quest.Value.Value >= DbSeasonPassQuest.Get(quest.Key).Goal)
                {
                    CanGetPoint.Value = true;
                    return;
                }
            }
            CanGetPoint.Value = false;
        }

        public void DoQuests(QuestType toDo, int add)
        {
            if (!data.Quest.ContainsKey(toDo)) return;

            if (data.Quest[toDo].Value == -1) return;
            var goal = DbSeasonPassQuest.Get(toDo).Goal;
            data.Quest[toDo].Value = Math.Min(data.Quest[toDo].Value+add, goal);
            if (data.Quest[toDo].Value >= goal) CanGetPoint.Value = true;
        }

        public void GetPoint(QuestType toDo)
        {
            data.Point.Value += DbSeasonPassQuest.Get(toDo).Point;
            data.Quest[toDo].Value = -1;
            SetNextRewardId();
            CheckCanGetPoint();
            CheckCanGetReward();
        }

        public void GetAllRewards()
        {
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            var rewardsForToast = new List<DbRewardBig>();
            var maxId = DbSeasonPassReward.Count;
            var canGetPremium = IsSeasonPassPurchased();
            for (var id = 1; id <= maxId; ++id)
            {
                if (data.Rewarded.Value.Contains(id)) continue;
                var reward = DbSeasonPassReward.Get(id);
                var need = reward.NeedPoint;
                if (data.Point.Value < need) break;
                
                if (!reward.IsFree && !canGetPremium) continue;
                
                var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                CurrencyController.I.GetReward(reward.RewardType, reward.RewardCount, reward.RewardId);
                if (reward.RewardType == CurrencyType.Dia) CurrencyController.I.SetDiaLog($"시즌 패스 {reward.RewardId} 보상", reward.RewardCount, prev);
                data.Rewarded.Value.Add(id);
                var rewardToast = rewardsForToast.Find(r => r.currencyType == reward.RewardType);
                if (rewardToast != null) rewardToast.count += reward.RewardCount;
                else rewardsForToast.Add(new DbRewardBig(reward.RewardType, reward.RewardCount, reward.RewardId));
            }
            data.Rewarded.Value.Sort();
            SetNextRewardId();
            CheckCanGetReward();
            toast.SetReward(210243, rewardsForToast);
            
        }

        public void GetReward(int id)
        {
            var reward = DbSeasonPassReward.Get(id);
            if ((!reward.IsFree && !IsSeasonPassPurchased()) || data.Point.Value < reward.NeedPoint) return;
            
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            CurrencyController.I.GetReward(reward.RewardType, reward.RewardCount, reward.RewardId);
            if (reward.RewardType == CurrencyType.Dia) CurrencyController.I.SetDiaLog($"시즌 패스 {reward.RewardId} 보상", reward.RewardCount, prev);
            data.Rewarded.Value.Add(id);
            data.Rewarded.Value.Sort();
            SetNextRewardId();
            CheckCanGetReward();
            
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            var rewardsForToast = new List<DbReward>();
            rewardsForToast.Add(new DbReward(reward.RewardType, reward.RewardCount, reward.RewardId));
            toast.SetReward(210243, rewardsForToast);
        }

        public void LevelUpTo(int id)
        {
            var reward = DbSeasonPassReward.Get(id);
            data.Point.Value = reward.NeedPoint;
            SetNextRewardId();
            CheckCanGetReward();
        }

        public bool IsSeasonPassPurchased()
        {
            if (data.CurrentId.Value == 0) return false;
            return CurrencyController.I.GetBuyCount(DbSeasonPass.Get(data.CurrentId.Value).ShopId) > 0;
        }
    }
}