using System;
using Controller;
using Controller.Currency;
using Data.DbShop;
using ThirdParty;
using TMPro;
using UIBases;
using UnityEngine;
using Utils;

namespace UIs.Shop.AdShop
{
    public class UI_AdShopNoAds: UI_Base
    {
        private IDbShop _item;

        enum GameObjects
        {
            IMG_SoldOutBG
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<GameObject>(typeof(GameObjects));
            return true;
        }

        public void SetInfo(IDbShop item)
        {
            if (!_isInit) Init();

            _item = item;

            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Cost", true).text = item.GetDisplayPrice();
            Util.FindChild(gameObject, "B_Buy").BindEvent(BuyCondition, _ => Buy(), UIEffectType.Bounce);
            SetCount();
        }

        private bool BuyCondition()
        {
            return CurrencyController.I.CanBuy(_item);
        }

        private void Buy()
        {
            IAPManager.I.Buy(_item, SetCount);
        }
        
        private void SetCount()
        {
            Get<GameObject>((int)GameObjects.IMG_SoldOutBG).SetActive(!BuyCondition());
        }

        private void OnEnable()
        {
            if (_isInit) SetCount();
        }
    }
}