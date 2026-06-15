using System;
using System.Numerics;
using Data.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.DbPetInfo
{
    [Serializable]
    public class DbBibleLevel : DbModel<DbBibleLevel, int>
    {
        public long Attack;
        public long Hp;
        [JsonConverter(typeof(StringToBigIntegerConverter))]
        public BigInteger SpendCount;
        
        public override void Load()
        {
            fileName = "BibleLevel";
            if (Application.isPlaying) Init();
        }

    }
}