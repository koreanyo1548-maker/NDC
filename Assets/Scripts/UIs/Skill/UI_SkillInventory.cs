using System;
using System.Collections.Generic;
using Controller;
using Controller.Have;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbEquipment;
using Managers;
using Managers.Game;
using UIBases;
using UIs.FieldMain;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Skill
{
    public class UI_SkillInventory: UI_Popup
    {
        private UI_SkillInventorySelected _selected;
        private UI_EquipSkill _equipUI;
        private Transform _skillParent;
        

        public UIField<bool> Check = new(false);
        public UI_SkillInventorySelected Selected => _selected;

        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);

            _skillParent = Util.FindChild<Transform>(gameObject, "G_SkillParent", true);
            
            _selected = Util.FindChild(gameObject, "UI_SkillSelected", true).GetOrAddComponent<UI_SkillInventorySelected>();
            _equipUI = Util.FindChild(gameObject, "UI_EquipSkill", true).GetOrAddComponent<UI_EquipSkill>();
            _equipUI.SetOrder(transform.GetComponent<Canvas>().sortingOrder + 1);
            _selected.SetEquipSkillUI(_equipUI);
            
            Util.FindChild(gameObject, "B_DecomposeAll", true).BindEvent(Functions.TrueCondition, _ => DecomposeAll(), UIEffectType.Bounce);

            DbSkill.ForEach(
                skill =>
                {
                    var item = Manager.UI.MakeSubItem<UI_Inventory_Item>(_skillParent, "UI_SkillInventory_Item");
                    item.SetInfo(skill, _selected);
                });
            
            _selected.Set(EquipmentType.Skill, 1);


            return true;
        }

        private void DecomposeAll()
        {
            var get = SkillController.I.DecomposeAll();
            if (get == 0)
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(210407);
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210406, new List<DbReward>
                {
                    new(CurrencyType.SkillGrowthStone, get)
                });
            }
        }
        
        public override void WhenPopupClosed()
        {
            Manager.UI.GetSceneUI<UI_MainBottom>().CloseInnerPopup();
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public void SetOrder(int order)
        {
            if (_equipUI != null)
            {
                _equipUI.SetOrder(order);
            }
        }

        private void OnDisable()
        {
            Check.ValueChanged = null;
            BadgeController.I.CheckAllSkills();
        }

        public void MoveToTop()
        {
            var position = _skillParent.localPosition;
            position.y = 0;
            _skillParent.localPosition = position;
        }
    }
}