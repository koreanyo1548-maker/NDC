using System;
using System.Collections.Generic;
using Controller.Currency;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEvent;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.EventShop
{
    public class UI_EventShop_Item: UI_Base
    {
        private EventsManager _eventMoneyChangeEventManager;
        
        protected DbDropEventShop _item;
        
        enum GameObjects
        {
            IMG_SoldOutBG,
            IMG_Badge
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
            
            Get<GameObject>((int) GameObjects.IMG_Badge).AddComponent<RepeatingScale>();
            
            return true;
        }
        
        public void SetInfo(DbDropEventShop item)
        {
            if (!_isInit) Init();

            _item = item;
            
            
            var isOfflineReward = item.RewardType == CurrencyType.OfflineExp || item.RewardType == CurrencyType.OfflineGold
                || item.RewardType == CurrencyType.OfflineWeaponGrowthStone || item.RewardType == CurrencyType.OfflineAccessoryGrowthStone;
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
            
            _eventMoneyChangeEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEventMoneyChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.DropEventMoney)}
            });

            WhenEventMoneyChanged();
            WhenBuyCountChanged();
        }

        private void WhenEventMoneyChanged()
        {
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(
                BuyCondition() && CurrencyController.I.GetMoneyModel(CurrencyType.DropEventMoney).Value >= _item.Price);
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
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200038);
            }
        }

        private void OnEnable()
        {
            _eventMoneyChangeEventManager?.Reconnect();
        }
        
        private void OnDisable()
        {
            _eventMoneyChangeEventManager?.Dispose();
        }
    }
}