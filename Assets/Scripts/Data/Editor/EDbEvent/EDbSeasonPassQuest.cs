using System;

namespace Data.Editor.EDbEvent
{
    [Serializable]
    public class EDbSeasonPassQuest
    {
        public QuestType Id;
        public int Goal;
        public int Point;
        public int NameId;
    }
}