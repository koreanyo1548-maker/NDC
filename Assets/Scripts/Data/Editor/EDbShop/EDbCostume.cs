using System;
using System.Collections.Generic;

namespace Data.Editor.EDbShop
{
    [Serializable]
    public class EDbCostume
    {
        public int Id;
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
    }
}