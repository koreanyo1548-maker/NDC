using System.Collections.Generic;
using Data.DbRecord;
using Data.Utils;
using Newtonsoft.Json;

namespace Data.DbUser.Progress
{
    public class DbUserNewbieQuest: DbUserModel<DbUserNewbieQuest, int>
    {
        public DbField<int> Count { get; private set; }
        public DbField<bool> IsRewarded { get; private set; }

        private DbNewbieQuest _meta;
        public DbNewbieQuest Meta => _meta;

        public bool CanRewarded => !IsRewarded.Value && Count.Value >= Meta.Goal;


        public override void Set(List<DbUserNewbieQuest> obj)
        {
            Init(obj);
            ForEach(nq => nq._meta = DbNewbieQuest.Get(nq.Id));
        }

        protected override List<DbUserNewbieQuest> GetInitials()
        {
            var quests = new List<DbUserNewbieQuest>();
            DbNewbieQuest.ForEach(nq => quests.Add(new DbUserNewbieQuest(nq)));
 
            return quests;
        }

        public override List<DbUserNewbieQuest> AdjustDataModification(List<DbUserNewbieQuest> obj)
        {
            DbNewbieQuest.ForEach(nq =>
            {
                if (!obj.Exists(o => o.Id == nq.Id)) obj.Add(new DbUserNewbieQuest(nq));
            });
            return obj;
        }

        public DbUserNewbieQuest()
        {

        }

        [JsonConstructor]
        public DbUserNewbieQuest(int Id, int Count, bool IsRewarded)
        {
            this.Id = Id;
            this.Count = new DbField<int>(Count, Id, this);
            this.IsRewarded = new DbField<bool>(IsRewarded, Id, this);
        }

        public DbUserNewbieQuest(DbNewbieQuest meta)
        {
            this.Id = meta.Id;
            this.Count = new DbField<int>(0, Id, this);
            this.IsRewarded = new DbField<bool>(false, Id, this);
        }
    }
}