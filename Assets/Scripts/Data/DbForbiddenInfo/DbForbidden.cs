using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbForbiddenInfo
{
    [Serializable]
    public class DbForbidden : DbModel<DbForbidden, int>
    {
        public string Word;
        public override void Load()
        {
            fileName = "Forbidden";
            if (Application.isPlaying) Init();
        }
    }
}