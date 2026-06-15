using System;

namespace Data.Editor.EDbPetInfo
{
    [Serializable]
    public class EDbPet
    {
        public int Id;
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
    }
}