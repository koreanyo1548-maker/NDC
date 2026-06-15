using System;
using System.Collections.Generic;
using System.Numerics;
using Controller.Currency;
using Controller.Have;
using Controller.Play;
using Data;
using Data.DbCharacter;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbDungeon;
using Data.DbPetInfo;
using Data.DbPromote;
using Data.DbStage;
using Data.DbSummon;
using Data.DbUser.Equipment;
using Data.DbUser.Progress;
using Data.Stores;
using Data.Utils;
using Exceptions;
using Managers;
using MEC;
using ThirdParty;
using UIs.Dungeon.TrainingGround;
using UIs.FieldMain.MainStage;
using UIs.StageResult;
using UIs.Toast;
using UnityEngine;
using Utils;

namespace Controller.Infos
{
    public class LevelController: Singleton<LevelController>
    {
        public static DbUserLevel data = DbUserLevel.Get(0);
        
        public static DbStageLevel stageMeta = DbStageLevel.Get(data.Stage.Value);
        private static DbCharacterLevel nextLevelMeta = DbCharacterLevel.Get(data.Level.Value + 1);

        // public LevelController()
        // {
        //     CalculatePower(this, null);
        //     DbUserWeapon.ForEach(w => w.Power.ValueChanged += CalculatePower);
        //     DbUserWeapon.ForEach(w => w.Power.ValueChanged += CalculatePower);
        //     StatController.attack.Power.ValueChanged += CalculatePower;
        //     StatController.hp.Power.ValueChanged += CalculatePower;
        //     StatController.criticalProbability.Power.ValueChanged += CalculatePower;
        //     StatController.criticalAttackBonus.Power.ValueChanged += CalculatePower;
        //     LevelPointController.attack.Power.ValueChanged += CalculatePower;
        //     LevelPointController.hp.Power.ValueChanged += CalculatePower;
        //     LevelPointController.criticalProbability.Power.ValueChanged += CalculatePower;
        //     LevelPointController.criticalAttackBonus.Power.ValueChanged += CalculatePower;
        // }


        #region Stage Setting
        
        public void SetFirstStage()
        {
            if (SettingController.data.IsAutoProgress.Value)
            {
                if (data.IsStageClear.Value && data.Stage.Value < DbStageLevel.Count)
                {
                    stageMeta = DbStageLevel.Get(data.Stage.Value + 1);
                    data.Stage.Value++;
                    data.IsStageClear.Value = false;
                    // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
                }
            }
        }
        
        public void MoveStage(int toMove)
        {
            Manager.Field.GameOver(GameOverType.StageMove, 3.25f, FieldType.Stage);
            var nextMeta = DbStageLevel.Get(toMove);
            stageMeta = nextMeta;
            data.Stage.Value = toMove;
            data.IsStageClear.Value = toMove <= data.MaxStage.Value;
            
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }
        
        private void GoNextStage()
        {
            var nextMeta = SettingController.data.IsAutoProgress.Value ? GetNextStage() :
                stageMeta.StageType == StageType.Boss ? DbStageLevel.Get(stageMeta.Id + 1) : stageMeta;
            
            stageMeta = nextMeta;
            data.Stage.Value = nextMeta.Id;
            data.IsStageClear.Value = stageMeta.StageType == StageType.Pass || stageMeta.Id <= data.MaxStage.Value;
            
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }

       

        #endregion
        
        
        #region Stage Result
        
        public long GetMonsterKillReward()
        {
            var stageReward = (long)(DbStageReward.Get(data.Stage.Value).Exp * 
                                     TotalStatController.I.GetStat(StatType.StageExpEarn) * TotalStatController.I.GetStat(StatType.AbilityExpEarn) * 0.000001f);
            // Debug.Log("원래: " + DbStageReward.Get(data.Stage.Value).Exp + " 버프: " + TotalStatController.I.GetStat(StatType.StageExpEarn) + " > " + stageReward);
            
            data.Exp.Value += stageReward;
            
            Manager.UI.RewardLog.Add(CurrencyType.Exp, stageReward);
            return stageReward;
        }
        
