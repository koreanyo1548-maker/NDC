using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbEquipment
{
    [Serializable]
    public class EDbAwakeningMaterial
    {
        public int Id;
        public List<int> EquipmentCounts;
        public List<int> Stones;
    }
}