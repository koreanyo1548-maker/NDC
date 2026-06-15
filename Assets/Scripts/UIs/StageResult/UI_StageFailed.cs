using System;
using Data;
using DG.Tweening;
using Managers;
using Managers.Base;
using MEC;
using UIBases;
using UIs.Character;
using UIs.FieldMain;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.StageResult
{
    public class UI_StageFailed: UI_Scene
    {
        private CanvasGroup _canvasGroup;
        private ParticleSystem[] _particles;
        private Canvas _canvas;
        
        private Image _touchClose;

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            _canvasGroup = transform.GetComponent<CanvasGroup>();
            _canvas = Util.FindChild<Canvas>(gameObject, "T_Title", true);
            var failedGuide = Util.FindChild<Transform>(gameObject, "G_FailedGuide", true);
            failedGuide.Find("B_Character").gameObject.BindEvent(Functions.TrueCondition, _ => OpenGuide(UI_MainBottom.GameObjects.B_Character));
            failedGuide.Find("B_Summon").gameObject.BindEvent(Functions.TrueCondition, _ => OpenGuide(UI_MainBottom.GameObjects.B_Summon));
            failedGuide.Find("B_Inventory").gameObject.BindEvent(Functions.TrueCondition, _ => OpenGuide(UI_MainBottom.GameObjects.B_Inventory));
            _particles = new[]
            {
                Util.FindChild<ParticleSystem>(gameObject, "SoulFrostDeath", true)
            };

            _touchClose = Util.FindChild<Image>(gameObject, "IMG_Dimmed", true);
            _touchClose.gameObject.BindEvent(Functions.TrueCondition, _ => WhenAnimationDone(), UIEffectType.None, false);

            return true;
        }

        public void Set(FieldType field)
        {
            var characterUI = Manager.UI.GetPopupUI<UI_Character>();
            var isAbilityChanging = characterUI != null && characterUI.IsChangingAbility();
            transform.GetComponent<Canvas>().sortingOrder = field == FieldType.Stage || field == FieldType.Promotion ? 10 : isAbilityChanging ? 10 : 30;
            _canvas.sortingOrder = transform.GetComponent<Canvas>().sortingOrder + 2;
        }

        private void OpenGuide(UI_MainBottom.GameObjects button)
        {
            Manager.UI.CloseSingleUI(this);
            Manager.UI.CloseAllPopupUI();
            Manager.UI.GetSceneUI<UI_MainBottom>().OpenPopup(button);
            // if (Manager.Field.CurField.Value == FieldType.Stage) Manager.Field.SpawnGame();
        }

        private void OnEnable()
        {
            if (!_isInit) Init();
            _touchClose.raycastTarget = false;
            Manager.Sound.PlaySFX(SFXType.Failed);
        }

        private void PlayParticle(int idx)
        {
            _particles[idx].Simulate( 0.0f, true, true );
            _particles[idx].Play();
        }
        
        private void WhenAnimationDone()
        {
            Manager.Sound.PlaySFX(SFXType.UI_Close);
            _canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                _canvasGroup.alpha = 1;
                Manager.UI.CloseSingleUI(this);
            });
        }
        
        private void EnableTouchClose()
        {
            _touchClose.raycastTarget = true;
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}