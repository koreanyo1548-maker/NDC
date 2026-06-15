using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Controller.Utils;
using Data.DbEvent;
using Data.DbShop;
using Data.Utils;
using Newtonsoft.Json;
using Utils;

namespace Data.DbUser.Progress
{
    [Serializable]
    public class DbUserEventAttend: DbUserModel<DbUserEventAttend, int>, IDayDiffChecker
    {
        [DataMember] public DbField<int> CurrentId { get; private set; } // 이걸로 메인에서 보여줄지 말지 
        [DataMember] public DbField<int> RewardedCount { get; private set; } // 0: 아무것도 안받은 상태
        [DataMember] public DateTime LastRewardedDate;
        public ControllerField<bool> CanRewarded { get; private set; } // 이걸로 뱃지

        public override void Set(List<DbUserEventAttend> obj)
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
                var curEvent = DbAttendEvent.Get(a => now >= a.StartDateCal && now <= a.StartDateCal.AddDays(a.Duration));

                // 현재 이벤트 없는데 이벤트 세팅되어있을 때
                if (curEvent == null && CurrentId.Value != 0)
                {
                    CurrentId.Value = 0;
                    RewardedCount.Value = 0;
                    LastRewardedDate = new();
                    CanRewarded.Value = false;
                }
                // 현재 이벤트와 유저 이벤트가 다를 때
                else if (curEvent != null && curEvent.Id != CurrentId.Value)
                {
                    CurrentId.Value = curEvent.Id;
                    RewardedCount.Value = 0;
                    LastRewardedDate = new();
                    CanRewarded.Value = true;
                }
                // 현재 이벤트와 유저 이벤트가 같을 때
                else if (curEvent != null)
                {
                    CanRewarded.Value = Define.GetDayDiff(LastRewardedDate, now) > 0 && RewardedCount.Value < 7;
                }
            }
            
        }

        protected override List<DbUserEventAttend> GetInitials()
        {
            return new List<DbUserEventAttend>
            {
                new()
            };
        }

        public override List<DbUserEventAttend> AdjustDataModification(List<DbUserEventAttend> obj)
        {
            return obj;
        }


        [JsonConstructor]
        public DbUserEventAttend(int Id, int CurrentId, int RewardedCount, DateTime LastRewardedDate)
        {
            this.Id = Id;
            this.CurrentId = new DbField<int>(CurrentId, 0, this);
            this.RewardedCount = new DbField<int>(RewardedCount, 0, this);
            this.LastRewardedDate = LastRewardedDate;
            this.CanRewarded = new ControllerField<bool>(false);
        }

        public DbUserEventAttend()
        {
            this.Id = 0;
            this.CurrentId = new DbField<int>(0, 0, this);
            this.RewardedCount = new DbField<int>(0, 0, this);
            this.LastRewardedDate = new DateTime();
            this.CanRewarded = new ControllerField<bool>(false);
        }
    }
}