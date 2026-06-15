using System;
using System.Collections.Generic;
using System.Numerics;
using Data.DbStage;
using Data.Utils;
using Exceptions;
using Managers.Base;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.DbDungeon
{
    [Serializable]
    public class DbBlackMarketDungeonLevel: DbModel<DbBlackMarketDungeonLevel, int>, IDbStage
    {
        public StageType StageType;
        public int Stage;
        public int SpawnCount;
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
            fileName = "BlackMarketDungeonLevel";
            if (Application.isPlaying) Init();
            ForEach(d =>
            {
                var stage = DbStageLevel.Get(d.Stage);
                d.MonsterAttack = stage.MonsterAttack * d.MonsterAttack / 10000;
                d.MonsterHp = stage.MonsterHp * d.MonsterHp / 10000;
                d.Power = stage.Power * d.Power / 10000;
            });
        }

        public DbMonster GetRandomMonster()
        {
            var count = Monsters.Count;
            return DbMonster.Get(Monsters[UnityEngine.Random.Range(0, count)]);
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
            return SpawnCount;
        }

        public int GetSpawnCount(bool isBigWave = false)
        {
            return SpawnCount;
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
            return BGMType.Dungeon3;
        }
        public bool IsBoss()
        {
            return StageType == StageType.Boss;
        }
    }
}