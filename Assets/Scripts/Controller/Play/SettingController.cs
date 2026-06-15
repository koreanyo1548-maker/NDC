using Controller.Infos;
using Data;
using Data.DbUser.Etc;
using Exceptions;
using Managers;
using UnityEngine;
using Utils;

namespace Controller.Play
{
    public class SettingController: Singleton<SettingController>
    {
        public static DbUserSetting data = DbUserSetting.Get(0);
        public static string UID;
        public static string Id;
        public static string Nickname;
        
        public void Init()
        {
            data.IsAutoProgress.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.IsAutoProgress.ToString(), data.IsAutoProgress.Value ? 1 : 0);
            data.IsAutoSkill.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.IsAutoSkill.ToString(), data.IsAutoSkill.Value ? 1 : 0);
            data.BGMSound.ValueChanged += (_, _) => PlayerPrefs.SetFloat(SettingType.BGMSound.ToString(), data.BGMSound.Value);
            data.SfxSound.ValueChanged += (_, _) => PlayerPrefs.SetFloat(SettingType.SfxSound.ToString(), data.SfxSound.Value);
            data.LevelUpCount.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.LevelUpCount.ToString(), data.LevelUpCount.Value);
            data.StatUpCount.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.StatUpCount.ToString(), data.StatUpCount.Value);
            data.IsCameraShaking.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.IsCameraShaking.ToString(), data.IsCameraShaking.Value ? 1 : 0);
            data.IsAutoPowerSaving.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.IsAutoPowerSaving.ToString(), data.IsAutoPowerSaving.Value ? 1 : 0);
            data.IsPushAlarm.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.IsPushAlarm.ToString(), data.IsPushAlarm.Value ? 1 : 0);
            data.IsNightPushAlarm.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.IsNightPushAlarm.ToString(), data.IsNightPushAlarm.Value ? 1 : 0);
            data.NormalSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.NormalSkillPreset.ToString(), data.NormalSkillPreset.Value);
            data.BossSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.BossSkillPreset.ToString(), data.BossSkillPreset.Value);
            data.SGSDungeonSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.SGSDungeonSkillPreset.ToString(), data.SGSDungeonSkillPreset.Value);
            data.ASDungeonSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.ASDungeonSkillPreset.ToString(), data.ASDungeonSkillPreset.Value);
            data.PetDungeonSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.PetDungeonSkillPreset.ToString(), data.PetDungeonSkillPreset.Value);
            data.BMDungeonSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.BMDungeonSkillPreset.ToString(), data.BMDungeonSkillPreset.Value);
            data.DiaDungeonSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.DiaDungeonSkillPreset.ToString(), data.DiaDungeonSkillPreset.Value);
            data.TrainingSkillPreset.ValueChanged += (_, _) => PlayerPrefs.SetInt(SettingType.TrainingSkillPreset.ToString(), data.TrainingSkillPreset.Value);
        }

        public void ChangeSkillPresetFor(FieldType fieldType, int idx)
        {
            switch (fieldType)
            {
                case FieldType.Awakening:
                    data.ASDungeonSkillPreset.Value = idx;
                    break;
                case FieldType.SkillGrowth:
                    data.SGSDungeonSkillPreset.Value = idx;
                    break;
                case FieldType.Pet:
                    data.PetDungeonSkillPreset.Value = idx;
                    break;
                case FieldType.BlackMarket:
                    data.BMDungeonSkillPreset.Value = idx;
                    break;
                case FieldType.Dia:
                    data.DiaDungeonSkillPreset.Value = idx;
                    break;
                case FieldType.Training:
                    data.TrainingSkillPreset.Value = idx;
                    break;
                default: throw new NotDefinedFieldException(fieldType);
            }
        }
        
        public int GetSkillPresetFor(FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Awakening:
                    return data.ASDungeonSkillPreset.Value;
                case FieldType.SkillGrowth:
                    return data.SGSDungeonSkillPreset.Value;
                case FieldType.Pet:
                    return data.PetDungeonSkillPreset.Value;
                case FieldType.BlackMarket:
                    return data.BMDungeonSkillPreset.Value;
                case FieldType.Dia:
                    return data.DiaDungeonSkillPreset.Value;
                case FieldType.Training:
                    return data.TrainingSkillPreset.Value;
                default: throw new NotDefinedFieldException(fieldType);
            }
        }
        
        public void SetSfxSound(float amount)
        {
            data.SfxSound.Value = amount;
        }
        
        public void SetBGMSound(float amount)
        {
            data.BGMSound.Value = amount;
        }
        
        public void OnStageFailed()
        {
            if (!data.IsAutoProgress.Value) return;

            data.IsAutoProgress.Value = false;
        }

        public void SetAutoProgress(bool isAuto)
        {
            if (data.IsAutoProgress.Value != isAuto)
            {
                data.IsAutoProgress.Value = isAuto;
                if (isAuto && LevelController.data.IsStageClear.Value && Manager.Field.CurField.Value == FieldType.Stage)
                {
                    LevelController.I.MoveStage(LevelController.I.GetNextStage().Id);
                }
            }
        }

        public void ChangeAutoSkill()
        {
            data.IsAutoSkill.Value = !data.IsAutoSkill.Value;
            QuestController.I.SetQuest(QuestType.CheckAutoSkillOn, data.IsAutoSkill.Value ? 1 : 0);
        }

        public void ChangeLevelUpCount(int count)
        {
            data.LevelUpCount.Value = count;
        }

        public void ChangeStatUpCount(int count)
        {
            data.StatUpCount.Value = count;
        }
    }
}