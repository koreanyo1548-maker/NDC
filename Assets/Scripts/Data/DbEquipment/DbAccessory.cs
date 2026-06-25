using System;
using System.Runtime.Serialization;
using Data.Utils;
using Exceptions;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbAccessory : DbModel<DbAccessory, int>, IDbEquipment, IDbCanSummon
    {
        public int NameId;
        public GradeType Grade;
        public FullGradeType FullGrade;
        public int EquipHp;
        public int EquipGrowthHp;
        public int OwnHp;
        public int OwnGrowthHp;
        public string Resource;
        public string SpineLeftHandSkin;
        public int Awakening;
        public int AwakeningMaterial;
        public int PrevId;
        public int NextId;

        public override void Load()
        {
            fileName = "Accessory";
            if (Application.isPlaying) Init();
        }

        public IDbAwakeningMaterial GetAwakeningMaterial()
        {
            return DbAwakeningMaterial.Get(AwakeningMaterial);
        }

        public IDbEquipmentAwakening GetAwakening()
        {
            return DbAccessoryAwakening.Get(Awakening);
        }

        public string GetOwnDescription(int level, bool isIncreasing)
        {
            return string.Empty;
        }

        public int GetCount()
        {
            return Count;
        }
        
        public EquipmentType GetEquipmentType()
        {
            return EquipmentType.Accessory;
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
            return EquipHp;
        }

        public StatType GetEquipStatType()
        {
            return StatType.HpBonus;
        }

        public int GetEquipGrowthStat()
        {
            return EquipGrowthHp;
        }

        public StatType GetOwnStatType()
        {
            return StatType.HpBonus;
        }

        public int GetOwnStat()
        {
            return OwnHp;
        }

        public int GetOwnGrowthStat()
        {
            return OwnGrowthHp;
        }

        public string GetResource()
        {
            return Resource;
        }

        public string GetSpineLeftHandSkin()
        {
            return SpineLeftHandSkin;
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
}
