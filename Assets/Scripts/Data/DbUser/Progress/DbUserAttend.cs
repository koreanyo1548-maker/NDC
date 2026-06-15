using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Data.Utils;
using Newtonsoft.Json;
using Utils;

namespace Data.DbUser.Progress
{
    [Serializable]
    public class DbUserAttend: DbUserModel<DbUserAttend, int>
    {
        [DataMember] public DbField<int> NextDay { get; private set; }
        [DataMember] public DateTime LastRewarded;

        public override void Set(List<DbUserAttend> obj)
        {
            Init(obj);
        }

        protected override List<DbUserAttend> GetInitials()
        {
            return new List<DbUserAttend>
            {
                new()
            };
        }

        public override List<DbUserAttend> AdjustDataModification(List<DbUserAttend> obj)
        {
            return obj;
        }


        [JsonConstructor]
        public DbUserAttend(int Id, int NextDay, DateTime LastRewarded)
        {
            this.Id = Id;
            this.LastRewarded = LastRewarded;
            this.NextDay = new DbField<int>(NextDay, 0, this);
        }

        public DbUserAttend()
        {
            Id = 0;
            LastRewarded = new DateTime();
            NextDay = new DbField<int>(1, 0, this);
        }
    }
}