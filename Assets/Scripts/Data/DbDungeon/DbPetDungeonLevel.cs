using System;
using System.Collections.Generic;
using System.Numerics;
using Data.DbStage;
using Data.Utils;
using Managers.Base;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.DbDungeon
{
    [Serializable]
    public class DbPetDungeonLevel : DbModel<DbPetDungeonLevel, int>, IDbStage
    {
        public StageType StageType;
        public int MinSpawnCount;
        public int MaxSpawnCount;
        public int MaxMonsterCount;
        public int SpawnCoolTime;
        public int GoalCount;
        [JsonConverter(typeof(StringToBigIntegerConverter))]
        public BigInteger MonsterAttack;
        [JsonConverter(typeof(StringToBigIntegerConverter))]
        public BigInteger MonsterHp;
        [JsonConverter(typeof(StringToBigIntegerConverter))]
        public BigInteger Power;
        public List<int> Monsters;
        public string Background;
        
        public override void Load()
        {
            fileName = "PetDungeonLevel";
            if (Application.isPlaying) Init();
        }

        public bool IsBoss()
        {
            return StageType == StageType.Boss;
        }

        public DbMonster GetRandomMonster()
        {
            var count = Monsters.Count;
            return DbMonster.Get(Monsters[Random.Range(0, count)]);
        }

        public int GetStageGoalCount()
        {
            return GoalCount;
        }

        public StageType GetStageType()
        {
            return StageType;
        }

        public BigInteger GetMonsterAttack()
        {
            return MonsterAttack;
        }

        public BigInteger GetMonsterHp()
        {
            return MonsterHp;
        }

        public int GetSpawnCoolTime(bool isBigWave = false)
        {
            return SpawnCoolTime;
        }

        public int GetMaxMonsterCount()
        {
            return MaxMonsterCount;
        }

        public int GetMinSpawnCount(bool isBigWave = false)
        {
            return MinSpawnCount;
        }

        public int GetSpawnCount(bool isBigWave = false)
        {
            return Random.Range(MinSpawnCount, MaxSpawnCount);
        }

        public int GetMaxSpawnCount(bool isBigWave = false)
        {
            return MaxSpawnCount;
        }

        public BigInteger GetPower()
        {
            return Power;
        }

        public string GetBossResource()
        {
            return DbMonster.Get(Monsters[0]).Resource;
        }

        public string GetBackground()
        {
            return Background;
        }
        public BGMType GetBGM()
        {
            return BGMType.Dungeon2;
        }
    }
}