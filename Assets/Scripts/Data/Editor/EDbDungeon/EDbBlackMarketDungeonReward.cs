using System;
using System.Collections.Generic;

namespace Data.Editor.EDbDungeon
{
    [Serializable]
    public class EDbBlackMarketDungeonReward
    {
        public int Id;
        public List<string> Reward;
        public int MonsterReward;
    }
}