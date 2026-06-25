using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Data;
using Data.DbCommon;
using Exceptions;
using Managers;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

namespace Utils
{
    public static class Define
    {
        public static Vector3 LookLeft = new(-1, 1, 1);
        public static Vector3 LookRight = new(1, 1, 1);
        public static Vector2 Zero2 = new(0, 0);
        public static Vector2 PivotLeftHigh2 = new(0.2f, 0.6f);
        public static Vector2 PivotMiddle2 = new(0.5f, 0.5f);
        public static Vector3 Zero3 = new(0, 0, 0);
        public static Vector3 Resolution = new(1000, 2165, 0);
        public static Vector3 Shrink = new(0.9f, 0.9f, 1);
        public static Vector3 PointTwo = new(0.2f, 0.2f, 0.2f);
        public static Vector3 One = new(1, 1, 1);
        public static Vector3 Ten = new(10, 10, 1);
        public static Vector3 OnePointFive = new(1.5f, 1.5f, 1);
        public static Color AttackedColor = new(0.9f, 0.9f, 0.9f);
        public static Color BlackClear = new(0f, 0f, 0f, 0f);
        public static Vector3 HitParticlePositionDiff = new(0f, 0.4f, 0f);
        public static Vector3 SlightDown = new(0, -1, 0);
        public static Vector3 Rotate90 = new(0, 0, 90);
        public static Vector3 RotateNegative90 = new(0, 0, -90);
        
        public static Color Color878787 = new(0.53f, 0.53f, 0.53f);
        public static Color ColorCFCFCF = new(0.81f, 0.81f, 0.81f);
        public static Color ColorFF454A = new(1, 0.27f, 0.29f);
        public static Color Color49FFE9 = new(0.29f, 1f, 0.91f);
        public static Color Color009AFF = new(0f, 0.6f, 1f);
        public static Color ColorDC3B38 = new(0.86f, 0.23f, 0.22f);
        public static Color ColorBE00DE = new(0.75f, 0f, 0.87f);
        public static Color ColorFFB700 = new(1f, 0.72f, 0f);
        public static Color ColorFFE400 = new(1, 0.89f, 0);
        public static Color Color808080 = new(0.5f, 0.5f, 0.5f);
        public static Color ColorFFF8AA = new(1, 0.97f, 0.67f);
        public static Color ColorE4E4F1 = new(0.89f, 0.89f, 0.95f);
        public static Color Color5D6682 = new(0.36f, 0.40f, 0.51f);
        public static Color ColorF5C959 = new(0.96f, 0.79f, 0.35f);
        public static Color ColorFFED8D = new(1f, 0.93f, 0.55f);
        public static Color Color5E5E5E = new(0.37f, 0.37f, 0.37f);
        public static Color Color3D4365 = new(0.24f, 0.26f, 0.40f);
        public static Color ColorA5AECF = new(0.65f, 0.68f, 0.81f); 
        public static Color ColorFF4F40 = new(1f, 0.31f, 0.25f);
        public static Color ColorD0D7F1 = new(0.82f, 0.84f, 0.95f, 0.7f);
        private static Color Color8A00FF = new(0.54f, 0, 1);
        private static Color Color00C0FF = new(0, 0.75f, 1);
        private static Color ColorFF000E = new(1, 0, 0.05f);
        private static Color Color00FF13 = new(0, 1, 0.07f);
        public static Color Color2B3143 = new(0.17f, 0.19f, 0.26f);
        public static Color ColorTransparent = new(1, 1, 1, 0);
        public static Color Color88E9FF = new(0.53f, 0.91f, 1);
        public static Color ColorF7D25E = new(0.97f, 0.82f, 0.37f);
        public static Color ColorE76A6A = new(0.91f, 0.42f, 0.42f);
        public static Color Color58CFE0 = new(0.35f, 0.81f, 0.88f);
        public static Color Color2492A4 = new(0.14f, 0.57f, 0.64f);
        public static Color ColorB4B4B4 = new(0.71f, 0.71f, 0.71f);
        public static Color ColorD0D2D9 = new(0.82f, 0.82f, 0.85f);
        public static Color ColorE3CFAA = new(0.89f, 0.81f, 0.67f);
        public static Color Color87E590 = new(0.53f, 0.90f, 0.56f);
        public static Color Color6CC4FF = new(0.42f, 0.77f, 1f);
        public static Color ColorFF999C = new(1f, 0.60f, 0.61f);
        public static Color ColorEF8DFF = new(0.94f, 0.55f, 1f);
        public static Color ColorFFE345 = new(1, 0.89f, 0.27f);
        public static Color ColorA1A1A1 = new(0.63f, 0.63f, 0.63f);
        public static Color Color7D7D7D = new(0.49f, 0.49f, 0.49f);
        public static Color ColorFFC34F = new(1, 0.76f, 0.31f);
        public static Color ColorE7CA93 = new(0.91f, 0.79f, 0.58f);
        public static Color Color767682 = new(0.46f, 0.46f, 0.51f);

