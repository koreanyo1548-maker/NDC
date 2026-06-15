using System.Numerics;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEquipment;

using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.OfflineReward
{
    public class UI_OfflineReward_Item: UI_Base
    {
        enum Images
        {
            IMG_Grade,
            IMG_Equip
        }

        enum Texts
        {
            T_Count,
            T_Grade
        }

        enum Particles
        {
            P_MagicDust
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<ParticleSystem>(typeof(Particles));

            
            return true;
        }

        public void Set(IDbEquipment equip, BigInteger count)
        {
            if (!_isInit) Init();
            
            Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(equip.GetGrade()).NameId);
            Get<Image>((int)Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(equip.GetGrade().ToString());
            Get<Image>((int) Images.IMG_Equip).sprite = Manager.Resource.Load<Sprite>(equip.GetResource());

            Get<TextMeshProUGUI>((int)Texts.T_Count).text = Define.AddUnit(count, 3, 2);
        }

        public void PlayParticle()
        {
            var particle = Get<ParticleSystem>((int) Particles.P_MagicDust);
            if (particle.gameObject.activeSelf)
            {
                particle.Simulate( 0.0f, true, true );
                particle.Play();
            }
        }
    }
}