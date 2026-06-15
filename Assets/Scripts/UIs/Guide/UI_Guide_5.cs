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
    public class UI_Guide_5: UI_Guide
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
            
            if (SetDisable<UI_EquipAwakening>()) return;
            if (SetDisable<UI_EquipGrowth>()) return;

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
                var merge = Manager.UI.GetPopupUI<UI_EquipMerge>();
                if (merge != null)
                {
                    InventoryCheckEvent();
                    Next(merge);
                    return;
                }
                
                Next(inventory);
                return;
            }

            Close();

            void InventoryCheckEvent()
            {
                var inventoryP = Manager.UI.GetPopupUI<UI_Inventory>();
                inventoryP.Check.ValueChanged = () => SelectAnotherTabInInventory(inventoryP);
            }
        }

        public override void Next(UI_Base popup)
        {
            ActiveIndicator(false);
            Timing.CallDelayed(0.15f, () =>
            {
                if (popup == null) return;
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
                        Indicate("UI_Inventory_Item", false, GuideHandPositionType.X0Y68, 4);
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
                        if (weapon.CanMerge)
                        {
                            Indicate("B_Merge");
                        }
                        else
                        {
                            popup = Manager.UI.GetPopupUI<UI_Inventory>();
                            Indicate("UI_Inventory_Item", false, GuideHandPositionType.X0Y68, 4);
                            (popup as UI_Inventory).MoveToTop(true);
                        }
                    }
                    else
                    {
                        popup = Manager.UI.GetPopupUI<UI_Inventory>();
                        Indicate("T_Weapon");
                    }
                }
                else if (popup is UI_EquipMerge merge && merge.EquipmentType == EquipmentType.Weapon)
                {
                    merge.Check.ValueChanged = () => SelectAnotherWeaponInMerge(popup);
                    if (!merge.Get<Image>("B_Merge").material.name.Contains("Gray"))
                    {
                        Indicate("B_Merge", true, GuideHandPositionType.X78Y59);
                    }
                    else
                    {
                        Indicate("B_Close");
                    }
                }
                else if (popup is UI_EquipAwakening or UI_EquipGrowth or UI_EquipMerge)
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

        private void SelectAnotherWeaponInMerge(UI_Base popup)
        {
            Next(popup);
        }
    }
}