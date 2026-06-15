using System.Numerics;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using Utils;

namespace UIs.Etc
{
    public class UI_PowerToast: UI_Scene
    {
        enum Texts
        {
            T_Power,
            T_AddPower
        }

        enum Particles
        {
            FireworkBlue
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<ParticleSystem>(typeof(Particles));

            transform.GetComponent<Canvas>().sortingOrder = 205;
            
            return true;
        }

        public void Set(BigInteger now, BigInteger diff)
        {
            if (!_isInit) Init();
            
            Get<TextMeshProUGUI>((int) Texts.T_Power).text = Define.AddUnit(now, 6, 0);
            Get<TextMeshProUGUI>((int)Texts.T_AddPower).text = "(+" + Define.AddUnit(diff, 6, 0) + ")";
            Get<ParticleSystem>((int)Particles.FireworkBlue).Simulate(0, true, true);
            Get<ParticleSystem>((int)Particles.FireworkBlue).Play();
        }

        private void WhenAnimationDone()
        {
            Manager.Resource.Destroy(gameObject);
        }
        public override bool NeedRaycast()
        {
            return false;
        }
    }
}