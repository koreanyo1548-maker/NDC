using System;
using System.Numerics;
using Controller.Currency;
using Controller.Utils;
using Data;
using Data.DbCommon;
using Data.DbEvent;
using Managers;
using Utils;
using Random = UnityEngine.Random;

namespace Controller.Play
{
    public class DropEventController : Singleton<DropEventController>, IDayDiffChecker
    {
        private bool _canDrop;
        public ControllerField<bool> CanUseShop = new(false);
        public int CurEvent { get; private set; }
        
        
        public void ForceHandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff == 0) dayDiff = -999;
            HandleDayDiff(now, dayDiff);
        }

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff != 0)
            {
                var curEvent = DbDropEvent.Get(a => now >= a.StartDateCal && now <= a.StartDateCal.AddDays(a.ShopDuration));
                CanUseShop.Value = curEvent != null;
                // 현재 이벤트 없을 때
                if (curEvent == null)
                {
                    CurEvent = 0;
                    // CurrencyController.I.GetMoneyModel(CurrencyType.DropEventMoney).Value = 0;
                }
                else
                {
                    CurEvent = curEvent.Id;
                }
                
                _canDrop = curEvent != null && now <= curEvent.StartDateCal.AddDays(curEvent.DropDuration);
            }
        }

        public void TryDrop()
        {
            if (!_canDrop) return;

            var count = Random.Range(1, 3);
            CurrencyController.I.GetReward(CurrencyType.DropEventMoney, count);
            if (Manager.Field.CurField.Value == FieldType.Stage) Manager.UI.RewardLog.Add(CurrencyType.DropEventMoney, count);
        }

        public DbRewardBig TryGetOfflineReward(long seconds)
        {
            if (!_canDrop) return null;

            var sum = 0;
            var count = seconds / 60;
            while (count-- > 0)
            {
                sum += Random.Range(1, 3);
            }
            return new DbRewardBig(CurrencyType.DropEventMoney, sum);
        }
    }
}