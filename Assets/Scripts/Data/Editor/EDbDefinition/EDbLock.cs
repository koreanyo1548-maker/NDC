using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbDefinition
{
    [Serializable]
    public class EDbLock
    {
        public LockType Id;
        public LockConditionType Condition;
        public int Goal;
        public int NameId;
        public bool UsePopup;
    }
}