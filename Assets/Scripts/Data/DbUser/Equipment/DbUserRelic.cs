using System.Collections.Generic;
using System.Numerics;
using Controller.Currency;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Data.DbDefinition;
using Data.DbRelicInfo;
using Data.Utils;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;

namespace Data.DbUser.Equipment
{
    public class DbUserRelic: DbUserModel<DbUserRelic, int>
    {
        public DbField<BigInteger> Count { get; private set; }
        public DbField<int> Level { get; private set; }
        
        public override void Set(List<DbUserRelic> obj)
        {
            Init(obj);
        }

        public bool TryGrowth()
        {
            var prMeta = DbRelicGrowthProbability.Get(Level.Value + 1);
            // 레벨부터 늘리고 카운트 줄여야함 (필수)
            if (Count.Value > 0 && prMeta != null)
            {
                var choose = Random.Range(0, 1000);
                var canGrowth = choose < prMeta.GetPr(Id);
                if (canGrowth)
                {
                    Level.Value += 1;
                    Count.Value -= 1;
                    TotalStatController.I.Apply(DbRelic.Get(Id).StatType);
                    QuestController.I.SetQuest(QuestType.CheckRelicLevel, RelicController.I.GetTotalLevelCount());
                    return true;
                }
                Count.Value -= 1;
                QuestController.I.DoQuests(QuestType.RelicUpgrade);
            }

            return false;
        }

        public void Sell()
        {
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            var reward = Count.Value * DbCost.Get(CostType.RelicSellCost).Cost;
            CurrencyController.I.GetReward(CurrencyType.Dia, reward, 0 , true);
            CurrencyController.I.SetDiaLog($"유믈 {Id} {Count.Value}개 판매 보상", reward, prev);
            Count.Value = 0;
        }

        protected override List<DbUserRelic> GetInitials()
        {
            var relics = new List<DbUserRelic>();
            DbRelic.ForEach(r =>
            {
                relics.Add(new DbUserRelic(r.Id, 0, 0));
            });

            return relics;
        }

        public override List<DbUserRelic> AdjustDataModification(List<DbUserRelic> obj)
        {
            DbRelic.ForEach(r =>
            {
                if (!obj.Exists(o => o.Id == r.Id)) obj.Add(new DbUserRelic(r.Id, 0, 0));
            });
            
            // TODO 지우기
            var remove = new List<DbUserRelic>();
            obj.ForEach(r =>
            {
                if (DbRelic.Get(r.Id) == null) remove.Add(r);
            });
            foreach (var r in remove) obj.Remove(r);
            
            return obj;
        }

        public DbUserRelic()
        {

        }

        [JsonConstructor]
        public DbUserRelic(int Id, BigInteger Count, int Level)
        {
            this.Id = Id;
            this.Count = new DbField<BigInteger>(Count, Id, this);
            this.Level = new DbField<int>(Level, Id, this);
        }
    }
}