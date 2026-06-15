using System;
using System.Collections.Generic;
using Data.DbAbility;
using Data.DbEquipment;
using Data.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.DbRelicInfo
{
    [Serializable]
    public class DbRelic: DbModel<DbRelic, int>, IDbCanSummon
    {
        public GradeType Grade;
        public int NameId;
        public StatType StatType;
        public string Resource;
        
        public override void Load()
        {
            fileName = "Relic";
            if (Application.isPlaying) Init();
        }

        public int GetId()
        {
            return Id;
        }
        public GradeType GetGrade()
        {
            return Grade;
        }

        public string GetResource()
        {
            return Resource;
        }
    }
}