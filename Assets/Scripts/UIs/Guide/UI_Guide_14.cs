using Data;

namespace UIs.Guide
{
    public class UI_Guide_14 : UI_Guide_Dungeon
    {
        public override bool Init()
        {
            if (!base.Init()) return false;

            _fieldType = FieldType.SkillGrowth;
            
            return true;
        }
    }

}