using System;
using System.Collections.Generic;
using Data.DbEquipment;
using Data.Utils;
using Managers;
using UnityEngine;

namespace Data.DbShop
{
    [Serializable]
    public class DbCostume : DbModel<DbCostume, int>
    {
        public CostumeCategoryType Category;
        public int ShopId;
        public CostumePositionType Position;
        public GradeType Grade;
        public List<StatType> Options;
        public List<int> Values;
        public int NameId;
        public string Resource;
        public int PrevId;
        public int NextId;
        public int DescriptionId;
        public string StartDate;
        public DateTime StartDateCal;
        
        
        public override void Load()
        {
            fileName = "Costume";
            Init();
            
            ForEach(s =>
            {
                if (!s.StartDate.Equals("0"))
                {
                    var date = s.StartDate.Split(".");
                    s.StartDateCal = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));
                }
            });
        }

        public Sprite GetResource()
        {
            return Manager.Resource.Load<Sprite>("Costume_" + Resource);
        }

        public string GetResourceString()
        {
            return "Costume_" + Resource;
        }

        public DateTime GetStartTime()
        {
            return StartDateCal;
        }
    }
}