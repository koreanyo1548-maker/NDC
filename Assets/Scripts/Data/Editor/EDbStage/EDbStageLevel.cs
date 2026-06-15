using System;
using System.Collections.Generic;

namespace Data.Editor.EDbStage
{
    [Serializable]
    public class EDbStageLevel
    {
        public int Id;
        public StageType StageType;
        public int MinSpawnCount;
        public int MaxSpawnCount;
        public int MaxMonsterCount;
        public int SpawnCoolTime;
        public int BWMinSpawnCount;
        public int BWMaxSpawnCount;
        public int BWSpawnCoolTime;
        public int GoalCount;
        public string MonsterAttack;
        public string MonsterHp;
        public string Power;
        public List<int> Monsters;
        public string Background;
    }
}