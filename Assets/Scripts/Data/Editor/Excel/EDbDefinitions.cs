using System.Collections.Generic;
using Data.DbDefinition;
using Data.Editor.EDbDefinition;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Definition")]
    public class EDbDefinitions : ScriptableObject
    {
        [SerializeField] public List<EDbCurrency> Currency;
        [SerializeField] public List<EDbStat> Stat;
        [SerializeField] public List<EDbCost> Cost;
        [SerializeField] public List<EDbPlay> Play;
        [SerializeField] public List<EDbGrade> Grade;
        [SerializeField] public List<EDbDungeonMeta> Dungeon;
        [SerializeField] public List<EDbLock> Lock;
    }

}
