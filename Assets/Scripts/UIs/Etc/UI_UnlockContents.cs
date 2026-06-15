using System;
using TMPro;
using UIBases;
using UIs.Utils;
using Utils;

namespace UIs.Etc
{
    public class UI_UnlockContents: UI_Popup
    {
        private TextMeshProUGUI _contents;
        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            Util.FindChild(gameObject, "B_Confirm", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            _contents = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Unlock", true);
            
            return true;
        }

        public void SetText(int text)
        {
            if (!_isInit) Init();
            _contents.text = LocalString.Get(text);
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