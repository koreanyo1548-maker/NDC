using System.Collections.Generic;
using Data.Editor.EDbDungeon;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Dungeon")]
    public class EDbDungeons: ScriptableObject
    {
        [SerializeField] public List<EDbAwakeningDungeonLevel> AwakeningDungeonLevel;
        [SerializeField] public List<EDbAwakeningDungeonReward> AwakeningDungeonReward;
        [SerializeField] public List<EDbSkillGrowthDungeonLevel> SkillGrowthDungeonLevel;
        [SerializeField] public List<EDbSkillGrowthDungeonReward> SkillGrowthDungeonReward;
        [SerializeField] public List<EDbPetDungeonLevel> PetDungeonLevel;
        [SerializeField] public List<EDbPetDungeonReward> PetDungeonReward;
        [SerializeField] public List<EDbBlackMarketDungeonLevel> BlackMarketDungeonLevel;
        [SerializeField] public List<EDbBlackMarketDungeonReward> BlackMarketDungeonReward;
        [SerializeField] public List<EDbDiaDungeonLevel> DiaDungeonLevel;
        [SerializeField] public List<EDbDiaDungeonReward> DiaDungeonReward;
        [SerializeField] public List<EDbTrainingGroundLevel> TrainingGroundLevel;
        [SerializeField] public List<EDbTrainingGroundReward> TrainingGroundReward;
    }

}