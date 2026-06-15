using System;
using Data.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.DbShop
{
    [Serializable]
    public class DbAdBuff : DbModel<DbAdBuff, AdBuffType>
    {
        public StatType BuffType;
        public int NameId;
        public int Buff;
        public int Duration;
        public int LimitPerDay;
        public int DescriptionId;
        public string Resource;
        public string MainResource;
        public string RewardResource;

        public override void Load()
        {
            fileName = "AdBuff";
            if (Application.isPlaying) Init();
        }
    }
}