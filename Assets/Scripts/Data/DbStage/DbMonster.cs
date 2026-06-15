using System;
using Data.Utils;
using Managers.Base;
using UnityEngine;

namespace Data.DbStage
{
    [Serializable]
    public class DbMonster : DbModel<DbMonster, int>
    {
        public float AttackRange;
        public string Resource;
        public int NameId;
        public SFXType Sound;

        public override void Load()
        {
            fileName = "Monster";
            if (Application.isPlaying) Init();
        }
    }
}