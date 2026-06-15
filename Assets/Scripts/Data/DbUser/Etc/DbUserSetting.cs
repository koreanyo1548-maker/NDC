using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Data.Utils;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Data.DbUser.Etc
{
    [Serializable]
    public class DbUserSetting: DbUserModel<DbUserSetting, int>
    {
        [DataMember] public DbField<bool> IsGuest { get; private set; }
        [DataMember] public DbField<bool> IsGoogle { get; private set; }
        [DataMember] public DbField<bool> IsHive { get; private set; }
        [DataMember] public DbField<bool> IsApple { get; private set; }
        [DataMember] public DbField<bool> IsAutoProgress { get; private set; }
        [DataMember] public DbField<bool> IsAutoSkill { get; private set; }
        [DataMember] public DbField<float> BGMSound { get; private set; }
        [DataMember] public DbField<float> SfxSound { get; private set; }
        [DataMember] public DbField<bool> IsCameraShaking { get; private set; }
        [DataMember] public DbField<bool> IsAutoPowerSaving { get; private set; }
        [DataMember] public DbField<bool> IsPushAlarm { get; private set; }
        [DataMember] public DbField<bool> IsNightPushAlarm { get; private set; }
        [DataMember] public DbField<int> StatUpCount { get; private set; }
        [DataMember] public DbField<int> LevelUpCount { get; private set; }
        [DataMember] public DbField<int> NormalSkillPreset { get; private set; }
        [DataMember] public DbField<int> BossSkillPreset { get; private set; }
        [DataMember] public DbField<int> SGSDungeonSkillPreset { get; private set; }
        [DataMember] public DbField<int> ASDungeonSkillPreset { get; private set; }
        [DataMember] public DbField<int> PetDungeonSkillPreset { get; private set; }
        [DataMember] public DbField<int> BMDungeonSkillPreset { get; private set; }
        [DataMember] public DbField<int> DiaDungeonSkillPreset { get; private set; }
        [DataMember] public DbField<int> TrainingSkillPreset { get; private set; }

        public void Set()
        {
            if (PlayerPrefs.HasKey(SettingType.Setting.ToString()))
            {
                VersionSetting(PlayerPrefs.GetInt(SettingType.Setting.ToString()));
            }
            else
            {
                VersionSetting(0);
            }
            
            Init(new List<DbUserSetting>
            {
                new(0, 
                    PlayerPrefs.GetInt(SettingType.IsAutoProgress.ToString()) == 1, 
                    PlayerPrefs.GetInt(SettingType.IsAutoSkill.ToString()) == 1, 
                    PlayerPrefs.GetFloat(SettingType.BGMSound.ToString()),
                    PlayerPrefs.GetFloat(SettingType.SfxSound.ToString()),
                    PlayerPrefs.GetInt(SettingType.StatUpCount.ToString()), 
                    PlayerPrefs.GetInt(SettingType.LevelUpCount.ToString()),
                    PlayerPrefs.GetInt(SettingType.IsCameraShaking.ToString()) == 1,
                    PlayerPrefs.GetInt(SettingType.IsAutoPowerSaving.ToString()) == 1,
                    PlayerPrefs.GetInt(SettingType.IsPushAlarm.ToString()) == 1,
                    PlayerPrefs.GetInt(SettingType.IsNightPushAlarm.ToString()) == 1,
                    PlayerPrefs.GetInt(SettingType.NormalSkillPreset.ToString()),
                    PlayerPrefs.GetInt(SettingType.BossSkillPreset.ToString()),
                    PlayerPrefs.GetInt(SettingType.SGSDungeonSkillPreset.ToString()),
                    PlayerPrefs.GetInt(SettingType.ASDungeonSkillPreset.ToString()),
                    PlayerPrefs.GetInt(SettingType.PetDungeonSkillPreset.ToString()),
                    PlayerPrefs.GetInt(SettingType.BMDungeonSkillPreset.ToString()),
                    PlayerPrefs.GetInt(SettingType.DiaDungeonSkillPreset.ToString()),
                    PlayerPrefs.GetInt(SettingType.TrainingSkillPreset.ToString())
                    ) 
            });
        }

        public void Reset()
        {
            IsAutoProgress.Value = false;
            IsAutoSkill.Value = false;
            BGMSound.Value = 0.8f;
            SfxSound.Value = 0.8f;
            IsCameraShaking.Value = true;
            IsAutoPowerSaving.Value = true;
            IsPushAlarm.Value = true;
            IsNightPushAlarm.Value = true;
            StatUpCount.Value = 1;
            LevelUpCount.Value = 1;
            NormalSkillPreset.Value = 0;
            BossSkillPreset.Value = 0;
            SGSDungeonSkillPreset.Value = 0;
            ASDungeonSkillPreset.Value = 0;
            PetDungeonSkillPreset.Value = 0;
            BMDungeonSkillPreset.Value = 0;
            DiaDungeonSkillPreset.Value = 0;
            TrainingSkillPreset.Value = 0;
        }

        private void VersionSetting(int version)
        {
            if (version < 2)
            {
                PlayerPrefs.SetInt(SettingType.TrainingSkillPreset.ToString(), 0);
            }
            if (version < 1)
            {
                PlayerPrefs.SetInt(SettingType.IsAutoSkill.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.PetDungeonSkillPreset.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.BMDungeonSkillPreset.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.DiaDungeonSkillPreset.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.NormalSkillPreset.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.BossSkillPreset.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.SGSDungeonSkillPreset.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.ASDungeonSkillPreset.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.IsAutoPowerSaving.ToString(), 1);
                PlayerPrefs.SetInt(SettingType.IsPushAlarm.ToString(), 1);
                PlayerPrefs.SetInt(SettingType.IsCameraShaking.ToString(), 1);
                PlayerPrefs.SetInt(SettingType.IsGuest.ToString(), 1);
                PlayerPrefs.SetInt(SettingType.StatUpCount.ToString(), 1);
                PlayerPrefs.SetInt(SettingType.LevelUpCount.ToString(), 1);
                PlayerPrefs.SetInt(SettingType.IsAutoProgress.ToString(), 0);
                PlayerPrefs.SetInt(SettingType.IsAutoSkill.ToString(), 0);
                PlayerPrefs.SetFloat(SettingType.BGMSound.ToString(), 0.8f);
                PlayerPrefs.SetFloat(SettingType.SfxSound.ToString(), 0.8f);
            }
            
            PlayerPrefs.SetInt(SettingType.Setting.ToString(), 2);
        }
        public override void Set(List<DbUserSetting> obj)
        {
        }

        protected override List<DbUserSetting> GetInitials()
        {
            return new List<DbUserSetting>
            {
                new()
            };
        }

        public override List<DbUserSetting> AdjustDataModification(List<DbUserSetting> obj)
        {
            return obj;
        }


        [JsonConstructor]
        public DbUserSetting(int Id, bool IsAutoProgress, bool IsAutoSkill,
            float BGMSound, float SfxSound, int StatUpCount, int LevelUpCount, bool IsCameraShaking, 
            bool IsAutoPowerSaving, bool IsPushAlarm, bool IsNightPushAlarm,
            int NormalSkillPreset, int BossSkillPreset, int SGSDungeonSkillPreset, int ASDungeonSkillPreset,
            int PetDungeonSkillPreset, int BMDungeonSkillPreset, int DiaDungeonSkillPreset,
            int TrainingSkillPreset)
        {
            this.Id = Id;
            this.IsGuest = new DbField<bool>(false, 0, this);
            this.IsGoogle = new DbField<bool>(false, 0, this);
            this.IsHive = new DbField<bool>(false, 0, this);
            this.IsApple = new DbField<bool>(false, 0, this);
            this.IsAutoProgress = new DbField<bool>(IsAutoProgress, 0, this);
            this.IsAutoSkill = new DbField<bool>(IsAutoSkill, 0, this);
            this.SfxSound = new DbField<float>(SfxSound, 0, this);
            this.BGMSound = new DbField<float>(BGMSound, 0, this);
            this.StatUpCount = new DbField<int>(StatUpCount, 0, this);
            this.LevelUpCount = new DbField<int>(LevelUpCount, 0, this);
            this.IsCameraShaking = new DbField<bool>(IsCameraShaking, 0, this);
            this.IsAutoPowerSaving = new DbField<bool>(IsAutoPowerSaving, 0, this);
            this.IsPushAlarm = new DbField<bool>(IsPushAlarm, 0, this);
            this.IsNightPushAlarm = new DbField<bool>(IsNightPushAlarm, 0, this);
            this.NormalSkillPreset = new DbField<int>(NormalSkillPreset, 0, this);
            this.BossSkillPreset = new DbField<int>(BossSkillPreset, 0, this);
            this.SGSDungeonSkillPreset = new DbField<int>(SGSDungeonSkillPreset, 0, this);
            this.ASDungeonSkillPreset = new DbField<int>(ASDungeonSkillPreset, 0, this);
            this.PetDungeonSkillPreset = new DbField<int>(PetDungeonSkillPreset, 0, this);
            this.BMDungeonSkillPreset = new DbField<int>(BMDungeonSkillPreset, 0, this);
            this.DiaDungeonSkillPreset = new DbField<int>(DiaDungeonSkillPreset, 0, this);
            this.TrainingSkillPreset = new DbField<int>(TrainingSkillPreset, 0, this);
        }

        public DbUserSetting()
        {
        }
    }
}