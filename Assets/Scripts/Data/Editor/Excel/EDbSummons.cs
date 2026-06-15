using System.Collections.Generic;
using Data.DbSummon;
using Data.Editor.EDbSummon;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Summon")]
    public class EDbSummons : ScriptableObject
    {
        [SerializeField] public List<EDbSummonGradeProbability> SummonGradeProbability;
        [SerializeField] public List<EDbSummonNumberProbability> SummonNumberProbability;
        [SerializeField] public List<EDbSummonSkillProbability> SummonSkillProbability;
        [SerializeField] public List<EDbSummonNecklaceProbability> SummonNecklaceProbability;
        [SerializeField] public List<EDbSummonProduct> SummonProduct;
        [SerializeField] public List<EDbSummonLevel> SummonLevel;
        [SerializeField] public List<EDbSummonRelicProbability> SummonRelicProbability; 
    }

}