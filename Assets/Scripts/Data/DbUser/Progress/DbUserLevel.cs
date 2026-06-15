using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using CodeStage.AntiCheat.ObscuredTypes;
using Controller.Currency;
using Data.DbDefinition;
using Data.DbShop;
using Data.Stores;
using Data.Utils;
using Newtonsoft.Json;
using Utils;

namespace Data.DbUser.Progress
{
    public class DbUserLevel: DbUserModel<DbUserLevel, int>, IDayDiffChecker
    {
        public DbField<int> Level { get; private set; }
        public DbField<BigInteger> Exp { get; private set; }
        public DbField<int> Stage { get; private set; }
        public DbField<int> MaxStage { get; private set; }
        public DbField<bool> IsStageClear { get; private set; }
        public DbField<int> AwakeningDungeonStage { get; private set; }
        public DbField<int> SkillGrowthDungeonStage { get; private set; }
        public DbField<int> PetDungeonStage { get; private set; }
        public DbField<int> BlackMarketDungeonStage { get; private set; }
        public DbField<int> DiaDungeonStage { get; private set; }
        public DbField<long> BlackMarketDungeonReward { get; private set; }
        public DbField<long> DiaDungeonReward { get; private set; }
        public DbField<int> TrainingGroundStage { get; private set; }
        public DbField<int> BibleLevel { get; private set; }
        public DbField<int> Promotion { get; private set; }
        public DbField<BigInteger> MaxPower { get; private set; }
        public DbField<ObscuredBigInteger> MaxTraining { get; private set; }
        public DbField<long> WeaponSummonCount { get; private set; }
        public DbField<int> WeaponSummonLevel { get; private set; }
        public DbField<int> WeaponSummonReward { get; private set; }
        public DbField<BigInteger> WeaponSummonExp { get; private set; }
        public DbField<long> AccessorySummonCount { get; private set; }
        public DbField<int> AccessorySummonLevel { get; private set; }
        public DbField<int> AccessorySummonReward { get; private set; }
        public DbField<BigInteger> AccessorySummonExp { get; private set; }
        public DbField<long> SkillSummonCount { get; private set; }
        public DbField<int> SkillSummonLevel { get; private set; }
        public DbField<int> SkillSummonReward { get; private set; }
        public DbField<BigInteger> SkillSummonExp { get; private set; }
        public DbField<long> RelicSummonCount { get; private set; }
        public DbField<long> NecklaceSummonCount { get; private set; }
        public DbField<int> NecklaceSummonLevel { get; private set; }
        public DbField<BigInteger> NecklaceSummonExp { get; private set; }
        // [IgnoreDataMember] public DbField<BigInteger> Power { get; private set; }
            

        public override void Set(List<DbUserLevel> obj)
        {
            Init(obj);
        }

        #region DayDiff

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            var prevReset = UserInfo.saved.info.levelResetDate;
            dayDiff = Define.GetDayDiff(prevReset, now);
            var isNewWeek = Define.IsWeekDiff(prevReset, now, dayDiff, (DayOfWeek)DbPlay.Get(PlayType.TrainingGroundResetDay).Value);
            
            if (isNewWeek)
            {
                TrainingGroundStage.Value = 0;
                MaxTraining.Value = 0;
            }

            UserInfo.saved.info.levelResetDate = now;
        }
        
        #endregion

        protected override List<DbUserLevel> GetInitials()
        {
            return new List<DbUserLevel>
            {
                new()
            };
        }

        public override List<DbUserLevel> AdjustDataModification(List<DbUserLevel> obj)
        {
            return obj;
        }

