using System.Collections.Generic;
using Controller.Currency;
using Data;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Shop.BlackMarket
{
    public class UI_BlackMarket: UI_Popup
    {
        private EventsManager _coinEventManager;

        private int _curVersion = 0;
        private Dictionary<int, List<UI_BlackMarket_Item>> _items = new() {{1, new()}, {2, new()}};
        
        enum Texts
        {
            T_Count,
            T_Title
        }

        enum GameObjects
        {
            T_Info
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());

            var parent = Util.FindChild<Transform>(gameObject, "Content", true);
            DbBlackMarket.ForEach(s =>
            {
                var item = Manager.UI.MakeSubItem<UI_BlackMarket_Item>(parent);
                item.SetInfo(s, CheckItem);
                _items[_items[1].Count < 6 ? 1 : 2].Add(item);
            });
            
            _coinEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = OnBlackMarketCoinChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.BlackMarketCoin)}
            });
            OnBlackMarketCoinChanged();
            CheckItem();
            
            return true;
        }

        public void CheckItem()
        {
            if (_curVersion == 2) return;
            
            var isPhaseTwo = true;
            for (var id = 5001; id <= 5006; ++id)
            {
                if (CurrencyController.I.CanBuy(DbBlackMarket.Get(id)))
                {
                    isPhaseTwo = false;
                    break;
                }
            }
            
            if (_curVersion == 1 && !isPhaseTwo) return;
            
            foreach (var item in _items[1]) item.gameObject.SetActive(!isPhaseTwo);
            foreach (var item in _items[2]) item.gameObject.SetActive(isPhaseTwo);
            Get<TextMeshProUGUI>((int)Texts.T_Title).text = LocalString.Get(isPhaseTwo ? 210415 : 210394);
            Get<GameObject>((int)GameObjects.T_Info).SetActive(!isPhaseTwo);
        }

        private void OnBlackMarketCoinChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(CurrencyController.I.GetMoneyModel(CurrencyType.BlackMarketCoin).Value,
                3, 2);
        }

        private void OnEnable()
        {
            _coinEventManager?.Reconnect();
        }

        private void OnDisable()
        {
            _coinEventManager?.Dispose();
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