using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbNecklaceInfo;
using Data.DbUser.Equipment;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Lock;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Necklace
{
    public class UI_Necklace_Item : UI_Base, ILanguageSet
    {
        private EventsManager _necklaceEventsManager;
        private EventsManager _equipEventHandler;
        private EventsManager _selectedEventHandler;

        private DbUserNecklace _necklace;

        private UI_Star _starUI;

        enum Texts
        {
            T_Level,
            T_Count
        }

        enum GameObjects
        {
            IMG_Equip,
            IMG_DontHave
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();

            gameObject.BindEvent(() => _necklace.Have.Value, _ => ShowNecklaceInfo(), UIEffectType.Bounce);
            
            return true;
        }

        public void SetInfo(DbNecklace necklace)
        {
            if (!_isInit) Init();
            _necklace = DbUserNecklace.Get(necklace.Id);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text =
                LocalString.Get(DbGrade.Get(necklace.Grade).NameId);
            Util.FindChild<Image>(gameObject, "IMG_Necklace").sprite =
                Manager.Resource.Load<Sprite>(necklace.Resource);
            Util.FindChild<Image>(gameObject, "IMG_Grade").sprite =
                Manager.Resource.Load<Sprite>(necklace.Grade.ToString());
            
            Util.FindChild(gameObject, "IMG_Badge").GetOrAddComponent<UI_Badge>().Set(new DbField[] 
            {
                _necklace.Count, CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone)
            }, () => _necklace.CanUpgrade());

            _necklaceEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenNecklaceChanged,
                updatedEntity = new[] {_necklace}
            });
            _equipEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = EquipController.data.Necklaces.Value.ToArray()
            });

            WhenNecklaceChanged();
            WhenEquipChanged();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenNecklaceChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Level).text =
                string.Format(LocalString.Get(210041), _necklace.Growth.Value);
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = Define.AddUnit(_necklace.Count.Value, 3, 2);
            Get<GameObject>((int) GameObjects.IMG_DontHave).SetActive(!_necklace.Have.Value);

            _starUI.Set(_necklace.Awakening.Value);
        }

        private void ShowNecklaceInfo()
        {
            Manager.UI.ShowPopupUI<UI_NecklaceGrowth>().Set(_necklace);
        }

        private void WhenEquipChanged()
        {
            Get<GameObject>((int) GameObjects.IMG_Equip).SetActive(
                _necklace.Have.Value && EquipController.I.IsEquipped(_necklace));
        }

        private void OnDisable()
        {
            _necklaceEventsManager?.Dispose();
            _equipEventHandler?.Dispose();
            _selectedEventHandler?.Dispose();
        }

        private void OnEnable()
        {
            _necklaceEventsManager?.Reconnect();
            _equipEventHandler?.Reconnect();
            _selectedEventHandler?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Grade").text =
                LocalString.Get(DbGrade.Get(_necklace.Meta.Grade).NameId);
        }
    }
}