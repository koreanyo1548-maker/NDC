using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data.DbCommon;
using Data.DbShop;
using Data.Stores;
using Data.Utils;
using Managers;
using ThirdParty;
using UIs.Toast;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.DbUser.Currency
{
    [Serializable]
    public class DbUserMail
    {
        public int Id;
        public int MailId;
        public DbField<bool> IsRewarded;
        public DbField<bool> IsHide;
        public bool IsShop;
        public DateTime RewardedTime;
        public string Receipt;
        public MailInfo MailInfo { get; private set; }
        public DbInAppShop InAppShop { get; private set; }
        public List<DbReward> Rewards => IsShop ? InAppShop.Reward : MailInfo.rewards;

        public DbUserMail(int id, int mailId, bool isRewarded, bool isHide, bool isShop, string receipt, DateTime rewardedTime, DbUserModel parent)
        {
            Id = id;
            MailId = mailId;
            IsRewarded = new DbField<bool>(isRewarded, 0, parent);
            IsHide =  new DbField<bool>(isHide, 0, parent);
            IsShop = isShop;
            Receipt = receipt;
            RewardedTime = rewardedTime;
            if (isShop) InAppShop = DbInAppShop.Get(mailId);
        }

        public void SetMailInfo(MailInfo mailInfo)
        {
            MailInfo = mailInfo;
        }

        public void GetReward()
        {
            CurrencyController.I.CheckMailCheat();
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            IsRewarded.Value = true;
            CurrencyController.I.GetRewards(Rewards);
            Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210261, Rewards);
            
            PlayFabManager.Store.DoWithTime(now => RewardedTime = now);

            if (Rewards.Exists(r => r.currencyType == CurrencyType.Dia))
            {
                var r = Rewards.Find(r => r.currencyType == CurrencyType.Dia);
                CurrencyController.I.SetDiaLog($"우편 {MailId} 보상", (int)r.count, prev);
            }
        }
    }
}