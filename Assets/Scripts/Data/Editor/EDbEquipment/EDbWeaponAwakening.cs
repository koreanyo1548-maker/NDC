using System;
using System.Collections.Generic;
using Data.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Data.Editor.EDbEquipment
{
    [Serializable]
    public class EDbWeaponAwakening
    {
        public int Id;
        public List<int> Levels;
        public List<StatType> Options;
        public List<int> Stats;
    }
}