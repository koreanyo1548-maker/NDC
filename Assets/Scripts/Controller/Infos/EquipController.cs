using System;
using Controller.Play;
using Controller.Utils;
using Data;
using Data.DbNecklaceInfo;
using Data.DbPetInfo;
using Data.DbShop;
using Data.DbUser.Equipment;
using Data.DbUser.Progress;
using Exceptions;
using Utils;

namespace Controller.Infos
{
    public class EquipController: Singleton<EquipController>
    {
        public static DbUserEquip data = DbUserEquip.Get(0);
        public static ControllerField<int> curSkillPreset = new(0);

        private bool _isInit;
        
        public void Init()
        {
            if (_isInit) return;

            _isInit = true;
        }

        
        #region Skill Preset
        
        public void SetCurSkillPreset(int idx)
        {
            curSkillPreset.check = true;
            curSkillPreset.Value = idx;
        }
        
        public void ChangeCurSkillPreset(FieldType fieldType, bool isBoss = false)
        {
            curSkillPreset.check = false;
            switch (fieldType)
            {
                case FieldType.Stage: case FieldType.Promotion:
                    if (isBoss) curSkillPreset.Value = SettingController.data.BossSkillPreset.Value;
                    else curSkillPreset.Value = SettingController.data.NormalSkillPreset.Value;
                    break;
                case FieldType.Awakening:
                    curSkillPreset.Value = SettingController.data.ASDungeonSkillPreset.Value;
                    break;
                case FieldType.SkillGrowth:
                    curSkillPreset.Value = SettingController.data.SGSDungeonSkillPreset.Value;
                    break;
                case FieldType.Pet:
                    curSkillPreset.Value = SettingController.data.PetDungeonSkillPreset.Value;
                    break;
                case FieldType.BlackMarket:
                    curSkillPreset.Value = SettingController.data.BMDungeonSkillPreset.Value;
                    break;
                case FieldType.Dia:
                    curSkillPreset.Value = SettingController.data.DiaDungeonSkillPreset.Value;
                    break;
                case FieldType.Training:
                    curSkillPreset.Value = SettingController.data.TrainingSkillPreset.Value;
                    break;
            }
        }
        
        #endregion
        
        
        #region IsEquipped
        
        public bool IsEquipped(EquipmentType equipmentType, int id, int presetIdx = 0)
        {
            switch (equipmentType)
            {
                case EquipmentType.Weapon: return data.Weapon.Value.Equals(id);
                case EquipmentType.Accessory: return data.Accessory.Value.Equals(id);
                case EquipmentType.Pet: return data.Pets.Have(p => p == id);
                case EquipmentType.Skill: return data.Skills[presetIdx].Have(s => s == id);
                default: throw new Exception(equipmentType + " is not defined equipment type");
            }
        }
        public bool IsEquipped(DbUserWeapon weapon)
        {
            return data.Weapon.Value.Equals(weapon.Id);
        }
        public bool IsEquipped(DbUserAccessory accessory)
        {
            return data.Accessory.Value.Equals(accessory.Id);
        }
        public bool IsEquipped(DbUserTitle title)
        {
            return data.Title.Value.Equals(title.Id);
        }
        public bool IsEquipped(CostumePositionType position, int costume)
        {
            if (position == CostumePositionType.Body) return data.BodyCostume.Value.Equals(costume);
            return data.WeaponCostume.Value.Equals(costume);
        }
        
        public bool IsEquipped(DbUserSkill skill, int presetIdx)
        {
            return data.Skills[presetIdx].Have(s => s == skill.Id);
        }

        public bool IsEquipped(DbUserSkill skill)
        {
            for (var idx = 0; idx < 5; ++idx)
            {
                for (var jdx = 0; jdx < 4; ++jdx)
                {
                    if (data.Skills[idx].Value[jdx].Value == skill.Id) return true;
                }
            }

            return false;
        }

        public bool IsEquipped(DbUserNecklace necklace)
        {
            return data.Necklaces.Have(n => n == necklace.Id);
        }
        public bool IsEquipped(DbUserPet pet)
        {
            return data.Pets.Have(p => p == pet.Id);
        }

        public bool IsEquipped(DbProfile profile)
        {
            return data.Profile.Value == profile.Id;
        }

        public int GetEquipped(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Weapon:
                    return data.Weapon.Value;
                case EquipmentType.Accessory:
                    return data.Accessory.Value;
                default:
                    throw new NotDefinedEquipmentException(type);
            }
        }

        public int GetEquippedNecklace(int index)
        {
            return data.Necklaces.Value[index].Value;
        }
        
        public bool ASkillEquipped(int skillIdx)
        {
            for (var idx = 0; idx < 5; ++idx)
            {
                if (data.Skills[idx].Value[skillIdx].Value != -1) return true;
            }
            return false;
        }

        public bool APetEquipped()
        {
            foreach (var petField in data.Pets.Value)
            {
                if (petField.Value != -1) return true;
            }

            return false;
        }
        #endregion

        
        #region Equip

