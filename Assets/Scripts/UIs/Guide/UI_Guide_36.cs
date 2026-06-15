using Managers;
using UIBases;
using UIs.FieldMain;
using UIs.FieldMain.MainStage;
using UnityEngine;

namespace UIs.Guide
{
    public class UI_Guide_36 : UI_Guide
    {
        public override void Open()
        {
            gameObject.SetActive(true);

            if (Manager.UI.PopupCount == 0)
            {
                var mainStage = Manager.UI.GetSceneUI<UI_MainStage>();
                var nextBtn = mainStage.transform.Find("SafeArea").Find("StageClear").Find("B_Next");
                if (nextBtn.gameObject.activeSelf)
                {
                    var indicate = nextBtn.GetComponent<RectTransform>();
                    Set(indicate);
                }
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