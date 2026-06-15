using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Data.Utils;
using Newtonsoft.Json;

namespace Data.DbUser.Progress
{
    [Serializable]
    public class DbUserStat: DbUserModel<DbUserStat, int>
    {
        [DataMember] public DbField<int> AttackLevel { get; private set; }
        [DataMember] public DbField<int> HpLevel { get; private set; }
        [DataMember] public DbField<int> CriticalProbabilityLevel { get; private set; }
        [DataMember] public DbField<int> CriticalAttackBonusLevel { get; private set; }
        [DataMember] public DbField<int> AttackBonusLevel { get; private set; }
        [DataMember] public DbField<int> HpBonusLevel { get; private set; }
        [DataMember] public DbField<int> BossAttackBonusLevel { get; private set; }


        public override void Set(List<DbUserStat> obj)
        {
            Init(obj);
        }


        protected override List<DbUserStat> GetInitials()
        {
            return new List<DbUserStat>
            {
                new()
            };
        }

        public override List<DbUserStat> AdjustDataModification(List<DbUserStat> obj)
        {
            return obj;
        }

        public DbUserStat()
        {
            Id = 0;
            AttackLevel = new DbField<int>(1, 0, this);
            HpLevel = new DbField<int>(1, 0, this);
            CriticalProbabilityLevel = new DbField<int>(1, 0, this);
            CriticalAttackBonusLevel = new DbField<int>(1, 0, this);
            AttackBonusLevel = new DbField<int>(0, 0, this);
            HpBonusLevel = new DbField<int>(0, 0, this);
            BossAttackBonusLevel = new DbField<int>(0, 0, this);
        }
        
        [JsonConstructor]
        public DbUserStat(int Id, int AttackLevel, int HpLevel, int CriticalProbabilityLevel, int CriticalAttackBonusLevel,
            int AttackBonusLevel, int HpBonusLevel, int BossAttackBonusLevel)
        {
            this.Id = Id;
            this.AttackLevel = new DbField<int>(AttackLevel, 0, this);
            this.HpLevel = new DbField<int>(HpLevel, 0, this);
            this.CriticalProbabilityLevel = new DbField<int>(CriticalProbabilityLevel, 0, this);
            this.CriticalAttackBonusLevel = new DbField<int>(CriticalAttackBonusLevel, 0, this);
            this.AttackBonusLevel = new DbField<int>(AttackBonusLevel, 0, this);
            this.HpBonusLevel = new DbField<int>(HpBonusLevel, 0, this);
            this.BossAttackBonusLevel = new DbField<int>(BossAttackBonusLevel, 0, this);
        }
    }
}