using System;
using Data;
using Data.DbDefinition;
using Data.DbUser.Equipment;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Inventory.Equipment
{
    public class UI_EquipMerge : UI_Popup
    {
        enum Texts
        {
            T_PrevName,
            T_PrevGrade,
            T_NextName,
            T_NextGrade,
            T_PrevCount,
            T_NextCount,
            T_MergeCount
        }

        enum GameObjects
        {
            B_Prev,
            B_Next
        }

        enum Images
        {
            IMG_PrevEquip,
            IMG_NextEquip,
            IMG_PrevGrade,
            IMG_NextGrade,
            IMG_MergeArrowGauge,
            B_Merge,
            B_Min,
            B_Minus,
            B_Max,
            B_Plus
        }

        enum Transforms
        {
            EffectPosition
        }

        enum Animators
        {
            IMG_MergeArrow
        }

        private bool _isWeapon;
        private IDbUserEquipment _prev;
        private IDbUserEquipment _next;

        public EquipmentType EquipmentType => _isWeapon ? EquipmentType.Weapon : EquipmentType.Accessory;

        private int _mergeCount;
        
        public UIField<bool> Check = new(false);

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<Transform>(typeof(Transforms));
            Bind<Animator>(typeof(Animators));
            
            Util.FindChild(gameObject, "B_Min", true).BindEvent(Functions.TrueCondition, _ => ChangeCount(1, false), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Minus", true).BindEvent(Functions.TrueCondition, _ => ChangeCount(-1, true), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Max", true).BindEvent(Functions.TrueCondition, _ => ChangeCount(GetMaxMergeCount(), false), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Plus", true).BindEvent(Functions.TrueCondition, _ => ChangeCount(1, true), UIEffectType.Bounce);
            
            Get<GameObject>((int)GameObjects.B_Prev).BindEvent(Functions.TrueCondition, _ => Set(_isWeapon, _prev.PrevHave()), UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Next).BindEvent(Functions.TrueCondition, _ => Set(_isWeapon, _prev.NextHave(true)), UIEffectType.Bounce);
            
            Util.FindChild(gameObject, "B_Merge", true).BindEvent(ConditionMerge, Merge, UIEffectType.Bounce, false);
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Get<Animator>((int) Animators.IMG_MergeArrow).enabled = false;
            
            Get<Animator>((int) Animators.IMG_MergeArrow).GetComponent<AnimationEventSetter>().SetAction(() =>
            {
                var levelUp = Manager.Resource.Instantiate("Particles/LevelUpEffect", 10).transform;
                levelUp.GetComponent<ParticleSystem>().Play();
                levelUp.position = Get<Transform>((int) Transforms.EffectPosition).position;
                Get<Image>((int) Images.IMG_MergeArrowGauge).fillAmount = 0;
                Get<Animator>((int) Animators.IMG_MergeArrow).enabled = false;
            });

            return true;
            
        }

        public void Set(bool isWeapon, IDbUserEquipment equipment)
        {
            if (!_isInit) Init();

            _isWeapon = isWeapon;
            _mergeCount = 1;
            _prev = equipment;
            _next = _isWeapon ? DbUserWeapon.Get(equipment.GetId() + 1) : DbUserAccessory.Get(equipment.GetId() + 1);

            var prev = _prev.GetMeta();
            var next = _next.GetMeta();
            
            Get<GameObject>((int)GameObjects.B_Prev).SetActive(_prev.PrevHave() != null);
            Get<GameObject>((int) GameObjects.B_Next).SetActive(_prev.NextHave(true) != null);
            
            Get<TextMeshProUGUI>((int) Texts.T_PrevName).text = LocalString.Get(prev.GetNameId());
            Get<TextMeshProUGUI>((int) Texts.T_PrevGrade).text = LocalString.Get(DbGrade.Get(prev.GetGrade()).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_NextName).text = LocalString.Get(next.GetNameId());
            Get<TextMeshProUGUI>((int) Texts.T_NextGrade).text = LocalString.Get(DbGrade.Get(next.GetGrade()).NameId);
            Get<Image>((int) Images.IMG_PrevEquip).sprite = Manager.Resource.Load<Sprite>(prev.GetResource());
            Get<Image>((int) Images.IMG_NextEquip).sprite = Manager.Resource.Load<Sprite>(next.GetResource());
            Get<Image>((int)Images.IMG_PrevGrade).sprite = Manager.Resource.Load<Sprite>(prev.GetGrade().ToString());
            Get<Image>((int)Images.IMG_NextGrade).sprite = Manager.Resource.Load<Sprite>(next.GetGrade().ToString());
            
            ChangeCount(0, true);
            Check.Value = !Check.Value;
        }
        
        private void ChangeCount(int count, bool isAdd)
        {
            if (isAdd) _mergeCount += count;
            else _mergeCount = count;

            var max = _prev.GetCount() / 5;
            _mergeCount = max == 0 ? 0 : Math.Clamp(_mergeCount, 1, max);
            
            Get<Image>((int)Images.B_Min).material = Define.GetUIMaterial(_mergeCount < 2);
            Get<Image>((int)Images.B_Minus).material = Define.GetUIMaterial(_mergeCount < 2);
            Get<Image>((int)Images.B_Max).material = Define.GetUIMaterial(_mergeCount > max - 1);
            Get<Image>((int)Images.B_Plus).material = Define.GetUIMaterial(_mergeCount > max - 1);
            
            Get<Image>((int)Images.B_Merge).material = Define.GetUIMaterial(_mergeCount < 1);

            Get<TextMeshProUGUI>((int) Texts.T_PrevCount).text = $"{Define.AddUnit(_prev.GetCount(), 3, 0)}(-{Define.AddUnit(5 * _mergeCount, 3, 0)})";
            Get<TextMeshProUGUI>((int) Texts.T_NextCount).text = $"{Define.AddUnit(_next.GetCount(), 3, 0)}(+{Define.AddUnit(_mergeCount, 3, 0)})";
            Get<TextMeshProUGUI>((int) Texts.T_MergeCount).text = _mergeCount.ToString("N0");
        }

        private int GetMaxMergeCount()
        {
            return _prev.GetCount() / 5;
        }

        private bool ConditionMerge()
        {
            if (_mergeCount < 1) return false;
            return _prev.CanMerge(_mergeCount);
        }

        
        private void Merge(PointerEventData eventData)
        {
            if (ConditionMerge())
            {
                Manager.Sound.PlaySFX(SFXType.UI_Upgrade);
                _prev.MergeIt(_mergeCount);
                
                ChangeCount(1, false);

                Get<Animator>((int) Animators.IMG_MergeArrow).enabled = true;
                Get<Animator>((int) Animators.IMG_MergeArrow).Play("synthesize", 0, 0);
            }
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        private void OnDisable()
        {
            Check.ValueChanged = null;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}