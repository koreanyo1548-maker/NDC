using System;
using System.Collections.Generic;
using Controller;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.Utils;
using ThirdParty;

namespace Data.DbUser.Equipment
{
    public class DbUserAccessory: DbUserModel<DbUserAccessory, int>, IDbUserEquipment
    {
        public DbField<int> Count { get; private set; }
        public DbField<int> Awakening { get; private set; }
        public DbField<int> Growth { get; private set; }
        public DbField<bool> Have { get; private set; }
        
        public DbAccessory Meta { get; private set; }
    
        private bool _isNew;

        public bool IsNew()
        {
            if (_isNew)
            {
                _isNew = false;
                return true;
            }
            return false;
        }

        public override void Set(List<DbUserAccessory> obj)
        {
            Init(obj);
            ForEach(a =>
            {
                a.Meta = DbAccessory.Get(a.Id);
                if (!a.Have.Value) a.Count.ValueChanged += a.OnCountUpdated;
            });
        }


        public bool IsMaxGrowth(bool checkAwakening)
        {
            if (checkAwakening) return Growth.Value == Meta.GetAwakening().GetLevel(Awakening.Value);
            return Growth.Value == DbGrowthMaterial.Count;
        }

        
        private void OnCountUpdated(object sender, DbEventArgs e)
        {
            if (!Have.Value)
            {
                _isNew = true;
                Have.Value = true;
                GrowthIt();
                TotalStatController.I.Apply(StatType.HpBonus);
                Count.ValueChanged -= OnCountUpdated;
            }
        }
        
        public long GetGrowthStoneCount()
        {
            return DbGrowthMaterial.Get(Growth.Value + 1).Get(Meta.FullGrade);
        }

        
        public void GrowthIt()
        {
            Growth.Value ++;
            if (EquipController.I.IsEquipped(this)) TotalStatController.I.Apply(StatType.Hp);
            TotalStatController.I.Apply(StatType.HpBonus);
            QuestController.I.DoQuests(QuestType.AccessoryGrowth);
            if (Growth.Value == 2) QuestController.I.DoQuests(QuestType.CheckAccessoryGrowth);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Accessory);
        }

        public void MergeIt(int count = 1)
        {
            if (Count.Value >= 5 * count)
            {
                Count.Value -= 5 * count;
                var merged = Get(Id + 1);
                var prev = merged.Count.Value;
                merged.Count.Value += count;
                PlayFabManager.Store.SetLog($"[장신구합성] {Id}를 {count}개 합성 {merged.Id}가 {prev}개 => {merged.Count.Value}개    |    ");
                QuestController.I.DoQuests(QuestType.AccessoryMerge, count);
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Accessory);
            }
        }

        public IDbEquipment GetMeta()
        {
            return DbAccessory.Get(Id);
        }
        
        public IDbUserEquipment PrevHave()
        {
            var prevId = Meta.PrevId;
            while (prevId != -1)
            {
                var prev = Get(prevId);
                var prevMeta = DbAccessory.Get(prevId);
                if (prev.Have.Value) return Get(prevId);
                prevId = prevMeta.PrevId;
            }
            
            return null;
        }
        
        public IDbUserEquipment NextHave(bool exceptLast = false)
        {
            var nextId = Meta.NextId;
            while (nextId != -1)
            {
                var next = Get(nextId);
                var nextMeta = DbAccessory.Get(nextId);
                if (exceptLast && nextMeta.NextId == -1) return null;
                if (next.Have.Value) return Get(nextId);
                nextId = nextMeta.NextId;
            }
            
            return null;
        }
        
        public bool CanMerge(int mergeCount = 1)
        {
            if (Meta.FullGrade == FullGradeType.Mythic5) return false;
            if (Count.Value < 5 * mergeCount) return false;
            return true;
        }
        
        public bool CanAwakening()
        {
            if (!Have.Value) return false;
            if (Awakening.Value == 7) return false;
            if (Count.Value < GetAwakeningEquipCount()) return false;
            return true;
        }

        public bool IsMaxAwakening()
        {
            return Awakening.Value == 7;
        }

        public void AwakeningIt()
        {
            Count.Value -= GetAwakeningEquipCount();
            Awakening.Value++;
            TotalStatController.I.Apply(Meta.GetAwakening().GetOption(Awakening.Value));
            QuestController.I.SetQuest(QuestType.CheckAccessoryAwakening, AccessoryController.I.GetTotalAwakeningCount());
            QuestController.I.DoQuests(QuestType.AccessoryAwakening);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Accessory);
        }
        public long GetAwakeningStoneCount()
        {
            return Meta.GetAwakeningMaterial().GetStone(Awakening.Value + 1);
        }

        public int GetAwakeningEquipCount()
        {
            return Meta.GetAwakeningMaterial().GetEquipment(Awakening.Value + 1);
        }

        public DbField<int> GetCountModel()
        {
            return Count;
        }

        protected override List<DbUserAccessory> GetInitials()
        {
            var accessories = new List<DbUserAccessory>();
            DbAccessory.ForEach(a => accessories.Add(new DbUserAccessory(a)));
            return accessories;
        }

        public override List<DbUserAccessory> AdjustDataModification(List<DbUserAccessory> obj)
        {
            DbAccessory.ForEach(a =>
            {
                if (!obj.Exists(o => o.Id == a.Id)) obj.Add(new DbUserAccessory(a));
            });
            return obj;
        }

        public DbUserAccessory(int Id, int Count, int Awakening, int Growth)
        {
            this.Id = Id;
            this.Count = new DbField<int>(Count, Id, this);
            this.Awakening = new DbField<int>(Awakening, Id, this);
            this.Growth = new DbField<int>(Growth, Id, this);
            this.Have = new DbField<bool>(Growth > 0, Id, this);
        }

        public DbUserAccessory(DbAccessory a)
        {
            Id = a.Id;
            Count = new DbField<int>(0, a.Id, this);
            Awakening = new DbField<int>(0, a.Id, this);
            Growth = new DbField<int>(0, a.Id, this);
            Have = new DbField<bool>(false, a.Id, this);
        }

        public DbUserAccessory()
        {
            
        }


        public int GetId()
        {
            return Id;
        }

        public int GetCount()
        {
            return Count.Value;
        }

        public int GetAwakening()
        {
            return Awakening.Value;
        }

        public int GetGrowth()
        {
            return Growth.Value;
        }

        public bool GetHave()
        {
            return Have.Value;
        }

        public DbUserModel GetModel()
        {
            return this;
        }
    }
}