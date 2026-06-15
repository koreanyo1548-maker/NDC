using Data;

namespace UIs.Guide
{
    public class UI_Guide_27: UI_Guide_Dungeon
    {
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            _fieldType = FieldType.Pet;
            
            return true;
        }
    }
}