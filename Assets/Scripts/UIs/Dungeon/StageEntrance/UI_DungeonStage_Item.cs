using Controller.Infos;
using dynamicscroll;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon.StageEntrance
{
    public class UI_DungeonStage_Item : DynamicScrollObject<DungeonStageItem>
    {
        private UI_DungeonStage _stage;
        
        private TextMeshProUGUI _stageLevelText;
        private GameObject _selected;
        private GameObject _locked;
        private Image _enterBtn;

        private Sprite[] _buttonSprites;

        private int _stageLevel;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _enterBtn = Util.FindChild<Image>(gameObject, "B_Enter", true);
            _locked = Util.FindChild(gameObject, "IMG_LockIcon", true);
            _selected =  Util.FindChild(gameObject, "IMG_Selected", true);
            _stageLevelText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Stage", true);
            
            _enterBtn.gameObject.BindEvent(Condition, SetStage, UIEffectType.Bounce);

            _buttonSprites = new[]
            {
                Manager.Resource.Load<Sprite>("UI_DefaultButton_round"),
                Manager.Resource.Load<Sprite>("UI_DefaultButton_round2")
            };
            
            _selected.SetActive(false);
            _stage = Manager.UI.GetPopupUI<UI_DungeonStage>();
            
            _stage.CurStage.ValueChanged += WhenSelectionChanged;
        }

        private bool Condition()
        {
            return _stageLevel <= LevelController.I.GetCurStage(_stage.CurDungeon);
        }

        private void WhenSelectionChanged()
        {
            var isSelected = _stageLevel == _stage.CurStage.Value;
            _selected.SetActive(isSelected);
            _enterBtn.sprite = _buttonSprites[isSelected ? 1 : 0];
        }
        
        public override void UpdateScrollObject(DungeonStageItem stage, int index)
        {
            base.UpdateScrollObject(stage, index);

            _stageLevel = stage.level;
            _stageLevelText.text = stage.level.ToString();
            
            var isLocked = !Condition();
            _enterBtn.material = Define.GetUIMaterial(isLocked);
            _locked.SetActive(isLocked);
            
            WhenSelectionChanged();
        }
        
        private void SetStage(PointerEventData eventData)
        {
            if (_selected.activeSelf) return;
            
            _stage.SetStage(_stageLevel);
        }
    }

    public class DungeonStageItem
    {
        public int level;
    }
}