using System;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbPromote;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.FieldMain;
using UIs.Shop.DefaultShop;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Costume
{
    public class UI_CostumeSelected: UI_Base
    {
        private EventsManager _equipEventsManager;
        
        private DbCostume _meta;

        private Sprite[] _equipBtnSprites;
        private UIField<int> _nowSelected = new(-1);
        
        enum Texts
        {
            T_Name,
            T_OwnEffect,
            T_Grade,
            T_Equip,
            T_Condition
        }

        enum Images
        {
            IMG_Grade,
            IMG_Costume,
            B_Equip,
        }

        enum GameObjects
        {
            IMG_DontHave,
            B_LeftMove,
            B_RightMove,
            B_Equip,
            Condition
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            Get<GameObject>((int)GameObjects.B_LeftMove).BindEvent(Functions.TrueCondition, PrevEquipment, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_RightMove).BindEvent(Functions.TrueCondition, NextEquipment, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Equip).BindEvent(Functions.TrueCondition, TryEquip, UIEffectType.Bounce);

            _equipBtnSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_DefaultButton_round3"), Manager.Resource.Load<Sprite>("UI_DefaultButton_round4")};
            
            return true;
        }

        private void PrevEquipment(PointerEventData eventData)
        {
            var prev = _meta.PrevId;
            if (prev == -1) return;
            Set(prev);
        }

        private void NextEquipment(PointerEventData eventData)
        {
            var next = _meta.NextId;
            if (next == -1) return;
            Set(next);
        }
        
        public UIField<int> NowSelected()
        {
            return _nowSelected;
        }

        public void Set(int id)
        { 
            if (!_isInit) Init();

            _nowSelected.Value = id;
            _meta = DbCostume.Get(id);

            Get<Image>((int) Images.IMG_Costume).sprite = _meta.GetResource();
            Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(_meta.Grade.ToString());
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text =  LocalString.Get(DbGrade.Get(_meta.Grade).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_meta.NameId);
            Get<GameObject>((int)GameObjects.B_LeftMove).SetActive(_meta.PrevId != -1);
            Get<GameObject>((int)GameObjects.B_RightMove).SetActive(_meta.NextId != -1);

            Get<TextMeshProUGUI>((int) Texts.T_OwnEffect).text = StringMaker.GetCostumeOptionString(_meta);


            _equipEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = new []{EquipController.data.BodyCostume, EquipController.data.WeaponCostume}
            });
            
            WhenEquipChanged();
            
            gameObject.SetActive(true);
        }

        private void WhenEquipChanged()
        {
            var have = CurrencyController.I.HaveCostume(_meta.Id);
            Get<GameObject>((int)GameObjects.IMG_DontHave).SetActive(!have);
            if (have)
            {
                var isEquipped = EquipController.I.IsEquipped(_meta.Position, _meta.Id);
                Get<Image>((int) Images.B_Equip).sprite = _equipBtnSprites[isEquipped ? 0 : 1];
                Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(isEquipped ? 210085 : 210086);
                Get<GameObject>((int)GameObjects.B_Equip).SetActive(true);
                Get<GameObject>((int)GameObjects.Condition).SetActive(false);
            }
            else
            {
                var canBuy = false;
                if (_meta.Category == CostumeCategoryType.InAppShop || _meta.Category == CostumeCategoryType.InGameShop)
                {
                    if (_meta.DescriptionId == 0) canBuy = true;
                    else
                    {
                        var cur = DateTime.UtcNow.AddHours(9);
                        var shopMeta = GetShopMeta();
                        canBuy = cur >= shopMeta.GetStartTime() && cur <= shopMeta.GetStartTime().AddDays(shopMeta.GetDuration());
                    }
                }
                Get<GameObject>((int)GameObjects.B_Equip).SetActive(canBuy);
                Get<GameObject>((int)GameObjects.Condition).SetActive(!canBuy);
                if (_meta.Category == CostumeCategoryType.Promotion)
                {
                    var promotion = DbPromotion.Get(p => p.CostumeId == _meta.Id);
                    Get<TextMeshProUGUI>((int) Texts.T_Condition).text = string.Format(LocalString.Get(210265),
                        LocalString.Get(promotion.NameId));
                }
                else if (_meta.Category == CostumeCategoryType.Event || !canBuy)
                {
                    Get<TextMeshProUGUI>((int) Texts.T_Condition).text = LocalString.Get(_meta.DescriptionId);
                }
                else
                {
                    Get<Image>((int) Images.B_Equip).color = Define.Color49FFE9;
                    Get<TextMeshProUGUI>((int) Texts.T_Equip).text = LocalString.Get(210266);
                }
            }
        }
        
        private void TryEquip(PointerEventData eventData)
        {
            if (CurrencyController.I.HaveCostume(_meta.Id))
            {
                EquipController.I.EquipCostume(_meta.Position, _meta.Id);
            }
            else
            {
                Manager.UI.ClosePopupUI();
                Manager.UI.GetSceneUI<UI_MainBottom>().OpenPopup(UI_MainBottom.GameObjects.B_Shop);
                var shop = Manager.UI.GetPopupUI<UI_Shop>();
                if (_meta.Category == CostumeCategoryType.InGameShop)
                {
                    shop.OpenTab(DbInGameShop.Get(_meta.ShopId).Category);
                }
                else if (_meta.Category == CostumeCategoryType.InAppShop)
                {
                    shop.OpenTab(GetShopMeta().GetCategory());
                }
            }
        }

        private IDbShop GetShopMeta()
        {
            if (_meta.Category == CostumeCategoryType.InAppShop)
            {
                return DbInAppShop.Get(_meta.ShopId);
            }

            return DbInGameShop.Get(_meta.ShopId);

        }
        
        private void OnDisable()
        {
            _equipEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _equipEventsManager?.Reconnect();
        }
    }
}