        public bool OnStageFailed(FieldType failedType, GameOverType gameOverReason)
        {
            PlayController.I.useAdTicket = false;
            var realFail = (gameOverReason == GameOverType.StageFail && !data.IsStageClear.Value) ||
                           (gameOverReason == GameOverType.DungeonFail && failedType != FieldType.BlackMarket && failedType != FieldType.Dia);
            if (realFail) Timing.CallDelayed(2.25f, () => Manager.UI.ShowSingleUI<UI_StageFailed>().Set(failedType));

            switch (failedType)
            {
                case FieldType.Stage: 
                    FailStage();
                    break;
                case FieldType.Promotion: 
                case FieldType.Training:
                    break;
                case FieldType.Pet: 
                case FieldType.Awakening: 
                case FieldType.SkillGrowth:
                    break;
                case FieldType.BlackMarket:
                case FieldType.Dia:
                    if (gameOverReason != GameOverType.GiveUp) FailSequenceDungeon(failedType);
                    break;
                default: throw new NotDefinedFieldException(failedType);
            }

            if (failedType != FieldType.Stage && failedType != FieldType.Promotion) Manager.UI.OpenDungeonEntrance(failedType, true);
            return realFail;
            
            void FailStage()
            {
                // CurrencyController.I.CheckEventPackage(data.Stage.Value);
                if (gameOverReason == GameOverType.StageFail) SettingController.I.OnStageFailed();
                if (gameOverReason == GameOverType.StageMove) return;
                
                if (data.IsStageClear.Value || data.Stage.Value == 1 || stageMeta.StageType == StageType.Pass)
                {
                    data.Stage.Value = stageMeta.Id;
                }
                else
                {
                    var nextMeta = DbStageLevel.Get(data.Stage.Value - 1);
                    stageMeta = nextMeta;
                    data.Stage.Value = nextMeta.Id;
                    data.IsStageClear.Value = true;
                }
            }

            void FailSequenceDungeon(FieldType dungeon)
            {
                if (gameOverReason == GameOverType.StageMove) return;
                
                if (TryUseTicket(dungeon))
                {
                    var level = Manager.Field.CurStage;
                    var reward = DbSelector.GetReward(dungeon, level);
                    var rewards = reward.GetRewards().Clone(); 
                    rewards[0].count += PlayController.I.sequenceReward;
                    StatType bonusStatType;
                    switch (dungeon)
                    {
                        case FieldType.BlackMarket:
                        {
                            data.BlackMarketDungeonReward.Value = Math.Max(rewards[0].count, data.BlackMarketDungeonReward.Value);
                            bonusStatType = StatType.BlackMarketDungeonEarn; break;
                        }
                        case FieldType.Dia:
                        {
                            data.DiaDungeonReward.Value = Math.Max(rewards[0].count, data.DiaDungeonReward.Value);
                            bonusStatType = StatType.DiaDungeonEarn; break;
                        }
                        default: throw new NotDefinedFieldException(dungeon);
                    }

                    rewards[0].count = rewards[0].count * TotalStatController.I.GetStat(bonusStatType) / 100;
                    PlayController.I.sequenceReward = 0;

                    var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                    CurrencyController.I.GetRewards(rewards);
                    if (dungeon == FieldType.Dia) CurrencyController.I.SetDiaLog($"다이아 던전 {level} 보상", rewards[0].count, prev);
                    
                    SetCurStage(dungeon, Manager.Field.CurStage);
                     
                    Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210215, rewards, false);
                    QuestController.I.DoQuests(QuestType.DungeonClear);
                    if (dungeon == FieldType.BlackMarket) QuestController.I.DoQuests(QuestType.BlackMarketDungeonClear);
                }
            }
        }

