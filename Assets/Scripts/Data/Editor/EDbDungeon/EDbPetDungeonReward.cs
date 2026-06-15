using System;
using System.Collections.Generic;

namespace Data.Editor.EDbDungeon
{
    [Serializable]
    public class EDbPetDungeonReward
    {
        public int Id;
        public int Counts;
        public List<string> Probability;
        public List<string> FirstClearReward;
    }
}