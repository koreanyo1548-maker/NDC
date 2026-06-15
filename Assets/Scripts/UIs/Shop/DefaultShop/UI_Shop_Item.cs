using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbShop;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Toast;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_Shop_Item: UI_Base
    {
        private EventsManager _currencyEventHandler;

        protected IDbShop _item;

        protected virtual int GetToastId()
        {
            return 200032;
        }

        enum GameObjects
        {
            IMG_SoldOutBG,
            // IMG_Badge
        }
        
        protected enum Texts
        {
            T_Count,
            T_Cost
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            // Get<GameObject>((int) GameObjects.IMG_Badge).AddComponent<RepeatingScale>();
            
            _currencyEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenCurrencyChanged,
                updatedEntity = new[] {CurrencyController.data}
            });
            return true;
        }

        public virtual void SetInfo(IDbShop item)
        {
            if (!_isInit) Init();

            _item = item;
            
            var isInApp = _item.IsInApp();
            Util.FindChild<Image>(gameObject, "IMG_Item").sprite =
                Manager.Resource.Load<Sprite>(item.GetResource());
            var reward = item.GetRewards();
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text = reward[0].count.ToString("N0");
            
            Get<TextMeshProUGUI>((int)Texts.T_Cost).text = isInApp ? item.GetDisplayPrice() :
                item.GetPriceType() == CurrencyType.Ad ? LocalString.Get(210240) : item.GetPrice().ToString("N0");
            // Util.FindChild(gameObject, "IMG_Cost", true).SetActive(!isInApp);
            
            var buyBtn = Util.FindChild(gameObject, "B_Buy");
            if (isInApp)
            {
                buyBtn.BindEvent(BuyCondition, _ => BuyInApp(), UIEffectType.Bounce, false);
            }
            else
            {
                buyBtn.BindEvent(BuyCondition, _ => Buy(), UIEffectType.Bounce, false);
                Util.FindChild<Image>(gameObject, "IMG_Cost", true).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(item.GetPriceType()).Resource);
            }

            if (_item.GetRenewalInterval() == RenewalType.Infinite)
            {
                Get<GameObject>((int)GameObjects.IMG_SoldOutBG).SetActive(false);
                if (Get<TextMeshProUGUI>((int) Texts.T_Count) != null) Get<TextMeshProUGUI>((int)Texts.T_Count).text = string.Empty;
            }
            else WhenCurrencyChanged();
        }

        protected virtual void WhenCurrencyChanged()
        {
            var renewal = _item.GetRenewalInterval();
            if (renewal == RenewalType.Infinite) return;
            
            var canBuy = BuyCondition();
            Get<GameObject>((int)GameObjects.IMG_SoldOutBG).SetActive(!canBuy);
            if (!canBuy)
            {
                Timing.CallDelayed(0.1f, () => transform.SetAsLastSibling());
                _currencyEventHandler.Dispose();
                _currencyEventHandler = null;
            }
            // Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(!_item.IsInApp() && _item.GetPriceType() == CurrencyType.Ad && canBuy);
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = StringMaker.GetBuyLimitText(_item);

        }
        
        private bool BuyCondition()
        {
            return CurrencyController.I.CanBuy(_item);
        }

        private void BuyInApp()
        { 
            IAPManager.I.Buy(_item, WhenCurrencyChanged);
        }

        protected virtual void Buy()
        {
            if (_item.GetPriceType() == CurrencyType.Ad)
            {
                Manager.Ad.ShowAd(eAdType.Shop, BuyWithDelay);
            }
            else
            {
                BuyItem();
            }

            void BuyWithDelay()
            {
                Timing.CallDelayed(0.1f, BuyItem);
            }

            void BuyItem()
            {
                var isSuccess = CurrencyController.I.Buy(_item);
                if (isSuccess)
                {
                    var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
                    var rewardsForToast = new List<DbReward>();
            
                    var rewards = _item.GetRewards();
                    for (var idx = 0; idx < rewards.Count; ++idx)
                    {
                        rewardsForToast.Add(
                            new DbReward(rewards[idx].currencyType, rewards[idx].count, rewards[idx].id));
                    }
                    toast.SetReward(GetToastId(), rewardsForToast);
                }
                else
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200038);
                }
            }
        }

        protected virtual void OnEnable()
        {
            if (_currencyEventHandler != null)
                _currencyEventHandler?.Reconnect();

            if (_item != null && _item.IsInApp())
            {
                Get<TextMeshProUGUI>((int) Texts.T_Cost).text = _item.GetDisplayPrice();
            }
        }

        private void OnDisable()
        {
            _currencyEventHandler?.Dispose();
        }
    }
}