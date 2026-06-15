using System;
using Data.Editor.EDbEquipment;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbDefinition
{
    [Serializable]
    public class EDbCurrency
    {
        public CurrencyType Id;
        public int NameId;
        public int InitialValue;
        public string Resource;
        public int DailyCharge;
        public int MaxHave;
        public CurrencyCategoryType Category;
    }
}