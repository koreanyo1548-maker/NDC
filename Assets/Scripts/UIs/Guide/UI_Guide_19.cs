using Managers;
using MEC;
using UIBases;
using UIs.Character;
using UIs.FieldMain;
using UnityEngine;

namespace UIs.Guide
{
    public class UI_Guide_19 : UI_Guide
    {
        public override void Open()
        {
            gameObject.SetActive(true);

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
                    
                    if (character.CurOpened == 1)
                    {
                        Set(popup.Get<Transform>("Level").Find("B_LevelUp").GetComponent<RectTransform>());
                    }
                    else
                    {
                        Indicate("T_Level");
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