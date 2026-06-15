using System;
using Data.Utils;
using UnityEngine;
using Utils;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbDungeonMeta : DbModel<DbDungeonMeta, FieldType>
    {
        public int NameId;
        public CurrencyType Use;
        public CurrencyType AdUse;
        public string Resource;

        public override void Load()
        {
            fileName = "Dungeon";
            if (Application.isPlaying) Init();
        }
    }
}