using System;
using System.Collections.Generic;
using Controller.Currency;
using Data;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Character;
using UIs.FieldMain;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Summon
{
    public class UI_Summon: UI_Popup
    {
        private EventsManager _weaponTicketHandler;
        private EventsManager _accessoryTicketHandler;
        private EventsManager _skillTicketHandler;
        private EventsManager _relicTicketHandler;
        private EventsManager _necklaceTicketHandler;
        
        private List<UI_Summon_Item> _items = new();

        enum Texts
        {
            T_WeaponSummonTicket,
            T_AccessorySummonTicket,
            T_SkillSummonTicket,
            T_RelicSummonTicket,
            T_NecklaceSummonTicket
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));

            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            var summonParent = Util.FindChild<Transform>(gameObject, "G_SummonParent", true);

            var idx = 0;
            foreach (SummonType summon in Enum.GetValues(typeof(SummonType)))
            {
                var item = summonParent.GetChild(idx).gameObject.GetOrAddComponent<UI_Summon_Item>();
                item.SetInfo(summon);
                _items.Add(item);
                idx++;
            }

            _weaponTicketHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenWeaponTicketChanged,
                updatedField = new DbField[] {CurrencyController.I.GetTicketModel(CurrencyType.WeaponSummonTicket)}
            });
            _accessoryTicketHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenAccessoryTicketChanged,
                updatedField = new DbField[] {CurrencyController.I.GetTicketModel(CurrencyType.AccessorySummonTicket)}
            });
            _skillTicketHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenSkillTicketChanged,
                updatedField = new DbField[] {CurrencyController.I.GetTicketModel(CurrencyType.SkillSummonTicket)}
            });
            _relicTicketHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenRelicTicketChanged,
                updatedField = new DbField[] {CurrencyController.I.GetTicketModel(CurrencyType.RelicSummonTicket)}
            });
            _necklaceTicketHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenNecklaceTicketChanged,
                updatedField = new DbField[] {CurrencyController.I.GetMoneyModel(CurrencyType.BeadsOre)}
            });
            WhenWeaponTicketChanged();
            WhenAccessoryTicketChanged();
            WhenSkillTicketChanged();
            WhenRelicTicketChanged();
            WhenNecklaceTicketChanged();
            return true;
        }

        private void WhenWeaponTicketChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_WeaponSummonTicket).text = Define.AddUnit(CurrencyController.I.GetTicketModel(CurrencyType.WeaponSummonTicket).Value, 3, 2);
        }
        private void WhenAccessoryTicketChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_AccessorySummonTicket).text = Define.AddUnit(CurrencyController.I.GetTicketModel(CurrencyType.AccessorySummonTicket).Value, 3, 2);
        }
        private void WhenSkillTicketChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_SkillSummonTicket).text = Define.AddUnit(CurrencyController.I.GetTicketModel(CurrencyType.SkillSummonTicket).Value, 3, 2);
        }
        private void WhenRelicTicketChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_RelicSummonTicket).text = Define.AddUnit(CurrencyController.I.GetTicketModel(CurrencyType.RelicSummonTicket).Value, 3, 2);
        }

        private void WhenNecklaceTicketChanged()
        {
            Get<TextMeshProUGUI>((int)Texts.T_NecklaceSummonTicket).text = Define.AddUnit(CurrencyController.I.GetMoneyModel(CurrencyType.BeadsOre).Value, 3, 2);
        }

        private void OnEnable()
        {
            _weaponTicketHandler?.Reconnect();
            _accessoryTicketHandler?.Reconnect();
            _skillTicketHandler?.Reconnect();
            _relicTicketHandler?.Reconnect();
            _necklaceTicketHandler?.Reconnect();
        }

        private void OnDisable()
        {
            _weaponTicketHandler?.Dispose();
            _accessoryTicketHandler?.Dispose();
            _skillTicketHandler?.Dispose();
            _relicTicketHandler?.Dispose();
            _necklaceTicketHandler?.Dispose();
        }

        public override void WhenPopupClosed()
        {
            Manager.UI.GetSceneUI<UI_MainBottom>().CloseInnerPopup();
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}