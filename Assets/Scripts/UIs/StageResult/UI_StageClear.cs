using System.Collections.Generic;
using System.Linq;
using Data;
using Data.DbDefinition;
using DG.Tweening;
using Managers;
using Managers.Base;
using MEC;
using UIBases;
using UIs.FieldMain;
using UIs.Shop.EventPackage;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.StageResult
{
    public class UI_StageClear: UI_Scene
    {
        private UI_StageRewardItem[] _items;
        private CanvasGroup _canvasGroup;
        private ParticleSystem[] _particles;

        private Image _touchClose;

        private bool _haveDia;
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            transform.GetComponent<Canvas>().sortingOrder = 10;
            _items = Util.FindChild(gameObject, "Rewards", true).GetComponentsInChildren<UI_StageRewardItem>();
            
            _touchClose = Util.FindChild<Image>(gameObject, "IMG_BlockTouch", true);
            _touchClose.gameObject.BindEvent(Functions.TrueCondition, _ => WhenAnimationDone(), UIEffectType.None, false);
            
            _particles = new[]
            {
                Util.FindChild<ParticleSystem>(gameObject, "FeatherExplosion", true),
                Util.FindChild<ParticleSystem>(gameObject, "MagicDust", true)
            };

            return true;
        }

        public void Set(List<UIReward> rewards)
        {
            if (!_isInit) Init();
            
            _touchClose.raycastTarget = false;
            Manager.Sound.PlaySFX(SFXType.Reward);
            
            for (var idx = 0; idx < _items.Length && idx < rewards.Count; ++idx)
            {
                _items[idx].Set(rewards[idx]);
                _items[idx].gameObject.SetActive(true);
            }

            for (var idx = rewards.Count; idx < _items.Length; ++idx)
            {
                _items[idx].gameObject.SetActive(false);
            }

            _haveDia = rewards.Exists(r => r.currency.Id == CurrencyType.Dia);
        }

        private void PlayParticles()
        {
            for (var idx = 0; idx < _particles.Length; ++idx)
            {
                _particles[idx].Simulate( 0.0f, true, true );
                _particles[idx].Play();
            }
        }

        private void WhenAnimationDone()
        {
            Manager.Sound.PlaySFX(SFXType.UI_Close);
            if (_haveDia) Manager.UI.ShowSceneUI<UI_RewardEffect>().Set(Vector3.zero);
            _canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                _canvasGroup.alpha = 1;
                Manager.UI.CloseSingleUI(this);
                if (Manager.Field.CurField.Value == FieldType.Stage) Manager.Field.SpawnGame();
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

    public class UIReward
    {
        public DbCurrency currency;
        public long count;

        public UIReward(DbCurrency currency, long count)
        {
            this.currency = currency;
            this.count = count;
        }
    }
}