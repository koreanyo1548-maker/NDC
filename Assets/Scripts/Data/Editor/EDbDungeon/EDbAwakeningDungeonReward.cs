using System;
using System.Collections.Generic;

namespace Data.Editor.EDbDungeon
{
    [Serializable]
    public class EDbAwakeningDungeonReward
    {
        public int Id;
        public List<string> Reward;
        public List<string> FirstClearReward;

    }
}