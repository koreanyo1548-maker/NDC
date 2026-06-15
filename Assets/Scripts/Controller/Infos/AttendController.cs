using System;
using Controller.Currency;
using Controller.Play;
using Controller.Utils;
using Data;
using Data.DbEvent;
using Data.DbUser.Progress;
using Managers;
using ThirdParty;
using Utils;

namespace Controller.Infos
{
    public class AttendController : Singleton<AttendController>, IDayDiffChecker
    {
        public static DbUserAttend data = DbUserAttend.Get(0);
        
        public ControllerField<bool> CanRewarded = new (false);

        private bool _isInit;
        public void ForceHandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff == 0) dayDiff = -999;
            HandleDayDiff(now, dayDiff);
        }

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff != 0)
            {
                if (now.Date != data.LastRewarded.Date)
                {
                    CanRewarded.Value = true;
                    if (_isInit) BadgeController.I.OnAttendUpdated(null, null);
                }
            }
            _isInit = true;
        }
        
        public void Attend(Action toDo)
        {
            PlayFabManager.Store.DoWithTime(time =>
            {
                data.LastRewarded = time;
            
                var reward = DbAttendReward.Get(data.NextDay.Value);
                var next = data.NextDay.Value + 1;
                if (next == 8) next = 1;
        
                var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                CurrencyController.I.GetRewards(reward.Items);

                if (reward.Items.Exists(r => r.currencyType == CurrencyType.Dia))
                {
                    var item = reward.Items.Find(r => r.currencyType == CurrencyType.Dia);
                    CurrencyController.I.SetDiaLog($"{data.NextDay.Value-1} 출석 보상", item.count, prev);
                }

                CanRewarded.Value = false;
                data.NextDay.Value = next;
                BadgeController.I.OnAttendUpdated(null, null);
                QuestController.I.DoQuests(QuestType.Attend);
                toDo();
            });
        }
    }
}