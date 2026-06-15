using System.Collections.Generic;
using Data.DbRecord;
using Data.Utils;
using Newtonsoft.Json;

namespace Data.DbUser.Progress
{
    public class DbUserQuest: DbUserModel<DbUserQuest, int>
    {
        public DbField<int> Count { get; private set; }
        public DbField<bool> IsRewarded { get; private set; }
        public QuestType ToDo { get; private set; }
        public DbQuest Meta => DbQuest.Get(Id);
        public bool CanRewarded => !IsRewarded.Value && Count.Value >= Meta.Goal;


        public override void Set(List<DbUserQuest> obj)
        {
            Init(obj);
        }


        protected override List<DbUserQuest> GetInitials()
        {
            var quests = new List<DbUserQuest>();
            DbQuest.ForEach(q => quests.Add(new DbUserQuest(q)));
            
            return quests;
        }

        public override List<DbUserQuest> AdjustDataModification(List<DbUserQuest> obj)
        {
            DbQuest.ForEach(q =>
            {
                if (!obj.Exists(o => o.Id == q.Id)) obj.Add(new DbUserQuest(q));
            });
            return obj;
        }

        public DbUserQuest(DbQuest q)
        {
            Id = q.Id;
            Count = new DbField<int>(0, q.Id, this);
            IsRewarded = new DbField<bool>(false, q.Id, this);
            ToDo = q.ToDo;
        }

        [JsonConstructor]
        public DbUserQuest(int Id, int Count, bool IsRewarded, QuestType ToDo)
        {
            this.Id = Id;
            this.Count = new DbField<int>(Count, Id, this);
            this.IsRewarded = new DbField<bool>(IsRewarded, Id, this);
            this.ToDo = ToDo;
        }

        public DbUserQuest()
        {
            
        }
    }
}