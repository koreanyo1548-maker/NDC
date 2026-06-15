using System;
using System.Collections.Generic;
using Controller;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.Stores;
using Data.Utils;
using Newtonsoft.Json;
using ThirdParty;

namespace Data.DbUser.Equipment
{
    public class DbUserWeapon: DbUserModel<DbUserWeapon, int>, IDbUserEquipment
    {
        public DbField<int> Count { get; private set; }
        public DbField<int> Awakening { get; private set; }
        public DbField<int> Growth { get; private set; }
        public DbField<bool> Have { get; private set; }

        public DbWeapon Meta { get; private set; }
        

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

        public override void Set(List<DbUserWeapon> obj)
        {
            Init(obj);

            ForEach(w =>
            {
                w.Meta = DbWeapon.Get(w.Id);
                if (!w.Have.Value) w.Count.ValueChanged += w.OnCountUpdated;
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
                Count.ValueChanged -= OnCountUpdated;
            }
        }

        public long GetGrowthStoneCount()
        {
            return DbGrowthMaterial.Get(Growth.Value+1).Get(Meta.FullGrade);
        }
        
        public void GrowthIt()
        {
            Growth.Value ++;
            if (EquipController.I.IsEquipped(this)) TotalStatController.I.Apply(StatType.Attack);
            TotalStatController.I.Apply(StatType.AttackBonus);
            QuestController.I.DoQuests(QuestType.WeaponGrowth);
            if (Growth.Value == 2) QuestController.I.DoQuests(QuestType.CheckWeaponGrowth);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Weapon);
        }
        
        public void MergeIt(int count = 1)
        {
            if (Count.Value >= 5 * count)
            {
                Count.Value -= 5 * count;
                var merged = Get(Id+1);
                var prev = merged.Count.Value;
                merged.Count.Value += count;
                PlayFabManager.Store.SetLog($"[무기합성] {Id}를 {count}개 합성 {merged.Id}가 {prev}개 => {merged.Count.Value}개    |    ");
                QuestController.I.DoQuests(QuestType.WeaponMerge, count);
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Weapon);
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
            TotalStatController.I.Apply(Meta.GetAwakening().GetOption(Awakening.Value));
            QuestController.I.SetQuest(QuestType.CheckWeaponAwakening, WeaponController.I.GetTotalAwakeningCount());
            QuestController.I.DoQuests(QuestType.WeaponAwakening);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Weapon);
        }

        public IDbEquipment GetMeta()
        {
            return DbWeapon.Get(Id);
        }

        public IDbUserEquipment PrevHave()
        {
            var prevId = Meta.PrevId;
            while (prevId != -1)
            {
                var prev = Get(prevId);
                var prevMeta = DbWeapon.Get(prevId);
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
                var nextMeta = DbWeapon.Get(nextId);
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

        protected override List<DbUserWeapon> GetInitials()
        {
            var weapons = new List<DbUserWeapon>();
            DbWeapon.ForEach(w => weapons.Add(new DbUserWeapon(w)));
 
            return weapons;
        }

        public override List<DbUserWeapon> AdjustDataModification(List<DbUserWeapon> obj)
        {
            DbWeapon.ForEach(w =>
            {
                if (!obj.Exists(o => o.Id == w.Id)) obj.Add(new DbUserWeapon(w));
            });
            return obj;
        }

        public DbUserWeapon()
        {
            
        }
        public DbUserWeapon(DbWeapon w)
        {
            Id = w.Id;
            Count = new DbField<int>(0, w.Id, this);
            Awakening = new DbField<int>(0, w.Id, this);
            Growth = new DbField<int>(0, w.Id, this);
            Have = new DbField<bool>(false, w.Id, this);
        }
        
        [JsonConstructor]
        public DbUserWeapon(int Id, int Count, int Awakening, int Growth)
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