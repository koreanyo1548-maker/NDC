using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbStage
{
    [Serializable]
    public class EDbStageBase
    {
        public StageType Id;
        public int TimeLimit;
        public bool IsCenterSpawn;
    }
}