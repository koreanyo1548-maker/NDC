using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Data.DbCommon;
using Data.DbNecklaceInfo;
using Data.Utils;
using Managers;
using Newtonsoft.Json;
using UIs.Toast;

namespace Data.DbUser.Equipment
{
    public class DbUserNecklace: DbUserModel<DbUserNecklace, int>
    {
        public DbField<int> Count { get; private set; }
        public DbField<int> Awakening { get; private set; }
        public DbField<int> Growth { get; private set; }
        public DbField<bool> Have { get; private set; }

        public DbNecklace Meta { get; private set; }


        public override void Set(List<DbUserNecklace> obj)
        {
            Init(obj);

            ForEach(n =>
            {
                n.Meta = DbNecklace.Get(n.Id);
                if (!n.Have.Value) n.Count.ValueChanged += n.OnCountUpdated;
            });
        }
        
        /// <returns> -1: no further upgrades, 0: growth, 1: awakening </returns>
        public int GetNextUpgrade()
        {
            if (Growth.Value == DbNecklaceAwakening.Get(Id).GetLevel(Awakening.Value))
            {
                if (Awakening.Value == 7) return -1;
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// update type에 따른 확인
        /// </summary>
        public bool CanUpgrade()
        {
            var upgrade = GetNextUpgrade();
            if (upgrade == -1) return CanMerge();
            if (upgrade == 0) return CanGrowth();
            if (upgrade == 1) return CanAwakening();
            return false;
        }

        /// <summary>
        /// update type에 따른 확인
        /// </summary>

        public void UpgradeIt()
        {
            var upgrade = GetNextUpgrade();
            if (upgrade == -1) MergeIt(false);
            if (upgrade == 0) GrowthIt(false);
            if (upgrade == 1) AwakeningIt();
            NecklaceController.I.CheckCanUpgrade();
        }
        
        public bool CanGrowth()
        {
            if (GetNextUpgrade() != 0) return false;
            if (Count.Value < GetGrowthNeedCount()) return false;
            return true;
        }

        public void GrowthIt(bool toMax)
        {
            var need = GetGrowthNeedCount();
            var maxCount = DbNecklaceAwakening.Get(Id).GetLevel(Awakening.Value) - Growth.Value;
            var count = toMax ? Math.Min(Count.Value / need, maxCount) : 1;
            Count.Value -= count * need;
            Growth.Value += count;
            if (EquipController.I.IsEquipped(this))
            {
                TotalStatController.I.Apply(Meta.EquipStat);
            }
            TotalStatController.I.Apply(StatType.NecklaceHpBonus);
        }

        public bool CanMerge()
        {
            if (GetNextUpgrade() != -1) return false;
            if (Meta.Grade == GradeType.Mythic) return false;
            if (Count.Value < GetMergeNeedCount()) return false;
            return true;
        }
        
        public Dictionary<int, int> MergeIt(bool toMax)
        {
            var need = DbNecklaceGrowthMaterial.Get(Meta.Grade).MergeCount;
            var count = toMax ? Count.Value / need : 1;
            
            Count.Value -= need * count;
            var generated = new Dictionary<int, int>();
            while (count-- > 0)
            {
                var added = Id + UnityEngine.Random.Range(5, 10);
                NecklaceController.I.Add(added, 1);
                if (generated.ContainsKey(added)) generated[added]++;
                else generated.Add(added, 1);
            }
            
            if (!toMax)
            {
                var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
                var rewardsForToast = new List<DbReward>();
                
                foreach (var r in generated)
                {
                    rewardsForToast.Add(
                        new DbReward(CurrencyType.Necklace, r.Value, r.Key));
                }
                toast.SetReward(210414, rewardsForToast);
            }

            return generated;
        }

        private bool CanAwakening()
        {
            if (!Have.Value) return false;
            if (Awakening.Value == 7) return false;
            if (Growth.Value != DbNecklaceAwakening.Get(Id).GetLevel(Awakening.Value)) return false;
            if (Count.Value < GetAwakeningNeedCount()) return false;
            if (CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone).Value < GetAwakeningStoneCount()) return false;
            return true;
        }
        
        private void AwakeningIt()
        {
            if (CurrencyController.I.TryUse(CurrencyType.AwakeningStone, GetAwakeningStoneCount()))
            {
                Count.Value -= GetAwakeningNeedCount();
                Awakening.Value++;
                TotalStatController.I.Apply(DbNecklaceAwakening.Get(Id).Options[Awakening.Value]);
            }
        }

        public int GetGrowthNeedCount()
        {
            return DbNecklaceGrowthMaterial.Get(Meta.Grade).Counts[Awakening.Value];
        }

        public int GetMergeNeedCount()
        {
            return DbNecklaceGrowthMaterial.Get(Meta.Grade).MergeCount;
        }

        public long GetAwakeningStoneCount()
        {
            return DbNecklaceAwakeningMaterial.Get(Meta.Grade).GetStone(Awakening.Value + 1);
        }
        
        public int GetAwakeningNeedCount()
        {
            return DbNecklaceAwakeningMaterial.Get(Meta.Grade).GetEquipment(Awakening.Value + 1);
        }

        /// <returns> (스탯타입, (현재값, 미래값)) </returns>
        public Dictionary<StatType, Tuple<int, int>> GetAwakeningStat(bool addIncreasing)
        {
            var stats = new Dictionary<StatType, int>();
            var awakening = DbNecklaceAwakening.Get(Id);
            for (var idx = 1; idx <= Awakening.Value; ++idx)
            {
                var option = awakening.GetOption(idx);
                var stat = awakening.GetStat(idx);
                if (stats.ContainsKey(option)) stats[option] += stat;
                else stats.Add(option, stat);
            }

            var increasingStats = new Dictionary<StatType, Tuple<int, int>>();
            var increasingOption = Awakening.Value == 7 || !addIncreasing ? StatType.None : awakening.GetOption(Awakening.Value + 1);
            var increasingStat = Awakening.Value == 7 || !addIncreasing ? 0 : awakening.GetStat(Awakening.Value + 1);
            foreach (var stat in stats)
            {
                increasingStats.Add(stat.Key, new Tuple<int, int>(stat.Value, increasingOption == stat.Key ? stat.Value + increasingStat : 0));
            }
            if (!stats.ContainsKey(increasingOption) && increasingOption != StatType.None) increasingStats.Add(increasingOption, new (0, increasingStat));

            return increasingStats;
        }

        public DbUserNecklace PrevHave()
        {
            var prevId = Meta.PrevId;
            while (prevId != -1)
            {
                var prev = Get(prevId);
                var prevMeta = DbNecklace.Get(prevId);
                if (prev.Have.Value) return Get(prevId);
                prevId = prevMeta.PrevId;
            }
            
            return null;
        }
        
        public DbUserNecklace NextHave(bool exceptLast = false)
        {
            var nextId = Meta.NextId;
            while (nextId != -1)
            {
                var next = Get(nextId);
                var nextMeta = DbNecklace.Get(nextId);
                if (exceptLast && nextMeta.NextId == -1) return null;
                if (next.Have.Value) return Get(nextId);
                nextId = nextMeta.NextId;
            }
            
            return null;
        }
        
        public DbUserNecklace Prev()
        {
            return Get(p => p.Next() == this);
        }
        
        public DbUserNecklace Next()
        {
            return Get(p => p.Id > Id);
        }
        
        private void OnCountUpdated(object sender, DbEventArgs e)
        {
            if (!Have.Value)
            {
                Have.Value = true;
                Growth.Value++;
                TotalStatController.I.Apply(StatType.NecklaceHpBonus);
                Count.ValueChanged -= OnCountUpdated;
            }
        }

        protected override List<DbUserNecklace> GetInitials()
        {
            var necklaces = new List<DbUserNecklace>();
            DbNecklace.ForEach(n => necklaces.Add(new DbUserNecklace(n)));
 
            return necklaces;
        }

        public override List<DbUserNecklace> AdjustDataModification(List<DbUserNecklace> obj)
        {
            DbNecklace.ForEach(n =>
            {
                if (!obj.Exists(o => o.Id == n.Id)) obj.Add(new DbUserNecklace(n));
            });
            return obj;
        }

        public DbUserNecklace()
        {
            
        }
        public DbUserNecklace(DbNecklace n)
        {
            Id = n.Id;
            Count = new DbField<int>(0, n.Id, this);
            Awakening = new DbField<int>(0, n.Id, this);
            Growth = new DbField<int>(0, n.Id, this);
            Have = new DbField<bool>(false, n.Id, this);
        }
        
        [JsonConstructor]
        public DbUserNecklace(int Id, int Count, int Awakening, int Growth)
        {
            this.Id = Id;
            this.Count = new DbField<int>(Count, Id, this);
            this.Awakening = new DbField<int>(Awakening, Id, this);
            this.Growth = new DbField<int>(Growth, Id, this);
            this.Have = new DbField<bool>(Growth > 0, Id, this);
        }
    }
}