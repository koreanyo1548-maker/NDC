using System;
using Controller.Have;
using Data;
using Data.DbAbility;
using Data.DbDefinition;
using Data.DbUser.Equipment;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Ability
{
    public class UI_Ability_Item: UI_Base
    {
        private Sprite[] _lockSprites; // 0: 잠김, 1: 열림
        
        private int _preset;
        private int _idx;

        private Animator _animator;
        private EventsManager _abilityChangeEventsManager;
        
        private string _changeAnim = "Change";
        private string _autoChangeAnim = "AutoChange";

        private DbUserAbility Ability() => DbUserAbility.Get(_preset * 5 + _idx);
        
        enum Texts
        {
            T_Grade,
            T_Option
        }

        enum Images
        {
            IMG_Rune,
            IMG_LockIcon,
            B_Lock
        }

        enum Particles
        {
            LevelUpEffect
        }

        enum Transforms
        {
            EffectPosition
        }


        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<ParticleSystem>(typeof(Particles));
            Bind<Transform>(typeof(Transforms));

            _animator = transform.GetComponent<Animator>();

            _lockSprites = new[] {Manager.Resource.Load<Sprite>("Icon_Lock_128x128"), Manager.Resource.Load<Sprite>("Icon_UnLock_128x128")};
            
            Util.FindChild(gameObject, "B_Lock", true).BindEvent(Functions.TrueCondition, _ => LockSlot(), UIEffectType.Bounce);

            _abilityChangeEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = SetInfoAndSlotLock,
                updatedController = new[] {AbilityController.I.preset}
            });

            return true;
        }

        public void Set(int idx)
        {
            if (!_isInit) Init();
            _idx = idx;

            SetInfoAndSlotLock();
        }

        private void SetInfoAndSlotLock()
        {
            _preset = AbilityController.I.preset.Value;
            SetInfo();
            SetLock();
        }

        private void SetInfo()
        {
            var ability = Ability();
            if (ability.Option.Value == StatType.None) return;
            Get<TextMeshProUGUI>((int) Texts.T_Option).text = StringMaker.GetFinalString(ability.Option.Value,
                DbAbilityOption.Get(ability.Option.Value).Value[ability.OptionGrade - GradeType.Normal]);
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(ability.OptionGrade).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Grade).color = Define.GetGradeTextColor(ability.OptionGrade);
            Get<Image>((int)Images.IMG_Rune).sprite = Manager.Resource.Load<Sprite>(DbAbilityRune.Get(ability.Rune).Resource);
        }

        public void Change(bool isAuto)
        {
            if (Ability().IsLocked.Value) return;
            SetInfo();
            if (isAuto && !IsHigh()) return;
            _animator.Play(isAuto ? _autoChangeAnim : _changeAnim, 0, 0);
        }

        private void SetLock()
        {
            var ability = Ability();
            Get<Image>((int) Images.IMG_LockIcon).sprite = _lockSprites[ability.IsLocked.Value ? 0 : 1];
            Get<Image>((int)Images.IMG_LockIcon).color = ability.IsLocked.Value ? Define.Color58CFE0 : Define.ColorE4E4F1;
            Get<Image>((int) Images.B_Lock).color = ability.IsLocked.Value ? Define.Color2492A4 : Define.ColorB4B4B4;
        }

        private void LockSlot()
        {
            var ability = Ability();
            ability.SetLock(!ability.IsLocked.Value);
            SetLock();
        }
        
        private void PlayParticle(int onlyForHigh)
        {
            if (onlyForHigh == 1 && !IsHigh()) return;
            var particle = Get<ParticleSystem>((int) Particles.LevelUpEffect);
            particle.Simulate( 0.0f, true, true );
            particle.Play();
            particle.transform.position = Get<Transform>((int) Transforms.EffectPosition).position;
        }

        private bool IsHigh()
        {
            return DbUserAbility.Get(AbilityController.I.preset.Value * 5 + _idx).OptionGrade >= GradeType.Legendary;
        }
        
        private void SetInfoIf(int setIfHigh)
        {
            if ((setIfHigh == 1) == IsHigh()) SetInfo();
        }
        
        private void OnDisable()
        {
            _abilityChangeEventsManager?.Dispose();
        }

        private void OnEnable()
        {
            _abilityChangeEventsManager?.Reconnect();
        }

    }
}