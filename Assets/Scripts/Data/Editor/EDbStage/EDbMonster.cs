using System;
using Data.Utils;
using Managers.Base;
using UnityEngine;

namespace Data.Editor.EDbStage
{
    [Serializable]
    public class EDbMonster
    {
        public int Id;
        public float AttackMultiplier;
        public float AttackRange;
        public string Resource;
        public int NameId;
        public SFXType Sound;
    }
}