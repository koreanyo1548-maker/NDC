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
    public class UI_Guide_10: UI_Guide
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

                var growth = Manager.UI.GetPopupUI<UI_EquipGrowth>();
                if (growth != null)
                {
                    InventoryCheckEvent();
                    Next(growth);
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

                    if (inventory.Get<Image>("B_AccessoryTab").sprite.name.Contains("Selected"))
                    {
                        Indicate("UI_Inventory_Item", false);
                        Manager.UI.GetPopupUI<UI_Inventory>().MoveToTop(false);
                    }
                    else
                    {
                        Indicate("T_Accessory");
                    }
                }
                else if ( popup is UI_InventorySelected accessory)
                {
                    if (!accessory.IsWeapon)
                    {
                        accessory.Check.ValueChanged = () => SelectAnotherAccessoryInAccessory(popup); 
                        if (accessory.CanGrowth)
                        {
                            Indicate("B_Growth");
                        }
                        else
                        {
                            popup = Manager.UI.GetPopupUI<UI_Inventory>();
                            Indicate("UI_Inventory_Item", false);
                            (popup as UI_Inventory).MoveToTop(false);
                        }
                    }
                    else
                    {
                        popup = Manager.UI.GetPopupUI<UI_Inventory>();
                        Indicate("T_Accessory");
                    }
                }
                else if (popup is UI_EquipGrowth growth && growth.EquipmentType == EquipmentType.Accessory)
                {
                    growth.Check.ValueChanged = () => SelectAnotherAccessoryInGrowth(popup);
                    if (!growth.Get<Image>("B_Growth").material.name.Contains("Gray"))
                    {
                        Indicate("B_Growth", true, GuideHandPositionType.X78Y59);
                    }
                    else
                    {
                        Indicate("B_Close");
                    }
                }
                else if (popup is UI_EquipMerge or UI_EquipAwakening or UI_EquipGrowth)
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
        }
        

        private void SelectAnotherTabInInventory(UI_Base popup)
        {
            Next(popup);
        }

        private void SelectAnotherAccessoryInAccessory(UI_Base popup)
        {
            Next(popup);
        }

        private void SelectAnotherAccessoryInGrowth(UI_Base popup)
        {
            Next(popup);
        }
    }
}