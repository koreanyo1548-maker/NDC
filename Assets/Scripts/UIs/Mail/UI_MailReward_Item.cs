using System;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Mail
{
    public class UI_MailReward_Item: UI_Base
    {
        enum Images
        {
            IMG_Reward
        }

        enum Texts
        {
            T_Count
        }

        enum GameObjects
        {
            IMG_Complete
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            
            return true;
        }

        public void Set(DbRewardBig reward)
        {
            if (!_isInit) Init();
            
            Get<Image>((int) Images.IMG_Reward).sprite =
                DbCurrency.Get(reward.currencyType).GetResource(reward.id);
            if (reward.currencyType == CurrencyType.OfflineExp || reward.currencyType == CurrencyType.OfflineGold 
                || reward.currencyType == CurrencyType.OfflineWeaponGrowthStone || reward.currencyType == CurrencyType.OfflineAccessoryGrowthStone 
                || reward.currencyType == CurrencyType.OfflineReward)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Count).text = StringMaker.GetDayTimeString(new TimeSpan(0, 0, (int)reward.count, 0));
            }
            else
            {
                Get<TextMeshProUGUI>((int) Texts.T_Count).text = Define.AddUnit(reward.count, 3, 2);
            }
        }

        public void SetRewarded(bool isRewarded)
        {
            Get<GameObject>((int) GameObjects.IMG_Complete).SetActive(isRewarded);
        }
    }
}