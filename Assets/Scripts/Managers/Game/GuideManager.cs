using System;
using Controller;
using Controller.Infos;
using UIBases;
using UIs.Guide;
using UnityEngine;

namespace Managers.Game
{
    public class GuideManager
    {
        private UI_Guide _curGuide;

        public GuideManager()
        {
            // Debug.Log("test2");
            QuestController.data.QuestId.ValueChanged += (_, _) => CheckCurQuest();
        }

        public void Init()
        {
            CheckCurQuest();
        }
        
        public void CheckCurQuest()
        {
            if (_curGuide != null)
            {
                _curGuide.Close();
                _curGuide = null;
            }            
            
            var effect = QuestController.data.GuideEffect;
            if (effect == -1) return;
            if (QuestController.data.CanRewarded) return;

            _curGuide =
                effect == 1 ? Manager.UI.ShowSceneUI<UI_Guide_1>("UI_Guide") :
                effect == 2 ? Manager.UI.ShowSceneUI<UI_Guide_2>("UI_Guide") :
                effect == 3 ? Manager.UI.ShowSceneUI<UI_Guide_3>("UI_Guide") :
                effect == 4 ? Manager.UI.ShowSceneUI<UI_Guide_4>("UI_Guide") :
                effect == 5 ? Manager.UI.ShowSceneUI<UI_Guide_5>("UI_Guide") :
                effect == 6 ? Manager.UI.ShowSceneUI<UI_Guide_6>("UI_Guide") :
                effect == 7 ? Manager.UI.ShowSceneUI<UI_Guide_7>("UI_Guide") :
                effect == 8 ? Manager.UI.ShowSceneUI<UI_Guide_8>("UI_Guide") :
                effect == 9 ? Manager.UI.ShowSceneUI<UI_Guide_9>("UI_Guide") :
                effect == 10 ? Manager.UI.ShowSceneUI<UI_Guide_10>("UI_Guide") :
                effect == 11 ? Manager.UI.ShowSceneUI<UI_Guide_11>("UI_Guide") :
                effect == 12 ? Manager.UI.ShowSceneUI<UI_Guide_12>("UI_Guide") :
                effect == 13 ? Manager.UI.ShowSceneUI<UI_Guide_13>("UI_Guide") :
                effect == 14 ? Manager.UI.ShowSceneUI<UI_Guide_14>("UI_Guide") :
                effect == 15 ? Manager.UI.ShowSceneUI<UI_Guide_15>("UI_Guide") :
                effect == 16 ? Manager.UI.ShowSceneUI<UI_Guide_16>("UI_Guide") :
                effect == 17 ? Manager.UI.ShowSceneUI<UI_Guide_17>("UI_Guide") :
                effect == 18 ? Manager.UI.ShowSceneUI<UI_Guide_18>("UI_Guide") :
                effect == 19 ? Manager.UI.ShowSceneUI<UI_Guide_19>("UI_Guide") :
                effect == 20 ? Manager.UI.ShowSceneUI<UI_Guide_20>("UI_Guide") :
                effect == 21 ? Manager.UI.ShowSceneUI<UI_Guide_21>("UI_Guide") :
                effect == 22 ? Manager.UI.ShowSceneUI<UI_Guide_22>("UI_Guide") :
                effect == 23 ? Manager.UI.ShowSceneUI<UI_Guide_23>("UI_Guide") :
                effect == 24 ? Manager.UI.ShowSceneUI<UI_Guide_24>("UI_Guide") :
                effect == 25 ? Manager.UI.ShowSceneUI<UI_Guide_25>("UI_Guide") :
                effect == 26 ? Manager.UI.ShowSceneUI<UI_Guide_26>("UI_Guide") :
                effect == 27 ? Manager.UI.ShowSceneUI<UI_Guide_27>("UI_Guide") :
                effect == 28 ? Manager.UI.ShowSceneUI<UI_Guide_28>("UI_Guide") :
                effect == 29 ? Manager.UI.ShowSceneUI<UI_Guide_29>("UI_Guide") :
                effect == 30 ? Manager.UI.ShowSceneUI<UI_Guide_30>("UI_Guide") :
                effect == 31 ? Manager.UI.ShowSceneUI<UI_Guide_31>("UI_Guide") :
                effect == 32 ? Manager.UI.ShowSceneUI<UI_Guide_32>("UI_Guide") :
                effect == 33 ? Manager.UI.ShowSceneUI<UI_Guide_33>("UI_Guide") :
                effect == 34 ? Manager.UI.ShowSceneUI<UI_Guide_34>("UI_Guide") :
                effect == 35 ? Manager.UI.ShowSceneUI<UI_Guide_35>("UI_Guide") :
                effect == 36 ? Manager.UI.ShowSceneUI<UI_Guide_36>("UI_Guide") :
                effect == 37 ? Manager.UI.ShowSceneUI<UI_Guide_37>("UI_Guide") : null;
            
            _curGuide.Open();
        }

        public void Check(UI_Base popup)
        {
            if (_curGuide == null) return;

            _curGuide.Next(popup);
        }
    }
}