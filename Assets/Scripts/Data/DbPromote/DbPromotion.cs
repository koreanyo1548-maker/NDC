using System;
using Data.Utils;
using UnityEngine;

namespace Data.DbPromote
{
    [Serializable]
    public class DbPromotion : DbModel<DbPromotion, int>
    {
        public int NameId;
        public int StartScenarioId;
        public int SuccessScenarioId;
        public int FailScenarioId;
        public int Attack;
        public int Hp;
        public int CostumeId;
        public string Resource;
        public int Salary;
        public int UnlockCondition;
        public bool IsOnUI;
        
        public override void Load()
        {
            fileName = "Promotion";
            if (Application.isPlaying) Init();
        }
    }
}