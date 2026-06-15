using System;
using System.Collections.Generic;
using Controller;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Controller.Utils;
using Data;
using Data.DbCommon;
using Data.DbEquipment;
using Data.DbNecklaceInfo;
using Data.DbRelicInfo;
using Data.DbShop;
using Data.DbUser;
using Data.Utils;
using Exceptions;
using Managers;
using Managers.Base;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.FieldMain;
using UIs.Inventory.Costume;
using UIs.Inventory.Equipment;
using UIs.Inventory.Necklace;
using UIs.Inventory.Relic;
using UIs.Lock;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Define = Utils.Define;

namespace UIs.Inventory
{
    public class UI_Inventory: UI_Popup
    {
        private Images? _curOpened = null;
        
        private Sprite[] _tabSprites; // 0: not selected 1: selected
        private Sprite[] _costumeTabSprites; // 0: not selected 1: selected

        private UI_InventorySelected _selected;
        private UI_CostumeSelected _costumeSelected;
        private Dictionary<Images, int> _prevSelected = new();

        private Transform _weaponParent;
        private Transform _accessoryParent;
        
        private bool _isBodyCostume = true;

        private EventsManager _canGrowthHandler;
        private EventsManager _canMergeHandler;
        
        public UIField<bool> Check = new(false);

        private Vector3 _positionSetter;
        public UI_InventorySelected Selected => _selected;

        enum GameObjects
        {
            V_Weapon,
            V_Accessory,
            V_Relic,
            V_Costume,
            V_BodyCostume,
            V_WeaponCostume,
            Equipment,
            IMG_InventoryBG,
            IMG_NecklaceBG
        }

        enum Images
        {
            B_WeaponTab,
            B_AccessoryTab,
            B_RelicTab,
            B_CostumeTab,
            B_NecklaceTab,
            B_MergeAll,
            B_GrowthAllNecklace,
            B_MergeAllNecklace,
            IMG_BodyCostumeTab,
            IMG_WeaponCostumeTab,
            IMG_Body,
            IMG_Weapon
        }

        enum Transforms
        {
            B_WeaponTab,
            B_AccessoryTab,
            B_RelicTab,
            B_CostumeTab,
            B_NecklaceTab
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<Transform>(typeof(Transforms));
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);

            _weaponParent = Util.FindChild<Transform>(gameObject, "G_WeaponParent", true);
            _accessoryParent = Util.FindChild<Transform>(gameObject, "G_AccessoryParent", true);
            var relicParent = Util.FindChild<Transform>(gameObject, "G_RelicParent", true);
            var bodyCostumeParent = Util.FindChild<Transform>(gameObject, "G_BodyCostumeParent", true);
            var weaponCostumeParent = Util.FindChild<Transform>(gameObject, "G_WeaponCostumeParent", true);
            var necklaceParent = Util.FindChild<Transform>(gameObject, "G_NecklaceParent", true);
            
            Get<GameObject>((int)GameObjects.V_Costume).transform.Find("IMG_BodyCostumeTab").gameObject.BindEvent(() => !_isBodyCostume, ChangeCostumeTab, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.V_Costume).transform.Find("IMG_WeaponCostumeTab").gameObject.BindEvent(() => _isBodyCostume, ChangeCostumeTab, UIEffectType.Bounce);
            
            Get<Image>((int)Images.B_MergeAll).gameObject.BindEvent(ConditionMergeAll, MergeAll, UIEffectType.Bounce, false);
            Util.FindChild(gameObject, "B_EquipRecommendation", true).BindEvent(Functions.TrueCondition, EquipRecommendation, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.V_Accessory).SetActive(false);
            
            _selected = Util.FindChild(gameObject, "UI_InventorySelected", true).GetOrAddComponent<UI_InventorySelected>();
            _costumeSelected = Util.FindChild(gameObject, "UI_CostumeSelected", true).GetOrAddComponent<UI_CostumeSelected>();
            Get<GameObject>((int)GameObjects.V_Costume).SetActive(false);
            Get<GameObject>((int)GameObjects.V_Relic).SetActive(false);

