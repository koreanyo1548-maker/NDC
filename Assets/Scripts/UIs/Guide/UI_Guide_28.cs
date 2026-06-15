using Controller.Currency;
using Controller.Have;
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
    public class UI_Guide_28: UI_Guide
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
            
            if (SetDisable<UI_EquipGrowth>()) return;
            if (SetDisable<UI_EquipAwakening>()) return;
            if (SetDisable<UI_Pet>()) return;

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
                var book = Manager.UI.GetPopupUI<UI_Book>();
                if (book != null)
                {
                    PetInventoryCheckEvent();
                    Next(book);
                    return;
                }
                
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

                    if (inventory.Get<Image>("B_BookTab").sprite.name.Contains("Selected"))
                    {
                        var bookshelf = CurrencyController.data.BookShelves.Value[0].Value;
                        if (bookshelf.HaveBook.Value)
                        {
                            if (bookshelf.CanOpen.Value) Indicate("B_Unseal", false);
                            else Indicate("B_DiaOpen", false);
                        }
                        else
                        {
                            Indicate("UI_Book_Item", false);
                            Manager.UI.GetPopupUI<UI_PetInventory>().MoveToTop(true);
                        }
                    }
                    else
                    {
                        Indicate("T_Book");
                    }
                }
                else if (popup is UI_Book)
                {
                    Indicate("B_Unseal", true, GuideHandPositionType.X78Y59);
                }
                else if (popup is UI_Pet or UI_EquipAwakening or UI_EquipGrowth or UI_EquipPet)
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
    }
}