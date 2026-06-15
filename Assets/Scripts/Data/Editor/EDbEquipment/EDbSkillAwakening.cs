using System;
using System.Collections.Generic;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbEquipment
{
    [Serializable]
    public class EDbSkillAwakening
    {
        public int Id;
        public List<int> Levels;
        public List<StatType> Options;
        public List<int> Stats;
    }
}