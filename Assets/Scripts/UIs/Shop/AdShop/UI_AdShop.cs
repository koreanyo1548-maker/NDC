using System;
using System.Collections.Generic;
using Data;
using Data.DbShop;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Shop.DefaultShop;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Shop.AdShop
{
    public class UI_AdShop: UI_Popup
    {
        enum Texts
        {
            T_ResetTime
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            
            transform.Find("IMG_Dimmed").gameObject.BindEvent(Functions.TrueCondition, _=>ClosePopupUI());

            var adParent = Util.FindChild<Transform>(gameObject, "G_AdParent", true);

            adParent.Find("IMG_NoAds").gameObject.GetOrAddComponent<UI_AdShopNoAds>().SetInfo(DbInAppShop.Get(1001));
            DbInGameShop.ForEach(SetItem);
            void SetItem(IDbShop shopItem)
            {
                if (shopItem.GetCategory() == ShopCategoryType.Ad)
                {
                    var item = Manager.UI.MakeSubItem<UI_ShopAd_Item>(adParent);
                    item.SetInfo(shopItem);
                } 
            }
            
            return true;
        }

        public void Set()
        {
            if (!_isInit) Init();
            
            Timing.RunCoroutine(_ResetTimeRoutine().CancelWith(gameObject));
        }

        IEnumerator<float> _ResetTimeRoutine()
        {
            var curTime = DateTime.UtcNow.AddHours(9);
            var nextDay = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0).AddDays(1);
            var resetTime = nextDay - curTime;
            while (true)
            {
                if (resetTime.TotalSeconds <= 0) break;
                Get<TextMeshProUGUI>((int) Texts.T_ResetTime).text = resetTime.ToString(@"hh\:mm\:ss");
                yield return Timing.WaitForSeconds(1);
                resetTime = resetTime.Subtract(Define.ASecond);
            }
            Timing.RunCoroutine(_ResetTimeRoutine().CancelWith(gameObject));
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