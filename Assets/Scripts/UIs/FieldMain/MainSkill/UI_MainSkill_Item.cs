using System.Collections.Generic;
using Controller.Infos;
using Controller.Play;
using Data.DbEquipment;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Toast;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.FieldMain.MainSkill
{
    public class UI_MainSkill_Item: UI_Base
    {
        private EventsManager _skillChangeEventManager;
        private EventsManager _autoSkillEventManager;

        private UI_MainSkill _mainSkill;
        
        private int _skillEquippedIdx;
        private int _curCoolTime;

        public int SkillId => _skillId;
        private int _skillId = -2;
        private int _maxCoolTime;
        
        private bool _canUse;
        
        private CoroutineHandle _coolTimer;
        
        private enum Images
        {
            IMG_Skill,
            IMG_SkillCoolTime
        }

        private enum Texts
        {
            T_SkillCoolTime
        }

        private void Awake()
        {
        }

        public override bool Init()
        {
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            //gameObject.BindEvent(Functions.TrueCondition, _ => UseSkill(), UIEffectType.Bounce);

            EquipController.I.Init();
            _skillChangeEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = () => WhenSkillChanged(true),
                updatedField = new[] {EquipController.data.Skills[EquipController.curSkillPreset.Value].Value[_skillEquippedIdx]}
            });

            _autoSkillEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenAutoSkillChanged,
                updatedField = new []{SettingController.data.IsAutoSkill}
            });
            
            EquipController.curSkillPreset.ValueChanged += (_, _) =>
            {
                _skillChangeEventManager.Set(() =>
                    {
                        WhenSkillChanged(EquipController.curSkillPreset.check);
                    },
                    new[] {EquipController.data.Skills[EquipController.curSkillPreset.Value].Value[_skillEquippedIdx]});
            };

            _mainSkill = Manager.UI.GetSceneUI<UI_MainSkill>();
            WhenSkillChanged(true);
            return true;
        }
        
        public void SetInfo(int idx)
        {
            _skillEquippedIdx = idx;
            Init();
        }

        private void StartCoolTime()
        {
            _canUse = false;
            _curCoolTime = _maxCoolTime;
            _coolTimer = Timing.RunCoroutine(_CoolTimeRoutine());
        }

        IEnumerator<float> _CoolTimeRoutine()
        {
            while (_curCoolTime-- > 0)
            {
                Get<TextMeshProUGUI>((int) Texts.T_SkillCoolTime).text = (_curCoolTime+1).ToString();
                var count = 10;
                while (count-- > 0)
                {
                    Get<Image>((int) Images.IMG_SkillCoolTime).fillAmount =  (_curCoolTime + count * 0.1f) / _maxCoolTime;
                    yield return Timing.WaitForSeconds(0.1f);
                }
            }

            CoolTimeDone();
        }

        
        private void WhenAutoSkillChanged()
        {
            if (SettingController.data.IsAutoSkill.Value && _curCoolTime <= 0) UseSkill();
        }

        public void StopTimer()
        {
            Timing.KillCoroutines(_coolTimer);
            Get<TextMeshProUGUI>((int) Texts.T_SkillCoolTime).text = string.Empty;
            Get<Image>((int) Images.IMG_SkillCoolTime).fillAmount = 1;
            _canUse = false;
        }

        public bool UseSkill(bool isAuto = true)
        {
            // if (_skillEquippedIdx == 0) Debug.Log("skill changed");
            if (!_canUse || !Manager.Skill.CanUseSkill || _skillId == -1 || Manager.Field.IsGameOver) return false;

            var useSkill = Manager.Skill.UseSkill(_skillId);
            if (useSkill.Item1)
            {
                _mainSkill.UseSkill();
                if (useSkill.Item2) StartCoolTime();
                return true;
            }

            if (isAuto) Timing.RunCoroutine(_RetryRoutine(), Define.KillWhenPlayerDieTag);
            else Manager.UI.ShowSingleUI<UI_Toast>().SetText(200042);
            return false;
        }

        IEnumerator<float> _RetryRoutine()
        {
            yield return Timing.WaitForSeconds(0.1f);
            UseSkill();
        }

        private void  WhenSkillChanged(bool startCoolTime)
        {
            //if (_skillEquippedIdx == 0) Debug.Log("skill changed");
            Timing.KillCoroutines(_coolTimer);
            var prev = _skillId;
            _skillId = EquipController.data.Skills[EquipController.curSkillPreset.Value].Value[_skillEquippedIdx].Value;
            if (_skillId == -1)
            {
                Get<Image>((int) Images.IMG_Skill).sprite = Manager.Resource.Load<Sprite>(Define.EmptySprite);
                CoolTimeDone();
                _canUse = false;
            }
            else
            {
                var meta = DbSkill.Get(_skillId);
                _maxCoolTime = meta.CoolTime;
                if (startCoolTime && prev != -2)
                {
                    _curCoolTime = meta.CoolTime;
                    StartCoolTime();
                }
                else
                {
                    CoolTimeDone();
                }
                Get<Image>((int) Images.IMG_Skill).sprite = Manager.Resource.Load<Sprite>(meta.Resource);
            }
        }

        private void CoolTimeDone()
        {
            _canUse = true;
            Get<TextMeshProUGUI>((int) Texts.T_SkillCoolTime).text = string.Empty;
            Get<Image>((int) Images.IMG_SkillCoolTime).fillAmount = Manager.Skill.CanUseSkill ? 0 : 1;
            if (SettingController.data.IsAutoSkill.Value) UseSkill();
        }

        public bool SetTotalCoolTime(bool canUse)
        {
            var use = false;
            
            if (!canUse && _canUse)
            {
                Get<Image>((int) Images.IMG_SkillCoolTime).fillAmount = 1;
            }
            else if (_canUse)
            {
                Get<Image>((int) Images.IMG_SkillCoolTime).fillAmount = 0;
                if (SettingController.data.IsAutoSkill.Value) use = UseSkill();
            }

            return use;
        }
        
        private void OnDisable()
        {
            _autoSkillEventManager.Dispose();
            _skillChangeEventManager.Dispose();
        }

        private void OnEnable()
        {
            _autoSkillEventManager?.Reconnect();
            _skillChangeEventManager?.Reconnect();
        }
    }
}