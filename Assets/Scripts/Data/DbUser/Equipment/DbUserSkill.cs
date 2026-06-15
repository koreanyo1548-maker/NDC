using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Controller;
using Controller.Currency;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbPetInfo;
using Data.Utils;
using Managers;
using Newtonsoft.Json;
using ThirdParty;
using UIs.Toast;

namespace Data.DbUser.Equipment
{
    [Serializable]
    public class DbUserSkill: DbUserModel<DbUserSkill, int>, IDbUserEquipment
    {
        public DbField<int> Count { get; private set; }
        public DbField<int> Awakening { get; private set; }
        public DbField<int> Growth { get; private set; }
        public DbField<bool> Have { get; private set; }
        public int Bonus { get; private set; }
        
        public DbSkill Meta { get; private set; }
        

        private bool _isNew;

        public void SetNew()
        {
            _isNew = true;
        }
        public bool IsNew()
        {
            if (_isNew)
            {
                _isNew = false;
                return true;
            }
            return false;
        }

        public override void Set(List<DbUserSkill> obj)
        {
            Init(obj);
            ForEach(s =>
            {
                s.Meta = DbSkill.Get(s.Id);
                s.SetBonus();
            });
        }

        private void SetBonus()
        {
            Bonus = Growth.Value * Meta.GrowthAttack;
            for (var idx = 1; idx <= Awakening.Value; ++idx)
            {
                var awakening = Meta.GetAwakening();
                if (awakening.GetOption(idx) == StatType.SpecificSkillAttackBonus) Bonus += awakening.GetStat(idx);
            }
        }

        public bool IsMaxGrowth(bool checkAwakening)
        {
            if (checkAwakening) return Growth.Value == Meta.GetAwakening().GetLevel(Awakening.Value);
            return Growth.Value == DbGrowthMaterial.Count;
        }

        public long GetGrowthStoneCount()
        {
            return DbGrowthMaterial.Get(Growth.Value+1).Get(Meta.FullGrade);
        }
        
        public void GrowthIt()
        {
            Growth.Value ++;
            SetBonus();
            QuestController.I.DoQuests(QuestType.SkillGrowth);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Skill);
        }
        
        public void MergeIt(int count = 1)
        {
            throw new Exception("Skill cannot be merged");
        }

        public long Decompose()
        {
            var growthStone = 1L * DbSkillDecomposition.Get(Meta.Grade).SkillGrowthStone * Count.Value;
            CurrencyController.I.GetReward(CurrencyType.SkillGrowthStone, growthStone);
            Count.Value = 0;
            return growthStone;
        }
        
        public bool CanAwakening()
        {
            if (!Have.Value) return false;
            if (Awakening.Value == 5) return false;
            if (Count.Value < GetAwakeningEquipCount()) return false;
            return true;
        }

        public bool IsMaxAwakening()
        {
            return Awakening.Value == 5;
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
            SetBonus();
            TotalStatController.I.Apply(Meta.GetAwakening().GetOption(Awakening.Value));
            QuestController.I.SetQuest(QuestType.CheckSkillAwakening, SkillController.I.GetTotalAwakeningCount());
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Skill);
        }

        public IDbEquipment GetMeta()
        {
            return DbSkill.Get(Id);
        }

        public IDbUserEquipment PrevHave()
        {
            var prevId = Meta.PrevId;
            while (prevId != -1)
            {
                var prev = Get(prevId);
                var prevMeta = DbSkill.Get(prevId);
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
                var nextMeta = DbSkill.Get(nextId);
                if (exceptLast && nextMeta.NextId == -1) return null;
                if (next.Have.Value) return Get(nextId);
                nextId = nextMeta.NextId;
            }
            
            return null;
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

        public bool CanMerge(int mergeCount = 1)
        {
            return false;
        }

        protected override List<DbUserSkill> GetInitials()
        {
            var skills = new List<DbUserSkill>();
            DbSkill.ForEach(s => skills.Add(new DbUserSkill(s)));
            
            return skills;
        }

        public override List<DbUserSkill> AdjustDataModification(List<DbUserSkill> obj)
        {
            DbSkill.ForEach(s =>
            {
                if (!obj.Exists(o => o.Id == s.Id)) obj.Add(new DbUserSkill(s));
            });
            return obj;
        }

        public DbUserSkill(DbSkill s)
        {
            Id = s.Id;
            Count = new DbField<int>(0, s.Id, this);
            Awakening = new DbField<int>(0, s.Id, this);
            Growth = new DbField<int>(0, s.Id, this);
            Have = new DbField<bool>(false, s.Id, this);
        }
        public DbUserSkill()
        {
            
        }

        [JsonConstructor]
        public DbUserSkill(int Id, int Count, int Awakening, int Growth)
        {
            this.Id = Id;
            this.Count = new DbField<int>(Count, Id, this);
            this.Awakening = new DbField<int>(Awakening, Id, this);
            this.Growth = new DbField<int>(Growth, Id, this);
            this.Have = new DbField<bool>(Growth > 0, Id, this);
        }
    }
}