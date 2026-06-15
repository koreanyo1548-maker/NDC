using System;
using System.Collections.Generic;

namespace Data.Editor.EDbDungeon
{
    [Serializable]
    public class EDbBlackMarketDungeonLevel
    {   
        public int Id;
        public int Stage;
        public StageType StageType;
        public int SpawnCount;
        public int MaxMonsterCount;
        public int SpawnCoolTime;
        public int GoalCount;
        public string MonsterAttack;
        public string MonsterHp;
        public string Power;
        public List<int> Monsters;
        public string Background;
    }
}