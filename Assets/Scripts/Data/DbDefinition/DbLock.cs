using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbLock: DbModel<DbLock, LockType>
    {
        public LockConditionType Condition;
        public int Goal;
        public int NameId;
        public bool UsePopup;

        public override void Load()
        {
            fileName = "Lock";
            if (Application.isPlaying) Init();
        }
    }
}