using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbShop;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Shop.DefaultShop;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Shop.EventPackage
{
    public class UI_OncePackage: UI_Popup
    {
        private DbInAppShop _item;
        enum Texts
        {
            T_ResetTime
        }

        enum Transforms
        {
            G_PackageRewards
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Transform>(typeof(Transforms));
            
            return true;
        }

        public void Set(DbInAppShop item)
        {
            if (!_isInit) Init();
            
            _item = item;
            
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Cost", true).text = item.GetDisplayPrice();
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Limit", true).text = StringMaker.GetBuyLimitText(item);
            Util.FindChild(gameObject, "B_Buy", true).BindEvent(Functions.TrueCondition, _ => Buy(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());

            var rewards = item.Reward;
            if (rewards.Count > 1)
            {
                for (var idx = 0; idx < rewards.Count; ++idx)
                {
                    Get<Transform>((int)Transforms.G_PackageRewards).GetChild(idx).gameObject.GetOrAddComponent<UI_Package_Item>()
                        .Set(DbCurrency.Get(rewards[idx].currencyType).GetResource(), rewards[idx].count);
                }
            }
            
            PlayFabManager.Store.DoWithTime(now =>
            {
                // CurrencyController.I.StartEventPackage(now, item);
                Timing.RunCoroutine(_ResetTimeRoutine(now).CancelWith(gameObject));
            });
        }

        private void Buy()
        {
            IAPManager.I.Buy(_item, ClosePopupUI);
        }
        
        IEnumerator<float> _ResetTimeRoutine(DateTime now)
        {
            var nextDay = now.AddHours(24);
            var resetTime = nextDay - now;
            while (true)
            {
                if (resetTime.TotalSeconds <= 0) break;
                Get<TextMeshProUGUI>((int) Texts.T_ResetTime).text = resetTime.ToString(@"hh\:mm\:ss");
                yield return Timing.WaitForSeconds(1);
                resetTime = resetTime.Subtract(Define.ASecond);
            }
            ClosePopupUI();
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
            
        }
    }
}