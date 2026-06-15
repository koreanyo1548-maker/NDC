using Data;
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

namespace UIs.Guide
{
    public class UI_Guide_15: UI_Guide
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

                    IndicateSkill();
                    inventory.MoveToTop();
                }
                else if (popup is UI_SkillInventorySelected skill)
                {
                    skill.Check.ValueChanged = () => SelectAnotherSkillInSkill(popup); 
                    if (skill.CanGrowth)
                    {
                        Indicate("B_Growth");
                    }
                    else
                    {
                        popup = Manager.UI.GetPopupUI<UI_SkillInventory>();
                        IndicateSkill();
                        (popup as UI_SkillInventory).MoveToTop();
                    } 
                }
                else if (popup is UI_EquipGrowthSP growth && growth.EquipmentType == EquipmentType.Skill)
                {
                    growth.Check.ValueChanged = () => SelectAnotherSkillInGrowth(popup);
                    if (!growth.Get<Image>("B_Growth").material.name.Contains("Gray"))
                    {
                        Indicate("B_Growth", true, GuideHandPositionType.X78Y59);
                    }
                    else
                    {
                        Indicate("B_Close");
                    }
                }
                else if (popup is UI_EquipAwakening or UI_EquipGrowth or UI_EquipSkill)
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

            void IndicateSkill()
            {
                Set(popup.Get<RectTransform>("G_SkillParent").GetChild(GetSkill()-1).GetComponent<RectTransform>(), false, false);
            }
            int GetSkill()
            {
                var haveSkill = false;
                var skillId = 1;
                while (!haveSkill)
                {
                    if (DbUserSkill.Get(skillId).Have.Value)
                    {
                        haveSkill = true;
                    }
                    else
                    {
                        skillId++;
                    }
                }

                return skillId;
            }
            
            void SelectAnotherSkillInSkill(UI_Base popup)
            {
                Next(popup);
            }
            void SelectAnotherSkillInGrowth(UI_Base popup)
            {
                Next(popup);
            }
        }
    }
}