using System;
using System.Numerics;
using Data.Utils;
using UnityEngine;

namespace Data.DbCharacter
{
    [Serializable]
    public class DbCharacterLevel: DbModel<DbCharacterLevel, int>
    {
        public BigInteger NeedExp;
        public CurrencyType Reward;
        public int RewardCount;
        
        public override void Load()
        {
            fileName = "CharacterLevel";
            if (Application.isPlaying) Init();
        }
    }
}