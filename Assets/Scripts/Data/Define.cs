using System.Collections.Generic;

namespace Data
{
    public  enum eLoginType
    {
        Guest,
        AOS_Google,
        AOS_Apple,
        IOS_Apple,
        IOS_Google
    }
    public enum StageType
    {
        Normal,
        Boss,
        Pass,
        Defense,
        Sequence,
        Training
    }
    
    public enum StatType
    {
        Attack,
        Hp,
        CriticalProbability,
        CriticalAttackBonus,
        StageExpEarn,
        StageGoldEarn,
        MoveSpeedBonus,
        AttackSpeedBonus,
        StageGrowthEarn,
        BlackMarketDungeonEarn,
        SkillGrowthDungeonEarn,
        DiaDungeonEarn,
        AwakeningDungeonEarn,
        BossAttackBonus,
        SpecificSkillAttackBonus,
        SkillAttackBonus,
        DashAttackBonus,
        FinalAttackBonus,
        FinalHpBonus,
        AwakeningAttackBonus,
        AwakeningHpBonus,
        PetAttackBonus,
        PetHpBonus,
        StageItemRate,
        DebuffMonsterHp,
        DebuffMonsterAttack,
        AttackBonus,
        HpBonus,
        Skill,
        PotentialAttackBonus,
        PotentialHpBonus,
        AbilityGoldEarn,
        AbilityExpEarn,
        RelicAttackBonus,
        RelicHpBonus,
        NecklaceAttackBonus,
        NecklaceHpBonus,
        NormalAttackBonus,
        
        None = 999
    }

    public enum StatConditionType
    {
        None,
        AttackLevel,
        HpLevel,
        Stage,
        Level
    }

    public enum SummonType
    {
        Weapon,
        Accessory,
        Skill,
        Relic,
        Necklace
    }

    public enum SummonFullType
    {
        WeaponSummon1,
        WeaponSummon2,
        WeaponSummon3,
        WeaponSummon4,
        WeaponAdSummon,
        AccessorySummon1,
        AccessorySummon2,
        AccessorySummon3,
        AccessorySummon4,
        AccessoryAdSummon,
        SkillSummon1,
        SkillSummon2,
        SkillSummon3,
        SkillSummon4,
        SkillAdSummon,
        RelicSummon1,
        RelicSummon2,
        RelicSummon3,
        RelicSummon4,
        RelicAdSummon,
        NecklaceSummon1,
        NecklaceSummon2,
        NecklaceSummon3,
        NecklaceSummon4,
        NecklaceAdSummon
    }
    
    public enum QuestCycleType
    {
        Daily,
        Weekly,
        Repeat
    }

    public enum LockType
    {
        SkillSlot2,
        SkillSlot3,
        SkillSlot4,
        Pet,
        Character,
        Summon,
        Skill,
        Level,
        SkillSummon,
        Inventory,
        AccessorySummon,
        Accessory,
        Dungeon,
        AwakeningDungeon,
        PetDungeon,
        AutoSkill,
        PetSlot2,
        PetSlot3,
        PetSlot4,
        BlackMarketDungeon,
        DiaDungeon,
        Ability,
        Relic,
        RelicSummon,
        Necklace,
        NecklaceSummon,
        BlackMarket,
        TrainingGround,
        NewbieQuestDay2,
        NewbieQuestDay3,
        NewbieQuestDay4,
        NewbieQuestDay5,
        NewbieQuestDay6,
        NewbieQuestDay7
    }

    public enum LockConditionType
    {
        Stage,
        Promotion,
        GuideQuest,
        BibleLevel,
        NewbieDay
    }

    public enum ProfileConditionType
    {
        None,
        Costume
    }

    public enum FieldType
    {
        Stage = 0,
        Awakening = 1,
        SkillGrowth = 2,
        Promotion = 3,
        Pet = 4,
        BlackMarket = 5,
        Dia = 6,
        Training = 7
    }

    public enum CurrencyCategoryType
    {
        Money,
        Stone,
        Ticket,
        Have,
        Etc,
        Book,
        None
    }

    public enum PassCondition
    {
        Open,
        Closed,
        CheckDate
    }
    
    public enum CurrencyType
    {
        Dia = 0,
        Gold = 1,
        Mileage = 44,
        Passion = 48,
        DropEventMoney = 61,
        BlackMarketCoin = 76,
        BeadsOre = 77,

        WeaponGrowthStone = 2,
        AwakeningStone = 3,
        AccessoryGrowthStone = 4,
        SkillGrowthStone = 5,
        PetGrowthStone = 9,
        
        LevelPoint = 6,
        
        AdSkip = 10,
        Ad = 11,
        
        Weapon = 12,
        Accessory = 13,
        Skill = 14,
        Costume = 43,
        Pet = 39,
        Necklace = 63,
        
        Exp = 15,
        
