using System;
using System.Collections.Generic;
using Data.DbEquipment;
using Data.Utils;
using Exceptions;
using UnityEngine;

namespace Data.DbPetInfo
{
    [Serializable]
    public class DbPet: DbModel<DbPet, int>, IDbEquipment
    {
        public static Dictionary<GradeType, List<DbPet>> GradeToPets;
        
        public int NameId;
        public GradeType Grade;
        public int EquipAttack;
        public int EquipHp;
        public int EquipGrowthAttack;
        public int EquipGrowthHp;
        public int Awakening;
        public int AwakeningMaterial;
        public string Resource;
        public int BibleHpBonus;
        public int PrevId;
        public int NextId;
        
        public override void Load()
        {
            fileName = "Pet";
            if (Application.isPlaying) Init();
            
            GradeToPets = new();
            ForEach(p =>
            {
                if (GradeToPets.TryGetValue(p.Grade, out var pets)) pets.Add(p);
                else GradeToPets.Add(p.Grade, new() {p});
            });
        }

        public EquipmentType GetEquipmentType()
        {
            return EquipmentType.Pet;
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
            throw new NotDefinedValueException("FullGrade of DbPet");
        }


        /// <summary>
        /// 아래 Stat들은 UI에만 쓰기 위한 용도로,
        /// 그러면 안되지만 Equip = EquipAttack으로
        /// Own = EquipHp로 사용중
        /// </summary>
        public int GetEquipStat()
        {
            return EquipAttack;
        }

        public StatType GetEquipStatType()
        {
            return StatType.PetAttackBonus;
        }

        public int GetEquipGrowthStat()
        {
            return EquipGrowthAttack;
        }

        public StatType GetOwnStatType()
        {
            return StatType.PetHpBonus;
        }

        public int GetOwnStat()
        {
            return EquipHp;
        }

        public int GetOwnGrowthStat()
        {
            return EquipGrowthHp;
        }
        
        public string GetResource()
        {
            return Resource;
        }

        public int GetCount()
        {
            return Count;
        }

        public IDbAwakeningMaterial GetAwakeningMaterial()
        {
            return DbPetAwakeningMaterial.Get(AwakeningMaterial);
        }

        public IDbEquipmentAwakening GetAwakening()
        {
            return DbPetAwakening.Get(Awakening);
        }

        public string GetOwnDescription(int level, bool isIncreasing)
        {
            throw new Exception();
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