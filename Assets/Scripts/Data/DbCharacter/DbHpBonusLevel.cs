using System;
using System.Numerics;
using Data.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.DbCharacter
{
    [Serializable]
    public class DbHpBonusLevel: DbModel<DbHpBonusLevel, int>, IDbCharacterStat
    {
        public long Value;
        [JsonConverter(typeof(StringToBigIntegerConverter))]
        public BigInteger SpendCount;
        
        public override void Load()
        {
            fileName = "HpBonusLevel";
            if (Application.isPlaying) Init();
        }

        public long GetValue()
        {
            return Value;
        }

        public BigInteger GetSpendCount()
        {
            return SpendCount;
        }
        
        public int GetMaxLevel()
        {
            return Count - 1;
        }
    }
}