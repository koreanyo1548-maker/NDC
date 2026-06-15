using Data;
using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UIs.Skill;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Guide
{
    public class UI_Guide_16: UI_Guide
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
                    Indicate("UI_Inventory_Item", false);
                    inventory.MoveToTop();
                }
                else if (popup is UI_SkillInventorySelected skill)
                {
                    skill.Check.ValueChanged = () => SelectAnotherSkillInSkill(popup); 
                    if (skill.CanAwakening)
                    {
                        Indicate("B_Awakening");
                    }
                    else
                    {
                        popup = Manager.UI.GetPopupUI<UI_SkillInventory>();
                        Indicate("UI_Inventory_Item", false);
                        (popup as UI_SkillInventory).MoveToTop();
                    } 
                }
                else if (popup is UI_EquipAwakening awakening && awakening.EquipmentType == EquipmentType.Skill)
                {
                    awakening.Check.ValueChanged = () => SelectAnotherSkillInAwakening(popup);
                    if (!awakening.Get<Image>("B_Awakening").material.name.Contains("Gray"))
                    {
                        Indicate("B_Awakening", true, GuideHandPositionType.X78Y59);
                    }
                    else
                    {
                        Indicate("B_Close");
                    }
                }
                else if (popup is UI_EquipGrowth or UI_EquipSkill)
                {
                    ActiveIndicator(false);
                }
                else
                {
                    Close();
                }
            }, gameObject);


            void Indicate(string indicateName, bool isCanvasEnabled = true, GuideHandPositionType position = GuideHandPositionType.X0Y68)
            {
                var indicate = popup.Get<RectTransform>(indicateName);
                Set(indicate, false, isCanvasEnabled, position);
            }
            
            void SelectAnotherSkillInSkill(UI_Base popup)
            {
                Next(popup);
            }
            void SelectAnotherSkillInAwakening(UI_Base popup)
            {
                Next(popup);
            }
           
        }
    }
}