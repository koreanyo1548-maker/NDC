using ThirdParty;
using UIBases;
using UIs.Utils;
using Utils;

namespace UIs.Etc
{
    public class UI_Logout: UI_Base
    {
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => gameObject.SetActive(false));
            Util.FindChild(gameObject, "B_Cancel", true).BindEvent(Functions.TrueCondition, _ => gameObject.SetActive(false));
            Util.FindChild(gameObject, "B_Logout", true).BindEvent(Functions.TrueCondition, _ => TryLogout());
            
            return true;
        }

        private void TryLogout()
        {
            //HiveManager.LogOut();
        }
    }
}