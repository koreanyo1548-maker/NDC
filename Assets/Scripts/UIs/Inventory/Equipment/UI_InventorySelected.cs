using Controller.Currency;
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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Equipment
{
    public class UI_InventorySelected: UI_Base, IUI_InventorySelected
    {
        private EventsManager _equipmentEventsManager;
        private EventsManager _equipEventsManager;
        
        private IDbEquipment _meta;
        private IDbUserEquipment _equipment;

        private UI_Star _starUI;
        
        public UIField<bool> Check = new(false);
        
        private UIField<int> _nowSelected = new(-1);
        public bool IsWeapon => _meta.GetEquipmentType() == EquipmentType.Weapon;
        public bool CanMerge => _equipment.CanMerge();
        public bool CanGrowth => _equipment.GetHave() && !_equipment.IsMaxGrowth(true) && CurrencyController.I.GetEquipGrowthStoneCount(_meta.GetEquipmentType()) >= _equipment.GetGrowthStoneCount();
        public bool CanAwakening => _equipment.CanAwakening() && CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone).Value >= _equipment.GetAwakeningStoneCount();

        enum Texts
        {
            T_Name,
            T_EquipEffect,
            T_OwnEffect,
            T_AwakeningLevel,
            T_Grade,
            T_Level,
            T_Count,
            T_Equip
        }

        enum Images
        {
            IMG_Grade,
            IMG_Equipment,
            B_Equip,
            B_Merge
        }

        enum GameObjects
        {
            IMG_DontHave,
            B_LeftMove,
            B_RightMove,
            B_Equip,
            B_Merge,
            B_Growth,
            B_Awakening
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            Get<GameObject>((int)GameObjects.B_LeftMove).BindEvent(Functions.TrueCondition, PrevEquipment, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_RightMove).BindEvent(Functions.TrueCondition, NextEquipment, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Equip).BindEvent(EquipCondition, TryEquip, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Merge).BindEvent(() => _meta.GetNext() != -1 , TryMerge, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Growth).BindEvent(Functions.TrueCondition , TryGrowth, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Awakening).BindEvent(Functions.TrueCondition , TryAwakening, UIEffectType.Bounce);

            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();

            return true;
        }

        private void PrevEquipment(PointerEventData eventData)
        {
            var prev = _meta.GetPrev();
            if (prev == -1) return;
            Set(_meta.GetEquipmentType(), prev);
            Check.Value = !Check.Value;
        }

        private void NextEquipment(PointerEventData eventData)
        {
            var next = _meta.GetNext();
            if (next == -1) return;
            Set(_meta.GetEquipmentType(), next);
            Check.Value = !Check.Value;
        }
        public UIField<int> NowSelected()
        {
            return _nowSelected;
        }

        public void Set(EquipmentType equipmentType, int id)
        { 
            if (!_isInit) Init();

            _nowSelected.Value = id;
            _meta = DbSelector.GetEquipment(equipmentType, id);
            _equipment = DbSelector.GetUserEquipment(equipmentType, id);

            Get<Image>((int)Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(_meta.GetResource());
            Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(_meta.GetGrade().ToString());
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text =  LocalString.Get(DbGrade.Get(_meta.GetGrade()).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_meta.GetNameId());
            Get<GameObject>((int)GameObjects.B_LeftMove).SetActive(_meta.GetPrev() != -1);
            Get<GameObject>((int)GameObjects.B_RightMove).SetActive(_meta.GetNext() != -1);
            Get<Image>((int) Images.B_Merge).material = Define.GetUIMaterial(id == _meta.GetCount());
            
            if (_equipmentEventsManager == null)
            {
                _equipmentEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenEquipmentChanged,
                    updatedEntity =  new []{_equipment.GetModel()}
                });
            }
            else
            {
                _equipmentEventsManager.Set(WhenEquipmentChanged, new[] {_equipment.GetModel()});
            }
            if (_equipEventsManager == null)
            {
                _equipEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenEquipChanged,
                    updatedField = new []{equipmentType == EquipmentType.Weapon ? EquipController.data.Weapon : EquipController.data.Accessory}
                });
            }
            else
            {
                _equipEventsManager.Set(WhenEquipChanged, new []{equipmentType == EquipmentType.Weapon ? EquipController.data.Weapon : EquipController.data.Accessory});
            }
            
            WhenEquipmentChanged();
            WhenEquipChanged();

            BadgeController.I.CheckEquipment(equipmentType, id);
            
            gameObject.SetActive(true);
            
            Manager.Guide.Check(this);
        }

        public UIField<int> GetPresetIdx()
        {
            return null;
        }

        private void WhenEquipChanged()
        {
            var isEquipped = EquipController.I.IsEquipped(_meta.GetEquipmentType(), _meta.GetId());
            Get<Image>((int) Images.B_Equip).material = Define.GetUIMaterial(isEquipped);
            Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(isEquipped ? 210206 : 210059);
        }

        private void WhenEquipmentChanged()
        {
            var have = _equipment.GetHave();
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), _equipment.GetGrowth());

            var awakening = _equipment.GetAwakening();
            Get<TextMeshProUGUI>((int) Texts.T_AwakeningLevel).text = string.Format(LocalString.Get(210081), awakening);
            _starUI.Set(awakening);
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = !have ? string.Empty : Define.AddUnit(_equipment.GetCount(), 3, 2);

            var growth = _equipment.GetGrowth();
            Get<TextMeshProUGUI>((int) Texts.T_EquipEffect).text = StringMaker.GetFinalString(_meta.GetEquipStatType(), _meta.GetEquipStat() + _meta.GetEquipGrowthStat() * (growth-1));
            Get<TextMeshProUGUI>((int) Texts.T_OwnEffect).text = StringMaker.GetFinalString(_meta.GetOwnStatType(), _meta.GetOwnStat() + _meta.GetOwnGrowthStat() * (growth-1));

            Get<GameObject>((int)GameObjects.IMG_DontHave).SetActive(!have);

            if (Get<GameObject>((int) GameObjects.B_Equip).activeSelf != have)
            {
                for (var idx = (int) GameObjects.B_Equip; idx <= (int) GameObjects.B_Awakening; ++idx)
                {
                    Get<GameObject>(idx).SetActive(have);
                }
            }
        }

        private bool EquipCondition()
        {
            return !EquipController.I.IsEquipped(_meta.GetEquipmentType(), _equipment.GetId());
        }

        private void TryEquip(PointerEventData eventData)
        {
            if (_equipment.GetHave())
            {
                EquipController.I.Equip(_meta.GetEquipmentType(), _equipment.GetId());
            }
        }
        
        private void TryMerge(PointerEventData eventData)
        {
            if (_equipment.GetHave())
            {
                var uiEquipMerge = Manager.UI.ShowPopupUI<UI_EquipMerge>();
                uiEquipMerge.Set(_meta.GetEquipmentType() == EquipmentType.Weapon, _equipment);
            }
        }
        
        private void TryGrowth(PointerEventData eventData)
        {
            if (_equipment.GetHave())
            {
                var uiEquipGrowth = Manager.UI.ShowPopupUI<UI_EquipGrowth>();
                uiEquipGrowth.Set(_meta.GetEquipmentType(), _equipment);
            }
        }
        
        private void TryAwakening(PointerEventData eventData)
        {
            if (_equipment.GetHave())
            {
                var uiEquipAwakening = Manager.UI.ShowPopupUI<UI_EquipAwakening>();
                uiEquipAwakening.Set(_meta.GetEquipmentType(), _equipment);
            }
        }

        private void OnDisable()
        {
            _equipEventsManager?.Dispose();
            _equipmentEventsManager?.Dispose();
            Check.ValueChanged = null;
        }

        private void OnEnable()
        {
            _equipEventsManager?.Reconnect();
            _equipmentEventsManager?.Reconnect();
        }
    }
}