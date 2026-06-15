using System;
using Data;
using DG.Tweening;
using Managers.Base;
using MEC;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UIs.Prologue
{
    public class UI_Prologue : UI_Base
    {
        [SerializeField] private Sprite[] _cuts;
        private Transform _cutScene;
        private Animator _animator;
        
        private int _curCut;
        private static readonly int CutNext = Animator.StringToHash("CutNext");

        private SoundManager _soundManager;

        private CoroutineHandle _sfxRoutine;

        enum Images
        {
            IMG_Cut,
            IMG_CutColor
        }

        enum GameObjects
        {
            B_Skip,
            B_Next
        }

        private void Awake()
        {
            Init();
            _soundManager = GameObject.Find("@Sound").GetComponent<SoundManager>();
            _soundManager.PlayBGM(BGMType.Prologue);
            _soundManager.PlaySFX(SFXType.Prologue_FilmFast, true);
        }

        public override bool Init()
        {   
            _cutScene = Util.FindChild<Transform>(gameObject, "G_CutScene", true);
            _animator = _cutScene.GetComponent<Animator>();
            var animEvent = _animator.gameObject.GetOrAddComponent<AnimationMultipleEventSetter>();
            animEvent.SetAction(FilmSlowStart);
            animEvent.SetAction(CutEnd);
            animEvent.SetAction(CutEndFinish);
            
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));
            Get<GameObject>((int) GameObjects.B_Skip).BindEvent(Functions.TrueCondition, Skip);
            Get<GameObject>((int) GameObjects.B_Next).BindEvent(Functions.TrueCondition, Next, UIEffectType.Bounce, false);
            Get<Image>((int)Images.IMG_CutColor).color = Define.ColorTransparent;
            Get<GameObject>((int)GameObjects.B_Next).SetActive(false);
            PlayerPrefs.SetInt(SettingType.DoPrologue1.ToString(), 1);

            Shake();
            return true;
        }

        private void Next(PointerEventData eventData)
        {
            _curCut++;
            _animator.SetTrigger(CutNext);
            Get<GameObject>((int)GameObjects.B_Next).SetActive(false);
            _sfxRoutine = Timing.CallDelayed(0.4f, () => _soundManager.PlaySFX(SFXType.Prologue_FilmFast, true));
            
            Shake();
        }

        private void FilmSlowStart()
        {
            Get<Image>((int) Images.IMG_Cut).sprite = _cuts[_curCut];
            _soundManager.StopSFX(SFXType.Prologue_FilmFast);
            _soundManager.PlaySFX(SFXType.Prologue_FilmSlow);
        }

        private void CutEnd()
        {
            if (_curCut < _cuts.Length - 2)
            {
                Get<GameObject>((int)GameObjects.B_Next).SetActive(true);
            }
        }

        private void CutEndFinish()
        {
            if (_curCut == _cuts.Length - 2)
            {
                Get<Image>((int)Images.IMG_CutColor).DOFade(1, 1).OnComplete(PrologueDone);
            }
        }

        private void Skip(PointerEventData eventData)
        {
            PrologueDone();
        }

        public void PrologueDone()
        {
            Timing.KillCoroutines(_sfxRoutine);
            _soundManager.StopSFX(SFXType.Prologue_FilmFast);
            Instantiate(Resources.Load<UI_FadeOnScene>("Prefabs/UI/Scene/UI_FadeOnScene"), transform).FadeIn(() =>
            {
                SceneManager.LoadScene(SceneType.Field.ToString());
            });
        }

        private void Shake()
        {
            _cutScene.DOShakePosition(2, new Vector3(40, 40, 40), 21, 34)
            .SetLoops(1).SetEase(Ease.OutQuad);
        }
    }
}