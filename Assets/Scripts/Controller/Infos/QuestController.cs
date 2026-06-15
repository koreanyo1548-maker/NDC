using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Have;
using Controller.Play;
using Data;
using Data.DbRecord;
using Data.DbUser.Equipment;
using Data.DbUser.Progress;
using Data.Stores;
using Managers;
using ThirdParty;
using UIs.Toast;
using Utils;

namespace Controller.Infos
{
    public class QuestController : Singleton<QuestController>, IDayDiffChecker
    {
        public static DbUserMainQuest data = DbUserMainQuest.Get(0);

        public void Init()
        {
            SetInitQuestCount(data.ToDo);
            PlayFabManager.Store.DoWithTime(now =>
            {
                HandleDayDiff(now, 0);
            });
        }
        
        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            var prevReset = UserInfo.saved.info.questResetDate;
            dayDiff = Define.GetDayDiff(prevReset, now);
            
            var isNewWeek = Define.IsWeekDiff(prevReset, now, dayDiff);
            
            DbUserQuest.ForEach(q => 
            {
                if (dayDiff > 0 && DbQuest.Get(q.Id).Cycle == QuestCycleType.Daily)
                {
                    q.Count.Value = 0;
                    q.IsRewarded.Value = false;
                }

                if (isNewWeek && DbQuest.Get(q.Id).Cycle == QuestCycleType.Weekly)
                {
                    q.Count.Value = 0;
                    q.IsRewarded.Value = false;
                }
            });
            UserInfo.saved.info.questResetDate = now;
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Quest);
        }
        
        public Tuple<CurrencyType, int> GetReward(DbUserQuest quest)
        {
            quest.IsRewarded.Value = true;
            var meta = DbQuest.Get(quest.Id);

            var count = 1;
            if (meta.Cycle == QuestCycleType.Repeat)
            {
                count = quest.Count.Value / meta.Goal;
                quest.Count.Value -= meta.Goal * count;
                quest.IsRewarded.Value = false;
            }

            var amount = meta.RewardCount * count;
            
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            CurrencyController.I.GetReward(meta.Reward, amount);
            if (meta.Reward == CurrencyType.Dia) 
                CurrencyController.I.SetDiaLog($"퀘스트 {quest.Id} 보상 {count}번", amount, prev);
            
            CheckAndClear(QuestCycleType.Daily, QuestType.DailyQuestClear);
            CheckAndClear(QuestCycleType.Weekly, QuestType.WeeklyQuestClear);

            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Quest);
            return new Tuple<CurrencyType, int>(meta.Reward, amount);
            
            void CheckAndClear(QuestCycleType check, QuestType clear)
            {
                if (meta.Cycle == check) DoQuests(clear);
            }
        }

        public void GetMainQuestReward()
        {
            if (data.IsOnGuide.Value)
            {
                var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                CurrencyController.I.GetRewards(data.GuideMeta.Rewards);
                var diaReward = data.GuideMeta.Rewards.Find(r => r.currencyType == CurrencyType.Dia);
                if (diaReward != null) CurrencyController.I.SetDiaLog($"메인 퀘스트 {data.QuestId.Value} 보상", (int)diaReward.count, prev);
                       
                if (data.GuideMeta.GoTo != -1)
                {
                    SetInitQuestCount(DbMainQuest.Get(data.GuideMeta.GoTo).ToDo);
                    data.IsOnGuide.Value = false;
                    data.QuestId.Value = data.GuideMeta.GoTo;
                }
                else
                {
                    var nextQuest = DbGuideQuest.Get(data.QuestId.Value + 1);
                    SetInitQuestCount(nextQuest.ToDo);
                    data.QuestId.Value = nextQuest.Id;
                }
            }
            else
            {
                var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
                var reward = data.MainMeta;
                CurrencyController.I.GetReward(reward.RewardType, reward.RewardCounts);
                if (reward.RewardType == CurrencyType.Dia) CurrencyController.I.SetDiaLog($"메인 퀘스트 {data.QuestId.Value} 보상", reward.RewardCounts, prev);
                
                SetNextQuest();

                // while (data.MainMeta.HaveEnd &&
                //     data.MainMeta.Goal + data.LoopCount.Value * data.MainMeta.IncreasingGoal >
                //     data.MainMeta.MaxGoal)
                // {
                //     SetNextQuest();
                // }

                void SetNextQuest()
                {
                    var cur = data.MainMeta;
                    
                    var nextMainQuest = DbMainQuest.Get(data.QuestId.Value + 1);

                    if (nextMainQuest == null)
                    {
                        data.LoopCount.Value++;
                    }
                    
                    if (cur.GoToGuide.TryGetValue(data.LoopCount.Value, out var guide))
                    {
                        var nextQuest = DbGuideQuest.Get(guide);
                        SetInitQuestCount(nextQuest.ToDo);
                        data.IsOnGuide.Value = true;
                        data.QuestId.Value = nextQuest.Id;
                    }
                    else
                    {
                        if (nextMainQuest == null)
                        {
                            SetInitQuestCount(DbMainQuest.Get(11001).ToDo);
                            data.QuestId.Value = 11001;
                        }
                        else
                        {
                            SetInitQuestCount(nextMainQuest.ToDo);
                            data.QuestId.Value = nextMainQuest.Id;
                        }
                    }
                }
            }
            
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }

        public Dictionary<CurrencyType, int> GetAllReward(QuestCycleType questType)
        {
            var rewards = new Dictionary<CurrencyType, int>();
            DbUserQuest.ForEach(q => q.Meta.Cycle == questType, quest =>
            {
                if (!quest.CanRewarded) return;
                var reward = GetReward(quest);
                if (rewards.ContainsKey(reward.Item1)) rewards[reward.Item1] += reward.Item2;
                else rewards.Add(reward.Item1, reward.Item2);
            });
            return rewards;
        }

        public void DoQuests(QuestType toDo, int add = 1)
        {
            DbUserQuest.ForEach(q => q.ToDo == toDo, DoQuest);
            TitleController.I.DoQuests(toDo, add);
            SeasonPassController.I.DoQuests(toDo, add);
            NewbieQuestController.I.DoQuest(toDo, add);
            if (data.ToDo == toDo) DoMainQuest();
            
            void DoMainQuest()
            {
                if (data.CanRewarded) return;
                data.DoCount.Value = Math.Min(data.Goal, data.DoCount.Value + add);
                if (data.DoCount.Value == data.Goal)
                {
                    Manager.Guide.CheckCurQuest();
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200007);
                }
            }

            void DoQuest(DbUserQuest q)
            {
                var meta = q.Meta;
                var isRepeat = meta.Cycle == QuestCycleType.Repeat;
                if (!isRepeat)
                {
                    if (q.IsRewarded.Value) return;
                    if (q.Count.Value == meta.Goal) return;
                }
                
                var count = isRepeat ? q.Count.Value + add : Math.Min(meta.Goal, q.Count.Value + add);
                q.Count.Value = count;
            }
        }

        public void SetQuest(QuestType toDo, int count)
        {
            TitleController.I.SetQuests(toDo, count);
            NewbieQuestController.I.SetQuests(toDo, count);
            if (data.ToDo == toDo)
            {
                data.DoCount.Value = count;
                if (data.CanRewarded) Manager.Guide.CheckCurQuest();
            }
        }

        private void SetInitQuestCount(QuestType toDo)
        {
            data.DoCount.Value = GetInitQuestCount(toDo);
        }

        public int GetInitQuestCount(QuestType toDo)
        {
            switch (toDo)
            {
                case QuestType.CheckLevelUp: return LevelController.data.Level.Value;
                case QuestType.CheckLevelPoint: return LevelController.I.GetTotalLevelPointUse();
                case QuestType.CheckPromotion: return LevelController.data.Promotion.Value;
                case QuestType.CheckBibleLevel: return LevelController.data.BibleLevel.Value;
                
                case QuestType.CheckAttackLevel: return StatController.attack.Level.Value;
                case QuestType.CheckHpLevel: return StatController.hp.Level.Value;
                case QuestType.CheckCriticalProbabilityLevel: return StatController.criticalProbability.Level.Value;
                case QuestType.CheckCriticalAttackBonusLevel: return StatController.criticalAttackBonus.Level.Value;
                
                case QuestType.CheckStageClear: return LevelController.data.MaxStage.Value;
                case QuestType.CheckAutoSkillOn: return SettingController.data.IsAutoSkill.Value ? 1 : 0;
                case QuestType.CheckBuffAdWatch: return CurrencyController.I.IsBuffAdWatching() ? 1 : 
                    CurrencyController.I.Have(CurrencyType.AdSkip) ? 1 : 0;
                
                case QuestType.CheckWeaponEquip: return EquipController.data.Weapon.Value != 0 ? 1 : 0;
                case QuestType.CheckAccessoryEquip: return EquipController.data.Accessory.Value != 0 ? 1 : 0;
                case QuestType.CheckSkill1Equip: return EquipController.I.ASkillEquipped(0) ? 1 : 0;
                case QuestType.CheckSkill2Equip: return EquipController.I.ASkillEquipped(1) ? 1 : 0;
                case QuestType.CheckPetEquip: return EquipController.I.APetEquipped() ? 1 : 0;
                
                case QuestType.CheckWeaponGrowth: return WeaponController.I.GetTotalGrowthCount();
                case QuestType.CheckAccessoryGrowth: return AccessoryController.I.GetTotalGrowthCount();;

                case QuestType.CheckWeaponAwakening: return WeaponController.I.GetTotalAwakeningCount();
                case QuestType.CheckAccessoryAwakening: return AccessoryController.I.GetTotalAwakeningCount();
                case QuestType.CheckSkillAwakening: return SkillController.I.GetTotalAwakeningCount();
                case QuestType.CheckPetAwakening: return PetController.I.GetTotalAwakeningCount();
                case QuestType.CheckRelicLevel: return RelicController.I.GetTotalLevelCount();
                
                case QuestType.CheckAwakeningDungeonClear: return LevelController.data.AwakeningDungeonStage.Value - 1;
                case QuestType.CheckSkillDungeonClear: return LevelController.data.SkillGrowthDungeonStage.Value - 1;
                case QuestType.CheckPetDungeonClear: return LevelController.data.PetDungeonStage.Value - 1;
                
                case QuestType.CheckWeaponSummon: return (int)LevelController.data.WeaponSummonCount.Value;
                case QuestType.CheckAccessorySummon: return (int)LevelController.data.AccessorySummonCount.Value;
                case QuestType.CheckSkillSummon: return (int)LevelController.data.SkillSummonCount.Value;
                case QuestType.CheckRelicSummon: return (int)LevelController.data.RelicSummonCount.Value;
                case QuestType.CheckNecklaceSummon: return (int) LevelController.data.NecklaceSummonCount.Value;
                default: return 0;
            }
        }
    }
}