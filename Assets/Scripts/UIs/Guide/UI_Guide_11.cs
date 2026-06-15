using Data;

namespace UIs.Guide
{
    public class UI_Guide_11: UI_Guide_Dungeon
    {
        public override bool Init()
        {
            if (!base.Init()) return false;

            _fieldType = FieldType.Awakening;
            
            return true;
        }
    }
}