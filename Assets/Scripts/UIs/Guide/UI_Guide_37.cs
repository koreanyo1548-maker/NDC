using Controller;
using Controller.Infos;
using Data.DbEquipment;
using Data.DbUser.Equipment;
using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UIs.Skill;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Guide
{
    public class UI_Guide_37: UI_Guide
    {
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainBottom>().Get<RectTransform>("B_Skill");
                Set(indicate);
                return;
            }
            
            if (SetDisable<UI_EquipAwakening>()) return;
            if (SetDisable<UI_EquipGrowth>()) return;

            bool SetDisable<T>() where T: UI_Popup
            {
                var disable = Manager.UI.GetPopupUI<T>();
                if (disable != null)
                {
                    Next(disable);
                    return true;
                }
                return false;
            }
            
            var inventory = Manager.UI.GetPopupUI<UI_SkillInventory>();
            if (inventory != null)
            {
                Next(inventory);
                return;
            }
            
            Close();
        }

        public override void Next(UI_Base popup)
        {
            ActiveIndicator(false);
            Timing.CallDelayed(0.15f, () =>
            {
                if (popup == null) return;
                if (popup is UI_SkillInventory inventory)
                { 
                    if (inventory.Selected.NowSelected().Value != -1)
                    {
                        Next(inventory.Selected);
                        return;
                    }
                    
                    Indicate("UI_Inventory_Item", false, GetIdxOfUneuipped());
                    inventory.MoveToTop();
                }
                else if (popup is UI_SkillInventorySelected skill)
                {
                    skill.Check.ValueChanged = () => SelectAnotherSkillInSkill(popup);
                    var equipButton = skill.Get<Transform>("B_Equip") != null;
                    if (equipButton && !skill.Get<Image>("B_Equip").material.name.Contains("Gray") && skill.Get<Image>("B_Equip").color == Define.Color49FFE9)
                    {
                        Indicate("B_Equip");
                    }
                    else
                    {
                        popup = Manager.UI.GetPopupUI<UI_SkillInventory>();
                        Indicate("UI_Inventory_Item", false, GetIdxOfUneuipped());
                        (popup as UI_SkillInventory).MoveToTop();
                    }
                }
                else if (popup is UI_EquipSkill equip)
                {
                    if (equip.CurMode == UI_EquipSkill.Mode.Edit) Indicate("ActiveSkill2");
                    else Next(Manager.UI.GetPopupUI<UI_SkillInventory>());
                }
                else if (popup is UI_EquipAwakening or UI_EquipGrowth)
                {
                    ActiveIndicator(false);
                }
                else
                {
                    Close();
                }
            }, gameObject);
           
            void Indicate(string indicateName, bool isCanvasEnabled = true, int idx = 0)
            {
                var indicate = idx == 0 ? popup.Get<RectTransform>(indicateName) : popup.Get<RectTransform>(indicateName, idx);
                Set(indicate, false, isCanvasEnabled);
            }

            int GetIdxOfUneuipped()
            {
                var idx = DbSkill.Count-1;
                var skill = DbSkill.Get(s => s.NextId == -1);
                while (!DbUserSkill.Get(skill.Id).Have.Value || EquipController.I.IsEquipped(DbUserSkill.Get(skill.Id)))
                {
                    idx--;
                    skill = DbSkill.Get(skill.PrevId);
                }

                return idx;
            }
        }
        private void SelectAnotherSkillInSkill(UI_Base popup)
        {
            Next(popup);
        }
    }
}