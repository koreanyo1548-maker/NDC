using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Data.DbAbility;
using Data.DbCharacter;
using Data.DbDefinition;
using Data.DbDungeon;
using Data.DbEquipment;
using Data.DbEvent;
using Data.DbForbiddenInfo;
using Data.DbNecklaceInfo;
using Data.DbPetInfo;
using Data.DbPromote;
using Data.DbRecord;
using Data.DbRelicInfo;
using Data.DbShop;
using Data.DbStage;
using Data.DbSummon;
using Data.Editor.EDbAbility;
using Data.Editor.EDbCharacter;
using Data.Editor.EDbDefinition;
using Data.Editor.EDbDungeon;
using Data.Editor.EDbEquipment;
using Data.Editor.EDbEvent;
using Data.Editor.EDbNecklaceInfo;
using Data.Editor.EDbPetInfo;
using Data.Editor.EDbPromote;
using Data.Editor.EDbRecord;
using Data.Editor.EDbRelicInfo;
using Data.Editor.EDbShop;
using Data.Editor.EDbStage;
using Data.Editor.EDbSummon;
using Data.Editor.Excel;
using Data.Stores;
using Data.Utils;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Data.Editor
{
    public class DataManageMenu : MonoBehaviour
    {
        /*
        [MenuItem("Data/Reset User Data")]
        static void ResetUserData()
        {
            var path = Application.persistentDataPath + "/save";
            if (File.Exists(path)) File.Delete( path );
            Debug.Log("유저 플레이 데이터 초기화 완료");
        }
        */
        static void SetIdVersion(string version)
        {
			PlayerPrefs.SetString("idVersion", version);
        }
        

        [MenuItem("Data/엑셀을 데이터로")]
        static void ExcelToData()
        {
            var character = Resources.Load<EDbCharacters>("Excels/EDbCharacters");
            Save<EDbGoldStat, DbGoldStat>(character.GoldStat,  "GoldStat");
            Save<EDbAttackLevel, DbAttackLevel>(character.AttackLevel,  "AttackLevel");
            Save<EDbHpLevel, DbHpLevel>(character.HpLevel,  "HpLevel");
            Save<EDbCriticalAttackBonusLevel, DbCriticalAttackBonusLevel>(character.CriticalAttackBonusLevel,  "CriticalAttackBonusLevel");
            Save<EDbCriticalProbabilityLevel, DbCriticalProbabilityLevel>(character.CriticalProbabilityLevel,  "CriticalProbabilityLevel");
            Save<EDbAttackBonusLevel, DbAttackBonusLevel>(character.AttackBonusLevel,  "AttackBonusLevel");
            Save<EDbHpBonusLevel, DbHpBonusLevel>(character.HpBonusLevel,  "HpBonusLevel");
            Save<EDbBossAttackBonusLevel, DbBossAttackBonusLevel>(character.BossAttackBonusLevel,  "BossAttackBonusLevel");
            Save<EDbLevelPoint, DbLevelPoint>(character.LevelPoint,  "LevelPoint");
            Save<EDbCharacterLevel, DbCharacterLevel>(character.CharacterLevel,  "CharacterLevel");
            
            var pet = Resources.Load<EDbPets>("Excels/EDbPets");
            Save<EDbBibleLevel, DbBibleLevel>(pet.BibleLevel,  "BibleLevel");
            Save<EDbPet, DbPet>(pet.Pet,  "Pet");
            Save<EDbPetAwakening, DbPetAwakening>(pet.PetAwakening,  "PetAwakening");
            Save<EDbPetAwakeningMaterial, DbPetAwakeningMaterial>(pet.PetAwakeningMaterial,  "PetAwakeningMaterial");
            Save<EDbPetGrowthMaterial, DbPetGrowthMaterial>(pet.PetGrowthMaterial,  "PetGrowthMaterial");
            Save<EDbBook, DbBook>(pet.Book,  "Book");
            
            var definition = Resources.Load<EDbDefinitions>("Excels/EDbDefinitions");
            Save<EDbCurrency, DbCurrency>(definition.Currency,  "Currency");
            Save<EDbStat, DbStat>(definition.Stat,  "Stat");
            Save<EDbCost, DbCost>(definition.Cost,  "Cost");
            Save<EDbPlay, DbPlay>(definition.Play,  "Play");
            Save<EDbGrade, DbGrade>(definition.Grade,  "Grade");
            Save<EDbDungeonMeta, DbDungeonMeta>(definition.Dungeon,  "Dungeon");
            Save<EDbLock, DbLock>(definition.Lock,  "Lock");
            
            var equipment = Resources.Load<EDbEquipments>("Excels/EDbEquipments");
            Save<EDbAccessory, DbAccessory>(equipment.Accessory,  "Accessory");
            Save<EDbAccessoryAwakening, DbAccessoryAwakening>(equipment.AccessoryAwakening,  "AccessoryAwakening");
            Save<EDbAwakeningMaterial, DbAwakeningMaterial>(equipment.AwakeningMaterial,  "AwakeningMaterial");
            Save<EDbGrowthMaterial, DbGrowthMaterial>(equipment.GrowthMaterial,  "GrowthMaterial");
            Save<EDbSkill, DbSkill>(equipment.Skill,  "Skill");
            Save<EDbSkillAwakening, DbSkillAwakening>(equipment.SkillAwakening,  "SkillAwakening");
            Save<EDbSkillDecomposition, DbSkillDecomposition>(equipment.SkillDecomposition,  "SkillDecomposition");
            Save<EDbWeapon, DbWeapon>(equipment.Weapon,  "Weapon");
            Save<EDbWeaponAwakening, DbWeaponAwakening>(equipment.WeaponAwakening,  "WeaponAwakening");
                
            var necklace = Resources.Load<EDbNecklaces>("Excels/EDbNecklaces");
            Save<EDbNecklace, DbNecklace>(necklace.Necklace,  "Necklace");
            Save<EDbNecklaceAwakening, DbNecklaceAwakening>(necklace.NecklaceAwakening,  "NecklaceAwakening");
            Save<EDbNecklaceAwakeningMaterial, DbNecklaceAwakeningMaterial>(necklace.NecklaceAwakeningMaterial,  "NecklaceAwakeningMaterial");
            Save<EDbNecklaceEquipStat, DbNecklaceEquipStat>(necklace.NecklaceEquipStat,  "NecklaceEquipStat");
            Save<EDbNecklaceGrowthMaterial, DbNecklaceGrowthMaterial>(necklace.NecklaceGrowthMaterial,  "NecklaceGrowthMaterial");
            Save<EDbNecklaceOwnStat, DbNecklaceOwnStat>(necklace.NecklaceOwnStat,  "NecklaceOwnStat");
            
            var stage = Resources.Load<EDbStages>("Excels/EDbStages");
            Save<EDbMonster, DbMonster>(stage.Monster,  "Monster");
            Save<EDbStageBase, DbStageBase>(stage.StageBase,  "StageBase");
            Save<EDbStageLevel, DbStageLevel>(stage.StageLevel,  "StageLevel");
            Save<EDbStageReward, DbStageReward>(stage.StageReward,  "StageReward");
            
            var summon = Resources.Load<EDbSummons>("Excels/EDbSummons");
            Save<EDbSummonGradeProbability, DbSummonGradeProbability>(summon.SummonGradeProbability,  "SummonGradeProbability");
            Save<EDbSummonNumberProbability, DbSummonNumberProbability>(summon.SummonNumberProbability,  "SummonNumberProbability");
            Save<EDbSummonSkillProbability, DbSummonSkillProbability>(summon.SummonSkillProbability,  "SummonSkillProbability");
            Save<EDbSummonNecklaceProbability, DbSummonNecklaceProbability>(summon.SummonNecklaceProbability,  "SummonNecklaceProbability");
            Save<EDbSummonProduct, DbSummonProduct>(summon.SummonProduct,  "SummonProduct");
            Save<EDbSummonLevel, DbSummonLevel>(summon.SummonLevel,  "SummonLevel");
            Save<EDbSummonRelicProbability, DbSummonRelicProbability>(summon.SummonRelicProbability,  "SummonRelicProbability");
            
            var records = Resources.Load<EDbRecords>("Excels/EDbRecords");
            Save<EDbQuest, DbQuest>(records.Quest,  "Quest");
            Save<EDbGuideQuest, DbGuideQuest>(records.GuideQuest,  "GuideQuest");
            Save<EDbMainQuest, DbMainQuest>(records.MainQuest,  "MainQuest");
            Save<EDbTitle, DbTitle>(records.Title,  "Title");
            Save<EDbNewbieQuest, DbNewbieQuest>(records.NewbieQuest,  "NewbieQuest");
            
            var dungeons = Resources.Load<EDbDungeons>("Excels/EDbDungeons");
            Save<EDbAwakeningDungeonLevel, DbAwakeningDungeonLevel>(dungeons.AwakeningDungeonLevel,  "AwakeningDungeonLevel");
            Save<EDbAwakeningDungeonReward, DbAwakeningDungeonReward>(dungeons.AwakeningDungeonReward,  "AwakeningDungeonReward");
            Save<EDbSkillGrowthDungeonLevel, DbSkillGrowthDungeonLevel>(dungeons.SkillGrowthDungeonLevel,  "SkillGrowthDungeonLevel");
            Save<EDbSkillGrowthDungeonReward, DbSkillGrowthDungeonReward>(dungeons.SkillGrowthDungeonReward,  "SkillGrowthDungeonReward");
            Save<EDbPetDungeonLevel, DbPetDungeonLevel>(dungeons.PetDungeonLevel,  "PetDungeonLevel");
            Save<EDbPetDungeonReward, DbPetDungeonReward>(dungeons.PetDungeonReward,  "PetDungeonReward");
            Save<EDbBlackMarketDungeonLevel, DbBlackMarketDungeonLevel>(dungeons.BlackMarketDungeonLevel,  "BlackMarketDungeonLevel");
            Save<EDbBlackMarketDungeonReward, DbBlackMarketDungeonReward>(dungeons.BlackMarketDungeonReward,  "BlackMarketDungeonReward");
            Save<EDbDiaDungeonLevel, DbDiaDungeonLevel>(dungeons.DiaDungeonLevel,  "DiaDungeonLevel");
            Save<EDbDiaDungeonReward, DbDiaDungeonReward>(dungeons.DiaDungeonReward,  "DiaDungeonReward");
            Save<EDbTrainingGroundLevel, DbTrainingGroundLevel>(dungeons.TrainingGroundLevel,  "TrainingGroundLevel");
            Save<EDbTrainingGroundReward, DbTrainingGroundReward>(dungeons.TrainingGroundReward,  "TrainingGroundReward");
            
            var shops = Resources.Load<EDbShops>("Excels/EDbShops");
            Save<EDbInAppShop, DbInAppShop>(shops.InAppShop,  "InAppShop");
            Save<EDbInGameShop, DbInGameShop>(shops.InGameShop, "InGameShop");
            Save<EDbBlackMarket, DbBlackMarket>(shops.BlackMarket,  "BlackMarket");
            Save<EDbPassShop, DbPassShop>(shops.PassShop,  "PassShop");
            Save<EDbLevelPass, DbLevelPass>(shops.LevelPass,  "LevelPass");
            Save<EDbStagePass, DbStagePass>(shops.StagePass,  "StagePass");
            Save<EDbSoulPass, DbSoulPass>(shops.SoulPass,  "SoulPass");
            Save<EDbCoupon, DbCoupon>(shops.Coupon,  "Coupon");
            Save<EDbAdBuff, DbAdBuff>(shops.AdBuff,  "AdBuff");
            Save<EDbCostume, DbCostume>(shops.Costume,  "Costume");
            Save<EDbProfile, DbProfile>(shops.Profile,  "Profile");

            var events = Resources.Load<EDbEvents>("Excels/EDbEvents");
            Save<EDbAttendReward, DbAttendReward>(events.AttendReward,  "AttendReward");
            Save<EDbAttendEvent, DbAttendEvent>(events.AttendEvent,  "AttendEvent");
            Save<EDbAttendEventReward, DbAttendEventReward>(events.AttendEventReward,  "AttendEventReward");
            Save<EDbSeasonPass, DbSeasonPass>(events.SeasonPass,  "SeasonPass");
            Save<EDbSeasonPassReward, DbSeasonPassReward>(events.SeasonPassReward,  "SeasonPassReward");
            Save<EDbSeasonPassQuest, DbSeasonPassQuest>(events.SeasonPassQuest,  "SeasonPassQuest");
            Save<EDbDropEvent, DbDropEvent>(events.DropEvent,  "DropEvent");
            Save<EDbDropEventShop, DbDropEventShop>(events.DropEventShop,  "DropEventShop");
            Save<EDbFriendReward, DbFriendReward>(events.FriendReward, "FriendReward");
            
            var promotions = Resources.Load<EDbPromotions>("Excels/EDbPromotions");
            Save<EDbPromotion, DbPromotion>(promotions.Promotion,  "Promotion");
            Save<EDbPromotionDungeon, DbPromotionDungeon>(promotions.PromotionDungeon,  "PromotionDungeon");

            var abilities = Resources.Load<EDbAbilities>("Excels/EDbAbilities");
            Save<EDbAbilityOption, DbAbilityOption>(abilities.AbilityOption,  "AbilityOption");
            Save<EDbAbilityRune, DbAbilityRune>(abilities.AbilityRune,  "AbilityRune");
            Save<EDbAbilityOptionSummon, DbAbilityOptionSummon>(abilities.AbilityOptionSummon,  "AbilityOptionSummon");
            
            var relics = Resources.Load<EDbRelics>("Excels/EDbRelics");
            Save<EDbRelic, DbRelic>(relics.Relic,  "Relic");
            Save<EDbRelicLevel, DbRelicLevel>(relics.RelicLevel,  "RelicLevel");
            Save<EDbRelicGrowthProbability, DbRelicGrowthProbability>(relics.RelicGrowthProbability,  "RelicGrowthProbability");
            
            var forbiddens = Resources.Load<EDbForbiddenInfo>("Excels/EDbForbiddenInfo");
            Save<EDbForbidden, DbForbidden>(forbiddens.Forbidden,  "Forbidden");
            Debug.Log("엑셀 업데이트 완료");
        }


        [MenuItem("Data/서버선택 초기화")]
        static void ChangeServer()
        {
            PlayerPrefs.DeleteKey("server");
        }


        [MenuItem("Data/아이디 1번")]
        static void SetIdVersionTo1()
        {
            SetIdVersion(string.Empty);
            Debug.Log("1번 아이디로 세팅되었습니다.");
        }

        [MenuItem("Data/아이디 2번")]
        static void SetIdVersionTo2()
        {
            SetIdVersion("1");
            Debug.Log("2번 아이디로 세팅되었습니다.");
        }

        [MenuItem("Data/아이디 3번")]
        static void SetIdVersionTo3()
        {
            SetIdVersion("2");
            Debug.Log("3번 아이디로 세팅되었습니다.");
        }


        [MenuItem("Data/아이디 4번")]
        static void SetIdVersionTo4()
        {
            SetIdVersion("3");
            Debug.Log("4번 아이디로 세팅되었습니다.");
        }

        static void Save<T,G>(List<T> meta, string path)
        {
            var saveData = new DbMeta<G>(JsonConvert.DeserializeObject<List<G>>(JsonConvert.SerializeObject(meta)));
            var ws = new FileStream("Assets/Resources/ExcelGenerated/" + path + ".bytes", FileMode.Create);
            var serializer = new BinaryFormatter();
            
            serializer.Serialize(ws, saveData);
            ws.Close();
        }
    }
}