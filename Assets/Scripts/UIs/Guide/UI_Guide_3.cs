using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.Summon;
using UnityEngine;

namespace UIs.Guide
{
    public class UI_Guide_3: UI_Guide
    {
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.UI.PopupCount == 0)
            {
                var indicate = Manager.UI.GetSceneUI<UI_MainBottom>().Get<RectTransform>("B_Summon");
                Set(indicate);
                return;
            }
            
            var summon = Manager.UI.GetPopupUI<UI_Summon>();
            if (summon != null)
            {
                Next(summon);
                return;
            }
            
            Close();
        }

        public override void Next(UI_Base popup)
        {
            if (popup is not UI_Summon)
            {
                Close();
                return;
            }

            ActiveIndicator(false);
            Timing.CallDelayed(0.15f, () =>
            {
                if (popup == null) return;
                var indicate = popup.Get<Transform>("UI_Summon_Item_0").Find("B_Summon1").GetComponent<RectTransform>();
                Set(indicate, false, false);
            }, gameObject);
        }
    }
}