        public static string KillWhenGetMail = "killWhenGetMail";
        public static string KillWhenPlayerDieTag = "killWhenDiePlayer";
        public static string KillSaveTag = "killWhenSave";
        public static string KillPowerSavingTimer = "killWhenMove";
        public static string StarIcon = "Icon_AwakeningGrade";
        public static string LockIcon = "Icon_Lock_128x128";
        public static string EmptySprite = "none";
        public static string NoneEquipSprite = "None";
        
        private static TMP_ColorGradient _playerAttackColor = Resources.Load<TMP_ColorGradient>("Prefabs/Fonts/PlayerAttack");
        private static TMP_ColorGradient _criticalAttackColor = Resources.Load<TMP_ColorGradient>("Prefabs/Fonts/PlayerAttack_critical");
        private static TMP_ColorGradient _skillAttackColor = Resources.Load<TMP_ColorGradient>("Prefabs/Fonts/PlayerAttack_skill");
        private static TMP_ColorGradient _skillCriticalAttackColor = Resources.Load<TMP_ColorGradient>("Prefabs/Fonts/PlayerAttack_skillCritical");
        private static TMP_ColorGradient _monsterAttackColor = Resources.Load<TMP_ColorGradient>("Prefabs/Fonts/MonsterAttack");

        private static Material _grayscaleMaterial;
        private static Material _defaultMaterial;

        // private static Color Unique = new(0, 34.78f, 65.22f); // #0057A6
        // private static Color Heroic = new(1, 0, 0); // #FF0000
        // private static Color Legendary = new(31.11f, 0, 64.89f);
        // private static Color Mythic = new(68.55f, 31.45f, 0);

        private static Color Heroic = new(1, 0.26f, 0.41f); // #FF4269
        private static Color Legendary = new(0.72f, 0.26f, 1); // #B742FF
        private static Color Mythic = new(1, 0.73f, 0.26f); // #FFB942

        
        public static TimeSpan ASecond = new TimeSpan(0, 0, 1);
        
        public static Dictionary<PassType, List<CurrencyType>> passToCurrency = new()
        {
            {PassType.LevelPass, new List<CurrencyType> {CurrencyType.LevelPass1, CurrencyType.LevelPass2, CurrencyType.LevelPass3, CurrencyType.LevelPass4, CurrencyType.LevelPass5}},
            {PassType.StagePass, new List<CurrencyType> {CurrencyType.StagePass1, CurrencyType.StagePass2, CurrencyType.StagePass3, CurrencyType.StagePass4, CurrencyType.StagePass5}},
            {PassType.SoulPass, new List<CurrencyType> {CurrencyType.SoulPass}}
        };

        public static string chatSplit = ">:<]";
        
        public static int GetShopId(CurrencyType currency)
        {
            return currency switch
            {
                CurrencyType.SkillPreset3 => 512,
                CurrencyType.SkillPreset4 => 513,
                CurrencyType.SkillPreset5 => 514,
                CurrencyType.Bookshelf2 => 515,
                CurrencyType.Bookshelf3 => 516,
                CurrencyType.AbilityPreset3 => 517,
                CurrencyType.AbilityPreset4 => 518,
                CurrencyType.AbilityPreset5 => 519,
                _ => throw new NotDefinedCurrencyException(currency)
            };
        }

