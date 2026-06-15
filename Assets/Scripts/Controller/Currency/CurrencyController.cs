using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CodeStage.AntiCheat.ObscuredTypes;
using Controller.Have;
using Controller.Infos;
using Controller.Play;
using Controller.Utils;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbEvent;
using Data.DbPetInfo;
using Data.DbPromote;
using Data.DbShop;
using Data.DbStage;
using Data.DbSummon;
using Data.DbUser.Currency;
using Data.Utils;
using Exceptions;
using Managers;
using MEC;
using Newtonsoft.Json;
using ThirdParty;
using UIBases;
using UIs.Shop;
using UIs.Shop.EventPackage;
using UIs.Summon;
using UIs.Toast;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Controller.Currency
{
    public class CurrencyController: Singleton<CurrencyController>
    {
        public static DbUserCurrency data = DbUserCurrency.Get(0);
        
        public ControllerField<bool> eventPackagesChanged = new(false);
        
        public void Init()
        {
            LevelController.data.Level.ValueChanged += (_, _) => SetLevelPassProgress();
            LevelController.data.MaxStage.ValueChanged += (_, _) => SetStagePassProgress();
            
            SetLevelPassProgress();
            SetStagePassProgress();
            var soul = GetPassInfo(PassType.SoulPass);
            if (soul != null) soul.SetGetCondition();
            
            GetReward(CurrencyType.LevelPoint, LevelController.I.GetLevelPointDiff());
        }

        #region Summon

        /// <param name="summonTypeIdx"> -1: 광고 </param>
        public bool CheckSummonCurrency(SummonType summonType, SummonCurrency currency, int summonTypeIdx)
        {
            if (summonTypeIdx == -1)
            {
                return GetTicketModel(Define.SummonTypeToAdTicket(summonType)).Value > 0;
            }
            var isMoney = currency != SummonCurrency.Ticket;
            var cost = DbSummonProduct.Get(summonType, summonTypeIdx).GetNeed(isMoney);
            if (isMoney) return cost <= GetMoneyModel(Define.SummonCurrencyToCurrency(currency, summonType)).Value;
            return cost <= GetTicketModel(Define.SummonTypeToTicket(summonType)).Value;
        }

        public bool TrySummon(SummonType summonType, SummonCurrency currency, int summonNumber)
        { 
            var prev = GetMoneyModel(CurrencyType.Dia).Value;
            var summonIdx = summonNumber == -1 ? 4 : summonNumber;
            var product = DbSummonProduct.Get(summonType, summonIdx);
            var isDia = currency == SummonCurrency.Dia;

            var canUse = summonNumber == -1 ? TryUse(Define.SummonTypeToAdTicket(summonType), 1) :
                TryUse(Define.SummonCurrencyToCurrency(currency, summonType), product.GetNeed(currency != SummonCurrency.Ticket));
            if (canUse)
            {
                if (isDia) SetDiaLog($"{summonType.ToString()} 소환 {summonIdx}번", -product.Cost, prev);
                var popup = Manager.UI.GetPopupUI<UI_SummonResult>();
                if (popup == null) popup = Manager.UI.ShowPopupUI<UI_SummonResult>();
                
                LevelController.I.AddSummonExp(summonType, product.Counts, product.Cost);
                switch (summonType)
                {
                    case SummonType.Weapon:
                        var weapons = WeaponController.I.AddRandom(product.Counts);
                        popup.SetResult(weapons, summonIdx, SummonType.Weapon, currency);
                        break;
                    case SummonType.Accessory:
                        var accessories = AccessoryController.I.AddRandom(product.Counts);
                        popup.SetResult(accessories, summonIdx, SummonType.Accessory, currency);
                        break;
                    case SummonType.Skill:
                        var skills = SkillController.I.AddRandom(product.Counts);
                        popup.SetResult(skills, summonIdx, SummonType.Skill, currency);
                        break;
                    case SummonType.Relic:
                        var relics = RelicController.I.AddRandom(product.Counts);
                        popup.SetResult(relics, summonIdx, SummonType.Relic, currency);
                        break;
                    case SummonType.Necklace:
                        var necklaces = NecklaceController.I.AddRandom(product.Counts);
                        popup.SetResult(necklaces, summonIdx, SummonType.Necklace, currency);
                        break;
                }
                PlayFabManager.Store.ForceSave();
                return true;
            }

            return false;
        }

        #endregion
        
        #region Use

        public void ReturnLevelPoint(int point)
        {
            GetReward(CurrencyType.LevelPoint, point);
            QuestController.I.SetQuest(QuestType.CheckLevelPoint,LevelController.I.GetTotalLevelPointUse());
        }
        
        public void UseCoupon(string use)
        {
            data.UsingCoupon.Add(use);
            PlayFabManager.Store.ForceSave();
        }

        public bool CanUse(string couponId)
        {
            return !data.UsingCoupon.Contains(couponId);
        }

        public bool TryUse(CurrencyType currency, int count)
        {
            if (currency == CurrencyType.Ad) return true;
            var category = DbCurrency.Get(currency).Category;
            switch (category)
            {
                case CurrencyCategoryType.Money:
                    if (data.Money[currency].Value < count) return false;
                    data.Money[currency].Value -= count;
                    return true;
                
                case CurrencyCategoryType.Stone:
                    if (data.Stones[currency].Value < count) return false;
                    data.Stones[currency].Value -= count;
                    return true;
                
                case CurrencyCategoryType.Ticket:
                    if (data.Tickets[currency].Value < count) return false;
                    data.Tickets[currency].Value -=  count;
                    // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
                    return true;
                
                case CurrencyCategoryType.Etc:
                    if (data.Etc[currency].Value < count) return false;
                    data.Etc[currency].Value -= count;
                    return true;
                
                case CurrencyCategoryType.Book:
                    if (data.Books[currency].Value < count) return false;
                    data.Books[currency].Value -= count;
                    return true;
                
                default:
                    throw new NotDefinedCurrencyException(currency);
            }
        }

        public bool TryUse(CurrencyType currency, BigInteger count)
        {
            var category = DbCurrency.Get(currency).Category;
            switch (category)
            {
                case CurrencyCategoryType.Money:
                    if (data.Money[currency].Value < count) return false;
                    BigInteger val = data.Money[currency].Value;
                    data.Money[currency].Value = val - count;
                    return true;
                
                case CurrencyCategoryType.Stone:
                    if (data.Stones[currency].Value < count) return false;
                    data.Stones[currency].Value -= count;
                    return true;
                default:
                    throw new NotDefinedCurrencyException(currency);
            }
        }
        
        #endregion
        
        #region Count
        
        public DbField<int> GetTicketModel(CurrencyType currency)
        {
            return data.Tickets[currency];
        }

        public DbField<ObscuredBigInteger> GetMoneyModel(CurrencyType currency)
        {
            return data.Money[currency];
        }

        public DbField<BigInteger> GetStoneModel(CurrencyType currency)
        {
            return data.Stones[currency];
        }

        public DbField<BigInteger> GetStoneModel(EquipmentType equipment)
        {
            return data.Stones[Define.EquipmentToGrowthStone(equipment)];
        }

        public DbField<int> GetEtcModel(CurrencyType currency)
        {
            return data.Etc[currency];
        }

        public DbField<int> GetBookModel(CurrencyType currency)
        {
            return data.Books[currency];
        }

        public DbField<int>[] GetAllBookModels()
        {
            return data.Books.Values.ToArray();
        }

        public void ResetHave(CurrencyType currency)
        {
            data.Have[currency].Value = false;
        }
        
        public BigInteger GetEquipGrowthStoneCount(EquipmentType equipment)
        {
            return data.Stones[Define.EquipmentToGrowthStone(equipment)].Value;
        }
        
        public bool Have(CurrencyType currencyType)
        {
            return data.Have[currencyType].Value;
        }

        public bool HaveCostume(int costume)
        {
            return data.Costumes.Have(c => c == costume);
        }
        
        private bool HaveIt(CurrencyType currency, long count)
        {
            var category = DbCurrency.Get(currency).Category;
            switch (category)
            {
                case CurrencyCategoryType.Money:
                    return data.Money[currency].Value >= count;
                case CurrencyCategoryType.Stone:
                    return data.Stones[currency].Value >= count;
                case CurrencyCategoryType.Ticket:
                    return data.Tickets[currency].Value >= count;
                case CurrencyCategoryType.Book:
                    return data.Books[currency].Value >= count;
                case CurrencyCategoryType.Etc:
                    return data.Etc[currency].Value >= count;
                default:
                    throw new NotDefinedCurrencyException(currency);
            }
        }
        
        #endregion
        
        #region Buy
        
        public DbUserBuying GetBuying(int id)
        {
            var buying = data.Buy.Value.Find(i => i.Value.ItemId == id);
            return buying?.Value;
        }

        public int GetBuyCount(int id)
        {
            var buy = GetBuying(id);
            if (buy == null) return 0;
            return buy.BuyCnt;
        }
        
        
        public bool CanBuy(IDbShop item)
        {
            return CanBuyCount(item) > 0;
        }

        public bool CanBuy(DbDropEventShop item)
        {
            return CanBuyCount(item) > 0;
        }

        private void SetBuyLog(int id, int count)
        {
            if (data.BuyLog.ContainsKey(id)) data.BuyLog[id] += count;
            else data.BuyLog.Add(id, count);
        }

        public bool Buy(IDbShop item, string receipt = "", Action afterSave = null)
        {
            // Debug.Log(">>>>> 구매 시작");
            var prev = GetMoneyModel(CurrencyType.Dia).Value;
            var realPrice = item.GetIncreasePrice() > 0 ? item.GetPrice() + GetBuyCount(item.GetId()) * item.GetIncreasePrice() : item.GetPrice();
            if (!item.IsInApp() && !TryUse(item.GetPriceType(), realPrice)) return false;
            
            
            if (item.IsInApp())
            {
                for (var idx = 0; idx < data.Mail.Value.Count; ++idx)
                {
                    var mail = data.Mail.Value[idx].Value;
                    if (mail.Receipt != null && mail.Receipt.Equals(receipt) && receipt != string.Empty) 
                    {
                        if (afterSave != null) afterSave();
                        return true;
                    }
                }
                
                data.Mail.Add(new DbField<DbUserMail>(new DbUserMail(
                    ++data.NextMailId, item.GetId(), false, false, true, receipt, new DateTime(), data)));
                GetReward(CurrencyType.Mileage, item.GetMileage());
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200037);
                if (item.GetMileage() > 0) Timing.CallDelayed(1.3f,
                    () =>
                    {
                        Manager.UI.ShowSingleUI<UI_Toast>()
                            .SetText(string.Format(LocalString.Get(210294), item.GetMileage()));
                    });
            }
            else
            {
                if (item.GetPriceType() == CurrencyType.Dia) SetDiaLog($"상품 {item.GetId()} 구매", -realPrice, prev);
                var rewards = item.GetRewards();
                for (var idx = 0; idx < rewards.Count; ++idx)
                {
                    var prev2 = GetMoneyModel(CurrencyType.Dia).Value;
                    GetReward(rewards[idx].currencyType, rewards[idx].count, rewards[idx].id);
                    if (rewards[idx].currencyType == CurrencyType.Dia) SetDiaLog($"상품 {item.GetId()} 구매 보상", (int)rewards[idx].count, prev2);
                }    
            }
            
            var buyInfo = GetBuying(item.GetId());
            if (buyInfo != null)
            {
                buyInfo.BuyCnt++;
                data.Buy.ValueUpdated(item.GetId());
            }
            else data.Buy.Add(new DbField<DbUserBuying>(new DbUserBuying(item.GetId(), 1)));

            
            if (item.GetCategory() == ShopCategoryType.Unlock)
            {
                if (!CanBuy(item))
                {
                    data.EventPackageTime.Remove(item.GetId());
                    eventPackagesChanged.Value = true;
                }
            }
            
            SetBuyLog(item.GetId(), 1);

            if (afterSave != null) afterSave();
            // Debug.Log(">>>>>>>> [구매저장하기] " + JsonConvert.SerializeObject(item));
            PlayFabManager.Store.ForceSave();
            return true;
        }

        public void Buy(DbPassShop item, Action afterSave = null)
        {
            var buyInfo = GetBuying(item.Id);
            if (buyInfo != null) buyInfo.BuyCnt++;
            else data.Buy.Value.Add(new DbField<DbUserBuying>(new DbUserBuying(item.Id, 1)));
            GetRewards(item.Reward);
            GetReward(CurrencyType.Mileage, item.Mileage);
            if (item.PassType != CurrencyType.SeasonPass)
            {
                data.Pass.Value.Find(p => p.Value.PassType == CurrencyToNormal(item.PassType)).Value.SetGetCondition();
            }
            Manager.UI.ShowSingleUI<UI_Toast>()
                .SetText(string.Format(LocalString.Get(210294), item.Mileage));
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);

            PassType CurrencyToNormal(CurrencyType currency)
            {
                switch (currency)
                {
                    case CurrencyType.LevelPass1: case CurrencyType.LevelPass2: case CurrencyType.LevelPass3:
                    case CurrencyType.LevelPass4: case CurrencyType.LevelPass5:
                        return PassType.LevelPass;
                    case CurrencyType.StagePass1: case CurrencyType.StagePass2: case CurrencyType.StagePass3:
                    case CurrencyType.StagePass4: case CurrencyType.StagePass5:
                        return PassType.StagePass;
                    case CurrencyType.SoulPass:
                        return PassType.SoulPass;
                    default: throw new Exception(currency + " is not defined pass type");
                }
            }
            SetBuyLog(item.Id, 1);
            PlayFabManager.Store.ForceSave(afterSave);
        }

        public bool Buy(DbDropEventShop item, Action afterSave = null)
        {
            if (!TryUse(CurrencyType.DropEventMoney, item.Price)) return false;
            var prev = GetMoneyModel(CurrencyType.Dia).Value;
            GetReward(item.RewardType, item.RewardCount, item.RewardId);
            if (item.RewardType == CurrencyType.Dia) SetDiaLog($"상품 {item.Id} 구매 보상",  item.RewardCount, prev);
            
            var buyInfo = GetBuying(item.Id);
            if (buyInfo != null)
            {
                buyInfo.BuyCnt++;
                data.Buy.ValueUpdated(item.Id);
            }
            else data.Buy.Add(new DbField<DbUserBuying>(new DbUserBuying(item.Id, 1)));

            SetBuyLog(item.Id, 1);

            if (afterSave != null) afterSave();
            PlayFabManager.Store.ForceSave();
            return true;
        }
    

        public int CanBuyCount(IDbShop item)
        {
            if (item.GetRenewalInterval() == RenewalType.Infinite) return 1;
            if (item.GetBuyLimit() == -1) return 1;
            
            var rewards = item.GetRewards();
            var isCostume = rewards[0].currencyType == CurrencyType.Costume;

            if (isCostume && HaveCostume(rewards[0].id)) return 0;
            
            var buyInfo = GetBuying(item.GetId());
            if (buyInfo != null) return item.GetBuyLimit() - buyInfo.BuyCnt;
            return item.GetBuyLimit();
        }
        
        public int CanBuyCount(DbDropEventShop item)
        {
            if (item.RenewalInterval == RenewalType.Infinite) return 1;
            if (item.BuyLimit == -1) return 1;

            var buyInfo = GetBuying(item.Id);
            if (buyInfo != null) return item.BuyLimit - buyInfo.BuyCnt;
            return item.BuyLimit;
        }
        
        public int CanBuyCount(DbPassShop item)
        {
            var buyInfo = GetBuying(item.Id);
            if (buyInfo != null) return item.BuyLimit - buyInfo.BuyCnt;
            return item.BuyLimit;
        }


        public bool CanBuy(DbInGameShop item)
        {
            return CanBuyCount(item) > 0 && (item.PriceType == CurrencyType.Ad || HaveIt(item.PriceType, item.Price));
        }
        
        #endregion
        
        #region Pass
        
        public DbUserPass GetPassInfo(PassType pass)
        {
            if (data.Pass.Have(p => p.PassType == pass)) return data.Pass.Get(p => p.PassType == pass);
            return null;
        }

        public int GetFreePassIndex(PassType passType, CurrencyType specificPass)
        {
            var cur = GetPassInfo(passType).LastFreeRewarded;
            if (cur == -1) return 0;
            var curSpecific = DbSelector.GetPass(passType, cur).GetSpecificPassType();
            if (specificPass < curSpecific) return 256;
            if (specificPass > curSpecific) return 0;
            
            return cur - DbSelector.GetFirstId(passType, curSpecific);
        }
        
        public void GetAllPassRewards(PassType passType, CurrencyType specificPass)
        {
            var passInfo = GetPassInfo(passType);
            var havePremium = data.Buy.Have(b => b.ItemId == DbPassShop.Get(p => p.PassType == specificPass).Id);

            Dictionary<Tuple<CurrencyType, int>, int> rewards = new();
            
            var nextPass = DbSelector.GetFirstLarge(passType, passInfo.Progress);
            var nextId = nextPass == null ? 256 : nextPass.GetId();
            
            GetFreeRewards();
            if (havePremium) GetPremiumRewards();
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
                
            void GetFreeRewards()
            {
                bool FreePredicate(IDbPass p) => p.GetId() > passInfo.LastFreeRewarded && p.GetSpecificPassType() <= specificPass && p.GetId() < nextId;

                var have = false;
                var freeGet = 0;
                DbSelector.ForEach(passType, FreePredicate, p => 
                {
                    freeGet = p.GetId();
                    var prev = GetMoneyModel(CurrencyType.Dia).Value;
                    GiveReward(p.GetFreeRewardType(), p.GetFreeRewardCounts(), p.GetFreeRewardValue());
                    if (p.GetFreeRewardType() == CurrencyType.Dia) SetDiaLog($"무료 패스 {passType} {freeGet} 보상", p.GetFreeRewardCounts(), prev);
                    have = true;
                });
                if (have) passInfo.LastFreeRewarded = freeGet;
                if (nextPass != null && nextPass.GetSpecificPassType() != specificPass) data.Pass.ValueUpdated(0);
            }
            
            void GetPremiumRewards()
            {
                bool PremiumPredicate(IDbPass p) => p.GetId() > passInfo.LastPremiumRewarded[p.GetSpecificPassTypeIdx()] 
                && p.GetSpecificPassType() == specificPass && p.GetId() < nextId;

                var premiumGet = 0;
                DbSelector.ForEach(passType, PremiumPredicate, p =>
                {
                    premiumGet = p.GetId();
                    var prev = GetMoneyModel(CurrencyType.Dia).Value;
                    GiveReward(p.GetPremiumRewardType(), p.GetPremiumRewardCounts(), p.GetPremiumRewardValue());
                    if (p.GetPremiumRewardType() == CurrencyType.Dia) SetDiaLog($"유료 패스 {passType} {premiumGet} 보상", p.GetPremiumRewardCounts(), prev);
                    passInfo.LastPremiumRewarded[p.GetSpecificPassTypeIdx()] = Math.Max(passInfo.LastPremiumRewarded[p.GetSpecificPassTypeIdx()], premiumGet);
                });
            }

            void GiveReward(CurrencyType currency, int count, int id)
            {
                GetReward(currency, count, id);
                var key = new Tuple<CurrencyType, int>(currency, id);
                if (rewards.ContainsKey(key)) rewards[key] += count;
                else rewards.Add(key, count);
            }
            
            passInfo.SetGetCondition();

            if (rewards.Count > 0)
            {
                var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
                var rewardsForToast = new List<DbReward>();
                foreach (var reward in rewards)
                {
                    rewardsForToast.Add(new DbReward(reward.Key.Item1, reward.Value, reward.Key.Item2));
                }
                toast.SetReward(210243, rewardsForToast);
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200006);
            }
        }

        private void SetLevelPassProgress()
        {
            var pass = GetPassInfo(PassType.LevelPass);
            if (pass == null) return;
            pass.Progress = LevelController.data.Level.Value;
            pass.SetGetCondition();
        }

        private void SetStagePassProgress()
        {
            var pass = GetPassInfo(PassType.StagePass);
            if (pass == null) return;
            pass.Progress = LevelController.data.MaxStage.Value;
            pass.SetGetCondition();
        }

        public void AddSoulPassProgress()
        {
            var pass = GetPassInfo(PassType.SoulPass);
            if (pass == null) return;
            pass.Progress++;
            pass.SetGetCondition();
        }
        
        #endregion
        
        #region Reward
        

        public void GetReward(DbBook book, int count = 1)
        {
            Timing.RunCoroutine(_RewardRoutine(book, count));
        }

        private IEnumerator<float> _RewardRoutine(DbBook book, int totalCount)
        { 
            while (totalCount > 1000)
            {
                Get(1000);
                totalCount -= 1000;
                yield return Timing.WaitForSeconds(1.5f);
            }
            Get(totalCount);

            void Get(int splitCount)
            {
                var pets = book.GetPets(splitCount);
                var rewards = new List<DbReward>();
                foreach (var pet in pets)
                {
                    PetController.I.Add(pet.Key, pet.Value);
                    rewards.Add(new DbReward(CurrencyType.Pet, pet.Value, pet.Key));
                }
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Pet);

                var stoneCount = 1L * book.GetStoneCount() * splitCount;
                GetReward(CurrencyType.PetGrowthStone, stoneCount);
                rewards.Add(new DbReward(CurrencyType.PetGrowthStone, stoneCount));
                Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210218, rewards);
            }
        }

        public void GetReward(PassType passType, IDbPass passMeta, int passId, bool isFree)
        {
            if (passType != passMeta.GetPassType()) return;
            if (passMeta.GetId() != passId) return;
            var info = GetPassInfo(passType);
            if (isFree && info.LastFreeRewarded + 1 != passId) return;
            if (!isFree && info.LastPremiumRewarded[passMeta.GetSpecificPassTypeIdx()] + 1 != passId) return;

            if (isFree)
            {
                var prev = GetMoneyModel(CurrencyType.Dia).Value;
                GetReward(passMeta.GetFreeRewardType(), passMeta.GetFreeRewardCounts(), passMeta.GetFreeRewardValue());
                if (passMeta.GetFreeRewardType() == CurrencyType.Dia) SetDiaLog($"무료 패스 {passType} {passMeta.GetId()} 보상", passMeta.GetFreeRewardCounts(), prev);
                info.LastFreeRewarded++;
                var next = DbSelector.GetPass(passType, info.LastFreeRewarded);
                if (next != null && next.GetSpecificPassType() != passMeta.GetSpecificPassType()) data.Pass.ValueUpdated(0);
            }
            else
            {
                var prev = GetMoneyModel(CurrencyType.Dia).Value;
                GetReward(passMeta.GetPremiumRewardType(), passMeta.GetPremiumRewardCounts(), passMeta.GetPremiumRewardValue());
                if (passMeta.GetPremiumRewardType() == CurrencyType.Dia) SetDiaLog($"유료 패스 {passType} {passMeta.GetId()} 보상", passMeta.GetPremiumRewardCounts(), prev);
                info.LastPremiumRewarded[passMeta.GetSpecificPassTypeIdx()]++;
            }
            
            info.SetGetCondition();
        }

        public void GetRewards(List<DbReward> rewards)
        {
            foreach (var reward in rewards)
            {
                GetReward(reward.currencyType, reward.count, reward.id);
            }
        }

        public void GetReward(CurrencyType currency, BigInteger count, int id = 0, bool doNotDiaQuest = false)
        {
            if (currency == CurrencyType.Dia && count > 0 && !doNotDiaQuest) QuestController.I.DoQuests(QuestType.Dia, (int)count);
            
            var category = DbCurrency.Get(currency).Category;
            switch (category)
            {
                case CurrencyCategoryType.Money:
                    BigInteger val = data.Money[currency].Value;
                    data.Money[currency].Value = val + count; ;
                    break;
                case CurrencyCategoryType.Stone:
                    data.Stones[currency].Value += count;
                    break;
                case CurrencyCategoryType.Ticket:
                    data.Tickets[currency].Value += (int)count;
                    break;
                case CurrencyCategoryType.Have:
                    data.Have[currency].Value = true;
                    if (currency == CurrencyType.AdSkip)
                    {
                        QuestController.I.DoQuests(QuestType.CheckBuffAdWatch, 1);
                        data.AdBuff.ForEach(a => a.AdSkipUse());
                    }
                    break;
                case CurrencyCategoryType.Etc:
                    data.Etc[currency].Value += (int)count;
                    break;
                case CurrencyCategoryType.Book:
                    data.Books[currency].Value += (int)count;
                    break;
                case CurrencyCategoryType.None:
                    switch (currency)
                    {
                        case CurrencyType.Weapon:
                            WeaponController.I.Add(id, (int)count);
                            return;
                        case CurrencyType.Accessory:
                            AccessoryController.I.Add(id, (int)count);
                            return;
                        case CurrencyType.Skill:
                            SkillController.I.Add(id, (int) count);
                            return;
                        case CurrencyType.Necklace:
                            NecklaceController.I.Add(id, (int) count);
                            break;
                        case CurrencyType.Pet:
                            PetController.I.Add(id, (int)count);
                            return;
                        case CurrencyType.Exp:
                            LevelController.data.Exp.Value += count;
                            break;
                        case CurrencyType.Costume:
                            data.Costumes.Add(new DbField<int>(id));
                            var costume = DbCostume.Get(id);
                            for (var idx = 0; idx < costume.Options.Count; ++idx) TotalStatController.I.Apply(costume.Options[idx]);
                            PlayFabManager.Store.SetLog($"[코스튬] {id} 획득    |    ");
                            break;
                        default:
                            throw new NotDefinedCurrencyException(currency);
                    }
                    break;
                default:
                    throw new NotDefinedCurrencyException(currency);
            }
            // PlayFabManager.Store.SaveAll();
        }

        public void GetReward(CurrencyType currency, long count, int id = 0)
        {
            if (currency == CurrencyType.Dia && count > 0) QuestController.I.DoQuests(QuestType.Dia, (int)count);
            
            var category = DbCurrency.Get(currency).Category;
            switch (category)
            {
                case CurrencyCategoryType.Money:
                    data.Money[currency].Value += count;
                    if (currency == CurrencyType.Passion) QuestController.I.DoQuests(QuestType.PassionGet, (int)count);
                    break;
                case CurrencyCategoryType.Stone:
                    data.Stones[currency].Value += count;
                    break;
                case CurrencyCategoryType.Ticket:
                    data.Tickets[currency].Value += (int)count;
                    break;
                case CurrencyCategoryType.Have:
                    data.Have[currency].Value = true;
                    if (currency == CurrencyType.AdSkip)
                    {
                        QuestController.I.DoQuests(QuestType.CheckBuffAdWatch, 1);
                        data.AdBuff.ForEach(a => a.AdSkipUse());
                    }
                    break;
                case CurrencyCategoryType.Etc:
                    data.Etc[currency].Value += (int)count;
                    break;
                case CurrencyCategoryType.Book:
                    data.Books[currency].Value += (int)count;
                    break;
                case CurrencyCategoryType.None:
                    switch (currency)
                    {
                        case CurrencyType.Weapon:
                            WeaponController.I.Add(id, (int)count);
                            return;
                        case CurrencyType.Accessory:
                            AccessoryController.I.Add(id, (int)count);
                            return;
                        case CurrencyType.Skill:
                            SkillController.I.Add(id, (int) count);
                            return;
                        case CurrencyType.Pet:
                            PetController.I.Add(id, (int)count);
                            return;
                        case CurrencyType.Necklace:
                            NecklaceController.I.Add(id, (int)count);
                            return;
                        case CurrencyType.Exp:
                            LevelController.data.Exp.Value += count;
                            break;
                        case CurrencyType.Costume:
                            data.Costumes.Add(new DbField<int>(id));
                            var costume = DbCostume.Get(id);
                            for (var idx = 0; idx < costume.Options.Count; ++idx) TotalStatController.I.Apply(costume.Options[idx]);
                            PlayFabManager.Store.SetLog($"[코스튬] {id} 획득    |    ");
                            break;
                        case CurrencyType.OfflineReward:
                            GetOfflineReward(count, OfflineRewardType.All);
                            break;
                        case CurrencyType.OfflineGold:
                            GetOfflineReward(count, OfflineRewardType.Gold);
                            break;
                        case CurrencyType.OfflineExp:
                            GetOfflineReward(count, OfflineRewardType.Exp);
                            break;
                        case CurrencyType.OfflineWeaponGrowthStone:
                            GetOfflineReward(count, OfflineRewardType.WeaponGrowthStone);
                            break;
                        case CurrencyType.OfflineAccessoryGrowthStone:
                            GetOfflineReward(count, OfflineRewardType.AccessoryGrowthStone);
                            break;
                        default:
                            throw new NotDefinedCurrencyException(currency);
                    }
                    break;
                default:
                    throw new NotDefinedCurrencyException(currency);
            }
            // PlayFabManager.Store.SaveAll();
        }
        
        public void GetMonsterKillReward()
        {
            var stageReward = DbStageReward.Get(LevelController.data.Stage.Value);
            GetMonsterKillRewardGold(stageReward);
            GetMonsterKillRewardWGS(stageReward);
            GetMonsterKillRewardAGS(stageReward);
            GetMonsterKillRewardBeadsOre(stageReward);
        }
        
        public long GetMonsterKillRewardGold(DbStageReward reward)
        {
            var addGold = (long)(reward.Gold * TotalStatController.I.GetStat(StatType.StageGoldEarn) * TotalStatController.I.GetStat(StatType.AbilityGoldEarn)* 0.000001f);
            data.Money[CurrencyType.Gold].Value += addGold;
            Manager.UI.RewardLog.Add(CurrencyType.Gold, addGold);
            return addGold;
        }

        public long GetMonsterKillRewardWGS(DbStageReward reward)
        {
            var haveWGS = Random.Range(0, 100) < reward.GrowthStoneProbability;
            if (haveWGS)
            {
                var addWGS = (long)(reward.GrowthStone * TotalStatController.I.GetStat(StatType.StageGrowthEarn) * 0.01f);
                data.Stones[CurrencyType.WeaponGrowthStone].Value += addWGS;
                Manager.UI.RewardLog.Add(CurrencyType.WeaponGrowthStone, addWGS);
                return addWGS;
            }

            return 0;
        }

        public long GetMonsterKillRewardAGS(DbStageReward reward)
        {
            var haveAGS = Random.Range(0, 100) < reward.GrowthStoneProbability;
            if (haveAGS)
            {
                var addAGS = (long)(reward.GrowthStone * TotalStatController.I.GetStat(StatType.StageGrowthEarn) * 0.01f);
                data.Stones[CurrencyType.AccessoryGrowthStone].Value += addAGS;
                Manager.UI.RewardLog.Add(CurrencyType.AccessoryGrowthStone, addAGS);
                return addAGS;
            }
            return 0;
        }
        
        public long GetMonsterKillRewardBeadsOre(DbStageReward reward)
        {
            var haveBeadsOre = Random.Range(0, 100) < reward.BeadsOreProbability;
            if (haveBeadsOre)
            {
                var addBeadsOre = (long)reward.BeadsOre;
                data.Money[CurrencyType.BeadsOre].Value += addBeadsOre;
                Manager.UI.RewardLog.Add(CurrencyType.BeadsOre, addBeadsOre);
                return addBeadsOre;
            }

            return 0;
        }

        #endregion
        
        #region Salary

        public void AddCostume(int promotion)
        {
            var costume = DbPromotion.Get(promotion).CostumeId;
            GetReward(CurrencyType.Costume, 1, costume);
            var meta = DbCostume.Get(costume);
            for (var idx = 0; idx < meta.Options.Count; ++idx)
            {
                TotalStatController.I.Apply(meta.Options[idx]);
            }
        }
        public void AddCostumeAll()
        {
            for (int i = 1; i < 14; ++i)
            {
                GetReward(CurrencyType.Costume, 1, i);
                var meta = DbCostume.Get(i);
                for (var idx = 0; idx < meta.Options.Count; ++idx)
                {
                    TotalStatController.I.Apply(meta.Options[idx]);
                }
            }   
        }
        
        #endregion

        #region AdBuff

        public DbUserAdBuff GetAdBuff(AdBuffType type)
        {
            return data.AdBuff.Get(a => a.AdBuffType == type);
        }

        public bool IsBuffAdWatching()
        {
            foreach (var adBuff in data.AdBuff.Value)
            {
                if (adBuff.Value.IsUsing.Value) return true;
            }

            return false;
        }

        #endregion

        #region Bookselves

        public DbUserBookshelf GetEmptyBookshelf()
        {
            var bookshelves = data.BookShelves.Value;
            for (var idx = 0; idx < bookshelves.Count; ++idx)
            {
                var bookshelf = bookshelves[idx].Value;
                if (Have(CurrencyType.Bookshelf1 + idx) && !bookshelf.HaveBook.Value) return bookshelf;
            }
            return null;
        }

        #endregion
        
        #region Mail

        public bool CanGetAnyMailReward()
        {
            var count = data.Mail.Value.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                if (!data.Mail.Value[idx].Value.IsRewarded.Value) return true;
            }
            
            return false;
        }

        public bool GetAllMailReward()
        {
            CheckMailCheat();
            var mails = new List<DbUserMail>();
            var rewards = new List<DbReward>();
            data.Mail.Value.ForEach(m =>
            {
                if (!m.Value.IsRewarded.Value)
                {
                    mails.Add(m.Value);
                    m.Value.IsRewarded.Value = true;
                    rewards.AddRange(m.Value.Rewards);
                }
            });
            if (rewards.Count > 0)
            {
                var dia = rewards.FindAll(r => r.currencyType == CurrencyType.Dia);
                var prev = data.Money[CurrencyType.Dia].Value;
                GetRewards(rewards);
                if (dia.Count > 0) SetDiaLog(MakeDiaLog(), GetTotalDia(), prev);
                Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210261, rewards);
                
                #region Dia Log
                string MakeDiaLog()
                {
                    var mailIds = "우편 " + mails[0].MailId;
                    for (var idx = 1; idx < mails.Count; ++idx)
                    {
                        mailIds += ", " + mails[idx].MailId;
                    }

                    mailIds += " 보상";
                    return mailIds;
                }

                int GetTotalDia()
                {
                    var total = 0;
                    foreach (var d in dia) total += (int)d.count;
                    return total;
                }
                #endregion
            }
            
            PlayFabManager.Store.DoWithTime(now =>
            {
                foreach (var mail in mails)
                {
                    mail.RewardedTime = now;
                }
            });
            return rewards.Count > 0;

        }
        
        public void DeleteAllRewardedMail()
        {
            data.Mail.Value.ForEach(m =>
            {
                if (m.Value.IsRewarded.Value) m.Value.IsHide.Value = true;
            });
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
        }

        public void CheckMailCheat()
        {
            var shopMail = new Dictionary<int, int>();
            foreach (var m in data.Mail.Value)
            {
                if (!m.Value.IsRewarded.Value && m.Value.IsShop)
                {
                    if (shopMail.ContainsKey(m.Value.MailId)) shopMail[m.Value.MailId]++;
                    else shopMail.Add(m.Value.MailId, 1);
                }
            }

            foreach (var m in shopMail)
            {
                if (!data.BuyLog.ContainsKey(m.Key) || data.BuyLog[m.Key] < m.Value)
                {
                    PlayFabManager.Leaderboard.UpdateCheat(false, $"{DateTime.UtcNow} 소지 메일: {m.Key} {m.Value}개, 구매내역과 다름");
                    break;
                }
            }
        }

        public bool HaveMail(int id)
        {
            return data.Mail.Value.Exists(m => m.Value.MailId == id);
        }

        public void RemoveMails(List<int> mails)
        {
            foreach (var m in mails)
            {
                data.Mail.Value.Remove(data.Mail.Value.Find(mail => mail.Value.Id == m));
            }
        }
        
        #endregion

        // #region EventPackages
        //
        // public void CheckEventPackage(int stage)
        // {
        //     var shop = DbInAppShop.Get(s => s.Category == ShopCategoryType.Unlock && s.UnlockStage == stage);
        //     if (shop != null && !data.EventPackages.Contains(shop.Id))
        //     {
        //         Manager.UI.ShowPopupUI<UI_OncePackage>(shop.Resource).Set(shop);
        //     }
        // }
        //
        // public void StartEventPackage(DateTime now, DbInAppShop shop)
        // {
        //     data.EventPackages.Add(shop.Id);
        //     data.EventPackageTime.Add(shop.Id, now.AddHours(24));
        //     eventPackagesChanged.Value = true;
        // }
        //
        // #endregion
        
        #region Passion

        public void GetFreePassion()
        {
            if (data.PassionLeftTime <= 0)
            {
                GetReward(CurrencyType.Passion, (int)DbPlay.Get(PlayType.FreePassion).Value);
                data.PassionLeftTime = 28800;
                PlayFabManager.Store.DoWithTime(now =>
                {
                    data.SetLastPassionRewarded(now);
                    data.SetPassion(now);
                });
            }
        }
        
        #endregion

        private BigInteger _prevDia = 0;
        public void SetDiaLog(string reason, BigInteger toIncrease, BigInteger prev)
        {
            if (toIncrease > 0) data.DiaGain += toIncrease;
            else data.DiaUse -= toIncrease;

            if (_prevDia > 0 && _prevDia != prev) PlayFabManager.Leaderboard.UpdateCheat(true, $"{DateTime.UtcNow} 이전 다이아: {_prevDia} > 현재 다이아: {prev}개");
            else if (toIncrease > 1500000) PlayFabManager.Leaderboard.UpdateCheat(true, $"{DateTime.UtcNow} 증가 다이아량: {toIncrease}개");
            _prevDia = data.Money[CurrencyType.Dia].Value;
            PlayFabManager.Store.SetLog($"[다이아] {reason}: {(toIncrease > 0 ? toIncrease + " 증가" : -toIncrease + " 감소")} ({prev} > {data.Money[CurrencyType.Dia].Value})    |    ");
        }

        public List<DbRewardBig> CalculateOfflineReward(long seconds, OfflineRewardType rewardType)
        {
            var meta = DbStageReward.Get(Math.Max(LevelController.data.MaxStage.Value, 1));
            var count = (int)(seconds/ 70 * 100);
            
            var rewards = new List<DbRewardBig>(16);
            
            if (rewardType == OfflineRewardType.All || rewardType == OfflineRewardType.Gold)
            {
                var gold = (BigInteger)(meta.Gold * count * TotalStatController.I.GetStat(StatType.StageGoldEarn) * TotalStatController.I.GetStat(StatType.AbilityGoldEarn) * 0.000001f);
                rewards.Add(new(CurrencyType.Gold, gold, 0));
            }

            if (rewardType == OfflineRewardType.All || rewardType == OfflineRewardType.Exp)
            {
                var exp = (BigInteger)(meta.Exp * count * TotalStatController.I.GetStat(StatType.StageExpEarn) * TotalStatController.I.GetStat(StatType.AbilityExpEarn) * 0.000001f);
                rewards.Add( new(CurrencyType.Exp, exp, 0));
            }
            
            if (rewardType == OfflineRewardType.All || rewardType == OfflineRewardType.WeaponGrowthStone)
            {
                var weaponStone = (BigInteger)(meta.GrowthStone * TotalStatController.I.GetStat(StatType.StageGrowthEarn) * 0.01f * Random.Range(count * meta.GrowthStoneProbability * 0.01f * 0.8f, count * meta.GrowthStoneProbability * 0.01f * 1.2f));
                rewards.Add(new(CurrencyType.WeaponGrowthStone, weaponStone, 0));
            }

            if (rewardType == OfflineRewardType.All || rewardType == OfflineRewardType.AccessoryGrowthStone)
            {
                var accessoryStone = (BigInteger)(meta.GrowthStone * TotalStatController.I.GetStat(StatType.StageGrowthEarn) * 0.01f * Random.Range(count * meta.GrowthStoneProbability * 0.01f * 0.8f, count * meta.GrowthStoneProbability * 0.01f * 1.2f));
                rewards.Add( new(CurrencyType.AccessoryGrowthStone, accessoryStone, 0));
            }

            if (rewardType == OfflineRewardType.All || rewardType == OfflineRewardType.BeadsOre)
            {
                var beadsOre = (BigInteger)(meta.BeadsOre * Random.Range(count * meta.BeadsOreProbability * 0.01f * 0.8f, count * meta.BeadsOreProbability * 0.01f * 1.2f));
                rewards.Add( new(CurrencyType.BeadsOre, beadsOre, 0));
            }
            
            if (rewardType == OfflineRewardType.All)
            {
                for (var idx = 0; idx < meta.Weapons.Count; ++idx)
                {
                    var weaponCount = (BigInteger)(Random.Range(count * meta.WeaponProbability * 0.01f * 0.8f / meta.Weapons.Count, count * meta.WeaponProbability * 0.01f * 1.2f / meta.Weapons.Count));
                    if (weaponCount > 0)
                    {
                        rewards.Add(new (CurrencyType.Weapon, weaponCount, meta.Weapons[idx]));
                    }
                } 
                for (var idx = 0; idx < meta.Accessories.Count; ++idx)
                {
                    var accessoryCount = (BigInteger)(Random.Range(count * meta.AccessoryProbability * 0.01f * 0.8f / meta.Accessories.Count, count * meta.AccessoryProbability * 0.01f * 1.2f / meta.Accessories.Count));
                    if (accessoryCount > 0)
                    {
                        rewards.Add(new (CurrencyType.Accessory, accessoryCount, meta.Accessories[idx]));
                    }
                }
            }

            return rewards;
        }

        private void GetOfflineReward(long minutes, OfflineRewardType rewardType)
        {
            var rewards = CalculateOfflineReward(minutes * 60, rewardType);
            
            foreach (var reward in rewards)
            {
                GetReward(reward.currencyType, reward.count, reward.id);
            }
        }

        public enum OfflineRewardType
        {
            All,
            Gold,
            Exp,
            WeaponGrowthStone,
            AccessoryGrowthStone,
            BeadsOre
        }
    }
}