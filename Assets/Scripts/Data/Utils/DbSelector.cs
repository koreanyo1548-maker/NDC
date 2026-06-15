using Data.DbDungeon;
using Data.DbEquipment;
using System;
using Controller.Have;
using Data.DbCharacter;
using Data.DbDefinition;
using Data.DbNecklaceInfo;
using Data.DbPetInfo;
using Data.DbPromote;
using Data.DbShop;
using Data.DbStage;
using Data.DbSummon;
using Data.DbUser.Equipment;
using Exceptions;
using Utils;

namespace Data.Utils
{
    public static class DbSelector
    {
        public static IDbCharacterStat GetCharacterStat(StatType stat, int level)
        {
            switch (stat)
            {
                case StatType.Attack :
                    return DbAttackLevel.Get(level);
                    
                case StatType.Hp :
                    return DbHpLevel.Get(level);
                    
                case StatType.CriticalProbability :
                    return DbCriticalProbabilityLevel.Get(level);
                    
                case StatType.CriticalAttackBonus :
                    return DbCriticalAttackBonusLevel.Get(level);
                    
                case StatType.AttackBonus :
                    return DbAttackBonusLevel.Get(level);
                    
                case StatType.HpBonus :
                    return DbHpBonusLevel.Get(level);
                    
                case StatType.BossAttackBonus :
                   return DbBossAttackBonusLevel.Get(level);
            }

            throw new Exception($"{stat}의 레벨{level}는 정의되지 않았습니다.");
        }
        
        public static IDbUserEquipment GetUserEquipment(EquipmentType type, int id)
        {
            if (type == EquipmentType.Accessory) return DbUserAccessory.Get(id);
            if (type == EquipmentType.Skill) return DbUserSkill.Get(id);
            if (type == EquipmentType.Weapon) return DbUserWeapon.Get(id);
            if (type == EquipmentType.Pet) return DbUserPet.Get(id);
            throw new NotDefinedEquipmentException(type);
        }
        
        public static IDbDungeonReward GetReward(FieldType dungeon, int stage)
        {
            if (dungeon == FieldType.Awakening) return DbAwakeningDungeonReward.Get(stage);
            if (dungeon == FieldType.Pet) return DbPetDungeonReward.Get(stage);
            if (dungeon == FieldType.SkillGrowth) return DbSkillGrowthDungeonReward.Get(stage);
            if (dungeon == FieldType.BlackMarket) return DbBlackMarketDungeonReward.Get(stage);
            if (dungeon == FieldType.Dia) return DbDiaDungeonReward.Get(stage);
            throw new NotDefinedFieldException(dungeon);
        }

        public static IDbSummonProbability GetSummonProbability(SummonType summonType, int level)
        {
            if (summonType == SummonType.Relic) return RelicController.I;
            if (summonType == SummonType.Skill) return DbSummonSkillProbability.Get(level);
            if (summonType == SummonType.Necklace) return DbSummonNecklaceProbability.Get(level);
            return DbSummonGradeProbability.Get(level);
        }
        
        public static IDbStage GetStage(FieldType dungeon, int stage)
        {
            if (dungeon == FieldType.Awakening) return DbAwakeningDungeonLevel.Get(stage);
            if (dungeon == FieldType.Pet) return DbPetDungeonLevel.Get(stage);
            if (dungeon == FieldType.SkillGrowth) return DbSkillGrowthDungeonLevel.Get(stage);
            if (dungeon == FieldType.BlackMarket) return DbBlackMarketDungeonLevel.Get(stage);
            if (dungeon == FieldType.Dia) return DbDiaDungeonLevel.Get(stage);
            if (dungeon == FieldType.Promotion) return DbPromotionDungeon.Get(stage);
            if (dungeon == FieldType.Training) return DbTrainingGroundLevel.Get(stage);
            throw new NotDefinedFieldException(dungeon);
        }

        
        public static int GetMaxStage(FieldType dungeon)
        {
            if (dungeon == FieldType.Awakening) return DbAwakeningDungeonLevel.Count;
            if (dungeon == FieldType.Pet) return DbPetDungeonLevel.Count;
            if (dungeon == FieldType.SkillGrowth) return DbSkillGrowthDungeonLevel.Count;
            if (dungeon == FieldType.BlackMarket) return DbBlackMarketDungeonLevel.Count;
            if (dungeon == FieldType.Dia) return DbDiaDungeonLevel.Count;
            if (dungeon == FieldType.Promotion) return DbPromotionDungeon.Count;
            throw new NotDefinedFieldException(dungeon);
        }

        public static IDbEquipment GetEquipment(CurrencyType type, int id)
        {
            if (type == CurrencyType.Weapon) return DbWeapon.Get(id);
            if (type == CurrencyType.Accessory) return DbAccessory.Get(id);
            if (type == CurrencyType.Skill) return DbSkill.Get(id);
            if (type == CurrencyType.Pet) return DbPet.Get(id);
            throw new NotDefinedEquipmentException(type);
        }

        public static GradeType GetGrade(CurrencyType type, int id)
        {
            if (type == CurrencyType.Weapon) return DbWeapon.Get(id).GetGrade();
            if (type == CurrencyType.Accessory) return DbAccessory.Get(id).GetGrade();
            if (type == CurrencyType.Skill) return DbSkill.Get(id).GetGrade();
            if (type == CurrencyType.Pet) return DbPet.Get(id).GetGrade();
            if (type == CurrencyType.Necklace) return DbNecklace.Get(id).GetGrade();
            throw new NotDefinedEquipmentException(type);
        }
        
        public static IDbEquipment GetEquipment(EquipmentType type, int id)
        {
            if (type == EquipmentType.Weapon) return DbWeapon.Get(id);
            if (type == EquipmentType.Accessory) return DbAccessory.Get(id);
            if (type == EquipmentType.Skill) return DbSkill.Get(id);
            if (type == EquipmentType.Pet) return DbPet.Get(id);
            throw new NotDefinedEquipmentException(type);
        }

        public static IDbPass GetPass(PassType pass, int level)
        {
            if (pass == PassType.LevelPass) return DbLevelPass.Get(level);
            if (pass == PassType.StagePass) return DbStagePass.Get(level);
            if (pass == PassType.SoulPass) return DbSoulPass.Get(level);
            throw new NotDefinedPassException(pass);
        }

        public static IDbPass GetFirstLarge(PassType pass, int progress)
        {
            if (pass == PassType.LevelPass) return DbLevelPass.GetFirstLarger(progress);
            if (pass == PassType.StagePass) return DbStagePass.GetFirstLarger(progress);
            if (pass == PassType.SoulPass) return DbSoulPass.GetFirstLarger(progress);
            throw new NotDefinedPassException(pass);
        }

        public static void ForEach(PassType pass, Predicate<IDbPass> predicate, Action<IDbPass> toDo)
        {
            if (pass == PassType.LevelPass) DbLevelPass.ForEach(predicate, toDo);
            else if (pass == PassType.StagePass) DbStagePass.ForEach(predicate, toDo);
            else if (pass == PassType.SoulPass) DbSoulPass.ForEach(predicate, toDo);
            else throw new NotDefinedPassException(pass);
        }

        public static int GetFirstId(PassType pass, CurrencyType specific)
        {
            if (pass == PassType.LevelPass) return DbLevelPass.GetFirstId(specific);
            if (pass == PassType.StagePass) return DbStagePass.GetFirstId(specific);
            return 0;
        }
    }
}