        public static CurrencyType EquipmentToGrowthStone(EquipmentType equipment)
        {
            CurrencyType currency;
            switch (equipment)
            {
                case EquipmentType.Accessory: currency = CurrencyType.AccessoryGrowthStone;
                    break;
                case EquipmentType.Skill: currency = CurrencyType.SkillGrowthStone;
                    break;
                case EquipmentType.Weapon: currency = CurrencyType.WeaponGrowthStone;
                    break;
                case EquipmentType.Pet: currency = CurrencyType.PetGrowthStone;
                    break;
                default:
                    throw new NotDefinedEquipmentException(equipment);
            }

            return currency;
        }
        
        public static void Set()
        {
            _grayscaleMaterial = Manager.Resource.Load<Material>("Materials/UI_GrayScale");
            _defaultMaterial = Canvas.GetDefaultCanvasMaterial();
        }

        public static Color GetGradeTextColor(GradeType grade)
        {
            if (grade == GradeType.Normal) return ColorD0D2D9;
            if (grade == GradeType.Magic) return ColorE3CFAA;
            if (grade == GradeType.Rare) return Color87E590;
            if (grade == GradeType.Unique) return Color6CC4FF;
            if (grade == GradeType.Heroic) return ColorFF999C;
            if (grade == GradeType.Legendary) return ColorEF8DFF;
            return ColorFFE345;
        }

        public static Color GetDungeonColor(FieldType field)
        {
            if (field == FieldType.Awakening) return Color8A00FF;
            if (field == FieldType.SkillGrowth) return Color00C0FF;
            if (field == FieldType.BlackMarket) return ColorFF000E;
            if (field == FieldType.Dia) return Color00FF13;
            if (field == FieldType.Pet) return Color00C0FF;
            return Color.white;
        }
        
        public static Material GetUIMaterial(bool isGray)
        {
            if (isGray) return _grayscaleMaterial;
            return _defaultMaterial;
        }
        
        public static Color SummonFxColor(GradeType grade)
        {
            if (grade == GradeType.Heroic) return Heroic;
            if (grade == GradeType.Legendary) return Legendary;
            if (grade == GradeType.Mythic) return Mythic;
            return Color.white;
        }
        
        // public static Color SummonParticleColor(GradeType grade)
        // {
        //     if (grade == GradeType.Unique) return Color009AFF;
        //     if (grade == GradeType.Heroic) return ColorDC3B38;
        //     if (grade == GradeType.Legendary) return ColorBE00DE;
        //     if (grade == GradeType.Mythic) return ColorFFB700;
        //     return Color.white;
        // }
        
        public static float DamageSize(bool isPlayerAttack)
        {
            if (isPlayerAttack) return 5;
            return 3;
        }
        public static TMP_ColorGradient DamageColor(bool isPlayerAttack, AttackType attackType = AttackType.Normal)
        {
            if (attackType == AttackType.Critical) return _criticalAttackColor;
            if (attackType == AttackType.Skill) return _skillAttackColor;
            if (attackType == AttackType.SkillCritical) return _skillCriticalAttackColor;
            if (isPlayerAttack) return _playerAttackColor;
            return _monsterAttackColor;
        }

        private static string[] units = {"", "A", "B", "C", "D", "E", "F", "G", "H"};
        private static char point = '.';
        private static Dictionary<int, string> zeros = new() {{1, "0"}, {2, "00"}, {3, "000"}, {4, "0000"}};
        public static string AddUnit(BigInteger number, int digit, int behindPoint)
        {
            if (IsKoreanLocale()) return AddKoreanUnit(number);

            var num = number.ToString();
            var realDigit = num.Length;
            
            var unit = 0;   
            while (realDigit > digit)
            {
                unit++;
                realDigit -= 3;
            }

            string tmp = "??";
            if (unit < units.Length)
                tmp = units[unit];

            var realBehindPoint = Math.Min(num.Length - realDigit, behindPoint);
            if (realBehindPoint == 0) return AddChunkSeparator(num.Substring(0, realDigit)) + tmp;
            var behinds = num.Substring(realDigit, realBehindPoint);
            if (behinds.Equals(zeros[realBehindPoint])) return AddChunkSeparator(num.Substring(0, realDigit)) + tmp;

            return AddChunkSeparator(num.Substring(0, realDigit)) + point + behinds + tmp;
        }

