using System;
using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbEvent;
using Data.DbShop;
using Data.Utils;
using UnityEngine;

namespace Utils
{
    public class StringMaker
    {


        public static string SecondsToTimeFull(int time)
        {
            var hours = time / 3600;
            var minutes = (time - hours * 3600) / 60;
            var seconds = time - hours * 3600 - minutes * 60;
            
            return hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }
        
        public static string GetTimeString(TimeSpan time)
        {
            var timeStr = string.Empty;
            
            if (time.Days > 0) timeStr += string.Format(LocalString.Get(210137), time.Days);
            if (time.Hours > 0) timeStr += string.Format(LocalString.Get(210138), time.Hours);
            if (time.Days == 0 && time.Minutes > 0)
            {     
                timeStr += string.Format(LocalString.Get(210139), time.Minutes);
            }
            if (time.Days == 0 && time.Hours == 0 && time.Minutes == 0)
            {
                timeStr += string.Format(LocalString.Get(210139), 1);
            }
            return timeStr;
        }

        public static string GetDayTimeString(TimeSpan time)
        {
            if (time.Days > 0) return string.Format(LocalString.Get(210137), time.Days);
            if (time.Hours > 0) return string.Format(LocalString.Get(210138), time.Hours);
            if (time <= TimeSpan.Zero) return string.Format(LocalString.Get(210138), 0);
            return string.Format(LocalString.Get(210138), 1);
        }
        
        public static string GetTimeString(int time)
        {
            var hours = time / 3600;
            if (hours > 0) return string.Format(LocalString.Get(210138), hours);
            var minutes = (time - hours * 3600) / 60;
            if (minutes > 0) return string.Format(LocalString.Get(210139), minutes);
            return string.Format(LocalString.Get(210139), 1);
        }
        
        public static string GetAwakeningString(StatType type, float value, string value2 = "")
        {
            var stat = DbStat.Get(type);
            var str = LocalString.Get(stat.NameId);
            
            switch (type)
            {
                case StatType.SpecificSkillAttackBonus: return string.Format(str, value2, value);
                case StatType.Attack:
                case StatType.Hp:
                    return string.Format(str, value);
                default:
                    return string.Format(str, (value * stat.ShowMultiply).ToString(stat.ShowMultiply > 0.9f ? "N0": "N1"));
            }
        }
        
        public static string GetAwakeningStringWithColor(StatType type, float value, string value2 = "")
        {
            var stat = DbStat.Get(type);
            var str = LocalString.Get(stat.NameId);
            var splits = str.Split('{');
            if (splits.Length == 2)
            {
                str = splits[0] + "<color=#F8EF66>{" + splits[1] + "</color>";
            }
            else if (splits.Length == 3)
            {
                str = "{" + splits[0] + splits[1] + "<color=#F8EF66>{" + splits[2] + "</color>";
            }

            switch (type)
            {
                case StatType.SpecificSkillAttackBonus: return string.Format(str, value2, value);
                case StatType.Attack:
                case StatType.Hp:
                    return string.Format(str, value);
                default:
                    return string.Format(str, (value * stat.ShowMultiply).ToString(stat.ShowMultiply > 0.9f ? "N0": "N1"));
            }
        }

        public static string GetIncreaseString(StatType type, long value1, long value2)
        {
            var stat = DbStat.Get(type);
            var str = LocalString.Get(stat.NameId);
            switch (type)
            {
                case StatType.Attack: case StatType.Hp:
                    return string.Format(str,  value1) + "<color=#FFD53B> > " + value2 + "</color>";
                default:
                    return string.Format(str, (value1 * stat.ShowMultiply).ToString(stat.ShowMultiply > 0.9f ? "N0": "N1")) + "<color=#FFD53B> > " 
                        + (value2 * stat.ShowMultiply).ToString(stat.ShowMultiply > 0.9f ? "N0": "N1") + "</color>";
            }
        }
        
