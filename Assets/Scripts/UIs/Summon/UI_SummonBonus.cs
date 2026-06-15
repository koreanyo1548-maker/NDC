using System.Collections.Generic;
using Data;
using Data.DbDungeon;

using Data.DbSummon;
using dynamicscroll;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Summon
{
    public class UI_SummonBonus: UI_Popup
    {
        private DynamicScroll<SummonBonusItem, UI_SummonBonus_Item> _bonus;
        private Dictionary<SummonType, List<SummonBonusItem>> _bonusData = new();

        private SummonType _summonType = SummonType.Weapon;
        enum Texts
        {
            T_SummonBonus
        }
        

        enum DynamicScrollRects
        {
            V_SummonBonus
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<DynamicScrollRect>(typeof(DynamicScrollRects));
            
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            var weapon = new List<SummonBonusItem>();
            var accessory = new List<SummonBonusItem>();
            var skill = new List<SummonBonusItem>();
            var skillMaxLevel = DbSummonLevel.Count;
            var elseMaxLevel = 10;
            DbSummonLevel.ForEach(l =>
            {
                if (l.Id == 1) return;
                if (l.NeedExp > 0)
                {
                    weapon.Add(new SummonBonusItem { isLast = l.Id == elseMaxLevel, type = l.WeaponId == 0 ? CurrencyType.WeaponGrowthStone : CurrencyType.Weapon, level = l.Id, dia = l.NeedExp, getId = l.WeaponId, getCount = l.WeaponCount});
                    accessory.Add(new SummonBonusItem { isLast = l.Id == elseMaxLevel, type = l.AccessoryId == 0 ? CurrencyType.AccessoryGrowthStone : CurrencyType.Accessory, level = l.Id, dia = l.NeedExp, getId = l.AccessoryId, getCount = l.AccessoryCount});
                }
                skill.Add(new SummonBonusItem { isLast = l.Id == skillMaxLevel, type = l.SkillId == 0 ? CurrencyType.SkillGrowthStone : CurrencyType.Skill, level = l.Id, dia = l.SkillNeedExp, getId = l.SkillId, getCount = l.SkillCount});
            });
            _bonusData.Add(SummonType.Weapon, weapon);
            _bonusData.Add(SummonType.Accessory, accessory);
            _bonusData.Add(SummonType.Skill, skill);
            
            _bonus = new DynamicScroll<SummonBonusItem, UI_SummonBonus_Item>();
            _bonus.Initiate(Get<DynamicScrollRect>((int)DynamicScrollRects.V_SummonBonus), _bonusData[_summonType],
                -1,"Prefabs/UI/SubItem/UI_SummonBonus_Item");
                                              
            return true;
        }
        
        public void Set(SummonType summonType)
        {
            if (!_isInit) Init();
            if (_summonType != summonType)
            {
                _summonType = summonType;
                _bonus.ChangeList(_bonusData[_summonType], 0);
            }
            
            _summonType = summonType;
            Get<TextMeshProUGUI>((int)Texts.T_SummonBonus).text = string.Format(LocalString.Get(210099), StringMaker.GetSummonName(_summonType));
        
            _bonus.MoveToIndex(0, 0.1f);
            _bonus.ForceUpdateList();
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