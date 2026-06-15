using System;
using Controller;
using Controller.Currency;
using Controller.Play;
using Data;
using Data.DbShop;
using Managers;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Skill
{
    public class UI_EquipSkillPreset_Item : UI_Base
    {
        private UI_SkillInventorySelected _selected;
        
        private EventsManager _selectionEventsManager;
        private EventsManager _bossPresetEventsManager;

        private Image _btn;
        
        private int _idx;

        enum GameObjects
        {
            IMG_BossPreset,
            IMG_Lock
        }
        
        enum Texts
        {
            T_SkillPresetNum
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            _btn = transform.GetComponent<Image>();
            
            return true;
        }

        public void Set(int idx, UI_SkillInventorySelected selected)
        {
            if (!_isInit) Init();
            
            _idx = idx;
            _selected = selected;

            gameObject.BindEvent(Functions.TrueCondition, _ => OnSelected(), UIEffectType.Bounce);
            
            Get<GameObject>((int)GameObjects.IMG_Lock).SetActive(!IsUnlocked());
            
            _selectionEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenSelectionChanged,
                updatedUI = new []{_selected.GetPresetIdx()}
            });
            
            _bossPresetEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenBossPresetChanged,
                updatedField = new []{SettingController.data.BossSkillPreset}
            });

            WhenSelectionChanged();
            WhenBossPresetChanged();
        }

        private bool IsUnlocked()
        {
            return _idx < 2 || CurrencyController.I.Have(CurrencyType.SkillPreset3 + _idx - 2);
        }

        private void OnSelected()
        {
            var have = IsUnlocked();
            if (have)
            {
                _selected.GetPresetIdx().Value = _idx;
                return;
            }

            for (var idx = CurrencyType.SkillPreset3; idx < CurrencyType.SkillPreset3 + _idx - 2; ++idx)
            {
                if (!CurrencyController.I.Have(idx))
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200026);
                    return;
                }
            }
            
            Manager.UI.ShowPopupUI<UI_UnlockSKillPreset>().Set(CurrencyType.SkillPreset3-2+_idx, () => 
                Get<GameObject>((int)GameObjects.IMG_Lock).SetActive(false));
        }
        
        private void WhenSelectionChanged()
        {
            var isSelected = _selected.GetPresetIdx().Value == _idx;
            _btn.color = isSelected ? Define.Color3D4365 : Define.ColorTransparent;
            _btn.raycastTarget = !isSelected;
            Get<TextMeshProUGUI>((int) Texts.T_SkillPresetNum).color = isSelected ? Color.white : Define.ColorA5AECF;
        }

        private void WhenBossPresetChanged()
        {
            Get<GameObject>((int)GameObjects.IMG_BossPreset).SetActive(SettingController.data.BossSkillPreset.Value == _idx);
        }
        
        private void OnDisable()
        {
            _selectionEventsManager?.Dispose();
            _bossPresetEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _selectionEventsManager?.Reconnect();
            _bossPresetEventsManager?.Reconnect();
        }

    }
}