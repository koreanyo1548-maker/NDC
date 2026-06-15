using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Data.Utils;
using Exceptions;
using Managers.Base;
using Newtonsoft.Json;
using Spine;
using UnityEngine;
using Utils;
using static System.Single;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbSkill : DbModel<DbSkill, int>, IDbEquipment, IDbCanSummon
    {
        public static Dictionary<GradeType, List<DbSkill>> GradeToSkills;
        
        public int NameId;
        public GradeType Grade;
        public FullGradeType FullGrade;
        public SFXType Sound;

        public float Delay;
        public int CoolTime;
        public SkillTarget Target;
        public List<SkillToDo> ToDos;
        public string Animation;
        public EffectPositionType EffectPosition;
        public string EffectPrefab;
        public bool ShakeOnHit;
        
        public string Resource;
        public int DescriptionId;

        public int GrowthAttack;
        public int Awakening;
        public int AwakeningMaterial;
        public string SkillTarget;
        public List<string> ToDo;
        
        public int PrevId;
        public int NextId;

        [IgnoreDataMember] public DbSkillAwakening AwakeningMeta => DbSkillAwakening.Get(Awakening);
        public override void Load()
        {
            fileName = "Skill";
            if (Application.isPlaying) Init();
            GradeToSkills = new();
            ForEach(s =>
            {
                s.Target = new SkillTarget(s.SkillTarget);
                s.ToDos = new List<SkillToDo>();
                foreach (var t in s.ToDo)
                {
                    s.ToDos.Add(new SkillToDo(t));
                }
                
                if (GradeToSkills.TryGetValue(s.Grade, out var skill)) skill.Add(s);
                else GradeToSkills.Add(s.Grade, new() {s});
            });
        }


        public string GetOwnDescription(int level, bool isIncreasing)
        {
            var str = LocalString.Get(DescriptionId);
            float additionalInfo = -999;
            for (var idx = 0; idx < ToDos.Count; ++idx)
            {
                switch (ToDos[idx].type)
                {
                    case SkillType.Stun:
                        additionalInfo = ToDos[idx].time;
                        break;
                    case SkillType.DotAttack:
                        var toDo = ToDos[idx];
                        return Description3(toDo.time, toDo.interval, toDo.attack);
                    case SkillType.Attack: 
                        if (additionalInfo != -999) return Description2(additionalInfo, ToDos[idx].attack);
                        return Description1(ToDos[idx].attack);
                    case SkillType.TargetAttack:
                        if (Target.targetCount != 1) return Description2(Target.targetCount, ToDos[idx].attack);
                        return Description1(ToDos[idx].attack);
                }
            }

            return string.Empty;

            string Description3(float value1, float value2, float value3)
            {
                if (isIncreasing)
                    return string.Format(str,
                        (int) value1, value2, value3 + "+" + GrowthAttack * (level-1) * 0.1f + "<color=#FFD53B> > " + (GrowthAttack * level * 0.1f) + "</color>");
                return string.Format(str, (int)value1, value2, value3 + "+" + (GrowthAttack * Math.Max(0, level-1) * 0.1f));
            }
            string Description2(float value1, float value2)
            {
                if (isIncreasing)
                    return string.Format(str,
                        (int) value1, value2 + "+" + GrowthAttack * (level-1) * 0.1f + "<color=#FFD53B> > " + (GrowthAttack * level * 0.1f) + "</color>");
                return string.Format(str, (int)value1, value2 + "+" + (GrowthAttack * Math.Max(0, level-1) * 0.1f));
            }
            string Description1(float value)
            {
                if (isIncreasing)
                    return string.Format(str,
                        (int) value + "+" + GrowthAttack * (level-1) * 0.1f + "<color=#FFD53B> > " + (GrowthAttack * level * 0.1f) + "</color>");
                return string.Format(str, (int)value + "+" + (GrowthAttack * Math.Max(0, level-1) * 0.1f));
            }
        }
        
        public int GetCount()
        {
            return Count;
        }

        public IDbAwakeningMaterial GetAwakeningMaterial()
        {
            return DbAwakeningMaterial.Get(AwakeningMaterial);
        }

        public IDbEquipmentAwakening GetAwakening()
        {
            return DbSkillAwakening.Get(Awakening);
        }

        public EquipmentType GetEquipmentType()
        {
            return EquipmentType.Skill;
        }

        public int GetId()
        {
            return Id;
        }

        public int GetNameId()
        {
            return NameId;
        }

        public GradeType GetGrade()
        {
            return Grade;
        }

        public FullGradeType GetFullGrade()
        {
            return FullGrade;
        }

        public int GetEquipStat()
        {
            return 0;
        }

        public StatType GetEquipStatType()
        {
            return StatType.None;
        }

        public int GetEquipGrowthStat()
        {
            return 0;
        }

        public StatType GetOwnStatType()
        {
            throw new NotDefinedValueException("OwnStat of Skill");
        }

        public int GetOwnStat()
        {
            throw new NotDefinedValueException("OwnStat of Skill");
        }

        public int GetOwnGrowthStat()
        {
            throw new NotDefinedValueException("OwnGrowthStat of Skill");
        }

        public string GetResource()
        {
            return Resource;
        }
        
        public int GetPrev()
        { 
            return PrevId;
        }

        public int GetNext()
        {
            return NextId;
        }
    }

    [Serializable]
    public class SkillToDo
    {
        public SkillType type;
        public int attack;
        public float interval;
        public float time;
        public float distance;
        public float delay;
        
        public SkillToDo(string str)
        {
            var strs = str.Split(":");
            try
            {
                type = (SkillType)Enum.Parse(typeof(SkillType), strs[0]);
                switch (type)
                {
                    case SkillType.Attack: case SkillType.TargetAttack:
                        attack = int.Parse(strs[1]);
                        break;
                    case SkillType.DotAttack:
                        attack = int.Parse(strs[1]);
                        interval = Parse(strs[2], NumberFormatInfo.InvariantInfo);
                        time = Parse(strs[3], NumberFormatInfo.InvariantInfo);
                        break;
                    case SkillType.Stun:
                        time = Parse(strs[1], NumberFormatInfo.InvariantInfo);
                        break;
                    case SkillType.Push:
                        distance = Parse(strs[1], NumberFormatInfo.InvariantInfo);
                        delay = Parse(strs[2], NumberFormatInfo.InvariantInfo);
                        interval = Parse(strs[3], NumberFormatInfo.InvariantInfo);
                        time = Parse(strs[4], NumberFormatInfo.InvariantInfo);
                        break;
                    case SkillType.Gather:
                        time = Parse(strs[1], NumberFormatInfo.InvariantInfo);
                        distance = Parse(strs[2], NumberFormatInfo.InvariantInfo);
                        break;
                    default:
                        throw new NotDefinedValueException($"{type} of DbSkill");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.Log(str);
                throw;
            }
        }
    }

    [Serializable]
    public class SkillTarget
    {
        public TargetRangeType targetRange;
        public TargetBaseType targetBase;
        public int targetCount;
        public float rangeFromPlayer;
        public float rangeFromBase;
        public float rangeFromPlayerExtra;

        public SkillTarget(string str)
        {
            var strs = str.Split(":");
            try
            {
                targetRange = (TargetRangeType)Enum.Parse(typeof(TargetRangeType), strs[0]);
                switch (targetRange)
                {
                    case TargetRangeType.Range:
                        targetBase = (TargetBaseType)Enum.Parse(typeof(TargetBaseType), strs[1]);
                        if (targetBase == TargetBaseType.Player)
                        {
                            rangeFromBase = Parse(strs[2], NumberFormatInfo.InvariantInfo);
                            targetCount = int.Parse(strs[3]);
                        }
                        else
                        {
                            rangeFromPlayer = Parse(strs[2], NumberFormatInfo.InvariantInfo);
                            rangeFromBase = Parse(strs[3], NumberFormatInfo.InvariantInfo);
                            targetCount = int.Parse(strs[4]);
                        }
                        break;
                    case TargetRangeType.Number:
                        rangeFromPlayer = Parse(strs[1], NumberFormatInfo.InvariantInfo);
                        targetCount = int.Parse(strs[2]);
                        break;
                    case TargetRangeType.Straight:
                        rangeFromPlayerExtra = Parse(strs[1], NumberFormatInfo.InvariantInfo);
                        rangeFromPlayer = Parse(strs[2], NumberFormatInfo.InvariantInfo);
                        rangeFromBase = Parse(strs[3], NumberFormatInfo.InvariantInfo);
                        break;
                    default:
                        throw new NotDefinedValueException($"{targetRange} of DbSkill");
                }
            }
            catch (Exception e)
            {
                Debug.Log(str);
                Console.WriteLine(e);
                throw;
            }
        }
    }
}