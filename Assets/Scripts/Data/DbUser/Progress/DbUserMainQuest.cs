using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Data.DbRecord;
using Data.Utils;
using Newtonsoft.Json;

namespace Data.DbUser.Progress
{
    [Serializable]
    public class DbUserMainQuest: DbUserModel<DbUserMainQuest, int>
    {
        [DataMember] public DbField<int> QuestId { get; private set; }
        [DataMember] public DbField<bool> IsOnGuide { get; private set; }
        [DataMember] public DbField<int> DoCount { get; private set; }
        [DataMember] public DbField<int> LoopCount { get; private set; }

        public DbGuideQuest GuideMeta => DbGuideQuest.Get(QuestId.Value);
        public DbMainQuest MainMeta => DbMainQuest.Get(QuestId.Value);
        public int Goal => IsOnGuide.Value ? GuideMeta.Goal : MainMeta.Goal + LoopCount.Value * MainMeta.IncreasingGoal;
        public QuestType ToDo => IsOnGuide.Value ? GuideMeta.ToDo : MainMeta.ToDo;
        public bool CanRewarded => DoCount.Value >= Goal;
        public int GuideEffect => IsOnGuide.Value ? GuideMeta.EffectId : -1;

        public override void Set(List<DbUserMainQuest> obj)
        {
            Init(obj);
        }

        protected override List<DbUserMainQuest> GetInitials()
        {
            return new List<DbUserMainQuest>
            {
                new()
            };
        }

        public override List<DbUserMainQuest> AdjustDataModification(List<DbUserMainQuest> obj)
        {
            return obj;
        }

        [JsonConstructor]
        public DbUserMainQuest(int Id, int QuestId, bool IsOnGuide, int DoCount, int LoopCount)
        {
            this.Id = Id;
            this.QuestId = new DbField<int>(QuestId, 0, this);
            this.IsOnGuide = new DbField<bool>(IsOnGuide, 0, this);
            this.DoCount = new DbField<int>(DoCount, 0, this);
            this.LoopCount = new DbField<int>(LoopCount, 0, this);
        }

        public DbUserMainQuest()
        {
            QuestId = new DbField<int>(10001, 0, this);
            IsOnGuide = new DbField<bool>(true, 0, this);
            DoCount = new DbField<int>(0, 0, this);
            LoopCount = new DbField<int>(0, 0, this);
        }

    }
}