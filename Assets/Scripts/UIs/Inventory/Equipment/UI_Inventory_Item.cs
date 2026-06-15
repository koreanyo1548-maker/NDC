using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbUser.Equipment;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;
using Util = Utils.Util;

namespace UIs.Inventory.Equipment
{
    public class UI_Inventory_Item: UI_Base, ILanguageSet
    {
        private EventsManager _equipmentEventsManager;
        private EventsManager _equipEventHandler;
        private EventsManager _selectedEventHandler;

        private EquipmentType _equipmentType;
        private IDbUserEquipment _equipment;

        private IUI_InventorySelected _selectedUI;

        private UI_Star _starUI;
        
        enum Texts
        {
            T_Level,
            T_Count
        }

        enum GameObjects
        {
            IMG_Badge,
            IMG_Equip,
            IMG_DontHave,
            IMG_Selected
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Get<GameObject>((int) GameObjects.IMG_Badge).AddComponent<RepeatingScale>();
            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();

            return true;
        }

        public void SetInfo(IDbEquipment equipment, IUI_InventorySelected selected)
        {
            if (!_isInit) Init();
            _selectedUI = selected;
            
            _equipmentType = equipment.GetEquipmentType();
            _equipment = DbSelector.GetUserEquipment(_equipmentType, equipment.GetId());
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text = LocalString.Get(DbGrade.Get(equipment.GetGrade()).NameId);
            Util.FindChild<Image>(gameObject, "IMG_Equipment").sprite = Manager.Resource.Load<Sprite>(equipment.GetResource());
            Util.FindChild<Image>(gameObject, "IMG_Grade").sprite = Manager.Resource.Load<Sprite>(equipment.GetGrade().ToString());
            
            gameObject.BindEvent(() => !Get<GameObject>((int) GameObjects.IMG_Selected).activeSelf, _=>OnInfoButtonClicked(), UIEffectType.Bounce);
            
            _equipmentEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipmentChanged,
                updatedEntity = new[] { _equipment.GetModel() }
            });
            _equipEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = _equipmentType == EquipmentType.Weapon ? new DbField[] {EquipController.data.Weapon} :
                    _equipmentType == EquipmentType.Accessory ? new DbField[] {EquipController.data.Accessory} :
                    EquipController.data.Skills.ToArray(),
                updatedUI = _equipmentType == EquipmentType.Skill ? new UIField[] {_selectedUI.GetPresetIdx()} : null
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
        }

        private void WhenEquipmentChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), _equipment.GetGrowth());
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = Define.AddUnit(_equipment.GetCount(), 3, 2);
            Get<GameObject>((int)GameObjects.IMG_DontHave).SetActive(!_equipment.GetHave());
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(BadgeController.I.IsEquipment(_equipmentType, _equipment.GetId()));
            
            var awakening = _equipment.GetAwakening();
            _starUI.Set(awakening);
        }

        private void WhenEquipChanged()
        {
            var preset = _selectedUI.GetPresetIdx();
            Get<GameObject>((int)GameObjects.IMG_Equip).SetActive(
                _equipment.GetHave() && EquipController.I.IsEquipped(_equipmentType, _equipment.GetId(), preset == null ? 0 : preset.Value));
        }

        private void WhenSelectionChanged()
        {
            Get<GameObject>((int) GameObjects.IMG_Selected).SetActive(_selectedUI.NowSelected().Value == _equipment.GetId());
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(BadgeController.I.IsEquipment(_equipmentType, _equipment.GetId()));
        }
        
        private void OnInfoButtonClicked()
        {
            _selectedUI.Set(_equipmentType, _equipment.GetId());
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
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text = LocalString.Get(DbGrade.Get(_equipment.GetMeta().GetGrade()).NameId);
        }
    }
}