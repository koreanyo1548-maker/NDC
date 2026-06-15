using System;

namespace Data.Editor.EDbSummon
{
    [Serializable]
    public class EDbSummonNecklaceProbability
    {
        public int Id;
        public int Normal;
        public int Magic;
        public int Rare;
        public int Unique;
        public int Heroic;
        public int Legendary;
        public int Mythic;
    }
}