using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Infos;
using Controller.Utils;
using Data;
using Data.DbEquipment;
using Data.DbNecklaceInfo;
using Data.DbSummon;
using Data.DbUser.Equipment;
using Utils;
using Random = UnityEngine.Random;

namespace Controller.Have
{
    public class NecklaceController: Singleton<NecklaceController>
    {
        public ControllerField<bool> CanUpgrade = new(false);
        public ControllerField<bool> CanGrowth = new(false);
        public ControllerField<bool> CanMerge = new(false);

        public void Init()
        {
            CurrencyController.I.GetStoneModel(CurrencyType.AwakeningStone).ValueChanged += (_, _) => CheckCanUpgrade();
        }
        
        public void GrowthAll()
        {
            DbUserNecklace.ForEach(n =>
            {
                if (!n.CanGrowth()) return;
                n.GrowthIt(true);
            });
            CheckCanUpgrade();
        }

        public Dictionary<int, int> MergeAll()
        {
            var generated = new Dictionary<int, int>();
            DbUserNecklace.ForEach(n =>
            {
                if (!n.CanMerge()) return;
                var aGenerated = n.MergeIt(true);
                foreach (var gen in aGenerated)
                {
                    if (generated.ContainsKey(gen.Key)) generated[gen.Key] += gen.Value;
                    else generated.Add(gen.Key, gen.Value);
                }
            });
            CheckCanUpgrade();
            return generated;
        }

        public Dictionary<StatType, int> GetAllOwnStat()
        {
            var stats = new Dictionary<StatType, int> {{StatType.NecklaceHpBonus, 0}};
            
            DbUserNecklace.ForEach(n =>
            {
                if (!n.Have.Value) return;
                stats[StatType.NecklaceHpBonus] += DbNecklaceOwnStat.Get(n.Growth.Value).Stats[n.Meta.OwnIdx];
                var awakening = DbNecklaceAwakening.Get(n.Id);
                for (var idx = 1; idx <= n.Awakening.Value; ++idx)
                {
                    var option = awakening.GetOption(idx);
                    var stat = awakening.GetStat(idx);
                    if (stats.ContainsKey(option)) stats[option] += stat;
                    else stats.Add(option, stat);
                }
            });
            if (stats[StatType.NecklaceHpBonus] == 0) stats.Clear();
            return stats;
        }

        public Dictionary<StatType, int> GetAllEquipStat()
        {
            var stats = new Dictionary<StatType, int>();

            for (int idx = 0; idx < 7; ++idx)
            {
                var id = EquipController.I.GetEquippedNecklace(idx);
                if (id == -1) continue;
                var necklace = DbUserNecklace.Get(id);
                var option = necklace.Meta.EquipStat;
                var stat = DbNecklaceEquipStat.Get(necklace.Growth.Value).Stats[necklace.Meta.EquipIdx];
                if (stats.ContainsKey(option)) stats[necklace.Meta.EquipStat] += stat;
                else stats.Add(option, stat);
            }
            return stats;
        }

        public List<IDbCanSummon> AddRandom(int count)
        {
            var totalCount = count;
            var added = new Dictionary<DbNecklace, int>();
            var result = new List<IDbCanSummon>();
            var prMeta = DbSummonNecklaceProbability.Get(
                LevelController.I.GetSummonLevelForMeta(SummonType.Necklace));
            while (count-- > 0)
            {
                var pick = GradeType.Normal;
                var gradeChoose = Random.Range(0, 1000000);

                var pr = 0;
                while (gradeChoose >= pr)
                {
                    pr += prMeta.GetPr(pick);
                    pick++;
                }
                var add = pick - 1;

                var necklaces = DbNecklace.GradeToNecklaces[add];
                var numberChoose = Random.Range(0, necklaces.Count);
                var necklace = necklaces[numberChoose];

                if (added.ContainsKey(necklace)) added[necklace]++;
                else added.Add(necklace, 1);
                result.Add(necklace);
            }

            foreach (var add in added)
            {
                var n = DbUserNecklace.Get(add.Key.Id);
                n.Count.Value += add.Value;
            }
            QuestController.I.DoQuests(QuestType.NecklaceSummon, totalCount);
            CheckCanUpgrade();
            return result;
        }
        
        public void Add(int necklaceId, int count)
        {
            DbUserNecklace.Get(necklaceId).Count.Value += count;
            CheckCanUpgrade();
        }

        public bool CheckCanUpgrade()
        {
            var canUpgrade = false;
            var canGrowth = false;
            var canMerge = false;
            DbUserNecklace.ForEach(n =>
            {
                if (!canGrowth && n.CanGrowth()) canGrowth = true;
                if (!canUpgrade && (canGrowth || n.CanUpgrade())) canUpgrade = true;
                if (!canMerge && n.CanMerge()) canMerge = true;
            });
            CanUpgrade.Value = canUpgrade;
            CanGrowth.Value = canGrowth;
            CanMerge.Value = canMerge;
            return CanUpgrade.Value;
        }
    }
}