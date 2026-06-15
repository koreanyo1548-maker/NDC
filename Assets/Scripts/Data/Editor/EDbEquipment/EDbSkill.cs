using System;
using System.Collections.Generic;
using Managers.Base;

namespace Data.Editor.EDbEquipment
{
    [Serializable]
    public class EDbSkill
    {
        public int Id;
        public int NameId;
        public GradeType Grade;
        public FullGradeType FullGrade;
        public SFXType Sound;

        public float Delay;
        public int CoolTime;
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
    }
}