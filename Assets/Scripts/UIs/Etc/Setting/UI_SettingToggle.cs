using System;
using Data.Utils;
using DG.Tweening;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc
{
    public class UI_SettingToggle: UI_Base
    {
        private Image _toggle;

        private bool _isOn;
        
        enum Images
        {
            IMG_Switch
        }

        enum Transforms
        {
            IMG_Switch
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            _toggle = transform.GetComponent<Image>();
            Bind<Image>(typeof(Images));
            Bind<Transform>(typeof(Transforms));

            return true;
        }

        public void Set(bool isOn, Action whenToggled)
        {
            if (!_isInit) Init();
            _isOn = isOn; 
            SetToggled();
            
            gameObject.BindEvent(Functions.TrueCondition, _ =>
            {
                _isOn = !_isOn;
                SetToggled();
                whenToggled();
            }, UIEffectType.Bounce);
        }

        private void SetToggled()
        {
            Get<Image>((int) Images.IMG_Switch).material = Define.GetUIMaterial(!_isOn);
            _toggle.color = _isOn ? Define.ColorFFED8D : Define.Color5E5E5E;
            Get<Transform>((int) Transforms.IMG_Switch).DOKill();
            Get<Transform>((int)Transforms.IMG_Switch).DOLocalMoveX(_isOn ? 47.7f : -47.7f, 0.3f);
        }
    }
}