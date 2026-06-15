using System.Collections.Generic;
using Data.DbPromote;
using Data.Editor.EDbPromote;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Promotion")]
    public class EDbPromotions : ScriptableObject
    {
        [SerializeField] public List<EDbPromotion> Promotion;
        [SerializeField] public List<EDbPromotionDungeon> PromotionDungeon;
    }

}