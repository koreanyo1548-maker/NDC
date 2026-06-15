using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbPassShop
    {
        public int Id;
        public int NameId;
        public CurrencyType PassType;
        public string Resource;
        public List<string> Rewards;
        public int BuyLimit;
        public string ProductId;

        public PassCondition Condition;
        public int ResetDay;

        public int Mileage;
    }
}