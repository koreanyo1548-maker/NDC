using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using CodeStage.AntiCheat.ObscuredTypes;
using Controller;
using Controller.Currency;
using Controller.Play;
using Data.DbDefinition;
using Data.DbPetInfo;
using Data.DbShop;
using Data.Stores;
using Data.Utils;
using Fight.Units;
using MEC;
using Newtonsoft.Json;
using ThirdParty;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Data.DbUser.Currency
{
    [Serializable]
    public class DbUserCurrency: DbUserModel<DbUserCurrency, int>, IDayDiffChecker, IBackgroundChecker
    {
        #region Values

        [DataMember] public Dictionary<CurrencyType, DbField<ObscuredBigInteger>> Money;
        [DataMember] public BigInteger DiaUse;
        [DataMember] public BigInteger DiaGain;
        [DataMember] public Dictionary<int, int> BuyLog;
        [DataMember] public Dictionary<CurrencyType, DbField<BigInteger>> Stones;
        [DataMember] public Dictionary<CurrencyType, DbField<int>> Tickets;
        [DataMember] public Dictionary<CurrencyType, DbField<int>> Books;
        [DataMember] public Dictionary<CurrencyType, DbField<bool>> Have;
        [DataMember] public Dictionary<CurrencyType, DbField<int>> Etc;
        [DataMember] public DbFieldList<int> Costumes { get; private set; }
        [DataMember] public List<string> UsingCoupon { get; private set; }
        [DataMember] public DbFieldList<DbUserBuying> Buy { get; private set; }
        [DataMember] public DbFieldList<DbUserBookshelf> BookShelves { get; private set; }
        [DataMember] public DbFieldList<DbUserPass> Pass { get; private set; }
        [DataMember] public DbFieldList<DbUserAdBuff> AdBuff { get; private set; }
        [DataMember] public DbFieldList<DbUserMail> Mail { get; private set; }
        [DataMember] public List<int> EventPackages { get; private set; }
        [DataMember] public Dictionary<int, DateTime> EventPackageTime { get; private set; }
        [DataMember] public DateTime LastPassionRewardedTime { get; private set; }
        
        public int NextMailId;
        public int PassionLeftTime;

        private CoroutineHandle _timeRoutine;
        
        #endregion

        
        #region DayDiff

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            var prevReset = UserInfo.saved.info.currencyResetDate;
            dayDiff = Define.GetDayDiff(prevReset, now);
            var monthDiff = Define.GetMonthDiff(prevReset, now);
            var isNewWeek = Define.IsWeekDiff(prevReset, now, dayDiff);
            
            var eventPackageKeys = EventPackageTime.Keys.ToList();
            foreach (var key in eventPackageKeys)
            {
                if (now > EventPackageTime[key])
                {
                    EventPackageTime.Remove(key);
                    CurrencyController.I.eventPackagesChanged.Value = true;
                }
            }

            if (dayDiff > 0)
            {
                // 티켓 충전
                foreach (var ticket in Tickets)
                {
                    var meta = DbCurrency.Get(ticket.Key);
                    if (meta.DailyCharge == 0) continue;
                    // 이미 10개 있으면 자동 충전 안해줌
                    if (ticket.Value.Value >= 10) continue;
                    ticket.Value.Value = Math.Min(meta.MaxHave, ticket.Value.Value + meta.DailyCharge);
                }

                // 구매 재충전
                foreach (var buying in Buy.Value)
                {
                    if (DbInAppShop.Have(buying.Value.ItemId))
                    {
                        var inApp = DbInAppShop.Get(buying.Value.ItemId);
                        if (inApp.RenewalInterval == RenewalType.Daily) buying.Value.BuyCnt = 0;
                    }
                    else if (DbInGameShop.Have(buying.Value.ItemId))
                    {
                        var inGame = DbInGameShop.Get(buying.Value.ItemId);
                        if (inGame.RenewalInterval == RenewalType.Daily) buying.Value.BuyCnt = 0;
                    }
                    else if (DbBlackMarket.Have(buying.Value.ItemId))
                    {
                        var blackMarket = DbBlackMarket.Get(buying.Value.ItemId);
                        if (blackMarket.RenewalInterval == RenewalType.Daily) buying.Value.BuyCnt = 0;
                    }
                }
                // 패스 종료 확인
                CheckPassSeason(now);
                
                // 광고 버프 재충전
                foreach (var adBuff in AdBuff.Value)
                {
                    adBuff.Value.UseCount = 0;
                }
            }

            if (monthDiff > 0)
            {
                // 구매 재충전
                foreach (var buying in Buy.Value)
                {
                    if (DbInAppShop.Have(buying.Value.ItemId))
                    {
                        var isInApp = DbInAppShop.Get(buying.Value.ItemId);
                        if (isInApp.RenewalInterval == RenewalType.Monthly) buying.Value.BuyCnt = 0;
                    }
                    else if (DbInGameShop.Have(buying.Value.ItemId))
                    {
                        var isInGame = DbInGameShop.Get(buying.Value.ItemId);
                        if (isInGame.RenewalInterval == RenewalType.Monthly) buying.Value.BuyCnt = 0;
                    }
                    else if (DbBlackMarket.Have(buying.Value.ItemId))
                    {
                        var blackMarket = DbBlackMarket.Get(buying.Value.ItemId);
                        if (blackMarket.RenewalInterval == RenewalType.Monthly) buying.Value.BuyCnt = 0;
                    }
                }
            }

            if (isNewWeek)
            { 
                // 구매 재충전
                foreach (var buying in Buy.Value)
                {
                    if (DbInAppShop.Have(buying.Value.ItemId))
                    {
                        var isInApp = DbInAppShop.Get(buying.Value.ItemId);
                        if (isInApp.RenewalInterval == RenewalType.Weekly) buying.Value.BuyCnt = 0;
                    }
                    else if (DbInGameShop.Have(buying.Value.ItemId))
                    {
                        var isInGame = DbInGameShop.Get(buying.Value.ItemId);
                        if (isInGame.RenewalInterval == RenewalType.Weekly) buying.Value.BuyCnt = 0;
                    }
                    else if (DbBlackMarket.Have(buying.Value.ItemId))
                    {
                        var blackMarket = DbBlackMarket.Get(buying.Value.ItemId);
                        if (blackMarket.RenewalInterval == RenewalType.Weekly) buying.Value.BuyCnt = 0;
                    }
                }
            }

            UserInfo.saved.info.currencyResetDate = now;
        }
        
        #endregion
        
        
        #region Set
        
        public override void Set(List<DbUserCurrency> obj)
        { 
            Init(obj);
        }

        protected override List<DbUserCurrency> GetInitials()
        {
            return new List<DbUserCurrency>
            {
                new()
            };
        }

        public override List<DbUserCurrency> AdjustDataModification(List<DbUserCurrency> obj)
        {
            DbCurrency.ForEach(c =>
            {
                switch (c.Category)
                {
                    case CurrencyCategoryType.Money:
                        if (!obj[0].Money.ContainsKey(c.Id)) obj[0].Money.Add(c.Id, new DbField<ObscuredBigInteger>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Stone:
                        if (!obj[0].Stones.ContainsKey(c.Id)) obj[0].Stones.Add(c.Id, new DbField<BigInteger>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Ticket:
                        if (!obj[0].Tickets.ContainsKey(c.Id)) obj[0].Tickets.Add(c.Id, new DbField<int>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Have:
                        if (!obj[0].Have.ContainsKey(c.Id)) obj[0].Have.Add(c.Id, new DbField<bool>(c.InitialValue == 1, Id, this));
                        break;
                    case CurrencyCategoryType.Etc:
                        if (!obj[0].Etc.ContainsKey(c.Id)) obj[0].Etc.Add(c.Id, new DbField<int>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Book:
                        if (!obj[0].Books.ContainsKey(c.Id)) obj[0].Books.Add(c.Id, new DbField<int>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            return obj;
        }

        /// <summary>
        /// 게임 시작 시 데이터 불러올 때
        /// </summary>
        public void SetBookshelves(DateTime now)
        {
            BookShelves.ForEach(bookshelf =>
            {
                var time = bookshelf.StartTime;
                var reduce = bookshelf.UseAd ? (int) DbPlay.Get(PlayType.BookReduceTimeByAd).Value : 0;
                // 사용 중이면
                if (bookshelf.HaveBook.Value)
                {
                    var duration = DbBook.Get(bookshelf.BookType).Time;
                    // 사용 기간 지났으면 leftTime 0
                    if (time.AddSeconds(duration - reduce) < now || bookshelf.UseDia)
                    {
                        bookshelf.LeftTime = 0;
                    }
                    // 사용 기간 안지났으면 남은 시간 세팅
                    else
                    {
                        bookshelf.LeftTime = Math.Max(0, duration - (int)now.Subtract(time).TotalSeconds - reduce);
                        bookshelf.StartBookShelfTimeRoutine();
                    }
                }
            });
        }

        /// <summary>
        /// 게임 시작 시 데이터 불러올 때
        /// </summary>
        public void SetAdBuff(DateTime now, bool isDayPass)
        {
            DbAdBuff.ForEach(b =>
            {
                // 갖고 있는 광고 버프일 때,
                if (AdBuff.Have(myBuff => myBuff.AdBuffType == b.Id))
                {
                    var buff = AdBuff.Get(myBuff => myBuff.AdBuffType == b.Id);
                    var time = buff.StartTime;
                    // 사용 중이면
                    if (buff.IsUsing.Value && !Have[CurrencyType.AdSkip].Value)
                    {
                        // 사용 기간 지났으면 버프 끄기
                        if (time.AddSeconds(b.Duration) < now)
                        {
                            buff.IsUsing.Value = false;
                            TotalStatController.I.Apply(buff.BuffType);
                        }
                        // 사용 기간 안지났으면 남은 시간 세팅
                        else
                        {
                            buff.LeftTime = b.Duration - (int)now.Subtract(time).TotalSeconds;
                        }
                    }
                    // else if (Have[CurrencyType.AdSkip].Value)
                    // {
                    //     buff.AdSkipUse();
                    // }
                    
                    // 날짜 지났으면 사용량 초기화해주기
                    if (isDayPass)
                    {
                        buff.UseCount = 0;
                    }
                }
                // 안갖고 있는 버프면 갖게 해주기
                else
                {
                    AdBuff.Add(new DbField<DbUserAdBuff>(new DbUserAdBuff(b.Id, b.BuffType, DateTime.MinValue, false, 0, 0, this), 0, this));
                    if (Have[CurrencyType.AdSkip].Value) AdBuff.Value.Find(a => a.Value.AdBuffType == b.Id).Value.AdSkipUse();
                }
            });
        }

        /// <summary>
        /// 게임 시작 시 데이터 불러올 때 사용
        /// 데이터에 있는 패스 + 현재 구매 가능한 패스인데 내가 갖고 있지 않은 경우 => 추가해줌
        /// 내가 갖고 있는 패스 + 현재 구매 불가능 => Lock해줌
        /// </summary>
        /// <param name="now"> 현재 시간 </param>
        /// <returns></returns>
        public void SetPass(DateTime now)
        {
            DbPassShop.ForEach(p =>
            {
                if (p.CanAdd(now))
                {
                    if (!Pass.Have(myPass => (CurrencyType) myPass.PassType == p.PassType))
                    {
                        Pass.Add(new DbField<DbUserPass>(new DbUserPass((PassType)p.PassType, -1, new [] {-1, -1, -1, -1, -1}, 0, 
                            p.Condition == PassCondition.CheckDate ? p.GetCurrentSeason(now) : 0), 0, this));
                    }
                }
            });
            CheckPassSeason(now);
        }

        /// <summary>
        /// 데이터 저장 시 시간 불러올 때마다 사용
        /// </summary>
        public void CheckPassSeason(DateTime now)
        {
            Pass.ForEach(p =>
            {
                var meta = DbPassShop.Get(meta => meta.PassType == (CurrencyType)p.PassType);

                if (meta.Condition == PassCondition.CheckDate)
                {
                    if (p.Season.Value != meta.GetCurrentSeason(now))
                    {
                        var buy = Buy.Value.Find(b => b.Value.ItemId == meta.Id);
                        if (buy != null)
                        {
                            Buy.Remove(buy);
                            var currency = meta.Reward[0].currencyType;
                            Have[currency].Value = false;
                        }
                        p.Reset(meta.GetCurrentSeason(now));
                    }
                }
            });
        }
        #endregion
        

        #region Mail
        /// <summary>
        /// 생성자에서 세팅한 다음 서버메일
        /// </summary>
        public void CheckMail()
        {
            var maxMailId = 0;
            Mail.ForEach(m =>
            {
                if (m.Id > maxMailId) maxMailId = m.Id;
            });
            NextMailId = maxMailId;

            PlayFabManager.Store.DoWithTime(ChangeMail);

            void ChangeMail(DateTime now)
            {
                PlayFabManager.Data.CheckMail((mail, shouldAdd) => AdjustMail(mail, shouldAdd, now), false);
            }
        }

        public void CheckNewMail()
        {
            PlayFabManager.Store.DoWithTime(AddMail);

            void AddMail(DateTime now)
            {
                PlayFabManager.Data.CheckMail((mail, shouldAdd) => AdjustMail(mail,shouldAdd, now), true);
            }
        }
        
        private void AdjustMail(MailInfo mail, bool shouldAdd, DateTime now)
        {
            var myMail = Mail.Value.Find(m => m.Value.MailId == mail.id);
            if (myMail == null && mail.users != null && mail.users.Count != 0 && !mail.users.Contains(SettingController.Id)) return;
            
            if (shouldAdd || myMail == null)
            {
                if (mail.type != MailType.Permanent)
                {
                    if (now > mail.endTime) return;
                }

                if (now < mail.startTime)
                {
                    return;
                }

                var newMail = new DbUserMail(++NextMailId, mail.id, false, false, false, string.Empty, new DateTime(), this);
                newMail.SetMailInfo(mail);
                Mail.Add(new DbField<DbUserMail>(newMail));
                myMail = Mail.Value.Find(m => m.Value.MailId == mail.id);
            }

            myMail.Value.SetMailInfo(mail);
            switch (mail.type)
            {
                case MailType.Everyday:
                    var todayResetTime = now.Subtract(now.TimeOfDay).Add(mail.resetTime.TimeOfDay);
                    if (now > todayResetTime && myMail.Value.RewardedTime < todayResetTime) 
                    {
                        myMail.Value.IsRewarded.Value = false;
                        myMail.Value.IsHide.Value = false;
                    }
                    else if (myMail.Value.RewardedTime > todayResetTime)
                    {
                        Timing.RunCoroutine(_ResetMailRoutine(mail.id, todayResetTime.Subtract(now).Add(new TimeSpan(1, 0, 0,0)).TotalSeconds), Define.KillWhenGetMail);
                    }
                    else 
                    {
                        Timing.RunCoroutine(_ResetMailRoutine(mail.id, todayResetTime.Subtract(now).TotalSeconds), Define.KillWhenGetMail);
                    }
                    Timing.RunCoroutine(_RemoveMailRoutine(mail.id, mail.endTime.Subtract(now).TotalSeconds), Define.KillWhenGetMail);
                    break;
                case MailType.Once:
                    Timing.RunCoroutine(_RemoveMailRoutine(mail.id, mail.endTime.Subtract(now).TotalSeconds), Define.KillWhenGetMail);
                    break;
                case MailType.Permanent:
                    break;
                default: throw new Exception("정의되지 않은 메일 타입입니다.");
            }
            
            IEnumerator<float> _RemoveMailRoutine(int id, double seconds)
            {
                yield return Timing.WaitForSeconds((float)seconds);
                Mail.Get(m => m.MailId == id).IsHide.Value = true;
            }

            IEnumerator<float> _ResetMailRoutine(int id, double seconds)
            {
                yield return Timing.WaitForSeconds((float)seconds);
                var userMail = Mail.Get(m => m.MailId == id);
                userMail.IsHide.Value = false;
                userMail.IsRewarded.Value = false;
            }
                
            // void DeleteMail(List<int> existing)
            // {
            //     var toDelete = new List<DbField<DbUserMail>>();
            //     foreach (var mail in Mail.Value)
            //     {
            //         if (!existing.Exists(m => m == mail.Value.Id) 
            //             && !mail.Value.IsShop) toDelete.Add(mail);
            //     }
            //
            //     foreach (var delete in toDelete)
            //     {
            //         Mail.Value.Remove(delete);
            //     }
            // }
        }

        #endregion
        
        
        #region Passion

        public void SetLastPassionRewarded(DateTime now)
        {
            LastPassionRewardedTime = now;
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
        }
        
        public void SetPassion(DateTime now)
        {
            Timing.KillCoroutines(_timeRoutine);
            PassionLeftTime = Math.Max(0, (int)((LastPassionRewardedTime.AddHours(8) - now).TotalSeconds+1));
            if (PassionLeftTime > 0)
            {
                StartPassionTimeRoutine();
            }
        }
        
        
        private void StartPassionTimeRoutine()
        {
            _timeRoutine = Timing.RunCoroutine(_PassionTimeRoutine());
        }
        
        private IEnumerator<float> _PassionTimeRoutine()
        {
            while (PassionLeftTime > 0)
            {
                yield return Timing.WaitForSeconds(1);
                PassionLeftTime--;
            }
        }

        public void WhenBackFromBackground(TimeSpan time, DateTime now)
        {
            PassionLeftTime = Math.Max(0, PassionLeftTime - (int)time.TotalSeconds);
        }
        
        
        #endregion
        
        
        [JsonConstructor]
        public DbUserCurrency(int Id, Dictionary<CurrencyType, ObscuredBigInteger> Money, BigInteger DiaUse, BigInteger DiaGain, 
            Dictionary<int, int> BuyLog, Dictionary<CurrencyType, BigInteger> Stones,
            Dictionary<CurrencyType, int> Tickets, Dictionary<CurrencyType, int> Books, Dictionary<CurrencyType, bool> Have,
            Dictionary<CurrencyType, int> Etc, List<DbUserBuying> Buy, List<CurrencyBookshelf> Bookshelf,
            List<CurrencyPass> Pass, List<string> UsingCoupon, List<CurrencyAdBuff> AdBuff,
            List<CurrencyMail> Mail, List<int> Costumes, List<int> EventPackages, Dictionary<int, DateTime> EventPackageTime,
            DateTime LastPassionRewardedTime)
        {
            this.Id = Id;
            this.Money = DictionaryConverter.NormalToField(Money, 0, this);
            this.DiaUse = DiaUse;
            this.DiaGain = DiaGain;
            this.BuyLog = BuyLog;
            this.Stones = DictionaryConverter.NormalToField(Stones, 0, this);
            this.Tickets = DictionaryConverter.NormalToField(Tickets, 0, this);
            this.Books = DictionaryConverter.NormalToField(Books, 0, this);
            this.Have = DictionaryConverter.NormalToField(Have, 0, this);
            this.Etc = DictionaryConverter.NormalToField(Etc, 0, this);
            this.Buy = new DbFieldList<DbUserBuying>(Buy, 0, this);
            this.Costumes = new DbFieldList<int>(Costumes.Distinct().ToList(), 0, this);
            this.EventPackages = EventPackages;
            this.EventPackageTime = EventPackageTime;
            
            var bookshelves = new List<DbUserBookshelf>();
            foreach (var b in Bookshelf)
                bookshelves.Add(new DbUserBookshelf(b.index, b.haveBook, (CurrencyType) b.bookType, b.startTime, b.leftTime, b.useAd, b.useDia, this));
            BookShelves = new DbFieldList<DbUserBookshelf>(bookshelves, 0, this);
            
            var pass = new List<DbUserPass>();
            foreach (var p in Pass) pass.Add(new DbUserPass((PassType)p.type, p.free, p.premiums, p.progress, p.season));
            this.Pass = new DbFieldList<DbUserPass>(pass, 0, this);
            
            this.UsingCoupon = UsingCoupon;
            
            var adBuff = new List<DbUserAdBuff>();
            foreach (var ab in AdBuff) adBuff.Add(new DbUserAdBuff((AdBuffType)ab.type, (StatType)ab.buffType,
                ab.startTime, ab.isUsing, ab.useCount, ab.leftTime, this));
            this.AdBuff = new DbFieldList<DbUserAdBuff>(adBuff, 0, this);
            
            var mail = new List<DbUserMail>();
            foreach (var m in Mail) mail.Add(new DbUserMail(m.id, m.mailId, m.isRewarded, m.isHide, m.isShop, m.receipt, m.rewardedTime, this));
            this.Mail = new DbFieldList<DbUserMail>(mail, 0, this);
            this.NextMailId = 0;

            this.LastPassionRewardedTime = LastPassionRewardedTime;
        }

        public DbUserCurrency()
        {
            Id = 0;
            Money = new Dictionary<CurrencyType, DbField<ObscuredBigInteger>>();
            DiaGain = 0;
            DiaUse = 0;
            BuyLog = new();
            Stones = new Dictionary<CurrencyType, DbField<BigInteger>>();
            Tickets = new Dictionary<CurrencyType, DbField<int>>();
            Books = new Dictionary<CurrencyType, DbField<int>>();
            Have = new Dictionary<CurrencyType, DbField<bool>>();
            Etc = new Dictionary<CurrencyType, DbField<int>>();
            DbCurrency.ForEach(c =>
            {
                switch (c.Category)
                {
                    case CurrencyCategoryType.Money:
                        Money.Add(c.Id, new DbField<ObscuredBigInteger>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Stone:
                        Stones.Add(c.Id, new DbField<BigInteger>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Ticket:
                        Tickets.Add(c.Id, new DbField<int>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Have:
                        Have.Add(c.Id, new DbField<bool>(c.InitialValue == 1, Id, this));
                        break;
                    case CurrencyCategoryType.Etc:
                        Etc.Add(c.Id, new DbField<int>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.Book:
                        Books.Add(c.Id, new DbField<int>(c.InitialValue, Id, this));
                        break;
                    case CurrencyCategoryType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            });
            Costumes = new DbFieldList<int>(new List<int>(), Id, this);
            BookShelves = new DbFieldList<DbUserBookshelf>(new List<DbUserBookshelf> {new(0, this), new(1, this), new(2, this)}, Id, this);
            Buy = new DbFieldList<DbUserBuying>(new List<DbUserBuying>(), Id, this);
            Pass = new DbFieldList<DbUserPass>(new List<DbUserPass>(), Id, this);
            AdBuff = new DbFieldList<DbUserAdBuff>(new List<DbUserAdBuff>(), Id, this);
            UsingCoupon = new List<string>();
            Mail = new DbFieldList<DbUserMail>(new List<DbUserMail>(), Id, this);
            EventPackages = new List<int>();
            EventPackageTime = new Dictionary<int, DateTime>();
            LastPassionRewardedTime = new DateTime();
            NextMailId = 0;
        }
    }
}