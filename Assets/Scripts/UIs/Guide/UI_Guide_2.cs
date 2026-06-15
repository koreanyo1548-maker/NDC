using Data.DbUser.Equipment;
using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UIs.Skill;
using UIs.Summon;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Guide
{
    public class UI_Guide_2: UI_Guide
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

                    IndicateSkill();
                    inventory.MoveToTop();
                }
                else if (popup is UI_SkillInventorySelected skill)
                {
                    skill.Check.ValueChanged = () => SelectAnotherSkillInSkill(popup);
                    var equipButton = skill.Get<Transform>("B_Equip") != null;
                    if (equipButton && !skill.Get<Image>("B_Equip").material.name.Contains("Gray"))
                    {
                        Indicate("B_Equip");
                    }
                    else
                    {
                        popup = Manager.UI.GetPopupUI<UI_SkillInventory>();
                        IndicateSkill();
                        (popup as UI_SkillInventory).MoveToTop();
                    }
                }
                else if (popup is UI_EquipSkill equip)
                {
                    if (equip.CurMode == UI_EquipSkill.Mode.Edit) Indicate("ActiveSkill1");
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
           
            void Indicate(string indicateName, bool isCanvasEnabled = true)
            {
                var indicate = popup.Get<RectTransform>(indicateName);
                Set(indicate, false, isCanvasEnabled);
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
        }
        private void SelectAnotherSkillInSkill(UI_Base popup)
        {
            Next(popup);
        }
    }
}