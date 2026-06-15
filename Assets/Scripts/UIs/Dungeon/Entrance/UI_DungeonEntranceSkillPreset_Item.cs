using Controller.Play;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon.Entrance
{
    public class UI_DungeonEntranceSkillPreset_Item : UI_Base
    {
        private UI_DungeonEntrance _selected;
        
        private EventsManager _selectionEventsManager;
        private EventsManager _bossPresetEventsManager;

        private Image _btn;
        
        private int _idx;

        enum GameObjects
        {
            IMG_BossPreset
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

        public void Set(int idx, UI_DungeonEntrance selected)
        {
            if (!_isInit) Init();
            
            _idx = idx;
            _selected = selected;

            gameObject.BindEvent(Functions.TrueCondition, _ => _selected.ChangeSkillPreset(_idx), UIEffectType.Bounce);
            
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
        
        private void WhenSelectionChanged()
        {
            var isSelected = _selected.GetPresetIdx().Value == _idx;
            _btn.color = isSelected ? Define.Color2B3143 : Define.ColorTransparent;
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