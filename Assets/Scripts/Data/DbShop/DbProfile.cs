using System;
using Controller.Currency;
using Data.Utils;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbProfile : DbModel<DbProfile, int>
    {
        public ProfileConditionType Condition;
        public int Goal;
        public string Resource;

        public override void Load()
        {
            fileName = "Profile";
            if (Application.isPlaying) Init();
        }
        
        public bool IsConditionPassed()
        {
            if (Condition == ProfileConditionType.None) return true;
            if (Condition == ProfileConditionType.Costume)
            {
                return CurrencyController.I.HaveCostume(Goal);
            }

            throw new Exception(Condition + " is not defined profile condition");
        }
    }
}