        LevelPass1 = 16,
        LevelPass2 = 17,
        LevelPass3 = 18,
        LevelPass4 = 50,
        LevelPass5 = 51,
        StagePass1 = 19,
        StagePass2 = 20,
        StagePass3 = 21,
        StagePass4 = 52,
        StagePass5 = 53,
        SoulPass = 22,
        SeasonPass = 59,
        
        SkillPreset3 = 23,
        SkillPreset4 = 24,
        SkillPreset5 = 25,
        
        SkillGrowthDungeonTicket = 7,
        AwakeningDungeonTicket = 8,
        PetDungeonTicket = 26,
        BlackMarketDungeonTicket = 27,
        DiaDungeonTicket = 28,
        
        AdSkillGrowthDungeonTicket = 64,
        AdAwakeningDungeonTicket = 65,
        AdPetDungeonTicket = 66,
        AdBlackMarketDungeonTicket = 67,
        AdDiaDungeonTicket = 68,

        WeaponSummonTicket = 55,
        AccessorySummonTicket = 56,
        SkillSummonTicket = 57,
        RelicSummonTicket = 58,
        NecklaceSummonTicket = 62,

        AdWeaponSummonTicket = 69,
        AdAccessorySummonTicket = 70,
        AdSkillSummonTicket = 71,
        AdRelicSummonTicket = 72,
        AdNecklaceSummonTicket = 73,

        NormalBook1 = 29,
        NormalBook2 = 30,
        NormalBook3 = 31,
        NormalBook4 = 32,
        SealedBook1 = 33,
        SealedBook2 = 34,
        SealedBook3 = 35,
        SealedBook4 = 36,
        
        Bookshelf1 = 40,
        Bookshelf2 = 41,
        Bookshelf3 = 42,
        
        AbilityPreset3 = 45,
        AbilityPreset4 = 46,
        AbilityPreset5 = 47,
        
        OfflineReward = 49,
        OfflineGold = 54,
        OfflineExp = 60,
        OfflineWeaponGrowthStone = 74,
        OfflineAccessoryGrowthStone = 75,
        
        None = 999
    }
    
    public enum PassType
    {
        LevelPass = 16,
        StagePass = 19,
        SoulPass = 22,
        SeasonPass = 59
    }

    public enum QuestType
    {
        Attend,
        PlayTime,
        LevelUp,
        GoldStatLevelUp,
        DungeonClear,
        MonsterKillCount,
        BossKillCount,
        CharacterDeath,
        WeaponGrowth,
        AccessoryGrowth,
        SkillGrowth,
        PetGrowth,
        WeaponMerge,
        AccessoryMerge,
        WeaponSummon,
        AccessorySummon,
        SkillSummon,
        RelicSummon,
        ChangeAbility,
        BookUnseal,
        SealedBookUnseal,
        EquipRecommendWeapon,
        EquipRecommendAccessory,
        Dia,
        AdWatch,
        UseSkill,
        DailyQuestClear,
        WeeklyQuestClear,
        CheckAttackLevel,
        CheckHpLevel,
        CheckCriticalProbabilityLevel,
        CheckCriticalAttackBonusLevel,
        CheckLevelUp,
        CheckLevelPoint,
        CheckBibleLevel,
        CheckPromotion,
        CheckWeaponAwakening,
        CheckAccessoryAwakening,
        CheckSkillAwakening,
        CheckPetAwakening,
        CheckWeaponEquip,
        CheckAccessoryEquip,
        CheckSkill1Equip,
        CheckSkill2Equip,
        CheckPetEquip,
        CheckStageClear,
        CheckAwakeningDungeonClear,
        CheckSkillDungeonClear,
        CheckPetDungeonClear,
        CheckAutoSkillOn,
        CheckBuffAdWatch,
        CheckWeaponGrowth,
        CheckAccessoryGrowth,
        CheckRelicLevel,
        NecklaceSummon,
        CheckWeaponSummon,
        CheckAccessorySummon,
        CheckSkillSummon,
        CheckRelicSummon,
        CheckNecklaceSummon,
        NewbieDayQuestClear,
        TrainingGroundClear, 
        PassionGet,
        BlackMarketDungeonClear,
        RelicUpgrade,
        WeaponAwakening,
        AccessoryAwakening,
        AwakeningDungeonClear,
        PetDungeonClear,
        None = 1000
    }
    
    public enum CostType
    {
        LevelPointResetDia,
        NicknameResetDia,
        BookUnsealingDia,
        AbilityRollDia,
        RelicSellCost
    }

    public enum GradeType
    {
        Normal = 0,
        Magic = 1,
        Rare = 2,
        Unique = 3,
        Heroic = 4,
        Legendary = 5,
        Mythic = 6
    }

    public enum EquipmentType
    {
        Weapon,
        Accessory,
        Skill,
        Pet
    }

