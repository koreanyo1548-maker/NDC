using System;
using System.Collections.Generic;
using System.Numerics;
using Data.Utils;
using Managers.Base;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Data.DbDungeon
{
    [Serializable]
    public class DbTrainingGroundReward : DbModel<DbTrainingGroundReward, int>
    {
        public CurrencyType RewardType;
        public int RewardCount;
        public int RewardId;

        public override void Load()
        {
            fileName = "TrainingGroundReward";
            if (Application.isPlaying) Init();
        }
    }
}