using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEquipment;

using Data.DbUser;
using Data.DbUser.Equipment;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Skill
{
    public class UI_SkillInventorySelected: UI_Base, IUI_InventorySelected, ILanguageSet
    {
        private EventsManager _equipmentEventsManager;
        private EventsManager _equipEventsManager;
        
        private Sprite[] _equipBtnSprites;

        private UI_EquipSkill _equipUI;

        private DbSkill _meta;
        private DbUserSkill _equipment;
        
        private UI_Star _starUI;

        public UIField<bool> Check = new(false);
        
        private UIField<int> _nowSelected = new(-1);
        
        private UIField<int> _preset = new(0);
        public bool CanMerge => _equipment.CanMerge();
        public bool CanGrowth => _equipment.GetHave() && !_equipment.IsMaxGrowth(true) && CurrencyController.I.GetEquipGrowthStoneCount(_meta.GetEquipmentType()) >= _equipment.GetGrowthStoneCount();
        public bool CanAwakening => _equipment.CanAwakening() && CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone).Value >= _equipment.GetAwakeningStoneCount();

        enum Texts
        {
            T_Name,
            T_EquipEffect,
            T_AwakeningLevel,
            T_Grade,
            T_Level,
            T_Count,
            T_Equip,
            T_CoolTime
        }

        enum Images
        {
            IMG_Grade,
            IMG_Equipment,
            B_Equip,
            B_Decompose
        }

        enum GameObjects
        {
            IMG_DontHave,
            B_LeftMove,
            B_RightMove,
            B_Equip,
            B_Growth,
            B_Awakening,
            B_Decompose
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            _equipBtnSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_DefaultButton_round3"), Manager.Resource.Load<Sprite>("UI_DefaultButton_round4")};
            
            Get<GameObject>((int)GameObjects.B_LeftMove).BindEvent(Functions.TrueCondition, PrevEquipment, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_RightMove).BindEvent(Functions.TrueCondition, NextEquipment, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Equip).BindEvent(Functions.TrueCondition, TryEquip, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Growth).BindEvent(Functions.TrueCondition , TryGrowth, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Awakening).BindEvent(Functions.TrueCondition , TryAwakening, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Decompose).BindEvent(Functions.TrueCondition , TryDecompose, UIEffectType.Bounce);
            
            _equipEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = EquipController.data.Skills.ToArray()
            });
            
            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            
            return true;
        }

        public void SetEquipSkillUI(UI_EquipSkill equipSkill)
        {
            _equipUI = equipSkill;
        }
        
        private void PrevEquipment(PointerEventData eventData)
        {
            var prev = _meta.PrevId;
            if (prev == -1) return;
            Set(EquipmentType.Skill, prev);
            Check.Value = !Check.Value;
        }

        private void NextEquipment(PointerEventData eventData)
        {
            var next = _meta.NextId;
            if (next == -1) return;
            Set(EquipmentType.Skill, next);
            Check.Value = !Check.Value;
        }

        public UIField<int> NowSelected()
        {
            return _nowSelected;
        }
        public UIField<int> GetPresetIdx()
        {
            return _preset;
        }

        public void Set(EquipmentType type, int id)
        { 
            _nowSelected.Value = id;
            _meta = DbSkill.Get(id);
            _equipment = DbUserSkill.Get(id);
            
            if (!_isInit) Init();


            Get<Image>((int)Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(_meta.GetResource());
            Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(_meta.GetGrade().ToString());
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text =  LocalString.Get(DbGrade.Get(_meta.GetGrade()).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_meta.GetNameId());
            Get<TextMeshProUGUI>((int) Texts.T_CoolTime).text = string.Format(LocalString.Get(210018), _meta.CoolTime);
            Get<GameObject>((int)GameObjects.B_LeftMove).SetActive(_meta.PrevId != -1);
            Get<GameObject>((int)GameObjects.B_RightMove).SetActive(_meta.NextId != -1);
            
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

            _preset.ValueChanged += WhenEquipChanged;
            
            WhenEquipmentChanged();
            WhenEquipChanged();

            BadgeController.I.CheckSkill(id);
            
            gameObject.SetActive(true);
            
            Manager.Guide.Check(this);
        }


        private void WhenEquipChanged()
        {
            var isEquipped = EquipController.I.IsEquipped(_meta.GetEquipmentType(), _meta.GetId(), _preset.Value);
            Get<Image>((int) Images.B_Equip).sprite = _equipBtnSprites[isEquipped ? 0 : 1];
            Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(isEquipped ? 210085 : 210086);
        }

        private void WhenEquipmentChanged()
        {
            var have = _equipment.GetHave();
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), _equipment.GetGrowth());
            
            var awakening = _equipment.GetAwakening();
            Get<TextMeshProUGUI>((int) Texts.T_AwakeningLevel).text = string.Format(LocalString.Get(210081), awakening);
            _starUI.Set(awakening);
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = !have ? string.Empty : _equipment.GetCount().ToString();

            var growth = _equipment.GetGrowth();
            Get<TextMeshProUGUI>((int) Texts.T_EquipEffect).text = _meta.GetOwnDescription(growth, false);
            // Get<TextMeshProUGUI>((int) Texts.T_OwnAttack).text = StringMaker.GetFinalString(StatType.Attack, _meta.GetOwnAttack() + _meta.GetOwnGrowthAttack() * growth);
            // Get<TextMeshProUGUI>((int) Texts.T_OwnHp).text = StringMaker.GetFinalString(StatType.Hp, _meta.GetOwnHp() + _meta.GetOwnGrowthHp() * growth);

            Get<GameObject>((int)GameObjects.IMG_DontHave).SetActive(!have);

            if (Get<GameObject>((int) GameObjects.B_Equip).activeSelf != have)
            {
                for (var idx = (int) GameObjects.B_Equip; idx <= (int) GameObjects.B_Decompose; ++idx)
                {
                    Get<GameObject>(idx).SetActive(have);
                }
            }
            
            Get<Image>((int)Images.B_Decompose).material = Define.GetUIMaterial(awakening < 5 || _equipment.GetCount() == 0);
        }

        private void TryEquip(PointerEventData eventData)
        {
            if (Manager.Field.CurField.Value == FieldType.Stage)
            {
                if (EquipController.I.IsEquipped(_equipment, _preset.Value))
                {
                    EquipController.I.RemoveSkillEquip(_equipment, _preset.Value);
                }
                else
                {
                    _equipUI.OpenEquipChange();
                }
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200028);
            }
        }
        
        private void TryGrowth(PointerEventData eventData)
        {
            if (_equipment.GetHave())
            {
                var uiEquipGrowth = Manager.UI.ShowPopupUI<UI_EquipGrowthSP>();
                uiEquipGrowth.Set(EquipmentType.Skill, _equipment);
            }
        }
        
        private void TryAwakening(PointerEventData eventData)
        {
            if (_equipment.Have.Value)
            {
                var uiEquipAwakening = Manager.UI.ShowPopupUI<UI_EquipAwakening>();
                uiEquipAwakening.Set(_meta.GetEquipmentType(), _equipment);
            }
        }

        private void TryDecompose(PointerEventData eventData)
        {
            if (_equipment.Awakening.Value < 5) Manager.UI.ShowSingleUI<UI_Toast>().SetText(210408);
            else if (_equipment.Count.Value == 0) Manager.UI.ShowSingleUI<UI_Toast>().SetText(210407);
            else
            {
                var get = _equipment.Decompose();
                Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210406, new List<DbReward>
                {
                    new(CurrencyType.SkillGrowthStone, get)
                });
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

        public void OnLanguageChanged(Locale locale)
        {
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text =  LocalString.Get(DbGrade.Get(_meta.GetGrade()).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_meta.GetNameId());
            Get<TextMeshProUGUI>((int) Texts.T_CoolTime).text = string.Format(LocalString.Get(210018), _meta.CoolTime);
        }
    }
}