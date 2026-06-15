using System;

using TMPro;
using UIBases;
using UIs.Utils;
using Utils;

namespace UIs.Etc
{
    public class UI_DefaultPopup : UI_Popup
    {
        private Action _yesAction;

        private TextMeshProUGUI _messageText;

        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "B_Yes", true).BindEvent(Functions.TrueCondition, _ => DoAction());
            Util.FindChild(gameObject, "B_No", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);

            _messageText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Message", true);
            
            return true;
        }

        public void Set(int message, Action toDo)
        {
            if (!_isInit) Init();
            
            _yesAction = toDo;
            _messageText.text = LocalString.Get(message);
        }
        private void DoAction()
        {
            ClosePopupUI();
            _yesAction();
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}