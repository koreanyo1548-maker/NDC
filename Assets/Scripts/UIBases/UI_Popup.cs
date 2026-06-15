using Managers;
using Managers.Base;

namespace UIBases
{
    public abstract class UI_Popup: UI_Base
    {
        
        public abstract bool NeedRaycast();
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Manager.UI.SetCanvas(gameObject, NeedRaycast(), true);
            return true;
        }

        public virtual void ClosePopupUI()
        { 
            Manager.Sound.PlaySFX(SFXType.UI_Close);
            Manager.UI.ClosePopupUI();
        }

        public abstract void WhenPopupClosed();

        public virtual bool CanClose()
        {
            return true;
        }
    }
}