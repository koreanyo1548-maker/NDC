using UIBases;
using Utils;

namespace UIs.FieldMain.MainLeft
{
    public class UI_MainLeft: UI_Scene
    {
        private void Start()
        {
            Util.FindChild(gameObject,"UI_MainQuest", true).GetOrAddComponent<UI_MainQuest>();
            Util.FindChild(gameObject,"UI_MainEvents", true).GetOrAddComponent<UI_MainEvents>();
            Util.FindChild(gameObject, "UI_AdBuff", true).GetOrAddComponent<UI_MainAdBuff>();
            Init();
        }
        public override bool NeedRaycast()
        {
            return true;
        }
    }
}