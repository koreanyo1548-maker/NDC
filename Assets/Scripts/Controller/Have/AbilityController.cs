using System;
using System.Collections.Generic;
using Controller.Infos;
using Controller.Play;
using Controller.Utils;
using Data;
using Data.DbAbility;
using Data.DbDefinition;
using Data.DbUser.Equipment;
using Data.DbUser.Progress;
using Managers;
using ThirdParty;
using Utils;

namespace Controller.Have
{
    public class AbilityController : Singleton<AbilityController>
    {
        public ControllerField<int> preset = new(0);
        public ControllerField<int> changeCost = new(0);
        public Dictionary<StatType, ControllerField<int>> runeLevel;
        
        public AbilityController()
        {
            runeLevel = new Dictionary<StatType, ControllerField<int>>();
            var runes = new List<StatType>
            {
                StatType.PotentialAttackBonus, StatType.PotentialHpBonus, StatType.AbilityGoldEarn,
                StatType.AbilityExpEarn, StatType.SkillAttackBonus
            };
            foreach (var rune in runes)
            {
                runeLevel.Add(rune, new ControllerField<int>(0));
            }

            DbUserAbility.ForEach(a => a.IsLocked.ValueChanged += (_, _) => SetLockCost());
            
            for (var idx = 0; idx < 5; ++idx)
            {
                if (DbUserAbility.Get(idx * 5).IsUsing.Value)
                {
                    preset.Value = idx;
                }
            }
            SetLockCost();
            SetRuneLevel(false);
        }

        private void SetLockCost()
        {
            var added = DbCost.Get(CostType.AbilityRollDia).Cost;
            var cost = added;
            for (var idx = 0; idx < 5; ++idx)
            {
                if (DbUserAbility.Get(preset.Value * 5 + idx).IsLocked.Value)
                {
                    cost += added;
                }
            }

            changeCost.Value = cost;
        }

        private void SetRuneLevel(bool adjustStat = true)
        {
            foreach (var level in runeLevel)
            {
                level.Value.Value = 0;
            }
            for (var idx = 0; idx < 5; ++idx)
            {
                var ability = DbUserAbility.Get(preset.Value * 5 + idx);
                var rune = ability.Rune;
                if (rune == StatType.None) continue;
                runeLevel[rune].Value += ability.OptionGrade >= GradeType.Legendary ? 2 : 1;
            }
            
            if (adjustStat) DbAbilityRune.ForEach(r => TotalStatController.I.Apply(r.Id));
        }
        
        /// <returns> true: 계속해야할 경우, false: 멈춰야할 경우 </returns>
        public bool ChangeAbilityAndCheckContinue(bool isAuto)
        {
            var hasHigh = false;
            for (var idx = 0; idx < 5; ++idx)
            {
                var ability = DbUserAbility.Get(preset.Value * 5 + idx);
                if (!ability.IsLocked.Value)
                {
                    var grade = ability.Change();
                    if (grade >= GradeType.Legendary) hasHigh = true;
                }
            }
            SetRuneLevel();
            QuestController.I.DoQuests(QuestType.ChangeAbility);
            if (isAuto && !hasHigh) return true;
            
            PlayFabManager.Store.ForceSave();
            return false;
        }
        
        public void ChangePreset(int changeTo)
        {
            var changed = new StatType[10];
            for (var idx = 0; idx < 5; ++idx)
            {
                changed[idx] = DbUserAbility.Get(preset.Value * 5 + idx).Option.Value;
                DbUserAbility.Get(preset.Value * 5 + idx).IsUsing.Value = false;
            }
            preset.Value = changeTo;
            for (var idx = 0; idx < 5; ++idx)
            {
                changed[5+idx] = DbUserAbility.Get(preset.Value * 5 + idx).Option.Value;
                DbUserAbility.Get(preset.Value * 5 + idx).IsUsing.Value = true;
            }

            for (var idx = 0; idx < 10; ++idx)
            {
                TotalStatController.I.Apply(changed[idx]);
            }
            SetLockCost();
            SetRuneLevel();
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Ability);
        }

        public bool HaveUnlockedHigh()
        {
            for (var idx = 0; idx < 5; ++idx)
            {
                var ability = DbUserAbility.Get(preset.Value * 5 + idx);
                if (!ability.IsLocked.Value && ability.OptionGrade >= GradeType.Legendary) return true;
            }

            return false;
        }

        public bool IsAllLocked()
        {
            for (var idx = 0; idx < 5; ++idx)
            {
                if (!DbUserAbility.Get(preset.Value * 5 + idx).IsLocked.Value) return false;
            }

            return true;
        }
    }
}