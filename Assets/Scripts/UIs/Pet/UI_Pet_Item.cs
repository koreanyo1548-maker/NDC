using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbPetInfo;

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

namespace UIs.Pet
{
    public class UI_Pet_Item: UI_Base, ILanguageSet
    {
        private EventsManager _equipmentEventsManager;
        private EventsManager _equipEventHandler;

        private DbUserPet _pet;

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
        
        void Start()
        {
            Init();
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

        public void Set(DbPet pet)
        {
            if (!_isInit) Init();
            
            _pet = DbUserPet.Get(pet.Id);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text = LocalString.Get(DbGrade.Get(pet.GetGrade()).NameId);
            Util.FindChild<Image>(gameObject, "IMG_Equipment").sprite = Manager.Resource.Load<Sprite>(pet.GetResource());
            Util.FindChild<Image>(gameObject, "IMG_Grade").sprite = Manager.Resource.Load<Sprite>(pet.GetGrade().ToString());
            
            gameObject.BindEvent(Functions.TrueCondition, _=>OnInfoButtonClicked(), UIEffectType.Bounce);
            
            _equipmentEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipmentChanged,
                updatedEntity = new[] { _pet.GetModel() }
            });
            _equipEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = EquipController.data.Pets.Value.ToArray()
            });
            
            WhenEquipmentChanged();
            WhenEquipChanged();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenEquipmentChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), _pet.GetGrowth());
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = Define.AddUnit(_pet.GetCount(), 3, 2);
            Get<GameObject>((int)GameObjects.IMG_DontHave).SetActive(!_pet.GetHave());
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(false);
            Get<GameObject>((int)GameObjects.IMG_Selected).SetActive(false);
            // Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(BadgeController.I.IsEquipment(_equipmentType, _pet.GetId()));
            
            var awakening = _pet.GetAwakening();
            _starUI.Set(awakening);
        }

        private void WhenEquipChanged()
        {
            Get<GameObject>((int)GameObjects.IMG_Equip).SetActive(
                _pet.GetHave() && EquipController.I.IsEquipped(EquipmentType.Pet, _pet.GetId()));
        }
        
        private void OnInfoButtonClicked()
        {
            Manager.UI.ShowPopupUI<UI_Pet>().Set(_pet);
        }
        
        private void OnDisable()
        {
            _equipmentEventsManager.Dispose();
            _equipEventHandler.Dispose();
        }

        private void OnEnable()
        {
            _equipmentEventsManager?.Reconnect();
            _equipEventHandler?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text = LocalString.Get(DbGrade.Get(_pet.Meta.GetGrade()).NameId);
        }
    }
}