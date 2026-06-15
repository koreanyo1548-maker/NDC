using System;

namespace Data.Editor.EDbCharacter
{
    [Serializable]
    public class EDbCharacterLevel
    {
        public int Id;
        public string NeedExp;
        public CurrencyType Reward;
        public int RewardCount;
    }
}