    public enum FullGradeType
    {
        Normal1 = 0,
        Normal2 = 1,
        Normal3 = 2,
        Normal4 = 3,
        Normal5 = 4,
        Magic1 = 5,
        Magic2 = 6,
        Magic3 = 7,
        Magic4 = 8,
        Magic5 = 9,
        Rare1 = 10,
        Rare2 = 11,
        Rare3 = 12,
        Rare4 = 13,
        Rare5 = 14,
        Unique1 = 15,
        Unique2 = 16,
        Unique3 = 17,
        Unique4 = 18,
        Unique5 = 19,
        Heroic1 = 20,
        Heroic2 = 21,
        Heroic3 = 22,
        Heroic4 = 23,
        Heroic5 = 24,
        Legendary1 = 25,
        Legendary2 = 26,
        Legendary3 = 27,
        Legendary4 = 28,
        Legendary5 = 29,
        Mythic1 = 30,
        Mythic2 = 31,
        Mythic3 = 32,
        Mythic4 = 33,
        Mythic5 = 34
    }

    public enum RenewalType
    {
        Infinite,
        OnlyOnce,
        Daily,
        Weekly,
        Monthly
    }

    public enum ShopCategoryType
    {
        Package,
        Dia,
        Hidden,
        Mileage,
        Costume,
        Ad,
        Unlock,
        Ticket,
        Normal
    }

    public enum PlatformType
    {
        All,
        OneStore,
        AppStore,
        GooglePlay,
        AppStoreReview
    }
    public enum LoginPlatform
    {
        None,
        Google,
        GooglePlayGames,
        Apple
    }

    public enum CostumeCategoryType
    {
        Promotion,
        InGameShop,
        InAppShop,
        Event
    }

    public enum CostumePositionType
    {
        Body,
        Weapon
    }

    
    public enum TargetRangeType
    {
        Range,
        Number,
        Straight
    }
    
    public enum TargetBaseType
    {
        Player,
        Target,
        Effect
    }


    public enum SkillType
    {
        Attack,
        DotAttack,
        Stun,
        Push,
        Gather,
        TargetAttack,
    /* ------------------ 
        여기서부터 펫 스킬  
       ------------------ */
        Heal,
        AttackBuff
    }
    
    public enum EffectPositionType
    {
        Player,
        Target,
        PlayerDirection,
        Random,
        Center,
        MoveRandom
    }
    

    public enum PlayType
    {
        BigWaveStartTime,
        BigWaveDuration,
        DefaultAttackSpeed,
        DefaultMoveSpeed,
        DefaultDashSpeed,
        DashRange,
        MultipleAttackMaxCount,
        OfflineReward,
        BibleDefenseRange,
        BookReduceTimeByAd,
        FreePassion,
        MainQuestMaxLoop,
        TrainingGroundResetDay
    }

    public enum PowerType
    {
        GoldAttack,
        GoldHp,
        GoldCriticalProbability,
        GoldCriticalDamageRate,
        LevelUpAttack,
        LevelUpHp,
        LevelUpCriticalProbability,
        LevelUpCriticalDamageRate,
        LevelUpSkillDamageRate,
        NormalGrowth,
        MagicGrowth,
        RareGrowth,
        UniqueGrowth,
        HeroicGrowth,
        LegendaryGrowth,
        MythicGrowth,
        NormalAwakening,
        MagicAwakening,
        RareAwakening,
        UniqueAwakening,
        HeroicAwakening,
        LegendaryAwakening,
        MythicAwakening,
        None

    }

    public enum SettingType
    {
        IsGuest,
        Setting,
        IsAutoProgress,
        IsAutoSkill,
        SfxSound,
        BGMSound,
        Language,
        StatUpCount,
        LevelUpCount,
        IsCameraShaking,
        IsAutoPowerSaving,
        IsPushAlarm,
        IsNightPushAlarm,
        NormalSkillPreset,
        BossSkillPreset,
        SGSDungeonSkillPreset,
        ASDungeonSkillPreset,
        DoPrologue1,
        PetDungeonSkillPreset,
        BMDungeonSkillPreset,
        DiaDungeonSkillPreset,
        TrainingSkillPreset
    }

    public enum SpeakType
    {
        Live,
        Chat,
        Message
    }

    public enum AdBuffType
    {
        GoldBuff = 0,
        GrowthStoneBuff = 1,
    }

    public enum ScenarioAnimationType
    {
        Idle = 0,
        Angry = 1,
        Surprised = 2,
        Smile = 3
    }
    /// <summary>
    /// 광고 종류
    /// </summary>
    public enum eAdType
    {
        Summon = 1,
        Dungeon,
        Buff,
        OfflineReward,
        Bookshelf,
        Shop
    }
    public enum eLog
    {
        None,
        Error,
        Cheat
    }
}