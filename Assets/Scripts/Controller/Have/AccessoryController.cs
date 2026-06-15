using System.Collections.Generic;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbEquipment;
using Data.DbStage;
using Data.DbSummon;
using Data.DbUser.Equipment;
using Managers;
using ThirdParty;
using Utils;
using Random = UnityEngine.Random;

namespace Controller.Have
{
    public class AccessoryController : Singleton<AccessoryController>
    {
        public bool AnyThingToMerge()
        {
            return DbUserAccessory.HaveAny(accessory => accessory.CanMerge());
        }
        
        public void MergeAll()
        {
            var log = string.Empty;
            DbUserAccessory.ForEach(MergeToMax);
            PlayFabManager.Store.SetLog(log);

            void MergeToMax(DbUserAccessory a)
            {
                // if (!SettingController.data.AccessoryMergeAll.Value && a.Awakening.Value < 7) return;
                if (a.Meta.FullGrade == FullGradeType.Mythic5) return;
                
                var mergeCount = a.Count.Value / 5;
                if (mergeCount < 1) return;
                
                var merged = DbUserAccessory.Get(a.Id+1);
                var prev = merged.Count.Value;
                merged.Count.Value += mergeCount;
                a.Count.Value -= 5 * mergeCount;

                QuestController.I.DoQuests(QuestType.AccessoryMerge, mergeCount);
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Accessory);
                log = $"[장신구합성] {a.Id}를 {mergeCount}개 합성 {merged.Id}가 {prev}개 => {merged.Count.Value}개    |    ";
            }
        }
        
        public List<IDbCanSummon> AddRandom(int count)
        {
            var totalCount = count;
            var added = new Dictionary<FullGradeType, int>();
            var result = new List<IDbCanSummon>();
            var prMeta = DbSummonGradeProbability.Get(LevelController.I.GetSummonLevelForMeta(SummonType.Accessory));
            while (count-- > 0)
            {
                var pick = FullGradeType.Normal1;
                var gradeChoose = Random.Range(0, 1000000);

                var pr = 0;
                while (gradeChoose >= pr)
                {
                    pr += prMeta.GetPr(pick);
                    pick += 5;
                }
                var add = pick-5;

                var pickNumber = 0;
                pr = 0;
                var numberPr = DbSummonNumberProbability.Get(GetGradeType(add)).Probability;
                var numberChoose = Random.Range(0, 1000000);
                while (numberChoose >= pr)
                {
                    pr += numberPr[pickNumber];
                    pickNumber++;
                }

                add += pickNumber - 1;

                if (added.ContainsKey(add)) added[add]++;
                else added.Add(add, 1);
                result.Add(DbAccessory.Get(a => a.FullGrade == add));
            }

            foreach (var add in added)
            {
                var a = DbUserAccessory.Get(w => w.Meta.FullGrade == add.Key);
                a.Count.Value += add.Value;
            }
            QuestController.I.DoQuests(QuestType.AccessorySummon, totalCount);
            return result;

            GradeType GetGradeType(FullGradeType grade)
            {
                if (grade >= FullGradeType.Mythic1) return GradeType.Mythic;
                if (grade >= FullGradeType.Legendary1) return GradeType.Legendary;
                if (grade >= FullGradeType.Heroic1) return GradeType.Heroic;
                if (grade >= FullGradeType.Unique1) return GradeType.Unique;
                if (grade >= FullGradeType.Rare1) return GradeType.Rare;
                if (grade >= FullGradeType.Magic1) return GradeType.Magic;
                return GradeType.Normal;
            }
        }
        
        public void Add(int accessoryId, int count)
        {
            DbUserAccessory.Get(accessoryId).Count.Value += count;
        }
        

        public bool AAccessoryAwakened()
        {
            var awakened = false;
            DbUserAccessory.ForEach(w =>
            {
                if (w.Awakening.Value > 0) awakened = true;
            });
            return awakened;
        }
        public long GetMonsterKillReward()
        {
            var stageReward = DbStageReward.Get(LevelController.data.Stage.Value);

            var haveReward = stageReward.Accessories.Count > 0 && Random.Range(0, 100) < stageReward.AccessoryProbability * TotalStatController.I.GetStat(StatType.StageItemRate) * 0.01f;
            if (haveReward)
            {
                var reward = stageReward.Accessories[Random.Range(0, stageReward.Accessories.Count)];
                
                var a = DbUserAccessory.Get(reward);
                a.Count.Value += 1;
                Manager.UI.RewardLog.Add(CurrencyType.Accessory, 1, a.Id);
                return 1;
            }

            return 0;
        }
        
        public int GetTotalAwakeningCount()
        {
            var count = 0;
            DbUserAccessory.ForEach(a => count += a.Awakening.Value);
            return count;
        }

        public int GetTotalGrowthCount()
        {
            var count = 0;
            DbUserAccessory.ForEach(a => count += (a.Growth.Value > 0 ? a.Growth.Value-1 : 0));
            return count;
        }
        // public int GetPower()
        // {
        //     var sum = 0;
        //     DbUserAccessory.ForEach(w => sum += w.Power.Value);
        //     return sum;
        // }
    }
}