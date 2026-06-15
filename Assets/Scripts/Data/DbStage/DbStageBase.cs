using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbStage
{
    [Serializable]
    public class DbStageBase : DbModel<DbStageBase, StageType>
    {
        public int TimeLimit;
        public bool IsCenterSpawn;
       
        public override void Load()
        {
            fileName = "StageBase";
            if (Application.isPlaying) Init();
        }
    }
}