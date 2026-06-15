using Data;
using Managers;
using MEC;
using UIBases;
using UIs.Character;
using UIs.FieldMain;
using UnityEngine;

namespace UIs.Guide
{
    public class UI_Guide_Promotion: UI_Guide
    {
        protected int _promotionIndex;
        
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.Field.CurField.Value != FieldType.Stage)
            {
                Close();
                return;
            }

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainBottom>().Get<RectTransform>("B_Character");
                Set(indicate);
                return;
            }

            var character = Manager.UI.GetPopupUI<UI_Character>();

            if (character != null)
            {
                Next(character);
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
                if (popup is UI_Character character)
                {
                    character.Check.ValueChanged = () => SelectAnotherTabInCharacter(popup);
                    
                    if (character.CurOpened == 3)
                    {
                        Set(popup.Get<Transform>("PromotionParent").GetChild(0).Find("B_Enter").GetComponent<RectTransform>(), true, false);
                    }
                    else
                    {
                        Indicate("T_Promotion");
                    }
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

        private void SelectAnotherTabInCharacter(UI_Base popup)
        {
            Next(popup);
        }
    }
}