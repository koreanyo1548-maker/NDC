using System.Collections.Generic;
using Data.Editor.EDbAbility;
using Data.Editor.EDbRelicInfo;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Relic")]
    public class EDbRelics: ScriptableObject
    {
        [SerializeField] public List<EDbRelic> Relic;
        [SerializeField] public List<EDbRelicGrowthProbability> RelicGrowthProbability;
        [SerializeField] public List<EDbRelicLevel> RelicLevel;
    }
}