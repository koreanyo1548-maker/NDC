using System.Linq;
using Controller;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbEquipment;
using Data.DbUser;
using Data.Utils;
using Managers;
using UIBases;
using UIs.Lock;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Skill
{
    public class UI_EquipSkill: UI_Base
    {
        public enum Mode
        {
            Edit,
            Show
        }
        private EventsManager _equipEventsManager;
        private EventsManager _selectionEventManager;
        
        private UI_SkillInventorySelected _selected;
        
        private Animator _animator;
        private Canvas _canvas;
        private int _parentOrder;
        
        private Mode _curMode = Mode.Show;
        public Mode CurMode => _curMode;

        enum GameObjects
        {
            SkillSelection1 = 0,
            SkillSelection2 = 1,
            SkillSelection3 = 2,
            SkillSelection4 = 3,
            IMG_SkillCover
        }

        enum Images
        {
            IMG_Skill1 = 0,
            IMG_Skill2 = 1,
            IMG_Skill3 = 2,
            IMG_Skill4 = 3,
            B_BossPreset
        }

        private void Awake()
        {
            _canvas = transform.GetComponent<Canvas>();
        }

        private void Start()
        {
            Init();
        }
        
        public override bool Init()
        {
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            _selected = Manager.UI.GetPopupUI<UI_SkillInventory>().Selected;
            _animator = Util.FindChild(gameObject, "EquipSkills").GetComponent<Animator>();
            
            Get<GameObject>((int)GameObjects.IMG_SkillCover).BindEvent(Functions.TrueCondition, CloseEquipChange);

            for (var idx = 1; idx <= 4; ++idx)
            {
                var curIdx = idx -1;
                var obj = Util.FindChild(gameObject,"ActiveSkill" + idx, true);
                if (idx == 1) obj.BindEvent(Functions.TrueCondition, eventData => SelectEquip(eventData, curIdx));
                else
                {
                    obj.GetOrAddComponent<UI_Locked>().Set(LockType.SkillSlot2 + idx - 2, null,
                        Util.FindChild(obj, "IMG_Lock"), null,
                        () => obj.BindEvent(Functions.TrueCondition, eventData => SelectEquip(eventData, curIdx)));
                }
            }
            
            for (var idx = 1; idx <= 5; ++idx)
            {
                var curIdx = idx -1;
                Util.FindChild(gameObject,"B_SkillPreset" + idx, true).GetOrAddComponent<UI_EquipSkillPreset_Item>().Set(curIdx, _selected);
            }
            
            Get<Image>((int)Images.B_BossPreset).gameObject.BindEvent(Functions.TrueCondition, _ => ChangeBossPreset(), UIEffectType.Bounce);
            
            _equipEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEquipChanged,
                updatedField = EquipController.data.Skills.ToArray(),
                updatedUI = new []{_selected.GetPresetIdx()}
            });
            
            _selectionEventManager = new EventsManager(this, new EventsManager.Config()
            {
                handler = WhenSelectionChanged,
                updatedUI = new [] {_selected.GetPresetIdx()}
            });
            
            WhenEquipChanged();
            WhenSelectionChanged();
            CloseEquipChange(null);
            return true;
        }

        private void ChangeBossPreset()
        {
            var idx = _selected.GetPresetIdx().Value;
            if (SettingController.data.BossSkillPreset.Value != idx)
            {
                SettingController.data.BossSkillPreset.Value = idx;
                if (Manager.Field.CurField.Value == FieldType.Stage &&
                    Manager.Field.StageMeta.GetStageType() == StageType.Boss)
                {
                    EquipController.I.SetCurSkillPreset(idx);
                }
                Get<Image>((int) Images.B_BossPreset).color = Define.ColorFF4F40;
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200025);
            }
        }

        private void WhenSelectionChanged()
        {
            Get<Image>((int) Images.B_BossPreset).color =
                _selected.GetPresetIdx().Value == SettingController.data.BossSkillPreset.Value
                    ? Define.ColorFF4F40 : Define.ColorA5AECF;
        }

        private void WhenEquipChanged()
        {
            var skills = EquipController.data.Skills[_selected.GetPresetIdx().Value].Value;
            var count = skills.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                var skill = skills[idx].Value;
                Get<Image>(idx).sprite = Manager.Resource.Load<Sprite>(skill == -1 ? Define.EmptySprite : DbSkill.Get(skill).Resource);
            }
        }

        public void OpenEquipChange()
        {
            _canvas.sortingOrder = _parentOrder + 2;
            Manager.Guide.Check(this);
            _animator.enabled = true;
            _curMode = Mode.Edit;
            for (var idx = (int)GameObjects.SkillSelection1; idx <= (int)GameObjects.SkillSelection4; ++idx)
            {
                Get<GameObject>(idx).SetActive(true);
            }
            Get<GameObject>((int)GameObjects.IMG_SkillCover).SetActive(true);
        }

        private void CloseEquipChange(PointerEventData eventData)
        {
            _canvas.sortingOrder = _parentOrder;
            Manager.Guide.Check(this);
            _animator.enabled = false;
            _curMode = Mode.Show;
            for (var idx = (int)GameObjects.SkillSelection1; idx <= (int)GameObjects.SkillSelection4; ++idx)
            {
                Get<GameObject>(idx).SetActive(false);
            }
            Get<GameObject>((int)GameObjects.IMG_SkillCover).SetActive(false);
        }

        private void SelectEquip(PointerEventData eventData, int idx)
        {
            if (_curMode == Mode.Show)
            {
                var skillId = EquipController.data.Skills[_selected.GetPresetIdx().Value].Value[idx].Value;
                if (skillId == -1) return;
                _selected.Set(EquipmentType.Skill, skillId);
            }
            else
            {
                EquipController.I.ChangeSkillEquip(idx, _selected.NowSelected().Value, _selected.GetPresetIdx().Value);
                CloseEquipChange(eventData);
            }
        }
        
        private void OnDisable()
        {
            _equipEventsManager?.Dispose();
            _selectionEventManager?.Dispose();
        }

        private void OnEnable()
        {
            _equipEventsManager?.Reconnect();
            _selectionEventManager?.Reconnect();
        }

        public void SetOrder(int order)
        {
            _parentOrder = order;
            _canvas.sortingOrder = _parentOrder;
        }
    }
}