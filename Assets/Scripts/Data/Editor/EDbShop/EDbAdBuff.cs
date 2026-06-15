using System;
using Data.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbAdBuff
    {
        public AdBuffType Id;
        public StatType BuffType;
        public int NameId;
        public int Buff;
        public int Duration;
        public int LimitPerDay;
        public int DescriptionId;
        public string Resource;
        public string MainResource;
        public string RewardResource;
    }
}