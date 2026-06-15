using Managers;
using UIBases;
using UIs.FieldMain;
using UnityEngine;

namespace UIs.Guide
{
    public class UI_Guide_33: UI_Guide
    {
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainRight>().Get<RectTransform>("B_AutoSkillOnOff");
                Set(indicate);
                return;
            }
            
            Close();
        }

        public override void Next(UI_Base popup)
        {
            ActiveIndicator(false);
            Close();
        }
    }
}