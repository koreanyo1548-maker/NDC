using System;
using System.Collections.Generic;
using Controller;
using Controller.Play;
using Data.DbRecord;
using Data.Utils;
using UnityEngine;

namespace Data.DbUser.Progress
{
    public class DbUserTitle: DbUserModel<DbUserTitle, int>
    {
        public DbField<int> Level { get; private set; }
        public DbField<int> DoCount { get; private set; }
        public DbTitle Meta => DbTitle.Get(Id);
        public bool CanHave => DoCount.Value >= Meta.Goal[0] && Level.Value == 0;

        public override void Set(List<DbUserTitle> obj)
        {
            Init(obj);
        }

        protected override List<DbUserTitle> GetInitials()
        {
            var titles = new List<DbUserTitle>();
            DbTitle.ForEach(t =>
            {
                titles.Add(new DbUserTitle(t));
            });
            return titles;
        }

        public override List<DbUserTitle> AdjustDataModification(List<DbUserTitle> obj)
        {
            DbTitle.ForEach(t =>
            {
                if (!obj.Exists(o => o.Id == t.Id)) obj.Add(new DbUserTitle(t));
                var title = obj.Find(o => o.Id == t.Id);
                title.Level.Value = Math.Min(title.Level.Value, t.MaxLevel);
            });
            return obj;
        }

        public DbUserTitle(DbTitle t)
        {
            Id = t.Id;
            Level = new DbField<int>(0, t.Id, this);
            DoCount = new DbField<int>(0, t.Id, this);
        }

        public int GetOrder()
        {
            var idx = 0;
            ForEach(title =>
            {
                if (Level.Value > 0)
                {
                    if (title.Id < Id && title.Level.Value > 0) idx++;
                }
                else
                {
                    if (title.Id < Id && title.CanHave) idx++; 
                    else if (title.Level.Value > 0) idx++;
                }
                
                // if (Level.Value > 0 && title.Level.Value > 0 && Id > title.Id) idx++; 
                // else if (Level.Value == 0 && title.Level.Value > 0) idx++;
                // else if (Level.Value == 0 && !CanHave && title.Level.Value == 0 && title.CanHave) idx++;
                // else if (Level.Value == 0 && title.Level.Value == 0 && !title.CanHave && Id > title.Id) idx++;
            });
            return idx;
        }

        public bool CanLevelUp()
        {
            if (Level.Value == 0) return false;
            if (Level.Value >= Meta.MaxLevel) return false;

            return DoCount.Value >= Meta.Goal[Level.Value];
        }

        public void GetTitle()
        {
            LevelUp();
        }

        public void LevelUp()
        {
            Level.Value += 1;
            TotalStatController.I.Apply(Meta.Option);
        }
        
        public DbUserTitle(int Id, int Level, int DoCount)
        {
            this.Id = Id;
            this.Level = new DbField<int>(Level, Id, this);
            this.DoCount = new DbField<int>(DoCount, Id, this);
        }

        public DbUserTitle()
        {
            
        }

    }
}