using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbDefinition
{
    [Serializable]
    public class DbPlay : DbModel<DbPlay, PlayType>
    {
        public float Value;

        public override void Load()
        {
            fileName = "Play";
            if (Application.isPlaying) Init();
        }
    }
}