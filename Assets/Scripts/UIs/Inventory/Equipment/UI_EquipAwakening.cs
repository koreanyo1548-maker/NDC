using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbPetInfo;
using Data.DbUser.Equipment;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Equipment
{
    public class UI_EquipAwakening : UI_Popup
    {
        private EventsManager _equipmentEventsManager;
        
        public UIField<bool> Check = new(false);
        
        private UI_Star _starUI;

        enum Texts
        {
            T_Name,
            T_Level,
            T_Grade,
            T_AwakeningCost,
            T_AwakeningBtn,
            T_EquipmentCount,
            T_AwakeningEffect,
            T_Awakening1,
            T_Awakening2,
            T_Awakening3,
            T_Awakening4,
            T_Awakening5,
            T_Awakening6,
            T_Awakening7
        }

        enum GameObjects
        {
            B_Prev,
            B_Next,
            IMG_Awakening1,
            IMG_Awakening2,
            IMG_Awakening3,
            IMG_Awakening4,
            IMG_Awakening5,
            IMG_Awakening6,
            IMG_Awakening7,
            AwakeningCost
        }

        enum Images
        {
            IMG_Grade,
            IMG_Equipment,
            B_Awakening,
            IMG_Awakening1,
            IMG_Awakening2,
            IMG_Awakening3,
            IMG_Awakening4,
            IMG_Awakening5,
            IMG_Awakening6,
            IMG_Awakening7,
            IMG_AwakeningLockIcon1,
            IMG_AwakeningLockIcon2,
            IMG_AwakeningLockIcon3,
            IMG_AwakeningLockIcon4,
            IMG_AwakeningLockIcon5,
            IMG_AwakeningLockIcon6,
            IMG_AwakeningLockIcon7
        }

        enum Transforms
        {
            EffectPosition
        }
        
        private EquipmentType _equipmentType;
        private IDbUserEquipment _equipment;
        
        public EquipmentType EquipmentType => _equipmentType;

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<Transform>(typeof(Transforms));
            
            Get<GameObject>((int)GameObjects.B_Prev).BindEvent(Functions.TrueCondition, _ => 
            {
                Set(_equipmentType, _equipment.PrevHave());
                Check.Value = !Check.Value;
            }, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Next).BindEvent(Functions.TrueCondition, _ =>
            {
                Set(_equipmentType, _equipment.NextHave());
                Check.Value = !Check.Value;
            }, UIEffectType.Bounce);
            Get<Image>((int)Images.B_Awakening).gameObject.BindEvent(AwakeningCondition, Awakening, UIEffectType.Bounce, false);
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);

            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();

            return true;
        }

        private bool AwakeningCondition()
        {
            return !_equipment.IsMaxAwakening() 
                   && CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone).Value >= _equipment.GetAwakeningStoneCount()
                   && _equipment.GetCount() >= _equipment.GetAwakeningEquipCount();
        }

        public void Set(EquipmentType type, IDbUserEquipment equipment)
        {
            if (!_isInit) Init();

            _equipmentType = type;
            _equipment = equipment;
            
            var meta = equipment.GetMeta();

            Get<GameObject>((int)GameObjects.B_Prev).SetActive(equipment.PrevHave() != null);
            Get<GameObject>((int)GameObjects.B_Next).SetActive(equipment.NextHave() != null);
            
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(meta.GetNameId());
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(meta.GetGrade()).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = "Lv." + equipment.GetGrowth();
            Get<Image>((int) Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(meta.GetResource());
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(meta.GetGrade().ToString());

            Get<TextMeshProUGUI>((int) Texts.T_AwakeningEffect).text =
                LocalString.Get(_equipmentType == EquipmentType.Pet ? 210402 : 210403);

            var awakening = meta.GetAwakening();
            for (var idx = 1; idx <= awakening.GetMaxAwakening(); ++idx)
            {
                Get<GameObject>((int) GameObjects.IMG_Awakening1 + idx - 1).gameObject.SetActive(true);
                string description;
                if (_equipmentType == EquipmentType.Pet)
                {
                    description = (awakening as DbPetAwakening).GetDescription(awakening.GetStat(idx)); 
                }
                else if (awakening.GetOption(idx) == StatType.SpecificSkillAttackBonus)
                {
                    description = StringMaker.GetAwakeningString(awakening.GetOption(idx), awakening.GetStat(idx), LocalString.Get(meta.GetNameId()));
                }
                else 
                {
                    description = StringMaker.GetAwakeningString(awakening.GetOption(idx), awakening.GetStat(idx));
                }
                Get<TextMeshProUGUI>((int) Texts.T_Awakening1 + idx - 1).text = string.Format(LocalString.Get(210080), idx, description, awakening.GetLevel(idx));
            }
            for (var idx = awakening.GetMaxAwakening() + 1; idx <= 7; ++idx)
            {
                Get<GameObject>((int) GameObjects.IMG_Awakening1 + idx - 1).gameObject.SetActive(false);
            }

            AwakeningInfoSet();
            WhenCurrencyChanged();
            WhenEquipmentCountChanged();
            
            if (_equipmentEventsManager == null)
            {
                _equipmentEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenEquipmentCountChanged,
                    updatedField = new[]{DbSelector.GetUserEquipment(_equipmentType, _equipment.GetId()).GetCountModel()}
                });
            }
            else
            {
                _equipmentEventsManager.Set(WhenEquipmentCountChanged,
                    new[]{DbSelector.GetUserEquipment(_equipmentType, _equipment.GetId()).GetCountModel()});
            }
        }

        private void AwakeningInfoSet()
        {
            var isMaxAwakening = _equipment.IsMaxAwakening();
            var awakening = _equipment.GetAwakening();

            _starUI.Set(awakening);

            Get<Image>((int) Images.B_Awakening).material =
                Define.GetUIMaterial(!AwakeningCondition());
            Get<TextMeshProUGUI>((int) Texts.T_AwakeningBtn).text = LocalString.Get(isMaxAwakening ? 210072 : 210062);
            Get<GameObject>((int) GameObjects.AwakeningCost).SetActive(!isMaxAwakening);

            var maxAwakening = _equipment.GetMeta().GetAwakening().GetMaxAwakening();
            for (var idx = 1; idx <= maxAwakening; ++idx)
            {
                var active = idx <= awakening;
                Get<TextMeshProUGUI>((int) Texts.T_Awakening1 + idx - 1).color =
                Get<Image>((int)Images.IMG_AwakeningLockIcon1 + idx - 1).color =
                    active ? Color.white : Define.Color878787;
                Get<Image>((int) Images.IMG_AwakeningLockIcon1 + idx - 1).sprite =
                    active ? Manager.Resource.Load<Sprite>(Define.StarIcon) : Manager.Resource.Load<Sprite>(Define.LockIcon);
                Get<Image>((int) Images.IMG_Awakening1 + idx - 1).enabled = active;
            }
        }
        
        private void WhenCurrencyChanged()
        {
            if (!_equipment.IsMaxAwakening())
            {
                var need = _equipment.GetAwakeningStoneCount();
                var have = CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone).Value;
                Get<TextMeshProUGUI>((int) Texts.T_AwakeningCost).text = Define.AddUnit(have, 3, 2) + "/" + Define.AddUnit(need, 3, 2);
                Get<Image>((int) Images.B_Awakening).material = Define.GetUIMaterial(!AwakeningCondition());
            }
        }

        private void WhenEquipmentCountChanged()
        {
            if (_equipment.IsMaxAwakening())
            {
                Get<TextMeshProUGUI>((int) Texts.T_EquipmentCount).text = Define.AddUnit(_equipment.GetCount(), 3, 2);
                Get<Image>((int) Images.B_Awakening).material = Define.GetUIMaterial(true);
            }
            else
            {
                var need = _equipment.GetAwakeningEquipCount();
                var have = _equipment.GetCount();
                var enough = have >= need;
                Get<TextMeshProUGUI>((int) Texts.T_EquipmentCount).text = enough
                    ? Define.AddUnit(have, 3, 2) + "/" + Define.AddUnit(need, 3, 2)
                    : $"<color=#FF3737>{Define.AddUnit(have, 3, 2)}</color>/{Define.AddUnit(need, 3,2)}";
                Get<Image>((int) Images.B_Awakening).material = Define.GetUIMaterial(!AwakeningCondition());
            }
        }
        
        private void Awakening(PointerEventData eventData)
        {
            if (!_equipment.CanAwakening()) return;
            
            if (CurrencyController.I.TryUse(CurrencyType.AwakeningStone, _equipment.GetAwakeningStoneCount()))
            {
                _equipment.AwakeningIt();
                var levelUp = Manager.Resource.Instantiate("Particles/LevelUpEffect", 10).transform;
                levelUp.GetComponent<ParticleSystem>().Play();
                levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
                AwakeningInfoSet();
                WhenCurrencyChanged();

                var meta = _equipment.GetMeta();
                var awakening = meta.GetAwakening();
                var level = _equipment.GetAwakening();
                string description;
                if (_equipmentType == EquipmentType.Pet)
                {
                    description = string.Format(awakening.GetDescription(), awakening.GetStat(level));
                }
                else if (awakening.GetOption(level) == StatType.SpecificSkillAttackBonus)
                {
                    description = StringMaker.GetAwakeningStringWithColor(awakening.GetOption(level), awakening.GetStat(level), LocalString.Get(meta.GetNameId()));
                }
                else 
                {
                    description = StringMaker.GetAwakeningStringWithColor(awakening.GetOption(level), awakening.GetStat(level));
                }
                Manager.UI.ShowSingleUI<UI_AwakeningToast>().SetInfo(Manager.Resource.Load<Sprite>(meta.GetResource()),
                    LocalString.Get(meta.GetNameId()), level,
                    awakening.GetLevel(level-1),
                    awakening.GetLevel(level),
                    meta.GetGrade(), description);
            }
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
        
        
        private void OnDisable()
        {
            _equipmentEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _equipmentEventsManager?.Reconnect();
        }
    }
}