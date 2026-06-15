using System;
using Data.DbShop;
using Managers;
using UIBases;
using UIs.Shop.DefaultShop;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Shop.EventPackage
{
    public class UI_NewbiePackage: UI_Popup
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            var shop = Util.FindChild(gameObject, "UI_ShopPackage_Item", true).GetOrAddComponent<UI_ShopPackage_Item>();
            shop.SetInfo(DateTime.UtcNow.AddHours(9), DbInAppShop.Get(1026), WhenPurchased);
            
            Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());

            return true;
        }

        private void WhenPurchased()
        {
            var shop = Manager.UI.GetPopupUI<UI_Shop>();
            if (shop != null)
            {
                shop.gameObject.SetActive(false);
                shop.gameObject.SetActive(true);
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