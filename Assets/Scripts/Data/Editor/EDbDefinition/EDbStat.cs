using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbDefinition
{
    [Serializable]
    public class EDbStat
    {
        public StatType Id;
        public int NameId;
        public int StaticNameId;
        public float ShowMultiply;
        public bool IsPercent;
    }
}