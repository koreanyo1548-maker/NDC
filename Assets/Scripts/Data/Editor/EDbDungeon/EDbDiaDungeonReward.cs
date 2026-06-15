using System;
using System.Collections.Generic;

namespace Data.Editor.EDbDungeon
{
    [Serializable]
    public class EDbDiaDungeonReward
    {
        public int Id;
        public List<string> Reward;
        public int MonsterReward;
    }
}