using System.Collections.Generic;
using Data.DbStage;
using Data.Editor.EDbStage;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Stage")]
    public class EDbStages : ScriptableObject
    {
        [SerializeField] public List<EDbMonster> Monster;
        [SerializeField] public List<EDbStageBase> StageBase;
        [SerializeField] public List<EDbStageLevel> StageLevel;
        [SerializeField] public List<EDbStageReward> StageReward;
    }

}