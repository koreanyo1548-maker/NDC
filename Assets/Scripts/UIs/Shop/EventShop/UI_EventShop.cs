using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Play;
using Data;
using Data.DbEvent;
using Data.DbShop;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Shop.DefaultShop;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Shop.EventShop
{
    public class UI_EventShop: UI_Popup
    {
        private EventsManager _eventShopCheckEventManager;
        private EventsManager _eventMoneyEventManager;
        
        private CoroutineHandle _eventShopTimeRoutine;
        
        enum Texts
        {
            T_Time,
            T_Count
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());

            var parent = Util.FindChild<Transform>(gameObject, "Content", true);
            DbDropEventShop.ForEach(s =>
            {
                var item = Manager.UI.MakeSubItem<UI_EventShop_Item>(parent);
                item.SetInfo(s);
            });
            
            _eventMoneyEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = OnEventMoneyChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.DropEventMoney)}
            });
            _eventShopCheckEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = OnEventShopChanged,
                updatedController = new[] {DropEventController.I.CanUseShop}
            });
            OnEventMoneyChanged();
            OnEventShopChanged();
            
            return true;
        }

        private void OnEventMoneyChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(CurrencyController.I.GetMoneyModel(CurrencyType.DropEventMoney).Value,
                3, 2);
        }
        
        private void OnEventShopChanged()
        {
            Timing.KillCoroutines(_eventShopTimeRoutine);
            if (DropEventController.I.CurEvent == 0)
            {
                ClosePopupUI();
                return;
            }
            
            _eventShopTimeRoutine = Timing.RunCoroutine(_ShopTimeRoutine().CancelWith(gameObject));
        }
        
        private IEnumerator<float> _ShopTimeRoutine()
        {
            var dropEvent = DbDropEvent.Get(DropEventController.I.CurEvent);
            while (true)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Time).text = string.Format(LocalString.Get(210367), dropEvent.GetLeftTime());
                yield return Timing.WaitForSeconds(dropEvent.GetNextUpdateSeconds());
            }
        }

        private void OnEnable()
        {
            _eventShopCheckEventManager?.Reconnect();
            _eventMoneyEventManager?.Reconnect();
        }

        private void OnDisable()
        {
            _eventShopCheckEventManager?.Dispose();
            _eventMoneyEventManager?.Dispose();
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