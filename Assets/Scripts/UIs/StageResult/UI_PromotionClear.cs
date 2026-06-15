using System.Collections.Generic;
using Controller;
using Controller.Infos;
using Data;
using Data.DbPromote;
using Data.DbShop;

using DG.Tweening;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.StageResult
{
    public class UI_PromotionClear: UI_Scene
    {
        private CanvasGroup _canvasGroup;
        private ParticleSystem[] _particles;

        private TextMeshProUGUI _promotionText;
        private TextMeshProUGUI _rewardText;
        private Image _promotionImg;

        private Image _touchClose;
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            transform.GetComponent<Canvas>().sortingOrder = 10;
            
            _touchClose = Util.FindChild<Image>(gameObject, "IMG_Dimmed", true);
            _touchClose.gameObject.BindEvent(Functions.TrueCondition, _ => WhenAnimationDone(), UIEffectType.None, false);

            _promotionText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Promotion", true);
            _rewardText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Reward", true);
            _promotionImg = Util.FindChild<Image>(gameObject, "IMG_PromotionIcon", true);
            
            _particles = new[]
            {
                Util.FindChild<ParticleSystem>(gameObject, "FeatherExplosion", true),
                Util.FindChild<ParticleSystem>(gameObject, "MagicDust", true)
            };

            return true;
        }

        public void Set()
        {
            if (!_isInit) Init();
            
            _touchClose.raycastTarget = false;
            Manager.Sound.PlaySFX(SFXType.Reward);

            var promotion = DbPromotion.Get(LevelController.data.Promotion.Value);
            _promotionText.text = string.Format(LocalString.Get(210269), LocalString.Get(promotion.NameId));
            _rewardText.text = string.Format(LocalString.Get(210270),
                    LocalString.Get(DbCostume.Get(promotion.CostumeId).NameId)) + "\n"
                + StringMaker.GetFinalStringWithColor(StatType.FinalAttackBonus, promotion.Attack, "FFF8AA") + "\n"
                + StringMaker.GetFinalStringWithColor(StatType.FinalHpBonus, promotion.Hp, "FFF8AA");
            _promotionImg.sprite = Manager.Resource.Load<Sprite>(promotion.Resource);
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
            _canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                _canvasGroup.alpha = 1;
                Manager.UI.CloseSingleUI(this);
                Manager.Field.SpawnGame();
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