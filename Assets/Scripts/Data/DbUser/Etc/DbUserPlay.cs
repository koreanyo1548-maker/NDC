using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using Data.Utils;

namespace Data.DbUser.Etc
{
    [Serializable]
    public class DbUserPlay: DbUserModel<DbUserPlay, int>
    {
        public DbField<int> KillCount { get; private set; }
        public DbField<bool> CheckKillCount { get; private set; }
        public DbField<int> TimeLimit { get; private set; }
        public DbField<BigInteger> RedBarProgress { get; private set; }
        public BigInteger RedBarMaxProgress;
        public DbField<int> MonsterCount { get; private set; }


        public override void Set(List<DbUserPlay> obj)
        {
            Init(obj);
        }

        protected override List<DbUserPlay> GetInitials()
        {
            return new List<DbUserPlay>
            {
                new()
            };
        }

        public override List<DbUserPlay> AdjustDataModification(List<DbUserPlay> obj)
        {
            throw new Exception("Should not be called");
        }

        public DbUserPlay()
        {
            KillCount = new DbField<int>(0, 0, this);
            CheckKillCount = new DbField<bool>(true, 0, this);
            TimeLimit = new DbField<int>(0, 0, this);
            RedBarProgress = new DbField<BigInteger>(0, 0, this);
            MonsterCount = new DbField<int>(0, 0, this);
        }
    }
}