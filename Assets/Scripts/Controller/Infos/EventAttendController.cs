using System;
using Controller.Currency;
using Data;
using Data.DbEvent;
using Data.DbShop;
using Data.DbUser.Progress;
using Managers;
using ThirdParty;
using UIs.Toast;
using Utils;

namespace Controller.Infos
{
    public class EventAttendController : Singleton<EventAttendController>
    {
        public static DbUserEventAttend data = DbUserEventAttend.Get(0);

        public void Attend(Action<Action> toDo)
        {
            PlayFabManager.Store.DoWithTime(time =>
            {
                data.LastRewardedDate = time;
                
                var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                var rewards = DbAttendEventReward.Get(DbAttendEvent.Get(data.CurrentId.Value).RewardIds[data.RewardedCount.Value]).Items;
                CurrencyController.I.GetRewards(rewards);
                
                var diaReward = rewards.Find(r => r.currencyType == CurrencyType.Dia);
                if (diaReward != null)
                {
                    CurrencyController.I.SetDiaLog($"이벤트 출석 {data.RewardedCount.Value+1}일 보상", (int)diaReward.count, prev);
                }
                data.RewardedCount.Value++;
                data.CanRewarded.Value = false;
                toDo(() =>
                {
                    Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210283, rewards);
                });
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
            });
        }
    }
}