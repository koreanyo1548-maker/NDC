using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbEquipment
{
    [Serializable]
    public class EDbAccessory
    {
        public int Id;
        public int NameId;
        public GradeType Grade;
        public FullGradeType FullGrade;
        public int EquipHp;
        public int EquipGrowthHp;
        public int OwnAttack;
        public int OwnGrowthAttack;
        public int OwnHp;
        public int OwnGrowthHp;
        public string Resource;
        public int Awakening;
        public int AwakeningMaterial;
        public int PrevId;
        public int NextId;
    }
}