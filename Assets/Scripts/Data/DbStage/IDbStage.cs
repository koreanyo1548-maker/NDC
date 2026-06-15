using System.Numerics;
using Managers.Base;

namespace Data.DbStage
{
    public interface IDbStage
    {
        public bool IsBoss();
        public DbMonster GetRandomMonster();
        public int GetStageGoalCount();
        public StageType GetStageType();
        public BigInteger GetMonsterAttack();
        public BigInteger GetMonsterHp();
        public int GetSpawnCoolTime(bool isBigWave = false);
        public int GetMaxMonsterCount();
        public int GetMinSpawnCount(bool isBigWave = false);
        public int GetSpawnCount(bool isBigWave = false);
        public BigInteger GetPower();
        public string GetBossResource();
        public string GetBackground();
        public BGMType GetBGM();
    }
}