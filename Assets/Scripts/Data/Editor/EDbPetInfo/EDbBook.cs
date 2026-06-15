using System;
using System.Collections.Generic;

namespace Data.Editor.EDbPetInfo
{
    [Serializable]
    public class EDbBook
    {
        public CurrencyType Id;
        public int NameId;
        public int Time;
        public List<string> Pet;
        public int MinPetCount;
        public int MaxPetCount;
        public int MinStoneCount;
        public int MaxStoneCount;


    }
}