        public int EquipRecommendation(EquipmentType type)
        {
            var id = 0;
            var equipStat = 0;
            switch (type)
            {
                case EquipmentType.Weapon:
                    DbUserWeapon.ForEach(w =>
                    {
                        if (!w.Have.Value) return;
                        
                        var meta = w.Meta;
                        var curStat = meta.GetEquipStat() + meta.GetEquipGrowthStat() * (w.Growth.Value - 1);
                        if (curStat > equipStat)
                        {
                            id = w.Id;
                            equipStat = curStat;
                        }
                    });
                    QuestController.I.DoQuests(QuestType.EquipRecommendWeapon);
                    break;
                case EquipmentType.Accessory:
                    DbUserAccessory.ForEach(a =>
                    {
                        if (!a.Have.Value) return;
                        
                        var meta = a.Meta;
                        var curStat = meta.GetEquipStat() + meta.GetEquipGrowthStat() * (a.Growth.Value - 1);
                        if (curStat > equipStat)
                        {
                            id = a.Id;
                            equipStat = curStat;
                        }
                    });
                    QuestController.I.DoQuests(QuestType.EquipRecommendAccessory);
                    break;
                default:
                    throw new NotDefinedEquipmentException(type);
            }
            Equip(type, id);
            return id;
        }
        
        public void Equip(EquipmentType type, int id)
        {
            if (IsEquipped(type, id)) return;
            switch (type)
            {
                case EquipmentType.Weapon:
                    data.Weapon.Value = id;
                    TotalStatController.I.Apply(StatType.AttackBonus);
                    QuestController.I.SetQuest(QuestType.CheckWeaponEquip, 1);
                    break;
                case EquipmentType.Accessory:
                    data.Accessory.Value = id;
                    TotalStatController.I.Apply(StatType.HpBonus);
                    QuestController.I.SetQuest(QuestType.CheckAccessoryEquip, 1);
                    break;
                default:
                    throw new NotDefinedEquipmentException(type);
            }
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }
        
        public void Equip(DbUserTitle title)
        {
            if (IsEquipped(title)) data.Title.Value = 0;
            else data.Title.Value = title.Id;

            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }

        public void Equip(DbProfile profile)
        {
            data.Profile.Value = profile.Id;
        }
        
        public void EquipCostume(CostumePositionType position, int costume)
        {
            if (IsEquipped(position, costume))
            {
                if (position == CostumePositionType.Body) data.BodyCostume.Value = 0;
                else data.WeaponCostume.Value = 0;
            }
            else
            {
                if (position == CostumePositionType.Body) data.BodyCostume.Value = costume;
                else data.WeaponCostume.Value = costume;
            }
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }

        public void ChangeSkillEquip(int idx, int id, int presetIdx)
        {
            data.Skills[presetIdx].SetValue(idx, id);
            if (id != -1)
            {
                if (idx == 0) QuestController.I.SetQuest(QuestType.CheckSkill1Equip, 1);
                else if (idx == 1) QuestController.I.SetQuest(QuestType.CheckSkill2Equip, 1);
            }
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Info);
        }

        public bool EquipNecklace(int idx, DbUserNecklace necklace)
        {
            if (idx == -1)
            {
                var emptyIdx = data.Necklaces.Value.FindIndex(n => n.Value == -1);
                if (emptyIdx == -1) return false;
                idx = emptyIdx;
            }
            var prev = GetEquippedNecklace(idx);
            data.Necklaces.SetValue(idx, necklace.Id);
            if (prev != -1) TotalStatController.I.Apply(DbNecklace.Get(prev).EquipStat);
            TotalStatController.I.Apply(necklace.Meta.EquipStat);
            return true;
        }
        
        public void ChangePetEquip(int idx, int id)
        {
            var prevPet = data.Pets.Value[idx].Value;
            data.Pets.SetValue(idx, id);
            TotalStatController.I.Apply(StatType.PetAttackBonus);
            TotalStatController.I.Apply(StatType.PetHpBonus);
            if (prevPet != -1) TotalStatController.I.Apply(DbPetAwakening.Get(prevPet).Option);
            if (id != -1) TotalStatController.I.Apply(DbPetAwakening.Get(id).Option);
            if (id != -1) QuestController.I.SetQuest(QuestType.CheckPetEquip, 1);
        }
        #endregion
        
        
        #region UnEquip

        public void RemoveNecklaceEquip(DbUserNecklace necklace)
        {
            var index = data.Necklaces.Value.FindIndex(n => n.Value == necklace.Id);
            data.Necklaces.SetValue(index, -1);
            TotalStatController.I.Apply(necklace.Meta.EquipStat);
        }
        
        public void RemoveSkillEquip(DbUserSkill skill, int presetIdx)
        {
            var removed = data.Skills[presetIdx].Value.FindIndex(s=> s.Value == skill.Id);
            ChangeSkillEquip(removed, -1, presetIdx);
        }
        
        public void RemovePetEquip(DbUserPet pet)
        {
            var removed = data.Pets.Value.FindIndex(p=> p.Value == pet.Id);
            ChangePetEquip(removed, -1);
        }

        #endregion
        
        public int GetPetHpBonus()
        {
            var bonus = 0;
            for (var idx = 0; idx < data.Pets.Value.Count; ++idx)
            {
                var pet = data.Pets.Value[idx].Value;
                if (pet != -1) bonus += DbPet.Get(pet).BibleHpBonus;
            }

            return bonus;
        }
    }
}