using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.FieldMain.MainLeft;
using UIs.Inventory;
using UIs.Skill;
using UnityEngine;
using UnityEngine.UI;

namespace UIs.Guide
{
    public class UI_Guide_22: UI_Guide
    {
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainLeft>().Get<RectTransform>("UI_AdBuff");
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