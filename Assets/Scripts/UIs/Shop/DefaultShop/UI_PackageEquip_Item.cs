using Data;
using Data.DbDefinition;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_PackageEquip_Item: UI_Base
    {
        private GradeType _gradeType;
        enum Images
        {
            IMG_Grade,
            IMG_Equip
        }

        enum Particles
        {
            P_MagicDust4
        }

        enum Texts
        {
            T_Grade,
            T_Reward
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<Image>(typeof(Images));
            Bind<ParticleSystem>(typeof(Particles));
            Bind<TextMeshProUGUI>(typeof(Texts));
            return true;
        }

        public void Set(Sprite resource, long count, GradeType grade, bool needParticle = true)
        {
            if (!_isInit) Init();

            _gradeType = grade;
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(grade.ToString());
            Get<Image>((int) Images.IMG_Equip).sprite = resource;
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(grade).NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Reward).text = Define.AddUnit(count, 5, 0);
            if (needParticle)
            {
                Get<ParticleSystem>((int)Particles.P_MagicDust4).Simulate( 0.0f, true, true );
                Get<ParticleSystem>((int)Particles.P_MagicDust4).Play();
            }
            
            LocalizationSettings.SelectedLocaleChanged += LanguageChanged;
        }

        public void LanguageChanged(Locale locale)
        {
            Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(_gradeType.ToString());
        }
    }
}