using System;
using System.Collections.Generic;

namespace Data.Editor.EDbDungeon
{
    [Serializable]
    public class EDbSkillGrowthDungeonReward
    {
        public int Id;
        public List<string> Reward;
        public List<string> FirstClearReward;
    }
}