using System;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Managers;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.FieldMain.MainSkill
{
    public class UI_MainSkillPreset_Item: UI_Base
    {
        private int _idx;

        enum GameObjects
        {
            IMG_ActiveSkillPreset,
            IMG_BossSkillPreset,
            IMG_SkillPreset,
            IMG_Lock
        }

        enum Texts
        {
            T_SkillPreset
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            return true;
        }

        public void Set(int idx)
        {
            if (!_isInit) Init();
            
            _idx = idx;

            gameObject.BindEvent(Functions.TrueCondition, _ => ChangePreset(), UIEffectType.Bounce);

            EquipController.curSkillPreset.ValueChanged += (_, _) => WhenSelectionChanged();
            SettingController.data.BossSkillPreset.ValueChanged += (_, _) => WhenBossPresetChanged();
            CurrencyController.data.Buy.ValueChanged += SetPresetLock;

            WhenSelectionChanged();
            WhenBossPresetChanged();
            SetPresetLock(this, null);
        }

        private void SetPresetLock(object sender, EventArgs eventArgs)
        {
            if (_idx > 1) Get<GameObject>((int)GameObjects.IMG_Lock).SetActive(!IsUnlocked());
            if (IsUnlocked())
            {
                CurrencyController.data.Buy.ValueChanged -= SetPresetLock;
            }
        }
        
        private bool IsUnlocked()
        {
            return _idx < 2 || CurrencyController.I.Have(CurrencyType.SkillPreset3 + _idx - 2);
        }

        private void ChangePreset()
        {
            if (!IsUnlocked())
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200017);
                return;
            }

            if (Manager.Field.CurField.Value == FieldType.Stage)
            {
                if (Manager.Field.StageMeta.GetStageType() == StageType.Boss)
                {
                    SettingController.data.BossSkillPreset.Value = _idx;
                }
                else
                {
                    SettingController.data.NormalSkillPreset.Value = _idx;
                }
                EquipController.I.SetCurSkillPreset(_idx);
            }
        }
        
        private void WhenBossPresetChanged()
        {
            var isBoss = SettingController.data.BossSkillPreset.Value == _idx;
            Get<GameObject>((int)GameObjects.IMG_BossSkillPreset).SetActive(isBoss);
            Get<GameObject>((int)GameObjects.IMG_SkillPreset).SetActive(!isBoss);
        }
        
        private void WhenSelectionChanged()
        {
            var isSelected = EquipController.curSkillPreset.Value == _idx;
            Get<GameObject>((int)GameObjects.IMG_ActiveSkillPreset).SetActive(isSelected);
            Get<TextMeshProUGUI>((int) Texts.T_SkillPreset).color = isSelected ? Define.ColorFFF8AA : Define.ColorD0D7F1;
        }
    }
}