        [JsonConstructor]
        public DbUserLevel(int Id, int Level, BigInteger Exp, int Stage, int MaxStage, bool IsStageClear, 
            int AwakeningDungeonStage, int SkillGrowthDungeonStage, int PetDungeonStage, 
            int BlackMarketDungeonStage, int DiaDungeonStage,
            long BlackMarketDungeonReward, long DiaDungeonReward,
            int TrainingGroundStage,
            int BibleLevel, int Promotion, BigInteger MaxPower, BigInteger MaxTraining,
            long WeaponSummonCount, int WeaponSummonLevel, int WeaponSummonReward, BigInteger WeaponSummonExp, 
            long AccessorySummonCount, int AccessorySummonLevel, int AccessorySummonReward, BigInteger AccessorySummonExp,
            long SkillSummonCount, int SkillSummonLevel, int SkillSummonReward, BigInteger SkillSummonExp,
            long RelicSummonCount, long NecklaceSummonCount, int NecklaceSummonLevel, BigInteger NecklaceSummonExp)
        {
            this.Id = Id;
            this.Level = new DbField<int>(Level, 0, this);
            this.Exp = new DbField<BigInteger>(Exp, 0, this);
            this.Stage = new DbField<int>(Stage, 0, this);
            this.MaxStage = new DbField<int>(MaxStage, 0, this);
            this.IsStageClear = new DbField<bool>(IsStageClear, 0, this);
            this.AwakeningDungeonStage = new DbField<int>(AwakeningDungeonStage, 0, this);
            this.SkillGrowthDungeonStage = new DbField<int>(SkillGrowthDungeonStage, 0, this);
            this.PetDungeonStage = new DbField<int>(PetDungeonStage, 0, this);
            this.BlackMarketDungeonStage = new DbField<int>(BlackMarketDungeonStage, 0, this);
            this.DiaDungeonStage = new DbField<int>(DiaDungeonStage, 0, this);
            this.BlackMarketDungeonReward = new DbField<long>(BlackMarketDungeonReward, 0, this);
            this.DiaDungeonReward = new DbField<long>(DiaDungeonReward, 0, this);
            this.TrainingGroundStage = new DbField<int>(TrainingGroundStage, 0, this);
            this.BibleLevel = new DbField<int>(BibleLevel, 0, this);
            this.Promotion = new DbField<int>(Promotion, 0, this);
            this.MaxPower = new DbField<BigInteger>(MaxPower, 0, this);
            this.MaxTraining = new DbField<ObscuredBigInteger>(MaxTraining, 0, this);
            this.WeaponSummonCount = new DbField<long>(WeaponSummonCount, 0, this);
            this.WeaponSummonLevel = new DbField<int>(WeaponSummonLevel, 0, this);
            this.WeaponSummonReward = new DbField<int>(WeaponSummonReward, 0, this);
            this.WeaponSummonExp = new DbField<BigInteger>(WeaponSummonExp, 0, this);
            this.AccessorySummonCount = new DbField<long>(AccessorySummonCount, 0, this);
            this.AccessorySummonLevel = new DbField<int>(AccessorySummonLevel, 0, this);
            this.AccessorySummonReward = new DbField<int>(AccessorySummonReward, 0, this);
            this.AccessorySummonExp = new DbField<BigInteger>(AccessorySummonExp, 0, this);
            this.SkillSummonCount = new DbField<long>(SkillSummonCount, 0, this);
            this.SkillSummonLevel = new DbField<int>(SkillSummonLevel, 0, this);
            this.SkillSummonReward = new DbField<int>(SkillSummonReward, 0, this);
            this.SkillSummonExp = new DbField<BigInteger>(SkillSummonExp, 0, this);
            this.RelicSummonCount = new DbField<long>(RelicSummonCount, 0, this);
            this.NecklaceSummonCount = new DbField<long>(NecklaceSummonCount, 0, this);
            this.NecklaceSummonLevel = new DbField<int>(NecklaceSummonLevel, 0, this);
            this.NecklaceSummonExp = new DbField<BigInteger>(NecklaceSummonExp, 0, this);
        }

        public DbUserLevel()
        {
            Id = 0;
            Level = new DbField<int>(1, Id, this);
            Exp = new DbField<BigInteger>(0, Id, this);
            Stage = new DbField<int>(1, Id, this);
            MaxStage = new DbField<int>(1, Id, this);
            IsStageClear = new DbField<bool>(true, Id, this);
            AwakeningDungeonStage = new DbField<int>(1, Id, this);
            SkillGrowthDungeonStage = new DbField<int>(1, Id, this);
            PetDungeonStage = new DbField<int>(1, Id, this);
            BlackMarketDungeonStage = new DbField<int>(1, Id, this);
            DiaDungeonStage = new DbField<int>(1, Id, this);
            BlackMarketDungeonReward = new DbField<long>(0, Id, this);
            DiaDungeonReward = new DbField<long>(0, Id, this);
            TrainingGroundStage = new DbField<int>(0, Id, this);
            BibleLevel = new DbField<int>(0, Id, this);
            Promotion = new DbField<int>(0, Id, this);
            MaxPower = new DbField<BigInteger>(0, Id, this);
            MaxTraining = new DbField<ObscuredBigInteger>(0, Id, this);
            WeaponSummonCount = new DbField<long>(0, Id, this);
            WeaponSummonLevel = new DbField<int>(1, Id, this);
            WeaponSummonReward = new DbField<int>(1, Id, this);
            WeaponSummonExp = new DbField<BigInteger>(0, Id, this);
            AccessorySummonCount = new DbField<long>(0, Id, this);
            AccessorySummonLevel = new DbField<int>(1, Id, this);
            AccessorySummonReward = new DbField<int>(1, Id, this);
            AccessorySummonExp = new DbField<BigInteger>(0, Id, this);
            SkillSummonCount = new DbField<long>(0, Id, this);
            SkillSummonLevel = new DbField<int>(1, Id, this);
            SkillSummonReward = new DbField<int>(1, Id, this);
            SkillSummonExp = new DbField<BigInteger>(0, Id, this);
            RelicSummonCount = new DbField<long>(0, Id, this);
            NecklaceSummonCount = new DbField<long>(0, Id, this);
            NecklaceSummonLevel = new DbField<int>(1, Id, this);
            NecklaceSummonExp = new DbField<BigInteger>(0, Id, this);
        }
    }
}