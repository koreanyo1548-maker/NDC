using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbSummon
{
    [Serializable]
    public class DbSummonProduct : DbModel<DbSummonProduct, SummonFullType>
    {
        public int Counts;
        public int Cost;

        public override void Load()
        {
            fileName = "SummonProduct";
            if (Application.isPlaying) Init();
        }

        public static DbSummonProduct Get(SummonType type, int summonNumber)
        {
            if (type == SummonType.Accessory) return Get(SummonFullType.AccessorySummon1 + summonNumber);
            if (type == SummonType.Weapon) return Get(SummonFullType.WeaponSummon1 + summonNumber);
            if (type == SummonType.Skill) return Get(SummonFullType.SkillSummon1 + summonNumber);
            if (type == SummonType.Relic) return Get(SummonFullType.RelicSummon1 + summonNumber);
            if (type == SummonType.Necklace) return Get(SummonFullType.NecklaceSummon1 + summonNumber);
            throw new Exception($"summon {type} is not defined");
        }

        public int GetNeed(bool isMoney)
        {
            if (isMoney) return Cost;
            return Counts;
        }

        public static int GetMaxCount()
        {
            var max = 0;
            ForEach(p =>
            {
                if (p.Counts > max) max = p.Counts;
            });

            return max;
        }

        public static int GetMinCost()
        {
            var min = int.MaxValue;
            ForEach(p =>
            {
                if (p.Cost < min) min = p.Cost;
            });

            return min;
        }
    }
}