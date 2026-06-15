using ThirdParty;
using UIBases;
using UIs.Utils;
using Utils;

namespace UIs.Etc.Warning
{
    public class UI_Quit: UI_Popup
    {
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Util.FindChild(gameObject, "B_No", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Util.FindChild(gameObject, "B_Yes", true).BindEvent(Functions.TrueCondition, _ => CloseGame(), UIEffectType.Bounce);

            return true;
        }

        private void CloseGame()
        {
            PlayFabManager.Store.SaveAndExit();
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