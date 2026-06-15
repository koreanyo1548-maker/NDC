using System;
using System.Collections.Generic;

namespace Data.Editor.EDbPromote
{
    [Serializable]
    public class EDbPromotionDungeon
    {
        public int Id;
        public StageType StageType;
        public int MinSpawnCount;
        public int MaxSpawnCount;
        public int MaxMonsterCount;
        public int SpawnCoolTime;
        public int GoalCount;
        public string MonsterAttack;
        public string MonsterHp;
        public string Power;
        public List<int> Monsters;
        public int NameId;
        public string Background;
    }
}