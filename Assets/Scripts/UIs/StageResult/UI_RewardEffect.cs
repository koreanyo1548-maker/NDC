using System;
using Managers;
using MEC;
using UIBases;
using UIs.FieldMain;
using UnityEngine;
using Utils;

namespace UIs.StageResult
{
    public class UI_RewardEffect: UI_Scene
    {
        private ParticleSystem _particle;
        private Transform _reward;
        private Vector3 _position;
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            _reward = Util.FindChild<Transform>(gameObject, "Reward", true);
            _particle = transform.GetComponentInChildren<ParticleSystem>();
            transform.GetComponent<Canvas>().sortingOrder = 11;

            return true;
        }
        
        
        public void Set(Vector3 center)
        {
            if (!_isInit) Init();

            _reward.position = center;
            _position = _reward.localPosition;
            _position.z = 0;
            _reward.localPosition = _position;
            Timing.CallDelayed(2, () => Manager.UI.CloseSceneUI(this));
            _particle.Simulate( 0.0f, true, true );
            _particle.Play();
        }
        
        public override bool NeedRaycast()
        {
            return false;
        }
    }
}