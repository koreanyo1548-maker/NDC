using System;
using Data.Utils;
using UnityEngine;

namespace Data.Editor.EDbEquipment
{
    [Serializable]
    public class EDbWeapon
    {
        public int Id;
        public int NameId;
        public GradeType Grade;
        public FullGradeType FullGrade;
        public int EquipAttack;
        public int EquipGrowthAttack;
        public int OwnAttack;
        public int OwnGrowthAttack;
        public int OwnHp;
        public int OwnGrowthHp;
        public string Resource;
        public string SpineRightHandSkin;
        public int Awakening;
        public int AwakeningMaterial;
        public int PrevId;
        public int NextId;
    }
}
