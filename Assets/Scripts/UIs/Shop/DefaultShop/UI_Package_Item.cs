using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_Package_Item : UI_Base
    {
        enum Images
        {
            IMG_Reward
        }

        enum Particles
        {
            P_MagicDust0
        }

        enum Texts
        {
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

        public void Set(Sprite resource, long count)
        {
            if (!_isInit) Init();

            Get<Image>((int) Images.IMG_Reward).sprite = resource;
            Get<TextMeshProUGUI>((int) Texts.T_Reward).text = Define.AddUnit(count, 5, 0);
            Get<ParticleSystem>((int)Particles.P_MagicDust0).Simulate( 0.0f, true, true );
            Get<ParticleSystem>((int)Particles.P_MagicDust0).Play();
        }
    }
}