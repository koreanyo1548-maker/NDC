using System;
using Controller.Currency;
using Controller.Infos;
using Data.DbDefinition;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Costume
{
    public class UI_Costume_Item: UI_Base, ILanguageSet, IDayDiffChecker
    {
        private EventsManager _equipmentEventsManager;
        private EventsManager _equipEventHandler;
        private EventsManager _selectedEventHandler;

        private DbCostume _costume;
        private UI_CostumeSelected _selectedUI;

        enum GameObjects
        {
            IMG_Equip,
            IMG_DontHave,
            IMG_Selected
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<GameObject>(typeof(GameObjects));

            return true;
        }

        public void SetInfo(DateTime now, DbCostume costume, UI_CostumeSelected selected)
        {
            if (!_isInit) Init();
            _selectedUI = selected;

            _costume = costume;
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text = LocalString.Get(DbGrade.Get(_costume.Grade).NameId);
            Util.FindChild<Image>(gameObject, "IMG_Costume").sprite = costume.GetResource();
            Util.FindChild<Image>(gameObject, "IMG_Grade").sprite = Manager.Resource.Load<Sprite>(_costume.Grade.ToString());
            
            gameObject.BindEvent(() => !Get<GameObject>((int) GameObjects.IMG_Selected).activeSelf, _=>OnInfoButtonClicked(), UIEffectType.Bounce);
            
            _equipmentEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipmentChanged,
                updatedField = new[] { CurrencyController.data.Costumes }
            });
            _equipEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = new []{EquipController.data.BodyCostume, EquipController.data.WeaponCostume}
            });
            _selectedEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenSelectionChanged,
                updatedUI = new[] {selected.NowSelected()}
            });
            
            WhenEquipmentChanged();
            WhenEquipChanged();
            WhenSelectionChanged();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            
            CheckValidate(now);
        }

        private void CheckValidate(DateTime now)
        {
            if (!_costume.StartDate.Equals("0"))
            {
                if (now < _costume.GetStartTime())
                {
                    gameObject.SetActive(false);
                    Manager.DayDiff.Add(this);
                    return;
                }
            }

            gameObject.SetActive(true);
            Manager.DayDiff.Remove(this);
        }
        
        private void WhenEquipmentChanged()
        {
            Get<GameObject>((int)GameObjects.IMG_DontHave).SetActive(!CurrencyController.I.HaveCostume(_costume.Id));
        }

        private void WhenEquipChanged()
        {
            Get<GameObject>((int)GameObjects.IMG_Equip).SetActive(EquipController.I.IsEquipped(_costume.Position, _costume.Id));
        }

        private void WhenSelectionChanged()
        {
            Get<GameObject>((int) GameObjects.IMG_Selected).SetActive(_selectedUI.NowSelected().Value == _costume.Id);
        }
        
        private void OnInfoButtonClicked()
        {
            _selectedUI.Set(_costume.Id);
        }
        
        private void OnDisable()
        {
            _equipmentEventsManager.Dispose();
            _equipEventHandler.Dispose();
            _selectedEventHandler.Dispose();
        }

        private void OnEnable()
        {
            _equipmentEventsManager?.Reconnect();
            _equipEventHandler?.Reconnect();
            _selectedEventHandler?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text = LocalString.Get(DbGrade.Get(_costume.Grade).NameId);
        }

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff > 0)
            {
                CheckValidate(now);
            }
        }
    }
}