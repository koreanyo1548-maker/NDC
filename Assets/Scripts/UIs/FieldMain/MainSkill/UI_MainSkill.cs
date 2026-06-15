using System.Collections.Generic;
using Controller.Infos;
using Data;
using Managers;
using UIBases;
using UIs.Lock;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.FieldMain.MainSkill
{
    public class UI_MainSkill: UI_Scene
    {
        private List<UI_MainSkill_Item> _items = new();

        private GameObject _skillPreset;

        // private BigInteger _damage;
        // private int _hitCount;
        // private TextMeshProUGUI _damageText;
        // private TextMeshProUGUI _hitText;
        
        private void Start()
        {
            Init();
        }

        // public void AddDamage(BigInteger damage)
        // {
        //     _damage += damage;
        //     _damageText.text = (_damage / TotalStatController.I.GetAttack(false, false)).ToString("N0");
        // }
        //
        // public void AddHit()
        // {
        //     _hitCount++;
        //     _hitText.text = _hitCount.ToString("N0");
        // }
        //
        // public void ResetDamage()
        // {
        //     Init();
        //     _damage = 0;
        //     _damageText.text = "0";
        //     _hitCount = 0;
        //     _hitText.text = "0";
        // }

        public override bool Init()
        {
            if (!base.Init()) return false;

            // _damageText = transform.Find("SafeArea").Find("T_Damage").GetComponent<TextMeshProUGUI>();
            // _hitText = transform.Find("SafeArea").Find("T_Hit").GetComponent<TextMeshProUGUI>();
            _skillPreset = Util.FindChild(gameObject, "G_SkillPreset", true);
            
            for (var idx = 0; idx < 4; ++idx)
            {
                var skillIdx = idx;
                var skill = Util.FindChild(gameObject, "ActiveSkill" + (skillIdx + 1), true).GetOrAddComponent<UI_MainSkill_Item>();
                skill.SetInfo(skillIdx);
                if (skillIdx == 0)
                {
                    skill.gameObject.BindEvent(Functions.TrueCondition, _ =>
                    {
                        var use = skill.UseSkill(false);
                        if (use) QuestController.I.DoQuests(QuestType.UseSkill);
                    }, UIEffectType.Bounce);    
                }
                else
                {
                    skill.gameObject.GetOrAddComponent<UI_Locked>().Set(LockType.SkillSlot2 + idx - 1, null,
                        Util.FindChild(skill.gameObject, "IMG_Lock"), null,
                        () => skill.gameObject.BindEvent(Functions.TrueCondition, _ =>
                        {
                            var use = skill.UseSkill(false);
                            if (use) QuestController.I.DoQuests(QuestType.UseSkill);
                        }, UIEffectType.Bounce));
                }
                
                _items.Add(skill);
            }

            for (var idx = 0; idx < 5; ++idx)
            {
                var presetIdx = idx;
                Util.FindChild(gameObject, "B_SkillPreset" + (presetIdx+1), true)
                .GetOrAddComponent<UI_MainSkillPreset_Item>().Set(presetIdx);
            }
            
            Manager.Skill.Init();
            Manager.Field.CurField.ValueChanged += (_, _) => ActivateSkillPreset();

            return true;
        }

        public void StopSkillTimers()
        {
            for (var idx = 0; idx < 4; ++idx)
            {
                _items[idx].StopTimer();
            }
        }

        private void ActivateSkillPreset()
        {
            _skillPreset.SetActive(Manager.Field.CurField.Value == FieldType.Stage);
        }

        public void UseSkill()
        {
             for (var idx = 0; idx < 4; ++idx)
             {
                 _items[idx].SetTotalCoolTime(false);
             }
        }

        public void ASkillUseDone()
        {
            for (var idx = 0; idx < 4; ++idx)
            {
                var useAgain = _items[idx].SetTotalCoolTime(true);
                if (useAgain) break;
            }
        }


        public override bool NeedRaycast()
        {
            return true;
        }
    }
}