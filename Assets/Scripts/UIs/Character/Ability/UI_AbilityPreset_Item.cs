using Controller.Currency;
using Controller.Have;
using Controller.Play;
using Data;
using Managers;
using TMPro;
using UIBases;
using UIs.Skill;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Ability
{
    public class UI_AbilityPreset_Item: UI_Base
    {
        private EventsManager _presetEventsManager;

        private int _idx;

        enum GameObjects
        {
            IMG_Lock,
            IMG_ActiveAbilityPreset
        }
        
        enum Texts
        {
            T_AbilityPreset
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

            gameObject.BindEvent(Functions.TrueCondition, _ => OnSelected(), UIEffectType.Bounce);
            
            Get<GameObject>((int)GameObjects.IMG_Lock).SetActive(!IsUnlocked());
            
            _presetEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenSelectionChanged,
                updatedController = new []{AbilityController.I.preset}
            });

            WhenSelectionChanged();
        }

        private bool IsUnlocked()
        {
            return _idx < 2 || CurrencyController.I.Have(CurrencyType.AbilityPreset3 + _idx - 2);
        }

        private void OnSelected()
        {
            var have = IsUnlocked();
            if (have)
            {
                if (AbilityController.I.preset.Value == _idx) return;
                AbilityController.I.ChangePreset(_idx);
                return;
            }

            for (var idx = CurrencyType.AbilityPreset3; idx < CurrencyType.AbilityPreset3 + _idx - 2; ++idx)
            {
                if (!CurrencyController.I.Have(idx))
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200026);
                    return;
                }
            }
            
            Manager.UI.ShowPopupUI<UI_UnlockSKillPreset>().Set(CurrencyType.AbilityPreset3-2+_idx, () => 
                Get<GameObject>((int)GameObjects.IMG_Lock).SetActive(false));
        }
        
        private void WhenSelectionChanged()
        {
            var isSelected = AbilityController.I.preset.Value == _idx;
            Get<GameObject>((int) GameObjects.IMG_ActiveAbilityPreset).SetActive(isSelected);
            Get<TextMeshProUGUI>((int) Texts.T_AbilityPreset).color = isSelected ? Color.white : Define.ColorA5AECF;
        }
        
        private void OnDisable()
        {
            _presetEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _presetEventsManager?.Reconnect();
        }

    }
}