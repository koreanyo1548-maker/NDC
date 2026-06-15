using System;
using System.Collections.Generic;
using Data.DbEquipment;
using Data.Utils;
using UnityEngine;

namespace Data.DbNecklaceInfo
{
    [Serializable]
    public class DbNecklace : DbModel<DbNecklace, int>, IDbCanSummon, IHaveGrade
    {
        public static Dictionary<GradeType, List<DbNecklace>> GradeToNecklaces;
        
        public int NameId;
        public GradeType Grade;
        public StatType EquipStat;
        public string Resource;
        public int PrevId;
        public int NextId;
        public int EquipIdx;
        public int OwnIdx;
        
        public override void Load()
        {
            fileName = "Necklace";
            if (Application.isPlaying) Init();

            GradeToNecklaces = new();
            ForEach(n =>
            {
                if (GradeToNecklaces.TryGetValue(n.Grade, out var necklace)) necklace.Add(n);
                else GradeToNecklaces.Add(n.Grade, new() {n});
            });
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