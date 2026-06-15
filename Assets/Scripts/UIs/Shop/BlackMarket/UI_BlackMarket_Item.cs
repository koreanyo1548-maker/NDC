using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbCommon;
using Data.DbEvent;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.BlackMarket
{
    public class UI_BlackMarket_Item: UI_Base
    {
        protected DbBlackMarket _item;
        private Action _whenBuyDone;
        
        enum GameObjects
        {
            IMG_SoldOutBG
        }
        
        enum Texts
        {
            T_Count,
            T_Cost
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            
            return true;
        }
        
        public void SetInfo(DbBlackMarket item, Action whenBuyDone)
        {
            if (!_isInit) Init();

            _item = item;
            _whenBuyDone = whenBuyDone;
            
            
            var isOfflineReward = item.RewardType == CurrencyType.OfflineExp || item.RewardType == CurrencyType.OfflineGold
                || item.RewardType == CurrencyType.OfflineAccessoryGrowthStone || item.RewardType == CurrencyType.OfflineWeaponGrowthStone;
            Util.FindChild<Image>(gameObject, "IMG_Item").sprite = Manager.Resource.Load<Sprite>(item.Resource);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text =
                isOfflineReward ? StringMaker.GetDayTimeString(new TimeSpan(0, 0, item.RewardCount, 0)) 
                : item.RewardCount.ToString("N0");
            Get<TextMeshProUGUI>((int)Texts.T_Cost).text =  item.Price.ToString("N0");

            Util.FindChild(gameObject, "B_Buy").BindEvent(BuyCondition, _ => Buy(), UIEffectType.Bounce, false);

            if (_item.RenewalInterval == RenewalType.Infinite)
            {
                Get<GameObject>((int)GameObjects.IMG_SoldOutBG).SetActive(false);
                Get<TextMeshProUGUI>((int) Texts.T_Count).text = string.Empty;
            }

            WhenBuyCountChanged();
        }

        private void WhenBuyCountChanged()
        {
            var renewal = _item.RenewalInterval;
            if (renewal == RenewalType.Infinite) return;
            
            var canBuy = BuyCondition();
            Get<GameObject>((int)GameObjects.IMG_SoldOutBG).SetActive(!canBuy);
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = StringMaker.GetBuyLimitText(_item);
        }
        
        private bool BuyCondition()
        {
            return CurrencyController.I.CanBuy(_item);
        }


        private void Buy()
        {
            var isSuccess = CurrencyController.I.Buy(_item); 
            if (isSuccess) 
            { 
                var toast = Manager.UI.ShowSingleUI<UI_RewardToast>(); 
                var rewardsForToast = new List<DbReward>();
     
                rewardsForToast.Add(
                    new DbReward(_item.RewardType, _item.RewardCount, _item.RewardId));
                toast.SetReward(200032, rewardsForToast); 
                WhenBuyCountChanged();

                if (!CurrencyController.I.CanBuy(_item)) _whenBuyDone();
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200038);
            }
        }
    }
}