        private bool TryUseTicket(FieldType field)
        {
            var dungeonMeta = DbDungeonMeta.Get(field);
            return CurrencyController.I.TryUse(dungeonMeta.Use, 1);
        }
        public void OnStageClearAll()
        {
#if CHEAT
            // 기존
            int val = data.Stage.Value;

            // 스테이지
            var nextMeta = DbStageLevel.Get(7999);
            stageMeta = nextMeta;
            data.Stage.Value = nextMeta.Id;
            data.MaxStage.Value = nextMeta.Id;
            data.IsStageClear.Value = stageMeta.StageType == StageType.Pass || stageMeta.Id <= data.MaxStage.Value;

            data.AwakeningDungeonStage.Value = 300;
            data.SkillGrowthDungeonStage.Value = 300;
            data.PetDungeonStage.Value = 300;
            data.BlackMarketDungeonStage.Value = 300;
            data.DiaDungeonStage.Value = 300;

            // 던전오픈까지 퀘스트 조건
            QuestController.data.QuestId.Value = 10034;
            QuestController.data.IsOnGuide.Value = true;
            QuestController.data.DoCount.Value = 0;
            QuestController.data.LoopCount.Value = 0;

            // 재화
            CurrencyController.data.Money[CurrencyType.Dia].Value = 9999999;
            CurrencyController.data.Money[CurrencyType.Gold].Value = 9999999999999;
            CurrencyController.data.Money[CurrencyType.Mileage].Value = 9999999;
            CurrencyController.data.Money[CurrencyType.BlackMarketCoin].Value = 9999999;

            CurrencyController.data.Tickets[CurrencyType.SkillGrowthDungeonTicket].Value = 999;
            CurrencyController.data.Tickets[CurrencyType.AwakeningDungeonTicket].Value = 999;
            CurrencyController.data.Tickets[CurrencyType.PetDungeonTicket].Value = 999;
            CurrencyController.data.Tickets[CurrencyType.BlackMarketDungeonTicket].Value = 999;
            CurrencyController.data.Tickets[CurrencyType.DiaDungeonTicket].Value = 999;

            // 장비
            UserInfo.saved.equipment.weapons.ForEach(x => {
                var val = DbUserWeapon.Get(x.id);
                val.Count.Value = 99;
                val.Growth.Value = 99;
                val.Awakening.Value = 7;
                val.Have.Value = true;
            });
            UserInfo.saved.equipment.accessories.ForEach(x => {
                var val = DbUserAccessory.Get(x.id);
                val.Count.Value = 99;
                val.Growth.Value = 99;
                val.Awakening.Value = 7;
                val.Have.Value = true;
            });
            UserInfo.saved.equipment.necklaces.ForEach(x => {
                var val = DbUserNecklace.Get(x.id);
                val.Count.Value = 99;
                val.Growth.Value = 99;
                val.Awakening.Value = 7;
                val.Have.Value = true;
            });

            // 스킬
            UserInfo.saved.equipment.skills.ForEach(x => {
                var val = DbUserSkill.Get(x.id);
                val.Count.Value = 99;
                val.Growth.Value = 99;
                val.Awakening.Value = 5;
                val.Have.Value = true;
            });

            // 펫
            UserInfo.saved.equipment.pets.ForEach(x => PetController.I.Add(x.id, 99));

            // 유물
            UserInfo.saved.relic.ForEach(x => {
                var relic = DbUserRelic.Get(x.id);
                relic.Count.Value = 99;
                relic.Level.Value = 100;
            });

            // 코스튬
            CurrencyController.I.AddCostumeAll();

            // 스테이지 현재껄로 이동
            Timing.CallDelayed(1f, () => MoveStage(val));
#endif
        }
        public float OnStageClear(FieldType clearType, int clearStage)
        { 
            var waitTime = -1f;

            if (clearType == FieldType.Stage) ClearField();
            else if (clearType == FieldType.Awakening) OnClearDungeon(clearType);
            else if (clearType == FieldType.SkillGrowth) OnClearDungeon(clearType);
            else if (clearType == FieldType.Promotion) ClearPromotion();
            else if (clearType == FieldType.Pet) OnClearDungeon(clearType);
            else if (clearType == FieldType.Training) ClearTraining();
            else throw new NotDefinedFieldException(clearType);
            GoNextStage();

            if (clearType != FieldType.Stage && clearType != FieldType.Promotion) Manager.UI.OpenDungeonEntrance(clearType, true);
            
            return waitTime;

            void ClearField()
            {
                // CurrencyController.I.CheckEventPackage(data.Stage.Value);
                var rewards = new List<UIReward>();
                var isFirstClear = data.Stage.Value > data.MaxStage.Value;
                if (isFirstClear)
                {
                    var stageReward = DbStageReward.Get(data.Stage.Value);
                    
                    var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                    CurrencyController.I.GetReward(stageReward.FirstClearReward, stageReward.FirstClearRewardCounts);
                    CurrencyController.I.SetDiaLog($"스테이지 {data.Stage.Value} 첫클리어 보상", stageReward.FirstClearRewardCounts, prev);
                    rewards.Add(new UIReward(DbCurrency.Get(stageReward.FirstClearReward), stageReward.FirstClearRewardCounts));

                    var nextStage = DbStageLevel.Get(data.Stage.Value + 1);
                    data.MaxStage.Value = nextStage != null && nextStage.StageType == StageType.Pass ? data.Stage.Value + 1 : data.Stage.Value;
                
                    data.IsStageClear.Value = true;
                    QuestController.I.SetQuest(QuestType.CheckStageClear, data.Stage.Value);
                    Manager.UI.ShowSingleUI<UI_StageClear>().Set(rewards);
                }
                 
                if (SettingController.data.IsAutoProgress.Value)
                {
                    if (!isFirstClear) waitTime = 2.5f;
                    // GoNextStage(isFirstClear ? -1 : 2);
                }
                // else if (stageMeta.StageType == StageType.Boss)
                // { 
                //     if (isFirstClear) waitTime = 2; 
                //     // GoNextStage(isFirstClear ? -1 : 2);
                // }
            }
            
            void OnClearDungeon(FieldType dungeon)
            {
                if (TryUseTicket(dungeon))
                { 
                    var reward = DbSelector.GetReward(dungeon, clearStage);
                    var rewards = reward.GetRewards().Clone();
                    
                    StatType bonusStatType;
                    switch (dungeon)
                    {
                        case FieldType.SkillGrowth: bonusStatType = StatType.SkillGrowthDungeonEarn; break;
                        case FieldType.Awakening : bonusStatType = StatType.AwakeningDungeonEarn; break;
                        case FieldType.Pet: case FieldType.Promotion: bonusStatType = StatType.None; break;
                        default: throw new NotDefinedFieldException(dungeon);
                    }

                    if (bonusStatType != StatType.None)
                    {
                        foreach (var r in rewards)
                        {
                            r.count = r.count * TotalStatController.I.GetStat(bonusStatType) / 100;
                        }
                    }

                    CurrencyController.I.GetRewards(rewards);
                    if (GetCurStage(dungeon) <= clearStage)
                    {
                        var firstRewards = reward.GetFirstClearReward();
                        var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                        CurrencyController.I.GetRewards(firstRewards);
                        var diaReward = firstRewards.Find(r => r.currencyType == CurrencyType.Dia);
                        if (diaReward != null) CurrencyController.I.SetDiaLog($"{dungeon} 던전 {clearStage} 첫클리어 보상", (int)diaReward.count, prev);
                        rewards.AddRange(firstRewards);
                        SetCurStage(dungeon, clearStage + 1);
                    }
                     
                    Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210216, rewards, true);

                    QuestController.I.DoQuests(QuestType.DungeonClear);
                    QuestController.I.SetQuest(GetQuestType(), GetCurStage(dungeon) - 1);
                    DoClearDungeonQuest(dungeon, 1);
                }
                 
                // GoNextStage(-1);

                
                QuestType GetQuestType()
                {
                    if (dungeon == FieldType.Awakening) return QuestType.CheckAwakeningDungeonClear;
                    if (dungeon == FieldType.Pet) return QuestType.CheckPetDungeonClear;
                    if (dungeon == FieldType.SkillGrowth) return QuestType.CheckSkillDungeonClear;
                    throw new NotDefinedFieldException(dungeon);
                }
            }