        public static string GetFinalStringWithColor(StatType type, long value, string color)
        {
            var stat = DbStat.Get(type);
            var str = LocalString.Get(stat.NameId);
            switch (type)
            {
                case StatType.Attack: case StatType.Hp:
                    return string.Format(str, "<color=#" + color + ">" + value+ "</color>");
                default:
                    var strs = str.Split("{0}%");
                    if (strs.Length == 1)
                    {
                        return strs[0] + "<color=#" + color + ">" + (value * stat.ShowMultiply).ToString(stat.ShowMultiply > 0.9f ? "N0": "N1") + "%</color>";
                    }
                    else
                    {
                        return strs[0] + "<color=#" + color + ">" + (value * stat.ShowMultiply).ToString(stat.ShowMultiply > 0.9f ? "N0": "N1") + "%</color>" + strs[1];
                    }
            }
        }

        public static string GetFinalString(StatType type, long value)
        {
            var stat = DbStat.Get(type);
            var str = LocalString.Get(stat.NameId);
            switch (type)
            {
                case StatType.Attack: case StatType.Hp:
                    return string.Format(str, value);
                default:
                    return string.Format(str, (value * stat.ShowMultiply).ToString(stat.ShowMultiply > 0.9f ? "N0": value.ToString()[value.ToString().Length-1].Equals('0') ? "N0" : "N1"));
            }
        }

        public static string GetCostumeOptionString(DbCostume costume)
        {
            var own = string.Empty;
            for (var idx = 0; idx < costume.Options.Count; ++idx)
            {
                own += GetFinalString(costume.Options[idx], costume.Values[idx]) + "\n";
            }
            if (own.Equals(string.Empty)) own = "-";
            return own;
        }

        public static string GetBuyLimitText(IDbShop shop)
        {
            var renewal = shop.GetRenewalInterval();
            return renewal == RenewalType.Infinite ? string.Empty : shop.GetBuyLimit() == - 1 ? string.Empty :
                string.Format(LocalString.Get(renewal == RenewalType.Daily ? 210287 : renewal == RenewalType.Monthly ? 210288 : renewal == RenewalType.Weekly ? 210360 : 210111), 
                                 CurrencyController.I.CanBuyCount(shop), shop.GetBuyLimit());
        }

        public static string GetBuyLimitText(DbDropEventShop shop)
        {
            var renewal = shop.RenewalInterval;
            return renewal == RenewalType.Infinite ? string.Empty : shop.BuyLimit == - 1 ? string.Empty :
                string.Format(LocalString.Get(renewal == RenewalType.Daily ? 210287 : renewal == RenewalType.Monthly ? 210288 : renewal == RenewalType.Weekly ? 210360 : 210111), 
                                 CurrencyController.I.CanBuyCount(shop), shop.BuyLimit);
        }
        
        public static string GetResetTime(int stringId)
        {
            var timeDiff = (DateTime.UtcNow - DateTime.Now).Hours + 9;
            return string.Format(LocalString.Get(stringId), timeDiff < 0 ? timeDiff + 24 : timeDiff);
        }

        public static string GetResetTime(int stringId, string arg1, string arg2)
        {
            var timeDiff = (DateTime.UtcNow - DateTime.Now).Hours + 9;
            return string.Format(LocalString.Get(stringId), timeDiff < 0 ? timeDiff + 24 : timeDiff, arg1, arg2);
        }

        public static string GetSummonName(SummonType summon)
        {
            return LocalString.Get(summon == SummonType.Accessory ? 210095 
               : summon == SummonType.Skill ? 210096
               : summon == SummonType.Weapon ? 210094 
               : summon == SummonType.Relic ? 210384 : 210395);
        }
        public static string GetSummonName(CurrencyType currency)
        {
            return LocalString.Get(currency == CurrencyType.Accessory ? 210095 
               : currency == CurrencyType.Weapon ? 210094 : 210096);
        }
    }
}