            Util.FindChild(gameObject, "B_GrowthAllNecklace", true).BindEvent(() => NecklaceController.I.CanGrowth.Value, _ => GrowthAllNecklace(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_MergeAllNecklace", true).BindEvent(() => NecklaceController.I.CanMerge.Value, _ => MergeAllNecklace(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_EquipEffect", true).BindEvent(Functions.TrueCondition, _ => ShowNecklaceEquipEffect(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_OwnEffect", true).BindEvent(Functions.TrueCondition, _ => ShowNecklaceOwnEffect(), UIEffectType.Bounce);

            _canGrowthHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = SetGrowthAllNecklaceBlock,
                updatedController = new[] {NecklaceController.I.CanGrowth}
            });

            _canMergeHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = SetMergeAllNecklaceBlock,
                updatedController = new[] {NecklaceController.I.CanMerge}
            });
            
            _tabSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};
            _costumeTabSprites = new[]
                {Manager.Resource.Load<Sprite>("Alert_Text_White"), Manager.Resource.Load<Sprite>("Alert_Text_Yellow")};

            DbWeapon.ForEach(
                weapon =>
                    Manager.UI.MakeSubItem<UI_Inventory_Item>(_weaponParent).SetInfo(weapon, _selected));
            
            DbAccessory.ForEach(
                accessory =>
                    Manager.UI.MakeSubItem<UI_Inventory_Item>(_accessoryParent).SetInfo(accessory, _selected));

            DbRelic.ForEach(
                relic =>
                    Manager.UI.MakeSubItem<UI_Relic_Item>(relicParent).SetInfo(relic));

            DbNecklace.ForEach(
                necklace =>
                    Manager.UI.MakeSubItem<UI_Necklace_Item>(necklaceParent).SetInfo(necklace));

            var necklaceEquippedParent = Util.FindChild<Transform>(gameObject, "UI_Necklace", true);
            for (var idx = 0; idx < 7; ++idx)
            {
                necklaceEquippedParent.Find("UI_NecklaceEquip_Item (" + idx.ToString() + ")").gameObject.GetOrAddComponent<UI_NecklaceEquip_Item>().Set(idx);
            }
            PlayFabManager.Store.DoWithTime(SetCostume);
            
            void SetCostume(DateTime now)
            {
                DbCostume.ForEach(c => c.Position == CostumePositionType.Body,
                    costume =>
                        Manager.UI.MakeSubItem<UI_Costume_Item>(bodyCostumeParent).SetInfo(now, costume, _costumeSelected));
            
                DbCostume.ForEach(c => c.Position == CostumePositionType.Weapon,
                    costume =>
                        Manager.UI.MakeSubItem<UI_Costume_Item>(weaponCostumeParent).SetInfo(now, costume, _costumeSelected));
            }
            
            foreach (Images tab in Enum.GetValues(typeof(Images)))
            {
                if (tab == Images.B_AccessoryTab)
                {
                    var obj = Get<Image>((int)tab).gameObject;
                    obj.GetOrAddComponent<UI_Locked>().Set(LockType.Accessory, obj.GetComponent<Image>(), Util.FindChild(obj, "IMG_LockIcon"), null,
                        () => obj.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce));
                }
                else if (tab == Images.B_RelicTab)
                {
                    var obj = Get<Image>((int)tab).gameObject;
                    obj.GetOrAddComponent<UI_Locked>().Set(LockType.Relic, obj.GetComponent<Image>(), Util.FindChild(obj, "IMG_LockIcon"), null,
                        () => obj.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce));
                }
                else if (tab == Images.B_NecklaceTab)
                {
                    var obj = Get<Image>((int) tab).gameObject;
                    obj.GetOrAddComponent<UI_Locked>().Set(LockType.Necklace, obj.GetComponent<Image>(),
                        Util.FindChild(obj, "IMG_LockIcon"), null,
                        () => obj.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce));
                }
                else if (tab <= Images.B_CostumeTab)
                {
                    Get<Image>((int)tab).gameObject.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce);
                }
            }
            
            OnTabClicked(Images.B_WeaponTab);
            SetBadges();
            Get<GameObject>((int)GameObjects.IMG_InventoryBG).SetActive(true);
            Get<GameObject>((int)GameObjects.IMG_NecklaceBG).SetActive(false);

            void SetBadges()
            {
                Util.FindChild(gameObject, "IMG_WeaponBadge", true).GetOrAddComponent<UI_Badge>()
                    .Set(new[] {BadgeController.data.Weapons}, BadgeController.I.IsWeaponBadgeOn);
                Util.FindChild(gameObject, "IMG_AccessoryBadge", true).GetOrAddComponent<UI_Badge>()
                    .Set(new[] {BadgeController.data.Accessories}, BadgeController.I.IsAccessoryBadgeOn, LockType.Accessory);
                Util.FindChild(gameObject, "IMG_RelicBadge", true).GetOrAddComponent<UI_Badge>()
                    .Set(new[] {BadgeController.data.Relics}, BadgeController.I.IsRelicBadgeOn, LockType.Relic);
                Util.FindChild(gameObject, "IMG_NecklaceBadge", true).GetOrAddComponent<UI_Badge>()
                    .Set(new[] {BadgeController.data.Necklace}, () => NecklaceController.I.CanUpgrade.Value, LockType.Necklace);
                Util.FindChild(gameObject, "IMG_GrowthAllBadge", true).GetOrAddComponent<UI_ControllerBadge>()
                    .Set(new[] {NecklaceController.I.CanGrowth}, () => NecklaceController.I.CanGrowth.Value);
                Util.FindChild(gameObject, "IMG_MergeAllBadge", true).GetOrAddComponent<UI_ControllerBadge>()
                    .Set(new[] {NecklaceController.I.CanMerge}, () => NecklaceController.I.CanMerge.Value);
            }
            
            return true;
        }

        private bool ConditionMergeAll()
        {
            if (_curOpened == Images.B_WeaponTab) return WeaponController.I.AnyThingToMerge();
            if (_curOpened == Images.B_AccessoryTab) return AccessoryController.I.AnyThingToMerge();
            return false;
        }
        
        public override void WhenPopupClosed()
        {
            Manager.UI.GetSceneUI<UI_MainBottom>().CloseInnerPopup();
        }

        private void MergeAll(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_DefaultPopup>().Set(210073, MergeAllAction);
            
            void MergeAllAction()
            {
                Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
                if (_curOpened == Images.B_WeaponTab)
                {
                    WeaponController.I.MergeAll();
                }
                else
                {
                    AccessoryController.I.MergeAll();
                }
                var toast = Manager.UI.ShowSingleUI<UI_Toast>();
                toast.SetText(200002);
                SetMergeBlock();
            }
        }

        private void EquipRecommendation(PointerEventData eventData)
        {
            var equipType = TabToEquipment(_curOpened);
            var prevEquip = EquipController.I.GetEquipped(equipType);
            var recommend = EquipController.I.EquipRecommendation(equipType);
            var isChanged = prevEquip != recommend;
            if (isChanged) _selected.Set(equipType, recommend);
            else Manager.UI.ShowSingleUI<UI_Toast>().SetText(200034);
        }
        
        private void SetMergeBlock()
        {
            Get<Image>((int) Images.B_MergeAll).material = Define.GetUIMaterial(!ConditionMergeAll());
        }

        private void SetGrowthAllNecklaceBlock()
        {
            Get<Image>((int) Images.B_GrowthAllNecklace).material = Define.GetUIMaterial(!NecklaceController.I.CanGrowth.Value);
        }

        private void SetMergeAllNecklaceBlock()
        {
            Get<Image>((int) Images.B_MergeAllNecklace).material = Define.GetUIMaterial(!NecklaceController.I.CanMerge.Value);
        }

        private void GrowthAllNecklace()
        {
            NecklaceController.I.GrowthAll();
        }

        private void MergeAllNecklace()
        {
            var reward = NecklaceController.I.MergeAll();
            
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            var rewardsForToast = new List<DbReward>();
            
            foreach (var r in reward)
            {
                rewardsForToast.Add(
                    new DbReward(CurrencyType.Necklace, r.Value, r.Key));
            }
            toast.SetReward(210414, rewardsForToast);
        }
        
        private void ShowNecklaceEquipEffect()
        {
            Manager.UI.ShowPopupUI<UI_NecklaceInfo>().Set(true);
        }
        
        private void ShowNecklaceOwnEffect()
        {
            Manager.UI.ShowPopupUI<UI_NecklaceInfo>().Set(false);
        }

        private void OnTabClicked(Images clicked)
        {
            if (_curOpened == clicked)
            {
                return;
            }

            if (_curOpened != null)
            {
                _positionSetter = Get<Transform>((int) _curOpened).localPosition;
                _positionSetter.y = 22.83f;
                Get<Transform>((int)_curOpened).localPosition = _positionSetter;
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
                CloseTab(_curOpened);
            }

            Check.Value = !Check.Value;
            
            _curOpened = clicked; 
            _positionSetter = Get<Transform>((int) _curOpened).localPosition;
            _positionSetter.y = 7.92f;
            Get<Transform>((int) clicked).localPosition = _positionSetter;
            Get<Image>((int) clicked).sprite = _tabSprites[1];
            OpenTab(clicked);
                
            void OpenTab(Images tab)
            {
                if (tab == Images.B_RelicTab || tab == Images.B_NecklaceTab)
                {
                    
                }   
                else if (tab == Images.B_CostumeTab)
                {
                    if (_prevSelected.TryGetValue(tab, out var value)) _costumeSelected.Set(value);
                    else
                    {
                        var equipped = EquipController.data.BodyCostume.Value;
                        if (equipped == 0) equipped = 1;
                        _costumeSelected.Set(equipped);
                    }
                }
                else
                {
                    var equipType = TabToEquipment(tab);
                    if (_prevSelected.TryGetValue(tab, out var value)) _selected.Set(equipType, value);
                    else
                    {
                        var equipped = EquipController.I.GetEquipped(equipType);
                        if (equipped == 0) equipped = 1;
                        _selected.Set(equipType, equipped);
                    }
                }
                
                switch (tab)
                {
                    case Images.B_WeaponTab:
                        Get<GameObject>((int)GameObjects.V_Weapon).SetActive(true);
                        Get<GameObject>((int)GameObjects.Equipment).SetActive(true);
                        break;
                    case Images.B_AccessoryTab:
                        Get<GameObject>((int)GameObjects.V_Accessory).SetActive(true);
                        Get<GameObject>((int)GameObjects.Equipment).SetActive(true);
                        break;
                    case Images.B_CostumeTab:
                        Get<GameObject>((int)GameObjects.V_Costume).SetActive(true);
                        Get<GameObject>((int)GameObjects.V_BodyCostume).SetActive(_isBodyCostume);
                        Get<GameObject>((int)GameObjects.V_WeaponCostume).SetActive(!_isBodyCostume);
                        break;
                    case Images.B_RelicTab:
                        Get<GameObject>((int)GameObjects.V_Relic).SetActive(true);
                        break;
                    case Images.B_NecklaceTab:
                        Get<GameObject>((int)GameObjects.IMG_InventoryBG).SetActive(false);
                        Get<GameObject>((int)GameObjects.IMG_NecklaceBG).SetActive(true);
                        SetGrowthAllNecklaceBlock();
                        break;
                }
                
                SetMergeBlock();
            }
            
            void CloseTab(Images? tab)
            {
                SetPrevSelected();
                
                switch (tab)
                {
                    case Images.B_WeaponTab:
                        Get<GameObject>((int)GameObjects.V_Weapon).SetActive(false);
                        Get<GameObject>((int)GameObjects.Equipment).SetActive(false);
                        BadgeController.I.CheckAllWeapon();
                        break;
                    case Images.B_AccessoryTab:
                        Get<GameObject>((int)GameObjects.V_Accessory).SetActive(false);
                        Get<GameObject>((int)GameObjects.Equipment).SetActive(false);
                        BadgeController.I.CheckAllAccessory();
                        break;
                    case Images.B_CostumeTab:
                        Get<GameObject>((int)GameObjects.V_Costume).SetActive(false);
                        break;
                    case Images.B_RelicTab:
                        Get<GameObject>((int)GameObjects.V_Relic).SetActive(false);
                        break;
                    case Images.B_NecklaceTab:
                        Get<GameObject>((int)GameObjects.IMG_InventoryBG).SetActive(true);
                        Get<GameObject>((int)GameObjects.IMG_NecklaceBG).SetActive(false);
                        break;
                }
            }
        }

        private void ChangeCostumeTab(PointerEventData eventData)
        {
            _isBodyCostume = !_isBodyCostume;
            Get<GameObject>((int)GameObjects.V_BodyCostume).SetActive(_isBodyCostume);
            Get<GameObject>((int)GameObjects.V_WeaponCostume).SetActive(!_isBodyCostume);
            Get<Image>((int) Images.IMG_BodyCostumeTab).sprite = _costumeTabSprites[_isBodyCostume ? 1 : 0];
            Get<Image>((int) Images.IMG_WeaponCostumeTab).sprite = _costumeTabSprites[_isBodyCostume ? 0 : 1];
            Get<Image>((int)Images.IMG_Body).color = _isBodyCostume ? Define.ColorF7D25E : Color.white;
            Get<Image>((int)Images.IMG_Weapon).color = _isBodyCostume ? Color.white : Define.ColorF7D25E;
        }

        private EquipmentType TabToEquipment(Images? tab)
        {
            if (tab == Images.B_WeaponTab) return EquipmentType.Weapon;
            if (tab == Images.B_AccessoryTab) return EquipmentType.Accessory;
            throw new NotDefinedValueException(tab.ToString());
        }

        private void SetPrevSelected()
        {
            if (_curOpened != null && _selected.NowSelected().Value != -1)
            {
                if (_prevSelected.ContainsKey((Images) _curOpened))
                    _prevSelected[(Images) _curOpened] = _curOpened == Images.B_CostumeTab ? _costumeSelected.NowSelected().Value : _selected.NowSelected().Value;
                else _prevSelected.Add((Images)_curOpened,  _curOpened == Images.B_CostumeTab ? _costumeSelected.NowSelected().Value : _selected.NowSelected().Value);
            }
        }
        
        public override void ClosePopupUI()
        {
            SetPrevSelected();
            base.ClosePopupUI();
        }

        private void OnEnable()
        {
            _canGrowthHandler?.Reconnect();
            _canMergeHandler?.Reconnect();
        }

        public override bool NeedRaycast()
        {
            return true;
        }
        
        private void OnDisable()
        {
            Check.ValueChanged = null;
            _canGrowthHandler?.Dispose();
            _canMergeHandler?.Dispose();
        }

        public void MoveToTop(bool isWeapon)
        {
            if (isWeapon)
            {
                var position = _weaponParent.localPosition;
                position.y = 0;
                _weaponParent.localPosition = position;
            }
            else
            {
                var position = _accessoryParent.localPosition;
                position.y = 0;
                _accessoryParent.localPosition = position;
            }
        }
    }
}