            void ClearPromotion() 
            {
                data.Promotion.Value++;
                CurrencyController.I.AddCostume(data.Promotion.Value);
                PlayController.I.OnStageRefreshed();
                TotalStatController.I.Apply(StatType.FinalAttackBonus);
                TotalStatController.I.Apply(StatType.FinalHpBonus); 
                //GoNextStage(-1);
                 
                Manager.UI.ShowSingleUI<UI_PromotionClear>().Set();
                QuestController.I.SetQuest(QuestType.CheckPromotion, data.Promotion.Value);
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
            }

            void ClearTraining()
            {
                var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value; 
                var level = DbTrainingGroundLevel.GetLevelOfDamage(PlayController.I.damage.Value);
                var prevLevel = data.TrainingGroundStage.Value;
                var prevDamage = data.MaxTraining.Value;
                var curDamage = PlayController.I.damage.Value;
                PlayController.I.damage.Value = 0;
                if (curDamage > prevDamage)
                {
                    data.MaxTraining.Value = curDamage;
                    PlayFabManager.Leaderboard.UpdateTraining(); 
                }
                var rewards = new List<DbReward>();
                if (level > prevLevel)
                {
                    for (var idx = prevLevel + 1; idx <= level; ++idx)
                    {
                        var reward = DbTrainingGroundReward.Get(idx);
                        CurrencyController.I.GetReward(reward.RewardType, reward.RewardCount, reward.RewardId);
                        var sameTypeReward = rewards.Find(r => r.currencyType == reward.RewardType && r.id == reward.RewardId);
                        if (sameTypeReward != null) sameTypeReward.count += reward.RewardCount;
                        else rewards.Add(new(reward.RewardType, reward.RewardCount, reward.RewardId));
                    }
                    SetCurStage(FieldType.Training, level);
                }

                var diaReward = rewards.Find(r => r.currencyType == CurrencyType.Dia);
                if (diaReward != null)
                {
                    CurrencyController.I.SetDiaLog("훈련장 보상", diaReward.count, prev);
                }
                Manager.UI.GetSceneUI<UI_MainStage>().RemoveTrainingStage();
                Manager.UI.ShowSceneUI<UI_TrainingGroundClear>().Set(rewards, prevLevel, level, prevDamage, curDamage);
                waitTime = 2.5f;
                
                QuestController.I.DoQuests(QuestType.TrainingGroundClear);
            }
        }
        
        
        private void DoClearDungeonQuest(FieldType dungeon, int count)
        {
            if (dungeon == FieldType.Awakening) QuestController.I.DoQuests(QuestType.AwakeningDungeonClear, count);
            else if (dungeon == FieldType.Pet) QuestController.I.DoQuests(QuestType.PetDungeonClear, count);
            else if (dungeon == FieldType.BlackMarket) QuestController.I.DoQuests(QuestType.BlackMarketDungeonClear, count);
        }
