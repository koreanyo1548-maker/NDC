using System;
using System.Runtime.Serialization;
using Data.Utils;
using Exceptions;
using UnityEngine;

namespace Data.DbEquipment
{
    [Serializable]
    public class DbWeapon : DbModel<DbWeapon, int>, IDbEquipment, IDbCanSummon
    {
        public int NameId;
        public GradeType Grade;
        public FullGradeType FullGrade;
        public int EquipAttack;
        public int EquipGrowthAttack;
        public int OwnAttack;
        public int OwnGrowthAttack;
        public string Resource;
        public int Awakening;
        public int AwakeningMaterial;
        public int PrevId;
        public int NextId;
        
        public override void Load()
        {
            fileName = "Weapon";
            if (Application.isPlaying) Init();
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
            return DbWeaponAwakening.Get(Awakening);
        }

        public string GetOwnDescription(int level, bool isIncreasing)
        {
            return string.Empty;
        }


        public EquipmentType GetEquipmentType()
        {
            return EquipmentType.Weapon;
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
            return EquipAttack;
        }

        public StatType GetEquipStatType()
        {
            return StatType.AttackBonus;
        }

        public int GetEquipGrowthStat()
        {
            return EquipGrowthAttack;
        }


        public StatType GetOwnStatType()
        {
            return StatType.AttackBonus;
        }

        public int GetOwnStat()
        {
            return OwnAttack;
        }

        public int GetOwnGrowthStat()
        {
            return OwnGrowthAttack;
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
}