using System;
using Controller;
using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Skill
{
    public class UI_UnlockSKillPreset: UI_Popup
    {
        private EventsManager _diaEventManager;
        private DbInGameShop _shop;

        private Action _whenBuy;

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
            
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Get<Image>((int)Images.B_Unlock).gameObject.BindEvent(UnlockCondition, OnUnlockButtonClicked, UIEffectType.Bounce);
                
            
            _diaEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenDiaChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Dia)}
            });

            return true;
        }

        public void Set(CurrencyType skillPreset, Action whenBuy)
        {
            if (!_isInit) Init();

            _shop = DbInGameShop.Get(Define.GetShopId(skillPreset));
            Get<TextMeshProUGUI>((int)Texts.T_Cost).text = _shop.GetPrice().ToString("N0");
            _whenBuy = whenBuy;
            
            WhenDiaChanged();
        }

        private void OnUnlockButtonClicked(PointerEventData eventData)
        {
            if (CurrencyController.I.Buy(_shop))
            {
                _whenBuy();
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200027);
                ClosePopupUI();
            }
        }
        
        private void WhenDiaChanged()
        {
            Get<Image>((int)Images.B_Unlock).material = Define.GetUIMaterial(!UnlockCondition());
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
        private bool UnlockCondition()
        {
            return CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value >= _shop.GetPrice();
        }
        
        private void OnEnable()
        {
            _diaEventManager?.Reconnect();
        }
        private void OnDisable()
        {
            _diaEventManager.Dispose();
        }
    }
}