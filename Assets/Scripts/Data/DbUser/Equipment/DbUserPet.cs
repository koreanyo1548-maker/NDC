using System.Collections.Generic;
using Controller;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Data.DbEquipment;
using Data.DbPetInfo;
using Data.Utils;
using Newtonsoft.Json;
using ThirdParty;

namespace Data.DbUser.Equipment
{
    public class DbUserPet: DbUserModel<DbUserPet, int>, IDbUserEquipment
    {
        public DbField<int> Count { get; private set; }
        public DbField<int> Awakening { get; private set; }
        public DbField<int> Growth { get; private set; }
        public DbField<bool> Have { get; private set; }
        
        public DbPet Meta { get; private set; }
        

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

        public override void Set(List<DbUserPet> obj)
        {
            Init(obj);

            ForEach(p =>
            {
                p.Meta = DbPet.Get(p.Id);
                if (!p.Have.Value) p.Count.ValueChanged += p.OnCountUpdated;
            });
        }
        
        public bool IsMaxGrowth(bool checkAwakening)
        {
            if (checkAwakening) return Growth.Value == Meta.GetAwakening().GetLevel(Awakening.Value);
            return Growth.Value == DbPetGrowthMaterial.Count;
        }
        
        private void OnCountUpdated(object sender, DbEventArgs e)
        {
            if (!Have.Value)
            {
                _isNew = true;
                Have.Value = true;
                GrowthIt();
                Count.ValueChanged -= OnCountUpdated;
            }
        }

        public long GetGrowthStoneCount()
        {
            return DbPetGrowthMaterial.Get(Growth.Value+1).Get(Meta.Grade);
        }
        
        public void GrowthIt()
        {
            Growth.Value++;
            if (EquipController.I.IsEquipped(this))
            {
                TotalStatController.I.Apply(StatType.PetAttackBonus);
                TotalStatController.I.Apply(StatType.PetHpBonus);
            }
            QuestController.I.DoQuests(QuestType.PetGrowth);
            PlayFabManager.Store.ForceSave();
        }
        
        public void MergeIt(int count = 1)
        {
            if (Count.Value >= 5 * count)
            {
                Count.Value -= 5 * count;
                var merged = Get(Id+1);
                merged.Count.Value += count;
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Pet);
            }
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

        public void AwakeningIt()
        {
            Count.Value -= GetAwakeningEquipCount();
            Awakening.Value++;
            if (EquipController.I.IsEquipped(this))
            {
                TotalStatController.I.Apply(DbPetAwakening.Get(Id).Option);
            }
            QuestController.I.SetQuest(QuestType.CheckPetAwakening, PetController.I.GetTotalAwakeningCount());
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Pet);
        }

        public IDbEquipment GetMeta()
        {
            return DbPet.Get(Id);
        }

        public IDbUserEquipment PrevHave()
        {
            var prevId = Meta.PrevId;
            while (prevId != -1)
            {
                var prev = Get(prevId);
                var prevMeta = DbPet.Get(prevId);
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
                var nextMeta = DbPet.Get(nextId);
                if (exceptLast && nextMeta.NextId == -1) return null;
                if (next.Have.Value) return Get(nextId);
                nextId = nextMeta.NextId;
            }
            
            return null;
        }
        
        public DbUserPet Prev()
        {
            return Get(p => p.Next() == this);
        }
        
        public DbUserPet Next()
        {
            return Get(p => p.Id > Id);
        }

        public bool CanMerge(int mergeCount = 1)
        {
            return false;
        }

        protected override List<DbUserPet> GetInitials()
        {
            var pets = new List<DbUserPet>();
            DbPet.ForEach(p => pets.Add(new DbUserPet(p)));
 
            return pets;
        }

        public override List<DbUserPet> AdjustDataModification(List<DbUserPet> obj)
        {
            DbPet.ForEach(p =>
            {
                if (!obj.Exists(o => o.Id == p.Id)) obj.Add(new DbUserPet(p));
            });
            return obj;
        }

        public DbUserPet()
        {
            
        }
        public DbUserPet(DbPet p)
        {
            Id = p.Id;
            Count = new DbField<int>(0, p.Id, this);
            Awakening = new DbField<int>(0, p.Id, this);
            Growth = new DbField<int>(0, p.Id, this);
            Have = new DbField<bool>(false, p.Id, this);
        }
        
        [JsonConstructor]
        public DbUserPet(int Id, int Count, int Awakening, int Growth)
        {
            this.Id = Id;
            this.Count = new DbField<int>(Count, Id, this);
            this.Awakening = new DbField<int>(Awakening, Id, this);
            this.Growth = new DbField<int>(Growth, Id, this);
            Have = new DbField<bool>(Growth > 0, Id, this);
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