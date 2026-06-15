using System;
using System.Collections.Generic;
using System.Numerics;
using Controller.Infos;
using Data.DbEquipment;
using Data.DbUser.Progress;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace Data.Stores
{
    [Serializable]
    public class Info
    {
        public int version;
        public TimeSpan playTime;
        public Level level;
        public MainQuest mainQuest;
        public LevelPoint levelPoint;
        public Stat stat;
        public SeasonPass seasonPass;
        public Attend attend;
        public EventAttend eventAttend;
        public Equip equip;
        public DateTime createdDate;
        public DateTime questResetDate;
        public DateTime currencyResetDate;
        public DateTime levelResetDate;
        public List<string> friends;
        public int friendRewarded;

        public Info()
        {
        }
        
        [JsonConstructor]
        public Info(int version, TimeSpan playTime, Equip equip, Level level, LevelPoint levelPoint, 
            Stat stat, SeasonPass seasonPass, MainQuest mainQuest, Attend attend, EventAttend eventAttend, 
            DateTime createdDate, DateTime questResetDate, DateTime currencyResetDate,
            DateTime attendResetDate, DateTime levelResetDate, List<string> friends, int friendRewarded)
        {
            this.version = version;
            this.playTime = playTime;
            this.equip = equip;
            this.level = level;
            this.levelPoint = levelPoint;
            this.stat = stat;
            this.seasonPass = seasonPass;
            this.mainQuest = mainQuest;
            this.attend = attend;
            this.eventAttend = eventAttend;
            this.createdDate = createdDate;
            this.questResetDate = questResetDate;
            this.currencyResetDate = currencyResetDate;
            this.levelResetDate = levelResetDate;
            if (friends == null) friends = new();
            this.friends = friends;
            this.friendRewarded = friendRewarded;
        }

        public List<DbUserAttend> ConvertToAttend()
        {
            return new List<DbUserAttend>
            {
                new (0, attend.nextDay, attend.lastRewarded)
            };
        }

        public List<DbUserEventAttend> ConvertToEventAttend()
        {
            return new List<DbUserEventAttend>
            {
                new (0, eventAttend.currentId, eventAttend.rewardedCount, eventAttend.lastRewardedDate)
            };
        }

        public List<DbUserSeasonPass> ConvertToSeasonPass()
        {
            return new List<DbUserSeasonPass>
            {
                new(0, seasonPass.currentId, seasonPass.rewarded, seasonPass.point, seasonPass.quest)
            };
        }

        public List<DbUserEquip> ConvertToEquip()
        {
            return new List<DbUserEquip>
            {
                new(0, equip.weapon, equip.accessory, equip.skill, equip.necklace, equip.pet, 
                equip.title, equip.bodyCostume, equip.weaponCostume, equip.profile)
            };
        }
        
        public List<DbUserLevel> ConvertToLevel()
        {
            return new List<DbUserLevel>
            {
                new(0, level.level, level.exp, level.stage, level.maxStage, level.isStageClear, 
                    level.awakeningDungeonStage, level.skillGrowthDungeonStage, level.petDungeonStage, 
                    level.blackMarketDungeonStage, level.diaDungeonStage, level.blackMarketDungeonReward,
                    level.diaDungeonReward, level.trainingGroundStage, level.bibleLevel,
                    level.promotion, level.maxPower, level.maxTraining, 
                    level.wSCount, level.wSLevel, level.wSReward, level.wSExp,
                    level.aSCount, level.aSLevel, level.aSReward, level.aSExp,
                     level.sSCount, level.sSLevel, level.sSReward, level.sSExp,
                    level.rSCount, level.nSCount, level.nSLevel, level.nSExp)
            };
        }
        public List<DbUserLevelPoint> ConvertToLevelPoint()
        {
            return new List<DbUserLevelPoint>
            {
                new(0, levelPoint.attackLevel, levelPoint.hpLevel, levelPoint.cpLevel, levelPoint.cdrLevel,
                    levelPoint.sdLevel, levelPoint.ebrLevel, levelPoint.gbrLevel,
                    levelPoint.dashAttackBonusLevel, levelPoint.debuffMonsterAttackLevel, levelPoint.debuffMonsterHpLevel)
            };
        }
        public List<DbUserStat> ConvertToStat()
        {
            return new List<DbUserStat>
            {
                new(0, stat.attackLevel, stat.hpLevel, stat.cpLevel, stat.cdrLevel, stat.attackBonusLevel, stat.hpBonusLevel, stat.bossAttackBonusLevel)
            };
        }
        public List<DbUserMainQuest> ConvertToMainQuest()
        {
            if (mainQuest.questId == 0) return null;
            return new List<DbUserMainQuest>
            {
                new(0, mainQuest.questId, mainQuest.isOnGuide, mainQuest.doCount, mainQuest.loopCount)
            };
        }
        
        public void WhenCreated(DateTime now)
        {
            version = 0;
            playTime = TimeSpan.Zero;
            createdDate = now;
            equip = new(DbUserEquip.Get(0));
            level = new(DbUserLevel.Get(0));
            levelPoint = new(DbUserLevelPoint.Get(0));
            stat = new(DbUserStat.Get(0));
            seasonPass = new(DbUserSeasonPass.Get(0));
            mainQuest = new(DbUserMainQuest.Get(0));
            attend = new(DbUserAttend.Get(0));
            eventAttend = new(DbUserEventAttend.Get(0));
            questResetDate = now;
            currencyResetDate = now;
            levelResetDate = now;
            friends = new();
            friendRewarded = 0;
        }
        
        public Info Set()
        {
            version = 0;
            equip.Set(DbUserEquip.Get(0));
            level.Set(DbUserLevel.Get(0));
            levelPoint.Set(DbUserLevelPoint.Get(0));
            stat.Set(DbUserStat.Get(0));
            seasonPass.Set(DbUserSeasonPass.Get(0));
            mainQuest.Set(DbUserMainQuest.Get(0));
            attend.Set(DbUserAttend.Get(0));
            eventAttend.Set(DbUserEventAttend.Get(0));
            friends = FriendController.I.Friends;
            friendRewarded = FriendController.I.FriendRewarded;
            return this;
        }
    }

    [Serializable]
    public class MainQuest
    {
        public int questId;
        public bool isOnGuide;
        public int doCount;
        public int loopCount;
        
        public MainQuest(DbUserMainQuest mainQuest)
        {
            Set(mainQuest);
        }

        public void Set(DbUserMainQuest mainQuest)
        {
            questId = mainQuest.QuestId.Value;
            isOnGuide = mainQuest.IsOnGuide.Value;
            doCount = mainQuest.DoCount.Value;
            loopCount = mainQuest.LoopCount.Value;
        }

        [JsonConstructor]
        public MainQuest(int questId, bool isOnGuide, int doCount, int loopCount)
        {
            this.questId = questId;
            this.isOnGuide = isOnGuide;
            this.doCount = doCount;
            this.loopCount = loopCount;
        }
    }
    
    [Serializable]
    public class Equip
    {
        public int weapon;
        public int accessory;
        public List<List<int>> skill;
        public List<int> necklace;
        public List<int> pet;
        public int title;
        public int bodyCostume;
        public int weaponCostume;
        public int profile;

        [JsonConstructor]
        public Equip(int weapon, int accessory, int title, int bodyCostume, int weaponCostume, 
        int profile, List<List<int>> skill, List<int> necklace, List<int> pet)
        {
            if (necklace == null) necklace = new(){-1, -1, -1, -1, -1, -1, -1};
            this.weapon = weapon;
            this.accessory = accessory;
            this.skill = skill;
            this.necklace = necklace;
            this.pet = pet;
            this.title = title;
            this.bodyCostume = bodyCostume;
            this.weaponCostume = weaponCostume;
            this.profile = profile;
            
            foreach (var skills in skill)
            {
                for (var idx = 0; idx < skills.Count; ++idx)
                {
                    if (DbSkill.Get(skills[idx]) == null) skills[idx] = -1;
                }
            }
        }
        
        public Equip(DbUserEquip equip)
        {
            Set(equip);
        }

        public void Set(DbUserEquip equip)
        {
            weapon = equip.Weapon.Value;
            accessory = equip.Accessory.Value;
            skill = new List<List<int>>();
            for (var idx = 0; idx < equip.Skills.Count; ++idx) skill.Add(equip.Skills[idx].ToList());
            necklace = equip.Necklaces.ToList();
            pet = equip.Pets.ToList();
            title = equip.Title.Value;
            bodyCostume = equip.BodyCostume.Value;
            weaponCostume = equip.WeaponCostume.Value;
            profile = equip.Profile.Value;
        }
    }

    [Serializable]
    public class EventAttend
    {
        public int currentId;
        public int rewardedCount;
        public DateTime lastRewardedDate;
        
        [JsonConstructor]
        public EventAttend(int currentId, int rewardedCount, DateTime lastRewardedDate)
        {
            this.currentId = currentId;
            this.rewardedCount = rewardedCount;
            this.lastRewardedDate = lastRewardedDate;
        }
        
        public EventAttend(DbUserEventAttend attend)
        {
            Set(attend);
        }
        
        public void Set(DbUserEventAttend attend)
        {
            currentId = attend.CurrentId.Value;
            rewardedCount = attend.RewardedCount.Value;
            lastRewardedDate = attend.LastRewardedDate;
        }
    }
    
    [Serializable]
    public class SeasonPass
    {
        public int currentId;
        public long point;
        public List<int> rewarded;
        public Dictionary<QuestType, int> quest;
        
        [JsonConstructor]
        public SeasonPass(int currentId, long point, List<int> rewarded, Dictionary<QuestType, int> quest)
        {
            this.currentId = currentId;
            this.point = point;
            this.rewarded = rewarded;
            this.quest = quest;
        }
        
        public SeasonPass(DbUserSeasonPass seasonPass)
        {
            Set(seasonPass);
        }
        
        public void Set(DbUserSeasonPass seasonPass)
        {
            currentId = seasonPass.CurrentId.Value;
            point = seasonPass.Point.Value;
            rewarded = seasonPass.Rewarded.Value;
            if (quest == null) quest = new();
            quest.Clear();
            foreach (var q in seasonPass.Quest)
            {
                quest.Add(q.Key, q.Value.Value);
            }
        }
    }
    
    [Serializable]
    public class Attend
    {
        public int nextDay;
        public DateTime lastRewarded;

        
        [JsonConstructor]
        public Attend(int nextDay, DateTime lastRewarded)
        {
            this.nextDay = nextDay;
            this.lastRewarded = lastRewarded;
        }
        
        public Attend(DbUserAttend attend)
        {
            Set(attend);
        }
        
        public void Set(DbUserAttend attend)
        {
            nextDay = attend.NextDay.Value;
            lastRewarded = attend.LastRewarded;
        }
    }
    
    [Serializable]
    public class Level
    {
        public int level;
        public BigInteger exp;
        public int stage;
        public int maxStage;
        public bool isStageClear;
        public int awakeningDungeonStage;
        public int skillGrowthDungeonStage;
        public int petDungeonStage;
        public int blackMarketDungeonStage;
        public int diaDungeonStage;
        public long blackMarketDungeonReward;
        public long diaDungeonReward;
        public int trainingGroundStage;
        public int bibleLevel;
        public int promotion;
        public BigInteger maxPower;
        public BigInteger maxTraining;
        public long wSCount;
        public int wSLevel;
        public int wSReward;
        public BigInteger wSExp;
        public long aSCount;
        public int aSLevel;
        public int aSReward;
        public BigInteger aSExp;
        public long sSCount;
        public int sSLevel;
        public int sSReward;
        public BigInteger sSExp;
        public long rSCount;
        public long nSCount;
        public int nSLevel;
        public BigInteger nSExp;


        public Level(DbUserLevel level)
        {
            Set(level);
        }
        public void Set(DbUserLevel level)
        {
            this.level = level.Level.Value;
            exp = level.Exp.Value;
            stage = level.Stage.Value;
            maxStage = level.MaxStage.Value;
            isStageClear = level.IsStageClear.Value;
            awakeningDungeonStage = level.AwakeningDungeonStage.Value;
            skillGrowthDungeonStage = level.SkillGrowthDungeonStage.Value;
            petDungeonStage = level.PetDungeonStage.Value;
            blackMarketDungeonStage = level.BlackMarketDungeonStage.Value;
            diaDungeonStage = level.DiaDungeonStage.Value;
            blackMarketDungeonReward = level.BlackMarketDungeonReward.Value;
            diaDungeonReward = level.DiaDungeonReward.Value;
            trainingGroundStage = level.TrainingGroundStage.Value;
            bibleLevel = level.BibleLevel.Value;
            promotion = level.Promotion.Value;
            maxPower = level.MaxPower.Value;
            maxTraining = level.MaxTraining.Value;
            wSCount = level.WeaponSummonCount.Value;
            wSLevel = level.WeaponSummonLevel.Value;
            wSReward = level.WeaponSummonReward.Value;
            wSExp = level.WeaponSummonExp.Value;
            aSCount = level.AccessorySummonCount.Value;
            aSLevel = level.AccessorySummonLevel.Value;
            aSReward = level.AccessorySummonReward.Value;
            aSExp = level.AccessorySummonExp.Value;
            sSCount = level.SkillSummonCount.Value;
            sSLevel = level.SkillSummonLevel.Value;
            sSReward = level.SkillSummonReward.Value;
            sSExp = level.SkillSummonExp.Value;
            rSCount = level.RelicSummonCount.Value;
            nSCount = level.NecklaceSummonCount.Value;
            nSLevel = level.NecklaceSummonLevel.Value;
            nSExp = level.NecklaceSummonExp.Value;
        }

        [JsonConstructor]
        public Level(int level, BigInteger exp, int stage, int maxStage, bool isStageClear,
            int awakeningDungeonStage, int skillGrowthDungeonStage, int petDungeonStage,
            int blackMarketDungeonStage, int diaDungeonStage, long blackMarketDungeonReward,
            long diaDungeonReward, int trainingGroundStage, int bibleLevel,
            int promotion, BigInteger maxPower, BigInteger maxTraining, 
            long wSCount, int wSLevel, int wSReward, BigInteger wSExp,
            long aSCount, int aSLevel, int aSReward, BigInteger aSExp, 
            long sSCount, int sSLevel, int sSReward, BigInteger sSExp,
            long rSCount, long nSCount, int nSLevel, BigInteger nSExp)
        {
            this.level = level;
            this.exp = exp;
            this.stage = stage;
            this.maxStage = maxStage;
            this.isStageClear = isStageClear;
            this.awakeningDungeonStage = awakeningDungeonStage;
            this.skillGrowthDungeonStage = skillGrowthDungeonStage;
            this.petDungeonStage = petDungeonStage;
            this.blackMarketDungeonStage = blackMarketDungeonStage;
            this.diaDungeonStage = diaDungeonStage;
            this.blackMarketDungeonReward = blackMarketDungeonReward;
            this.diaDungeonReward = diaDungeonReward;
            this.trainingGroundStage = trainingGroundStage;
            this.bibleLevel = bibleLevel;
            this.promotion = promotion;
            this.maxPower = maxPower;
            this.maxTraining = maxTraining;
            this.wSCount = wSCount;
            this.wSLevel = wSLevel;
            this.wSReward = wSReward;
            this.wSExp = wSExp;
            this.aSCount = aSCount;
            this.aSLevel = aSLevel;
            this.aSReward = aSReward;
            this.aSExp = aSExp;
            this.sSCount = sSCount;
            this.sSLevel = sSLevel;
            this.sSReward = sSReward;
            this.sSExp = sSExp;
            this.rSCount = rSCount;
            this.nSCount = nSCount;
            this.nSLevel = nSLevel;
            this.nSExp = nSExp;
        }
    }
    
    [Serializable]
    public class LevelPoint
    {
        public int attackLevel;
        public int hpLevel;
        public int cpLevel;
        public int cdrLevel;
        public int sdLevel;
        public int ebrLevel;
        public int gbrLevel;
        public int dashAttackBonusLevel;
        public int debuffMonsterAttackLevel;
        public int debuffMonsterHpLevel;

        public LevelPoint(DbUserLevelPoint level)
        {
            Set(level);
        }
        public void Set(DbUserLevelPoint level)
        {
            attackLevel = level.AttackLevel.Value;
            hpLevel = level.HpLevel.Value;
            cpLevel = level.CriticalProbabilityLevel.Value;
            cdrLevel = level.CriticalAttackBonusLevel.Value;
            sdLevel = level.SkillDamageRateLevel.Value;
            ebrLevel = level.ExpBonusRateLevel.Value;
            gbrLevel = level.GoldBonusRateLevel.Value;
            dashAttackBonusLevel = level.DashAttackBonusLevel.Value;
            debuffMonsterAttackLevel = level.DebuffMonsterAttackLevel.Value;
            debuffMonsterHpLevel = level.DebuffMonsterHpLevel.Value;
        }

        [JsonConstructor]
        public LevelPoint(int attackLevel, int hpLevel, int cpLevel, int cdrLevel, int sdLevel, 
            int ebrLevel, int gbrLevel, int dashAttackBonusLevel, int debuffMonsterAttackLevel,
            int debuffMonsterHpLevel)
        {
            this.attackLevel = attackLevel;
            this.hpLevel = hpLevel;
            this.cpLevel = cpLevel;
            this.cdrLevel = cdrLevel;
            this.sdLevel = sdLevel;
            this.ebrLevel = ebrLevel;
            this.gbrLevel = gbrLevel;
            this.dashAttackBonusLevel = dashAttackBonusLevel;
            this.debuffMonsterAttackLevel = debuffMonsterAttackLevel;
            this.debuffMonsterHpLevel = debuffMonsterHpLevel;
        }
    }
    
    [Serializable]
    public class Stat
    {
        public int attackLevel;
        public int hpLevel;
        public int cpLevel;
        public int cdrLevel;
        public int attackBonusLevel;
        public int hpBonusLevel;
        public int bossAttackBonusLevel;

        public void Set(DbUserStat stat)
        {
            attackLevel = stat.AttackLevel.Value;
            hpLevel = stat.HpLevel.Value;
            cpLevel = stat.CriticalProbabilityLevel.Value;
            cdrLevel = stat.CriticalAttackBonusLevel.Value;
            attackBonusLevel = stat.AttackBonusLevel.Value;
            hpBonusLevel = stat.HpBonusLevel.Value;
            bossAttackBonusLevel = stat.BossAttackBonusLevel.Value;
        }
        public Stat(DbUserStat stat)
        {
            Set(stat);
        }

        [JsonConstructor]
        public Stat(int attackLevel, int hpLevel, int cpLevel, int cdrLevel, int attackBonusLevel, int hpBonusLevel, int bossAttackBonusLevel)
        {
            this.attackLevel = attackLevel;
            this.hpLevel = hpLevel;
            this.cpLevel = cpLevel;
            this.cdrLevel = cdrLevel;
            this.attackBonusLevel = attackBonusLevel;
            this.hpBonusLevel = hpBonusLevel;
            this.bossAttackBonusLevel = bossAttackBonusLevel;
        }
    }
}