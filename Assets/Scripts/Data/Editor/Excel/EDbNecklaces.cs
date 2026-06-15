using System.Collections.Generic;
using Data.Editor.EDbNecklaceInfo;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Necklace")]
    public class EDbNecklaces : ScriptableObject
    {
        [SerializeField] public List<EDbNecklace> Necklace;
        [SerializeField] public List<EDbNecklaceAwakening> NecklaceAwakening;
        [SerializeField] public List<EDbNecklaceAwakeningMaterial> NecklaceAwakeningMaterial;
        [SerializeField] public List<EDbNecklaceEquipStat> NecklaceEquipStat;
        [SerializeField] public List<EDbNecklaceGrowthMaterial> NecklaceGrowthMaterial;
        [SerializeField] public List<EDbNecklaceOwnStat> NecklaceOwnStat;
    }
}