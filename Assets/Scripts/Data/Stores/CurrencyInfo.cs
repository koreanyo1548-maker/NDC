using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using CodeStage.AntiCheat.ObscuredTypes;
using Controller;
using Data.DbDefinition;
using Data.DbShop;
using Data.DbUser;
using Data.DbUser.Currency;
using Data.DbUser.Progress;
using Data.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Data.Stores
{
    [Serializable]
    public class CurrencyInfo
    {
        public int version;
        public Currency currency;

        public CurrencyInfo(int version, Currency currency)
        {
            this.version = version;
            this.currency = currency;
        }
        
        public List<DbUserCurrency> ConvertToCurrency(DateTime now)
        {
            if (version < 1)
            {
                var levelPass = currency.pass.Find(p => p.type == (int) PassType.LevelPass);
                if (levelPass.premiums == null)
                {
                    levelPass.premiums = new [] {-1, 20, 41, 63, 83};
                    var last = levelPass.premium;
                    if (last >= 0 && currency.have[CurrencyType.LevelPass1]) levelPass.premiums[0] = Math.Min(last, 20);
                    if (last >= 21 && currency.have[CurrencyType.LevelPass2]) levelPass.premiums[1] = Math.Min(last, 41);
                    if (last >= 42 && currency.have[CurrencyType.LevelPass3]) levelPass.premiums[2] = Math.Min(last, 63);
                    if (last >= 64 && currency.have[CurrencyType.LevelPass4]) levelPass.premiums[3] = Math.Min(last, 83);
                    if (last >= 84 && currency.have[CurrencyType.LevelPass5]) levelPass.premiums[4] = Math.Min(last, 103);
                }
                var stagePass = currency.pass.Find(p => p.type == (int) PassType.StagePass);
                if (stagePass.premiums == null)
                {
                    stagePass.premiums = new [] {-1, 20, 41, 63, 83};
                    var last = stagePass.premium;
                    if (last >= 0 && currency.have[CurrencyType.StagePass1]) stagePass.premiums[0] = Math.Min(last, 20);
                    if (last >= 21 && currency.have[CurrencyType.StagePass2]) stagePass.premiums[1] = Math.Min(last, 41);
                    if (last >= 42 && currency.have[CurrencyType.StagePass3]) stagePass.premiums[2] = Math.Min(last, 63);
                    if (last >= 64 && currency.have[CurrencyType.StagePass4]) stagePass.premiums[3] = Math.Min(last, 83);
                    if (last >= 84 && currency.have[CurrencyType.StagePass5]) stagePass.premiums[4] = Math.Min(last, 103);
                }
                currency.pass.Find(p => p.type == (int) PassType.SoulPass).premiums = new[]{-1};
            }
            
            return new List<DbUserCurrency>
            {
                new(0, currency.money, currency.diaUse, currency.diaGain, currency.buyLog, currency.stones, currency.tickets, 
                    currency.books, currency.have, currency.etc, currency.buy, currency.bookshelves, currency.pass,
                    currency.usingCoupon, currency.adBuff, currency.mail, 
                    currency.costumes, currency.eventPackages, currency.eventPackageTime, currency.lastPassionRewardedTime)
            };
        }
    }
    
    
    [Serializable]
    public class Currency
    {
        public Dictionary<CurrencyType, ObscuredBigInteger> money;
        public BigInteger diaUse;
        public BigInteger diaGain;
        public Dictionary<int, int> buyLog;
        public Dictionary<CurrencyType, BigInteger> stones;
        public Dictionary<CurrencyType, int> tickets;
        public Dictionary<CurrencyType, int> books;
        public Dictionary<CurrencyType, bool> have;
        public Dictionary<CurrencyType, int> etc;
        public List<CurrencyPass> pass;
        public List<int> costumes;

        public List<DbUserBuying> buy;
        public List<CurrencyAdBuff> adBuff;
        public List<CurrencyBookshelf> bookshelves;
        public List<string> usingCoupon;
        public List<int> eventPackages;
        public Dictionary<int, DateTime> eventPackageTime;
        public DateTime lastPassionRewardedTime;
        public List<CurrencyMail> mail;

        public Currency()
        {
            
        }
        public Currency(DbUserCurrency currency)
        {
            Set(currency);
        }
        public void Set(DbUserCurrency currency)
        {
            money = DictionaryConverter.FieldToNormal<CurrencyType, DbField<ObscuredBigInteger>, ObscuredBigInteger>(currency.Money);
            diaUse = currency.DiaUse;
            diaGain = currency.DiaGain;
            buyLog = currency.BuyLog;
            stones = DictionaryConverter.FieldToNormal<CurrencyType, DbField<BigInteger>, BigInteger>(currency.Stones);
            tickets = DictionaryConverter.FieldToNormal<CurrencyType, DbField<int>, int>(currency.Tickets);
            books = DictionaryConverter.FieldToNormal<CurrencyType, DbField<int>, int>(currency.Books);
            have = DictionaryConverter.FieldToNormal<CurrencyType, DbField<bool>, bool>(currency.Have);
            etc = DictionaryConverter.FieldToNormal<CurrencyType, DbField<int>, int>(currency.Etc);
            
            buy = currency.Buy.ToList();
            pass = new List<CurrencyPass>();
            var passList = currency.Pass.ToList();
            foreach (var p in passList) pass.Add(new CurrencyPass(p));
            adBuff = new List<CurrencyAdBuff>();
            var buffList = currency.AdBuff.ToList();
            foreach (var b in buffList) adBuff.Add(new CurrencyAdBuff(b));
            mail = new List<CurrencyMail>();
            var mailList = currency.Mail.ToList();
            foreach (var m in mailList) mail.Add(new CurrencyMail(m));
            bookshelves = new List<CurrencyBookshelf>();
            var bookShelfList = currency.BookShelves.ToList();
            foreach (var b in bookShelfList) bookshelves.Add(new CurrencyBookshelf(b));
            usingCoupon = currency.UsingCoupon;
            costumes = currency.Costumes.ToList();
            eventPackages = currency.EventPackages;
            eventPackageTime = currency.EventPackageTime;
            lastPassionRewardedTime = currency.LastPassionRewardedTime;
        }

        [JsonConstructor]
        public Currency(Dictionary<CurrencyType, ObscuredBigInteger> money, BigInteger diaUse, BigInteger diaGain,
            Dictionary<int, int> buyLog, Dictionary<CurrencyType, BigInteger> stones,
            Dictionary<CurrencyType, int> tickets, Dictionary<CurrencyType, int> books, 
            Dictionary<CurrencyType, bool> have, Dictionary<CurrencyType, int> etc, List<CurrencyBookshelf> bookshelves,
            List<DbUserBuying> buy, List<CurrencyPass> pass, List<CurrencyAdBuff> adBuff, List<string> usingCoupon,
            List<CurrencyMail> mail, List<int> costumes, List<int> eventPackages, Dictionary<int, DateTime> eventPackageTime,
            DateTime lastPassionRewardedTime)
        {
            if (eventPackages == null) eventPackages = new();
            if (eventPackageTime == null) eventPackageTime = new();
            if (buyLog == null) buyLog = new();
            this.money = money;
            this.diaUse = diaUse;
            this.diaGain = diaGain;
            this.buyLog = buyLog;
            this.stones = stones;
            this.tickets = tickets;
            this.books = books;
            this.have = have;
            this.etc = etc;
            this.buy = buy;
            this.pass = pass;
            this.adBuff = adBuff;
            this.usingCoupon = usingCoupon;
            this.bookshelves = bookshelves;
            this.mail = mail;

            var realCostume = new List<int>();
            foreach (var c in costumes)
            {
                if (DbCostume.Get(c) != null) realCostume.Add(c);
            }
            this.costumes = realCostume;
            this.eventPackages = eventPackages;
            this.eventPackageTime = eventPackageTime;
            this.lastPassionRewardedTime = lastPassionRewardedTime;
        }
    }

    [Serializable]
    public class CurrencyPass
    {
        public int type;
        public int free;
        public int premium;
        public int[] premiums;
        public int progress;
        public int season;

        public CurrencyPass(DbUserPass pass)
        {
            type = (int)pass.PassType;
            free = pass.LastFreeRewarded;
            premiums = pass.LastPremiumRewarded;
            progress = pass.Progress;
            season = pass.Season.Value;
        }

        [JsonConstructor]
        public CurrencyPass(int type, int free, int premium, int[] premiums, int progress, int season)
        {
            this.type = type;
            this.free = free;
            this.premium = premium;
            this.premiums = premiums;
            this.progress = progress;
            this.season = season;
        }
    }

    [Serializable]
    public class CurrencyAdBuff
    {
        public int type;
        public int buffType;
        public DateTime startTime;
        public bool isUsing;
        public int leftTime;
        public int useCount;

        public CurrencyAdBuff(DbUserAdBuff adBuff)
        {
            type = (int)adBuff.AdBuffType;
            buffType = (int) adBuff.BuffType;
            startTime = adBuff.StartTime;
            isUsing = adBuff.IsUsing.Value;
            useCount = adBuff.UseCount;
        }

        [JsonConstructor]
        public CurrencyAdBuff(int type, int buffType, DateTime startTime, bool isUsing, int useCount)
        {
            this.type = type;
            if (buffType == 0) buffType = (int)DbAdBuff.Get((AdBuffType)type).BuffType;
            this.buffType = buffType;
            this.startTime = startTime;
            this.isUsing = isUsing;
            this.useCount = useCount;
        }
    }
    [Serializable]
    public class CurrencyMail
    {
        public int id;
        public int mailId;
        public bool isRewarded;
        public bool isHide;
        public bool isShop;
        public DateTime rewardedTime;
        public string receipt;

        public CurrencyMail(DbUserMail mail)
        {
            id = mail.Id;
            mailId = mail.MailId;
            isRewarded = mail.IsRewarded.Value;
            isHide = mail.IsHide.Value;
            isShop = mail.IsShop;
            rewardedTime = mail.RewardedTime;
            receipt = mail.Receipt;
        }

        [JsonConstructor]
        public CurrencyMail(int id, int mailId, bool isRewarded, bool isHide, bool isShop, string receipt, DateTime rewardedTime)
        {
            this.id = id;
            this.mailId = mailId;
            this.isRewarded = isRewarded;
            this.isHide = isHide;
            this.isShop = isShop;
            this.rewardedTime = rewardedTime;
            this.receipt = receipt;
        }
    }
    [Serializable]
    public class CurrencyBookshelf
    {
        public int index;
        public bool haveBook;
        public DateTime startTime;
        public int bookType;
        public bool useAd;
        public int leftTime;
        public bool useDia;

        public CurrencyBookshelf(DbUserBookshelf bookshelf)
        {
            index = bookshelf.Index;
            haveBook = bookshelf.HaveBook.Value;
            startTime = bookshelf.StartTime;
            bookType = (int)bookshelf.BookType;
            useAd = bookshelf.UseAd;
            useDia = bookshelf.UseDia;
        }

        [JsonConstructor]
        public CurrencyBookshelf(int index, DateTime startTime, bool haveBook, int bookType, bool useAd)
        {
            this.index = index;
            this.haveBook = haveBook;
            this.startTime = startTime;
            this.bookType = bookType;
            this.useAd = useAd;
        }
    }
}