        private static bool IsKoreanLocale()
        {
            var locale = LocalizationSettings.SelectedLocale;
            return locale != null && locale.Identifier.Code.StartsWith("ko");
        }

        private static string[] koreanUnits = {"", "만", "억", "조", "경", "해"};
        private static string AddKoreanUnit(BigInteger number)
        {
            if (number < 0) return "-" + AddKoreanUnit(BigInteger.Abs(number));
            if (number < 1000) return number.ToString();
            if (number < 10000)
            {
                var thousand = number / 1000;
                var rest = number % 1000;
                return rest == 0 ? $"{thousand}천" : $"{thousand}천{rest}";
            }

            if (number >= 100000000000)
            {
                number -= number % 100000000;
            }
            else if (number >= 10000000)
            {
                number -= number % 10000;
            }

            var groups = new List<int>();
            while (number > 0)
            {
                groups.Add((int)(number % 10000));
                number /= 10000;
            }

            var builder = new StringBuilder();
            var visibleGroups = 0;
            for (var idx = groups.Count - 1; idx >= 0 && visibleGroups < 2; --idx)
            {
                var group = groups[idx];
                if (group == 0) continue;

                builder.Append(group);
                builder.Append(idx < koreanUnits.Length ? koreanUnits[idx] : "??");
                visibleGroups++;
            }

            return builder.ToString();
        }

        public static float GetCanvasRatio()
        {
            var ratio = 1f * Screen.height / Screen.width;
            if (ratio > 2165 / 1000f) return 0;
            return Mathf.Clamp(-0.0556f / (2165/1000f - ratio) + 0.7473f, 0, 1);
            // var ratio = 1f * Screen.height / Screen.width;
            // return Mathf.Clamp((1.7f - ratio) * 2, 0, 1);
        }

        private static int _chunkLen = 3;
        private static char _comma = ',';
        private static string AddChunkSeparator (string str)
        {
            if (str == null || str.Length < _chunkLen) {
                return str;
            }
            var builder = new StringBuilder();
            var first = str.Length%3;
            if (first == 0) first = 3;
            builder.Append(str, 0, first);
            for (var index = first; index < str.Length; index += _chunkLen) {
                builder.Append(_comma);
                builder.Append(str, index, _chunkLen);
            }
            return builder.ToString();
        }

        public static int GetDayDiff(DateTime prev, DateTime now)
        {
            if (now.Year == prev.Year) return now.DayOfYear - prev.DayOfYear;
            var days = new DateTime(prev.Year, 12, 31).DayOfYear - prev.DayOfYear + now.DayOfYear;
            for (var idx = prev.Year + 1; idx < now.Year; ++idx)
            {
                days += new DateTime(idx, 12, 31).DayOfYear;
            }

            return days;
        }

        public static int GetMonthDiff(DateTime prev, DateTime now)
        {
            if (now.Year == prev.Year) return now.Month - prev.Month;

            return 12 - prev.Month + now.Month + 12 * (now.Year - prev.Year - 1);
        }

        public static bool IsWeekDiff(DateTime prev, DateTime now, int dayDiff, DayOfWeek resetDay = DayOfWeek.Monday)
        {
            var lastDay = DayToInt(prev.DayOfWeek);
            var nowDay = DayToInt(now.DayOfWeek);

            return nowDay < lastDay || dayDiff > 6;

            int DayToInt(DayOfWeek day)
            {
                if (day < resetDay) return (int)day + 7;
                return (int)day;
            }
        }

