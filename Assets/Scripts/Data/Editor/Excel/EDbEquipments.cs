using System.Collections.Generic;
using Data.DbEquipment;
using Data.Editor.EDbEquipment;
using UnityEngine;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Equipment")]
    public class EDbEquipments : ScriptableObject
    {
        [SerializeField] public List<EDbAccessory> Accessory;
        [SerializeField] public List<EDbAccessoryAwakening> AccessoryAwakening;
        [SerializeField] public List<EDbAwakeningMaterial> AwakeningMaterial;
        [SerializeField] public List<EDbGrowthMaterial> GrowthMaterial;
        [SerializeField] public List<EDbSkill> Skill;
        [SerializeField] public List<EDbSkillAwakening> SkillAwakening;
        [SerializeField] public List<EDbSkillDecomposition> SkillDecomposition;
        [SerializeField] public List<EDbWeapon> Weapon;
        [SerializeField] public List<EDbWeaponAwakening> WeaponAwakening;
    }

}