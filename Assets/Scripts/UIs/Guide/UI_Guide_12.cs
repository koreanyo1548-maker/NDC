using Data;
using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Guide
{
    public class UI_Guide_12: UI_Guide
    {   

        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainBottom>().Get<RectTransform>("B_Inventory");
                Set(indicate);
                return;
            } 
            if (SetDisable<UI_EquipGrowth>()) return;
            if (SetDisable<UI_EquipMerge>()) return;

            bool SetDisable<T>() where T: UI_Popup
            {
                var disable = Manager.UI.GetPopupUI<T>();
                if (disable != null)
                {
                    InventoryCheckEvent();  
                    Next(disable);
                    return true;
                }

                return false;
            }
            
            var inventory = Manager.UI.GetPopupUI<UI_Inventory>();

            if (inventory != null)
            {
                var selected = inventory.Selected;

                var awakening = Manager.UI.GetPopupUI<UI_EquipAwakening>();
                if (awakening != null)
                {
                    InventoryCheckEvent();
                    Next(awakening);
                    return;
                }
                Next(inventory);
                return;
            }

            void InventoryCheckEvent()
            {
                var inventoryP = Manager.UI.GetPopupUI<UI_Inventory>();
                inventoryP.Check.ValueChanged = () => SelectAnotherTabInInventory(inventoryP);
            }
            
            Close();
        }

        public override void Next(UI_Base popup)
        {
            ActiveIndicator(false);
            Timing.CallDelayed(0.15f, () =>
            {
                if (popup == null) return;
                var inven = Manager.UI.GetPopupUI<UI_Inventory>();
                if (inven == null) return;
                if (popup is UI_Inventory inventory)
                {
                    inventory.Check.ValueChanged = () => SelectAnotherTabInInventory(popup);

                    if (inventory.Selected.NowSelected().Value != -1)
                    {
                        Next(inventory.Selected);
                        return;
                    }
                    
                    if (inventory.Get<Image>("B_WeaponTab").sprite.name.Contains("Selected"))
                    {
                        Indicate("UI_Inventory_Item", false, GuideHandPositionType.X0Y68, 5);
                        Manager.UI.GetPopupUI<UI_Inventory>().MoveToTop(true);
                    }
                    else
                    {
                        Indicate("T_Weapon");
                    }
                }
                else if (popup is UI_InventorySelected weapon)
                {
                    if (weapon.IsWeapon)
                    {
                        weapon.Check.ValueChanged = () => SelectAnotherWeaponInWeapon(popup); 
                        if (weapon.CanAwakening)
                        {
                            Indicate("B_Awakening");
                        }
                        else
                        {
                            popup = Manager.UI.GetPopupUI<UI_Inventory>();
                            Indicate("UI_Inventory_Item", false, GuideHandPositionType.X0Y68, 5);
                            (popup as UI_Inventory).MoveToTop(true);
                        }
                    }
                    else
                    {
                        popup = Manager.UI.GetPopupUI<UI_Inventory>();
                        Indicate("T_Weapon");
                    }
                    
                }
                else if (popup is UI_EquipAwakening awakening && awakening.EquipmentType == EquipmentType.Weapon)
                {
                    awakening.Check.ValueChanged = () => SelectAnotherWeaponInAwakening(popup);
                    if (!awakening.Get<Image>("B_Awakening").material.name.Contains("Gray"))
                    {
                        Indicate("B_Awakening", true, GuideHandPositionType.X78Y59);
                    }
                    else
                    {
                        Indicate("B_Close");
                    }
                }
                else if (popup is UI_EquipMerge or UI_EquipGrowth or UI_EquipAwakening)
                {
                    ActiveIndicator(false);
                }
                else
                {
                    Close();
                }
            }, gameObject);

            void Indicate(string indicateName, bool isCanvasEnabled = true, GuideHandPositionType position = GuideHandPositionType.X0Y68, int idx = 0)
            {
                var indicate = idx == 0 ? popup.Get<RectTransform>(indicateName) : popup.Get<RectTransform>(indicateName, idx);
                Set(indicate, false, isCanvasEnabled, position);
            }
        }
        
        private void SelectAnotherTabInInventory(UI_Base popup)
        {
            Next(popup);
        }

        private void SelectAnotherWeaponInWeapon(UI_Base popup)
        {
            Next(popup);
        }

        private void SelectAnotherWeaponInAwakening(UI_Base popup)
        {
            Next(popup);
        }
    }
}