using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbPromote
{
    [Serializable]
    public class EDbPromotion
    {
        public int Id;
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
    }
}