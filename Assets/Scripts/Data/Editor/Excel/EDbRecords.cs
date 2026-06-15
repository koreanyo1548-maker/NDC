using System.Collections.Generic;
using Data.DbRecord;
using Data.Editor.EDbRecord;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Record")]
    public class EDbRecords: ScriptableObject
    {
        [SerializeField] public List<EDbQuest> Quest;
        [SerializeField] public List<EDbGuideQuest> GuideQuest;
        [SerializeField] public List<EDbMainQuest> MainQuest;
        [SerializeField] public List<EDbTitle> Title;
        [SerializeField] public List<EDbNewbieQuest> NewbieQuest;
    }
}