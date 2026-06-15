using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.Inventory;
using UIs.Inventory.Equipment;
using UIs.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Guide
{
    public class UI_Guide_29: UI_Guide
    {
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainBottom>().Get<RectTransform>("B_Pet");
                Set(indicate);
                return;
            }

            if (SetDisable<UI_EquipAwakening>()) return;
            if (SetDisable<UI_EquipGrowth>()) return;
            if (SetDisable<UI_Pet>()) return;
            if (SetDisable<UI_Book>()) return;

            bool SetDisable<T>() where T: UI_Popup
            {
                var disable = Manager.UI.GetPopupUI<T>();
                if (disable != null)
                {
                    PetInventoryCheckEvent();  
                    Next(disable);
                    return true;
                }
                return false;
            }
            
            var inventory = Manager.UI.GetPopupUI<UI_PetInventory>();

            if (inventory != null)
            {
                Next(inventory);
                return;
            }

            Close();

            void PetInventoryCheckEvent()
            {
                var inventoryP = Manager.UI.GetPopupUI<UI_PetInventory>();
                inventoryP.Check.ValueChanged = () => SelectAnotherTabInInventory(inventoryP);
            }
        }

        public override void Next(UI_Base popup)
        {
            ActiveIndicator(false);
            Timing.CallDelayed(0.15f, () =>
            {
                if (popup == null) return;
                if (popup is UI_PetInventory inventory)
                {
                    inventory.Check.ValueChanged = () => SelectAnotherTabInInventory(popup);

                    if (inventory.Get<Image>("B_PetTab").sprite.name.Contains("Selected"))
                    {
                        Indicate("B_Growth");
                    }
                    else
                    {
                        Indicate("T_Pet");
                    }
                }
                else if (popup is UI_Pet or UI_Book or UI_EquipPet or UI_EquipGrowth or UI_EquipAwakening)
                {
                    ActiveIndicator(false);
                }
                else
                {
                    Close();
                }
            }, gameObject);

            void Indicate(string indicateName)
            {
                var indicate = popup.Get<RectTransform>(indicateName);
                Set(indicate);
            }
        }

        private void SelectAnotherTabInInventory(UI_Base popup)
        {
            Next(popup);
        }
    }
}