using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbGrade : DbModel<DbGrade, GradeType>
    {
        public int NameId;

        public override void Load()
        {
            fileName = "Grade";
            if (Application.isPlaying) Init();
        }
    }
}