#endregion
        
        
        #region User Level & Exp
        
        public float ExpPerNeed()
        {
            return (float)data.Exp.Value / (float)nextLevelMeta.NeedExp;
        }

        public bool CanLevelUp()
        {
            return data.Exp.Value >= nextLevelMeta.NeedExp;
        }
        
        public void LevelUp()
        {
            if (data.Exp.Value >= nextLevelMeta.NeedExp && DbCharacterLevel.Get(data.Level.Value + 1) is {} nextMeta)
            {
                data.Exp.Value -= nextLevelMeta.NeedExp;
                nextLevelMeta = nextMeta;
                data.Level.Value++;
                CurrencyController.I.GetReward(nextLevelMeta.Reward, nextLevelMeta.RewardCount);
                
                QuestController.I.SetQuest(QuestType.CheckLevelUp, data.Level.Value);
                QuestController.I.DoQuests(QuestType.LevelUp);
                
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
            }
        }

        public int GetTotalLevelPointUse()
        {
            return data.Level.Value - CurrencyController.I.GetEtcModel(CurrencyType.LevelPoint).Value;
        }

        public int GetLevelPointDiff()
        {
            var sum = LevelPointController.attack.Level.Value + LevelPointController.hp.Level.Value +
                      LevelPointController.criticalProbability.Level.Value + LevelPointController.criticalAttackBonus.Level.Value + 
                      LevelPointController.stageExpEarn.Level.Value + LevelPointController.stageGoldEarn.Level.Value + 
                      LevelPointController.dashAttackBonus.Level.Value + LevelPointController.debuffMonsterAttack.Level.Value +
                      LevelPointController.debuffMonsterHp.Level.Value;
            return GetTotalLevelPointUse() - sum;
        }
        
        #endregion
       
        
        #region Summon Level
        
        public DbSummonLevel GetNextSummonLevelMeta(SummonType summonType)
        {
            var curLevel = GetSummonLevel(summonType);

            if (summonType == SummonType.Accessory || summonType == SummonType.Weapon)
            {
                if (curLevel >= 10) return DbSummonLevel.Get(10);
            }
            if (curLevel+1 > DbSummonLevel.Count) return DbSummonLevel.Get(DbSummonLevel.Count);
            
            return DbSummonLevel.Get(curLevel + 1);
        }

        private DbSummonLevel GetNextSummonRewardMeta(SummonType summonType)
        {
            var curLevel = GetSummonReward(summonType);

            if (summonType == SummonType.Accessory || summonType == SummonType.Weapon)
            {
                if (curLevel >= 10) return DbSummonLevel.Get(10);
            }
            if (curLevel+1 > DbSummonLevel.Count) return DbSummonLevel.Get(DbSummonLevel.Count);
            
            var next = DbSummonLevel.Get(curLevel + 1);
            if (next.NeedExp == 0) return DbSummonLevel.Get(curLevel);
            return DbSummonLevel.Get(curLevel + 1);
        }
        
        public int GetSummonLevelForMeta(SummonType summonType)
        {
            var max = summonType == SummonType.Weapon || summonType == SummonType.Accessory ? 10 : DbSummonLevel.Count;
            return Math.Min(max, GetSummonLevel(summonType));
        }
        
        public int GetSummonLevel(SummonType summonType)
        {
            switch (summonType)
            {
                case SummonType.Weapon: return data.WeaponSummonLevel.Value;
                case SummonType.Accessory: return data.AccessorySummonLevel.Value;
                case SummonType.Skill: return data.SkillSummonLevel.Value;
                case SummonType.Necklace: return data.NecklaceSummonLevel.Value;
                case SummonType.Relic: return 1;
                default : throw new Exception($"summonType {summonType} is not defined");
            }
        }

        private int GetSummonReward(SummonType summonType)
        {
            switch (summonType)
            {
                case SummonType.Weapon: return data.WeaponSummonReward.Value;
                case SummonType.Accessory: return data.AccessorySummonReward.Value;
                case SummonType.Skill: return data.SkillSummonReward.Value;
                default : throw new Exception($"summonType {summonType} is not defined");
            }
        }
        public BigInteger GetSummonExp(SummonType summonType)
        {
            switch (summonType)
            {
                case SummonType.Weapon: return data.WeaponSummonExp.Value;
                case SummonType.Accessory: return data.AccessorySummonExp.Value;
                case SummonType.Skill: return data.SkillSummonExp.Value;
                case SummonType.Necklace: return data.NecklaceSummonExp.Value;
                default : throw new Exception($"summonType {summonType} is not defined");
            }
        }
        
        public void AddSummonExp(SummonType summonType, int count, int exp)
        {
            switch (summonType)
            {
                case SummonType.Weapon:
                    data.WeaponSummonCount.Value += count;
                    data.WeaponSummonExp.Value += exp;
                    QuestController.I.SetQuest(QuestType.CheckWeaponSummon, (int)data.WeaponSummonCount.Value);
                    break;
                case SummonType.Accessory:
                    data.AccessorySummonCount.Value += count;
                    data.AccessorySummonExp.Value += exp;
                    QuestController.I.SetQuest(QuestType.CheckAccessorySummon, (int)data.AccessorySummonCount.Value);
                    break;
                case SummonType.Skill:
                    data.SkillSummonCount.Value += count;
                    data.SkillSummonExp.Value += exp;
                    QuestController.I.SetQuest(QuestType.CheckSkillSummon, (int)data.SkillSummonCount.Value);
                    break;
                case SummonType.Necklace:
                    data.NecklaceSummonCount.Value += count;
                    data.NecklaceSummonExp.Value += exp;
                    QuestController.I.SetQuest(QuestType.CheckNecklaceSummon, (int)data.NecklaceSummonCount.Value);
                    break;
                case SummonType.Relic:
                    data.RelicSummonCount.Value += count;
                    QuestController.I.SetQuest(QuestType.CheckRelicSummon, (int)data.RelicSummonCount.Value);
                    return;
            }

            while (true)
            {
                var next = GetNextSummonLevelMeta(summonType);
                switch (summonType)
                {
                    case SummonType.Weapon: 
                        if (data.WeaponSummonExp.Value >= next.NeedExp)
                        {
                            data.WeaponSummonLevel.Value++;
                            data.WeaponSummonExp.Value -= next.NeedExp;
                            break;
                        }
                        return;
                    case SummonType.Accessory:
                        if (data.AccessorySummonExp.Value >= next.NeedExp)
                        {
                            data.AccessorySummonLevel.Value++;
                            data.AccessorySummonExp.Value -= next.NeedExp;
                            break;
                        }
                        return;
                    case SummonType.Skill:
                        if (data.SkillSummonExp.Value >= next.SkillNeedExp)
                        {
                            data.SkillSummonLevel.Value++;
                            data.SkillSummonExp.Value -= next.SkillNeedExp;
                            break;
                        }
                        return;
                    case SummonType.Necklace:
                        if (next.NecklaceNeedExp == 0) return;
                        if (data.NecklaceSummonExp.Value >= next.NecklaceNeedExp)
                        {
                            data.NecklaceSummonLevel.Value++;
                            data.NecklaceSummonExp.Value -= next.NecklaceNeedExp;
                            break;
                        }
                        return;
                }
            }
        }
        
        public bool CanAnyGetSummonReward()
        {
            return CanGetSummonReward(SummonType.Accessory) || CanGetSummonReward(SummonType.Weapon) ||
                   CanGetSummonReward(SummonType.Skill);
        }

        public bool CanGetSummonReward(SummonType summonType)
        {
            return GetSummonReward(summonType) < GetSummonLevel(summonType);
        }

        public void GetSummonLevelReward(SummonType summonType)
        {
            var next = GetNextSummonRewardMeta(summonType);
            var rewardsForToast = new List<DbReward>();
            switch (summonType)
            {
                case SummonType.Weapon:
                    var weaponReward = next.WeaponId == 0 ? CurrencyType.WeaponGrowthStone : CurrencyType.Weapon;
                    CurrencyController.I.GetReward(weaponReward, next.WeaponCount, next.WeaponId);
                    rewardsForToast.Add(new DbReward(weaponReward, next.WeaponCount, next.WeaponId));
                    data.WeaponSummonReward.Value++;
                    break;
                case SummonType.Accessory:
                    var accessoryReward = next.AccessoryId == 0 ? CurrencyType.AccessoryGrowthStone : CurrencyType.Accessory;
                    CurrencyController.I.GetReward(accessoryReward, next.AccessoryCount, next.AccessoryId);
                    rewardsForToast.Add(new DbReward(accessoryReward, next.AccessoryCount, next.AccessoryId));
                    data.AccessorySummonReward.Value++;
                    break;
                case SummonType.Skill:
                    var skillReward = next.SkillId == 0 ? CurrencyType.SkillGrowthStone : CurrencyType.Accessory;
                    CurrencyController.I.GetReward(skillReward, next.SkillCount, next.SkillId);
                    rewardsForToast.Add(new DbReward(skillReward, next.SkillCount, next.SkillId));
                    data.SkillSummonReward.Value++;
                    break;
                default: throw new NotDefinedSummonException(summonType);
            }
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            toast.SetReward(210211, rewardsForToast);
        }
        
        #endregion
       
        
        #region Bible Level

        public void BibleLevelUp()
        {
            data.BibleLevel.Value++;
            TotalStatController.I.Apply(StatType.Attack);
            TotalStatController.I.Apply(StatType.Hp);
            QuestController.I.SetQuest(QuestType.CheckBibleLevel, data.BibleLevel.Value);
            PlayFabManager.Store.ForceSave();
        }

        public long GetBibleHp()
        {
            var bonus = 100 + EquipController.I.GetPetHpBonus();
            return DbBibleLevel.Get(data.BibleLevel.Value).Hp * bonus / 100;
        }
        
        #endregion
        
        
        #region StageInfo

        public DbStageLevel GetNextStage()
        { 
            if (DbStageLevel.Get(data.MaxStage.Value + 1) is { } nextMeta)
            {
                if (SettingController.data.IsAutoProgress.Value && nextMeta.StageType == StageType.Pass)
                {
                    nextMeta = DbStageLevel.Get(data.MaxStage.Value + 2);
                    if (nextMeta == null) return stageMeta;
                    return nextMeta;
                }

                return nextMeta;
            }

            if (data.MaxStage.Value == DbStageLevel.Count)
            {
                SettingController.data.IsAutoProgress.Value = false;
                return DbStageLevel.Get(data.MaxStage.Value);
            }
            return stageMeta;
        }

        public int GetCurStage(FieldType dungeon)
        {
            return dungeon switch
            {
                FieldType.Awakening => data.AwakeningDungeonStage.Value,
                FieldType.SkillGrowth => data.SkillGrowthDungeonStage.Value,
                FieldType.Pet => data.PetDungeonStage.Value,
                FieldType.BlackMarket => data.BlackMarketDungeonStage.Value,
                FieldType.Dia => data.DiaDungeonStage.Value,
                _ => throw new NotDefinedFieldException(dungeon)
            };
        }
        
        public void SetCurStage(FieldType dungeon, int stage)
        {
            switch (dungeon)
            {
                case FieldType.Awakening:
                    data.AwakeningDungeonStage.Value = stage;
                    break;
                case FieldType.SkillGrowth:
                    data.SkillGrowthDungeonStage.Value = stage;
                    break;
                case FieldType.Pet:
                    data.PetDungeonStage.Value = stage;
                    break;
                case FieldType.BlackMarket:
                    data.BlackMarketDungeonStage.Value = stage;
                    break;
                case FieldType.Dia:
                    data.DiaDungeonStage.Value = stage;
                    break;
                case FieldType.Training:
                    data.TrainingGroundStage.Value = stage;
                    break;
                default:
                    throw new NotDefinedFieldException(dungeon);
            }
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }
        
        public bool GetIsStageClear()
        {
            var type = Manager.Field.CurField.Value;
            switch (type)
            {
                case FieldType.Stage:
                    return data.Stage.Value <= data.MaxStage.Value || data.IsStageClear.Value;
                case FieldType.Awakening:
                case FieldType.SkillGrowth:
                case FieldType.Promotion:
                case FieldType.Pet:
                case FieldType.BlackMarket:
                case FieldType.Dia:
                case FieldType.Training:
                    return false;
                default:
                    throw new NotDefinedFieldException(type);
            }
        }
        
        #endregion
        
        
        #region Lock
        
        
        public bool CheckIsLocked(DbLock lockMeta)
        {
            switch (lockMeta.Condition)
            {
                case LockConditionType.Promotion: return data.Promotion.Value < lockMeta.Goal;
                case LockConditionType.Stage: return data.MaxStage.Value < lockMeta.Goal;
                case LockConditionType.GuideQuest:
                    return QuestController.data.IsOnGuide.Value && QuestController.data.QuestId.Value < lockMeta.Goal;
                case LockConditionType.BibleLevel: return data.BibleLevel.Value < lockMeta.Goal;
                case LockConditionType.NewbieDay: return NewbieQuestController.I.CurDay.Value < lockMeta.Goal;
                default: throw new NotDefinedLockConditionException(lockMeta.Condition);
            }
        }

        public DbField[] GetUpdatedFieldForLock(DbLock lockMeta)
        {
            switch (lockMeta.Condition)
            {
                case LockConditionType.Promotion: return new[]{ data.Promotion };
                case LockConditionType.Stage: return new[]{ data.MaxStage};
                case LockConditionType.BibleLevel: return new[]{data.BibleLevel};
                case LockConditionType.GuideQuest: return new[]{QuestController.data.QuestId};
                case LockConditionType.NewbieDay: return new[] {NewbieQuestController.I.CurDay};
                default: throw new Exception($"{lockMeta.Condition} is not defined lock type");
            }
        }
        
        public bool CheckIsLocked(StatConditionType condition, int goal)
        {
            switch (condition)
            {
                case StatConditionType.None: return false;
                case StatConditionType.AttackLevel: return StatController.attack.Level.Value < goal;
                case StatConditionType.HpLevel: return StatController.hp.Level.Value < goal;
                case StatConditionType.Stage: return data.MaxStage.Value < goal;
                case StatConditionType.Level: return data.Level.Value < goal;
                default: throw new Exception($"{condition} is not defined gold stat lock type");
            }
        }

        public DbField[] GetUpdatedFieldForLock(StatConditionType condition)
        {
            switch (condition)
            {
                case StatConditionType.AttackLevel: return new[]{StatController.attack.Level};
                case StatConditionType.HpLevel: return new[]{StatController.hp.Level};
                case StatConditionType.Stage: return new[]{data.MaxStage};
                case StatConditionType.Level: return new[]{data.Level};
                default: throw new Exception($"{condition} is not defined gold stat lock type");
            }
        }
        
        #endregion


        #region  Dungeon Clear

        public bool ClearDungeon(DbDungeonMeta dungeon, CurrencyType use, int count, int stage)
        {
            if (CurrencyController.I.TryUse(use, count))
            {
                List<DbReward> rewards = new();
                if (dungeon.Id == FieldType.Pet)
                {
                    for (var idx = 0; idx < count; ++idx)
                    {
                        var addedRewards = DbSelector.GetReward(dungeon.Id, stage).GetRewards().Clone();
                        for (var jdx = 0; jdx < addedRewards.Count; ++jdx)
                        {
                            if (rewards.Exists(r => r.currencyType == addedRewards[jdx].currencyType))
                            {
                                rewards.Find(r => r.currencyType == addedRewards[jdx].currencyType).count += addedRewards[jdx].count;
                            }
                            else rewards.Add(addedRewards[jdx]);
                        }
                    }
                }
                else if (dungeon.Id == FieldType.Dia || dungeon.Id == FieldType.BlackMarket)
                {
                    rewards = DbSelector.GetReward(dungeon.Id, stage).GetRewards().Clone();
                    var rewardAmount = 0L;
                    switch (dungeon.Id)
                    {
                        case FieldType.BlackMarket:
                        {
                            rewards[0].count = rewards[0].count * 
                                TotalStatController.I.GetStat(StatType.BlackMarketDungeonEarn) * count / 100;
                            break;
                        }
                        case FieldType.Dia:
                        {
                            rewards[0].count = data.DiaDungeonReward.Value *
                            TotalStatController.I.GetStat(StatType.DiaDungeonEarn) * count / 100;
                            break;
                        }
                        default: throw new NotDefinedFieldException(dungeon.Id);
                    }
                }
                else
                {
                    var bonus = dungeon.Id == FieldType.Awakening ? TotalStatController.I.GetStat(StatType.AwakeningDungeonEarn) :
                        TotalStatController.I.GetStat(StatType.SkillGrowthDungeonEarn);
                    rewards = DbSelector.GetReward(dungeon.Id, stage).GetRewards().Clone();
                    foreach (var reward in rewards)
                    {
                        reward.count = reward.count * count * bonus / 100;
                    }
                }

                var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                CurrencyController.I.GetRewards(rewards);
                if (dungeon.Id == FieldType.Dia) CurrencyController.I.SetDiaLog($"다이아 던전 {stage} 소탕 {count}번 보상", rewards[0].count, prev);
                Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210217, rewards);
                QuestController.I.DoQuests(QuestType.DungeonClear, count);
                DoClearDungeonQuest(dungeon.Id, count);
                return true;
            }

            return false;
        }

        #endregion

    }
}