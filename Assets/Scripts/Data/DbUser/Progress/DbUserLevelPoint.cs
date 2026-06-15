using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Data.Utils;
using Newtonsoft.Json;

namespace Data.DbUser.Progress
{
    [Serializable]
    public class DbUserLevelPoint: DbUserModel<DbUserLevelPoint, int>
    {
        [DataMember] public DbField<int> AttackLevel { get; private set; }
        [DataMember] public DbField<int> HpLevel { get; private set; }
        [DataMember] public DbField<int> CriticalProbabilityLevel { get; private set; }
        [DataMember] public DbField<int> CriticalAttackBonusLevel { get; private set; }
        [DataMember]  public DbField<int> SkillDamageRateLevel { get; private set; }
        [DataMember]  public DbField<int> ExpBonusRateLevel { get; private set; }
        [DataMember]  public DbField<int> GoldBonusRateLevel { get; private set; }
        [DataMember]  public DbField<int> DashAttackBonusLevel { get; private set; }
        [DataMember]  public DbField<int> DebuffMonsterAttackLevel { get; private set; }
        [DataMember]  public DbField<int> DebuffMonsterHpLevel { get; private set; }


        public override void Set(List<DbUserLevelPoint> obj)
        {
            Init(obj);
        }


        protected override List<DbUserLevelPoint> GetInitials()
        {
            return new List<DbUserLevelPoint>
            {
                new ()
            };
        }

        public override List<DbUserLevelPoint> AdjustDataModification(List<DbUserLevelPoint> obj)
        {
            return obj;
        }

        [JsonConstructor]
        public DbUserLevelPoint(int Id, int AttackLevel, int HpLevel, int CriticalProbabilityLevel, int CriticalAttackBonusLevel,
            int SkillDamageRateLevel, int ExpBonusRateLevel, int GoldBonusRateLevel, int DashAttackBonusLevel,
            int DebuffMonsterAttackLevel, int DebuffMonsterHpLevel)
        {
            this.Id = Id;
            this.AttackLevel = new DbField<int>(AttackLevel, 0, this);
            this.HpLevel = new DbField<int>(HpLevel, 0, this);
            this.CriticalProbabilityLevel = new DbField<int>(CriticalProbabilityLevel, 0, this);
            this.CriticalAttackBonusLevel = new DbField<int>(CriticalAttackBonusLevel, 0, this);
            this.SkillDamageRateLevel = new DbField<int>(SkillDamageRateLevel, 0, this);
            this.ExpBonusRateLevel = new DbField<int>(ExpBonusRateLevel, 0, this);
            this.GoldBonusRateLevel = new DbField<int>(GoldBonusRateLevel, 0, this);
            this.DashAttackBonusLevel = new DbField<int>(DashAttackBonusLevel, 0, this);
            this.DebuffMonsterAttackLevel = new DbField<int>(DebuffMonsterAttackLevel, 0, this);
            this.DebuffMonsterHpLevel = new DbField<int>(DebuffMonsterHpLevel, 0, this);
        }

        public DbUserLevelPoint()
        {
            Id = 0;
            AttackLevel = new DbField<int>(0, Id, this);
            HpLevel = new DbField<int>(0, Id, this);
            CriticalProbabilityLevel = new DbField<int>(0, Id, this);
            CriticalAttackBonusLevel = new DbField<int>(0, Id, this);
            SkillDamageRateLevel = new DbField<int>(0, Id, this);
            ExpBonusRateLevel = new DbField<int>(0, Id, this);
            GoldBonusRateLevel = new DbField<int>(0, Id, this);
            DashAttackBonusLevel = new DbField<int>(0, Id, this);
            DebuffMonsterAttackLevel = new DbField<int>(0, Id, this);
            DebuffMonsterHpLevel = new DbField<int>(0, Id, this);
        }
    }
}