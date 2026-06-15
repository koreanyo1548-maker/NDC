using Data;
using Data.DbDefinition;

using Managers;
using Managers.Base;
using MEC;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Toast
{
    public class UI_AwakeningToast : UI_Scene
    {
        private int _prevPlayed = 0;

        private Image _touchClose;
        private UI_Star _starUI;
        
        enum Particles
        {
            GrPower_Impact_Light_01_1 = 0,
            DustMotesLively = 1,
            FireworkBlue = 2
            
        }
        enum Texts
        {
            T_Grade,
            T_Name,
            T_AwakeningLevel,
            T_MaxLevel,
            T_Awakening
        }

        enum Images
        {
            IMG_Equip,
            IMG_Grade
        }

        private void Start()
        {
            transform.GetComponent<Canvas>().sortingOrder = 200;
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<ParticleSystem>(typeof(Particles));
            
            _touchClose = Util.FindChild<Image>(gameObject, "IMG_Dimmed", true);
            _touchClose.gameObject.BindEvent(Functions.TrueCondition, _ => WhenAnimationDone(), UIEffectType.None, false);


            _starUI = Util.FindChild(gameObject, "G_AwakeningStar", true).GetOrAddComponent<UI_Star>();

            return true;
        }
        
        private void OnEnable()
        {
            if (!_isInit) Init();
            _touchClose.raycastTarget = false;
        }


        public void SetInfo(Sprite equipSprite, string equipName, int curLevel, int prevMax, int nowMax,
            GradeType grade, string effect)
        {
            if (!_isInit) Init();
            _prevPlayed = 0;

            Get<Image>((int) Images.IMG_Equip).sprite = equipSprite;
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(grade.ToString());
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = equipName;
            Get<TextMeshProUGUI>((int) Texts.T_AwakeningLevel).text = string.Format(LocalString.Get(210081), curLevel);
            Get<TextMeshProUGUI>((int) Texts.T_MaxLevel).text = 
                string.Format(LocalString.Get(210079), prevMax, nowMax);
            Get<TextMeshProUGUI>((int) Texts.T_Awakening).text = effect;
            Get<TextMeshProUGUI>((int)Texts.T_Grade).text = LocalString.Get(DbGrade.Get(grade).NameId);
            _starUI.Set(curLevel);
            
            Manager.Sound.PlaySFX(SFXType.Reward);
        }

        private void PlayParticles(int max)
        {
            for (;_prevPlayed <= max; ++_prevPlayed)
            {
                var particle = Get<ParticleSystem>(_prevPlayed);
                if (particle.gameObject.activeSelf)
                {
                    particle.Simulate( 0.0f, true, true );
                    particle.Play();
                }
            }
        }
        
        private void EnableTouchClose()
        {
            _touchClose.raycastTarget = true;
        }

        private void WhenAnimationDone()
        {
            Manager.Sound.PlaySFX(SFXType.UI_Close);
            Manager.UI.CloseSingleUI(this);
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}