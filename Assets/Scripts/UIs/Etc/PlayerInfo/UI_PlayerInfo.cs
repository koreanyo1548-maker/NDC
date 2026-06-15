using System;
using System.Collections.Generic;
using Controller;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbPetInfo;
using Data.DbRecord;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc.PlayerInfo
{
    public class UI_PlayerInfo: UI_Popup
    {
        private EventsManager _profileEventManager;
        
        enum Texts
        {
            T_Power,
            T_Title,
            T_Nickname,
            T_Level,
            T_StatInfo,
            T_Stat,
        }

        enum Images
        {
            IMG_Profile
        }

        enum Items
        {
            G_Weapon,
            G_Accessory,
            G_Skill1,
            G_Skill2,
            G_Skill3,
            G_Skill4,
            G_Pet1,
            G_Pet2,
            G_Pet3,
            G_Pet4
        }

        private void OnEnable()
        {
            if (!_isInit)
            {
                Init();
            }
            else
            {
                _profileEventManager?.Reconnect();
            }
            Set();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));

            foreach (var item in Enum.GetValues(typeof(Items)))
            {
                Util.FindChild(gameObject, item.ToString(), true).GetOrAddComponent<UI_PlayerInfoEquip_Item>();
            }
            
            Bind<UI_PlayerInfoEquip_Item>(typeof(Items));
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Util.FindChild(gameObject, "B_NicknameEdit", true).BindEvent(Functions.TrueCondition, _ => 
            {
                Manager.UI.ShowPopupUI<UI_Nickname>().Set(false);
            });
            
            Get<Image>((int)Images.IMG_Profile).gameObject.BindEvent(Functions.TrueCondition, _ => OpenProfileSelection(), UIEffectType.Bounce);
            
            _profileEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenProfileChanged,
                updatedField = new[] {EquipController.data.Profile}
            });
            WhenProfileChanged();


            return true;
        }

        public void SetNickname()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Nickname).text = SettingController.Nickname;
        }

        private List<StatType> _showStat = new()
        {
            StatType.Attack, StatType.Hp, StatType.CriticalProbability, StatType.CriticalAttackBonus,
            StatType.AttackBonus, StatType.PetAttackBonus, StatType.AwakeningAttackBonus, 
            StatType.FinalAttackBonus, StatType.PotentialAttackBonus, StatType.RelicAttackBonus, StatType.NecklaceAttackBonus,
            StatType.HpBonus, StatType.PetHpBonus, StatType.AwakeningHpBonus, StatType.FinalHpBonus, 
            StatType.PotentialHpBonus, StatType.RelicHpBonus, StatType.NecklaceHpBonus,
            StatType.NormalAttackBonus, StatType.DashAttackBonus, StatType.SkillAttackBonus, StatType.BossAttackBonus,
            StatType.DebuffMonsterAttack, StatType.DebuffMonsterHp, StatType.AttackSpeedBonus, StatType.MoveSpeedBonus, 
            StatType.StageGoldEarn, StatType.AbilityGoldEarn, StatType.StageExpEarn, StatType.AbilityExpEarn, 
            StatType.StageGrowthEarn, StatType.StageItemRate, StatType.BlackMarketDungeonEarn,
            StatType.DiaDungeonEarn, StatType.SkillGrowthDungeonEarn, StatType.AwakeningDungeonEarn
        };
        
        private void Set()
        {
            Get<TextMeshProUGUI>((int)Texts.T_Power).text = LocalString.Get(210188) + " " + Define.AddUnit(TotalStatController.Power.Value, 3, 2);
           
            var title = EquipController.data.Title.Value;
            Get<TextMeshProUGUI>((int)Texts.T_Title).text =  title == 0 ? LocalString.Get(210160) : DbTitle.Get(title).GetNameWithColor();
            
            Get<TextMeshProUGUI>((int) Texts.T_Nickname).text = SettingController.Nickname;
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), LevelController.data.Level.Value);
            
            var statInfo = $"{LocalString.Get(210245)}\n{LocalString.Get(210246)}";
            var stat = $"{Define.AddUnit(TotalStatController.I.GetAttack(false, false), 3, 2)}\n{Define.AddUnit(TotalStatController.I.Hp, 3, 2)}";
            for (var idx = 0; idx < _showStat.Count; ++idx)
            {
                var meta = DbStat.Get(_showStat[idx]);
                var staticName = LocalString.Get(meta.StaticNameId);
                var isNameLong = staticName.Length > 35;
                statInfo += "\n" + (isNameLong ? staticName.Substring(0, 35) + "\n" + staticName.Substring(35).Trim() : staticName);
                
                var statValue = TotalStatController.I.GetStat(meta.Id) * meta.ShowMultiply;
                if (statValue < 1000) stat += "\n" + statValue;
                else stat += "\n" + Define.AddUnit(Mathf.FloorToInt(statValue), 3, 2);
                
                if (meta.IsPercent) stat += "%";
                if (isNameLong) stat += "\n";
            }
            Get<TextMeshProUGUI>((int) Texts.T_StatInfo).text = statInfo;
            Get<TextMeshProUGUI>((int) Texts.T_Stat).text = stat;

            var equipWeapon = EquipController.data.Weapon.Value;
            if (equipWeapon == 0) Get<UI_PlayerInfoEquip_Item>((int)Items.G_Weapon).SetNull();
            else Get<UI_PlayerInfoEquip_Item>((int)Items.G_Weapon).SetInfo(DbWeapon.Get(equipWeapon));
            
            var equipAccessory = EquipController.data.Accessory.Value;
            if (equipAccessory == 0) Get<UI_PlayerInfoEquip_Item>((int)Items.G_Accessory).SetNull();
            else Get<UI_PlayerInfoEquip_Item>((int)Items.G_Accessory).SetInfo(DbAccessory.Get(equipAccessory));
            
            var equipSkill = EquipController.data.Skills[SettingController.data.NormalSkillPreset.Value].Value;
            for (var idx = 0; idx < 4; ++idx)
            {
                if (equipSkill[idx].Value == -1) Get<UI_PlayerInfoEquip_Item>((int)Items.G_Skill1 + idx).SetNull();
                else Get<UI_PlayerInfoEquip_Item>((int)Items.G_Skill1 + idx).SetInfo(DbSkill.Get(equipSkill[idx].Value));
            }
            
            var equipPet = EquipController.data.Pets.Value;
            for (var idx = 0; idx < 4; ++idx)
            {
                if (equipPet[idx].Value == -1) Get<UI_PlayerInfoEquip_Item>((int)Items.G_Pet1 + idx).SetNull();
                else Get<UI_PlayerInfoEquip_Item>((int)Items.G_Pet1 + idx).SetInfo(DbPet.Get(equipPet[idx].Value));
            }
        }

        private void OpenProfileSelection()
        {
            Manager.UI.ShowPopupUI<UI_ProfileSelect>();
        }

        private void WhenProfileChanged()
        {
            Get<Image>((int)Images.IMG_Profile).sprite = 
                Manager.Resource.Load<Sprite>(DbProfile.Get(EquipController.data.Profile.Value).Resource);
        }
        
        private void OnDisable()
        {
            _profileEventManager.Dispose();
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}