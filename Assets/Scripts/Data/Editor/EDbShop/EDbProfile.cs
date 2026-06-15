using System;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbProfile
    {
        public int Id;
        public ProfileConditionType Condition;
        public int Goal;
        public string Resource;
    }
}