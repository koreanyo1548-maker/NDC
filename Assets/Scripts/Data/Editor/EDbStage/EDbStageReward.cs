using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbStage
{
    [Serializable]
    public class EDbStageReward
    {
        public int Id;
        public long Exp;
        public long Gold;
        public int GrowthStone;
        public int GrowthStoneProbability;
        public int BeadsOre;
        public int BeadsOreProbability;
        public int WeaponProbability;
        public List<int> Weapons;
        public int AccessoryProbability;
        public List<int> Accessories;
        public CurrencyType FirstClearReward;
        public int FirstClearRewardCounts;
    }
}