        public static List<DbRewardBig> SmallToBigReward(List<DbReward> rewards)
        {
            var bigReward = new List<DbRewardBig>(rewards.Count);
            foreach (var r in rewards)
            {
                bigReward.Add(new DbRewardBig(r.currencyType, r.count, r.id));
            }
            return bigReward;
        }

        public static CurrencyType SummonTypeToTicket(SummonType summonType)
        {
            switch (summonType)
            {
                case SummonType.Weapon: return CurrencyType.WeaponSummonTicket;
                case SummonType.Accessory: return CurrencyType.AccessorySummonTicket;
                case SummonType.Skill: return CurrencyType.SkillSummonTicket;
                case SummonType.Relic: return CurrencyType.RelicSummonTicket;
                case SummonType.Necklace: return CurrencyType.NecklaceSummonTicket;
                default: throw new Exception($"{summonType} has no defined ticket");
            }
        }

        public static CurrencyType SummonTypeToAdTicket(SummonType summonType)
        {
            switch (summonType)
            {
                case SummonType.Weapon: return CurrencyType.AdWeaponSummonTicket;
                case SummonType.Accessory: return CurrencyType.AdAccessorySummonTicket;
                case SummonType.Skill: return CurrencyType.AdSkillSummonTicket;
                case SummonType.Relic: return CurrencyType.AdRelicSummonTicket;
                case SummonType.Necklace: return CurrencyType.AdNecklaceSummonTicket;
                default: throw new Exception($"{summonType} has no defined ticket");
            }
        }

        public static EquipmentType SummonTypeToEquipmentType(SummonType summonType)
        {
            switch (summonType)
            {
                case SummonType.Weapon: return EquipmentType.Weapon;
                case SummonType.Accessory: return EquipmentType.Accessory;
                case SummonType.Skill: return EquipmentType.Skill;
                default: throw new Exception($"{summonType} has no defined equipment");
            }
        }

        public static CurrencyType SummonCurrencyToCurrency(SummonCurrency summonCurrency, SummonType summonType)
        {
            if (summonCurrency == SummonCurrency.Dia) return CurrencyType.Dia;
            if (summonCurrency == SummonCurrency.BeadsOre) return CurrencyType.BeadsOre;
            return SummonTypeToTicket(summonType);
        }
        
        public static string LanguageIndexToCode(int language)
        {
            switch (language)
            {
                case 0: return "en";
                case 1: return "ko";
                case 2: return "zh-hans";
                case 3: return "zh-hant";
                case 4: return "ja";
                default: return "en"; 
            }
        }

        public static int ToInt(this BigInteger value)
        {
            if (value <= 0)
                return 0;

            double val = (double)value;
            string str = val.ToString("e").Substring(11);
            str += val.ToString("0000000").Substring(0, 7);
            return int.Parse(str);
        }

        public static BigInteger ToBigInt(this int value)
        {
            if (value <= 0)
                return 0;

            int e = (int)(value * 0.000001);
            int v = value % 1000000;
            return (BigInteger)(v * Math.Pow(10, e));
        }
    }
        
    public enum UIEvent
    {
        Click,
        Down,
        Up,
        LongClick
    }

    public enum UIEffectType
    {
        None,
        // Expend,
        // Shrink,
        Bounce,
        BiBounce        
    }
    public enum MouseEvent
    {
        Press,
        PointerDown,
        PointerUp,
        Click
    }
    public enum SceneType
    {
        Unknown,
        Field,
        Login,
        Prologue
    }

    public enum SoundType
    {
        Bgm,
        Effect,
        MaxCount
    }

    public enum LanguageType
    {
        English = 0,
        Korean = 1,
        Japanese = 2,
        ChineseSimplified = 3,
        ChineseTraditional = 4
    }

    public enum GameOverType
    {
        StageFail,
        StageMove,
        DungeonFail,
        Success,
        GiveUp
    }

    public enum SummonCurrency
    {
        Dia, 
        Ticket,
        BeadsOre,
        None = 999
    }
    
    public enum AttackType
    {
        Normal,
        Critical,
        Skill,
        SkillCritical
    }
}
