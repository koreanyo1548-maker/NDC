using System;
using Controller;
using Controller.Currency;
using Data;
using Data.DbPetInfo;
using Data.DbShop;
using Data.DbUser.Currency;
using Managers;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Pet
{
    public class UI_UnlockBookshelf: UI_Popup
    {
        private EventsManager _diaEventManager;
        private DbInGameShop _shop;
        private Action _afterBuy;
        
        enum Images
        {
            B_Unlock
        }
        enum Texts
        {
            T_Cost
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Get<Image>((int)Images.B_Unlock).gameObject.BindEvent(UnlockCondition, OnUnlockButtonClicked, UIEffectType.Bounce);
            
            
            _diaEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenDiaChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Dia)}
            });

            return true;
        }


        public void Set(CurrencyType buy, Action afterBuy)
        {
            if (!_isInit) Init();

            _shop = DbInGameShop.Get(Define.GetShopId(buy));
            _afterBuy = afterBuy;
            Get<TextMeshProUGUI>((int)Texts.T_Cost).text = _shop.GetPrice().ToString("N0");
            WhenDiaChanged();
        }


        private void OnUnlockButtonClicked(PointerEventData eventData)
        {
            if (CurrencyController.I.Buy(_shop))
            {
                _afterBuy();
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200023);
                ClosePopupUI();
            }
        }
        
        private void WhenDiaChanged()
        {
            Get<Image>((int)Images.B_Unlock).material = Define.GetUIMaterial(!UnlockCondition());
        }
        
        private bool UnlockCondition()
        {
            return CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value >= _shop.GetPrice();
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