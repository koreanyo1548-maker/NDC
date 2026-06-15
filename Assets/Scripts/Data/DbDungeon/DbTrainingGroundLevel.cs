using System;
using System.Collections.Generic;
using System.Numerics;
using Data.Utils;
using Managers.Base;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using Data.DbStage;

namespace Data.DbDungeon
{
    [Serializable]
    public class DbTrainingGroundLevel: DbModel<DbTrainingGroundLevel, int>, IDbStage
    {
        [JsonConverter(typeof(StringToBigIntegerConverter))]
        public BigInteger Damage;

        private string Background = "_dungeonTraining";
        
        public override void Load()
        {
            fileName = "TrainingGroundLevel";
            if (Application.isPlaying) Init();
        }

        public static int GetLevelOfDamage(BigInteger damage)
        {
            foreach (var l in Meta)
            {
                if (damage < l.Value.Damage) return l.Key-1;
            }
            return Get(Count).Id;
        }

        public bool IsBoss()
        {
            return true;
        }

        public DbMonster GetRandomMonster()
        {
            return DbMonster.Get(32);
        }

        public int GetStageGoalCount()
        {
            return 0;
        }

        public StageType GetStageType()
        {
            return StageType.Training;
        }

        public BigInteger GetMonsterAttack()
        {
            return 0;
        }

        public BigInteger GetMonsterHp()
        {
            return 0;
        }

        public int GetSpawnCoolTime(bool isBigWave = false)
        {
            return -1;
        }

        public int GetMaxMonsterCount()
        {
            return 1;
        }

        public int GetMinSpawnCount(bool isBigWave = false)
        {
            return 1;
        }

        public int GetSpawnCount(bool isBigWave = false)
        {
            return 1;
        }

        public BigInteger GetPower()
        {
            throw new Exception();
        }

        public string GetBossResource()
        {
            return DbMonster.Get(32).Resource;
        }

        public string GetBackground()
        {
            return Background;
        }

        public BGMType GetBGM()
        {
            return BGMType.Dungeon1;
        }
    }
}