using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Controller.Currency;
using Controller.Infos;
using Data.DbEvent;
using Data.Utils;
using Newtonsoft.Json;
using Utils;

namespace Data.DbUser.Progress
{
    [Serializable]
    public class DbUserSeasonPass: DbUserModel<DbUserSeasonPass, int>, IDayDiffChecker
    {
        [DataMember] public DbField<int> CurrentId { get; private set; } // 이걸로 메인에서 보여줄지 말지 
        [DataMember] public DbField<List<int>> Rewarded { get; private set; }
        [DataMember] public DbField<long> Point { get; private set; }
        [DataMember] public Dictionary<QuestType, DbField<int>> Quest { get; private set; }

        public override void Set(List<DbUserSeasonPass> obj)
        {
            Init(obj);
        }
        
        public void ForceHandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff == 0) dayDiff = -999;
            HandleDayDiff(now, dayDiff);
        }
        
        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff != 0)
            {
                var curEvent = DbSeasonPass.Get(a => now >= a.StartDateCal && now <= a.StartDateCal.AddDays(a.Duration));
                // 현재 이벤트 없는데 이벤트 세팅되어있을 때
                if (curEvent == null && CurrentId.Value != 0)
                {
                    InitSeasonPassBuy();
                    CurrentId.Value = 0;
                    Rewarded.Value.Clear();
                    Quest.Clear();
                    Point.Value = 0;
                }
                // 현재 이벤트와 유저 이벤트가 다를 때
                else if (curEvent != null && curEvent.Id != CurrentId.Value)
                {
                    InitSeasonPassBuy();
                    CurrentId.Value = curEvent.Id;
                    Rewarded.Value.Clear();
                    InitQuest();
                    Point.Value = 0;
                    SeasonPassController.I.Init();
                }
                // 날짜만 바뀌었을때 퀘스트 초기화
                else if (dayDiff != -999)
                {
                    foreach (var q in Quest)
                    {
                        q.Value.Value = 0;
                    }
                }
            }
            
        }

        private void InitQuest()
        {
            Quest.Clear();
            DbSeasonPassQuest.ForEach(q =>
            {
                Quest.Add(q.Id, new DbField<int>(0, 0, this));
            });
        }

        private void InitSeasonPassBuy()
        {
            if (CurrentId.Value == 0) return;
            var buy = CurrencyController.I.GetBuying(DbSeasonPass.Get(CurrentId.Value).ShopId);
            if (buy != null)
            {
                buy.BuyCnt = 0;
                CurrencyController.I.ResetHave(CurrencyType.SeasonPass);
            }
        }

        protected override List<DbUserSeasonPass> GetInitials()
        {
            var init = new DbUserSeasonPass();
            InitQuest();
            return new List<DbUserSeasonPass>
            {
                init
            };
        }

        public override List<DbUserSeasonPass> AdjustDataModification(List<DbUserSeasonPass> obj)
        {
            return obj;
        }


        [JsonConstructor]
        public DbUserSeasonPass(int Id, int CurrentId, List<int> Rewarded, long Point, Dictionary<QuestType, int> Quest)
        {
            this.Id = Id;
            this.CurrentId = new DbField<int>(CurrentId, 0, this);
            this.Rewarded = new DbField<List<int>>(Rewarded, 0, this);
            this.Point = new DbField<long>(Point, 0, this);
            var changedQuest = new Dictionary<QuestType, DbField<int>>();
            foreach (var q in Quest)
            {
                changedQuest.Add(q.Key, new DbField<int>(q.Value, 0, this));
            }
            this.Quest = changedQuest;
        }

        public DbUserSeasonPass()
        {
            this.Id = 0;
            this.CurrentId = new DbField<int>(0, 0, this);
            this.Rewarded = new DbField<List<int>>(new List<int>(), 0, this);
            this.Point = new DbField<long>(0, 0, this);
            this.Quest = new Dictionary<QuestType, DbField<int>>();
        }
    }
}