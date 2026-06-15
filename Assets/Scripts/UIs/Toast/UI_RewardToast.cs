using System.Collections.Generic;
using System.Numerics;
using Data;
using Data.DbCommon;
using Data.DbDefinition;

using Managers;
using Managers.Base;
using MEC;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Toast
{
    public class UI_RewardToast: UI_Scene
    {
        private int _prevPlayed = 0;
        private bool _needRestartStage = false;
        
        private List<UI_RewardToast_Item> _items = new ();
        
        enum Texts
        {
            T_RewardInfo
        }

        enum Images
        {
            IMG_Dimmed
        }
        
        enum Particles
        {
            GrPower_Impact_Light_01_1 = 0,
            DustMotesLively = 1
        }

        enum Transforms
        {
            Rewards
        }
        
        private void Start()
        {
            transform.GetComponent<Canvas>().sortingOrder = 200;
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<ParticleSystem>(typeof(Particles));
            Bind<Transform>(typeof(Transforms));
            
            Get<Image>((int)Images.IMG_Dimmed).gameObject.BindEvent(Functions.TrueCondition, _ => WhenAnimationDone(), UIEffectType.None, false);

            return true;
        }

        private void Set(string rewardInfo, bool needRestartStage)
        {
            _needRestartStage = needRestartStage;
            _prevPlayed = 0;
            Get<Image>((int) Images.IMG_Dimmed).raycastTarget = false;
            Get<TextMeshProUGUI>((int) Texts.T_RewardInfo).text = rewardInfo;
            Manager.Sound.PlaySFX(SFXType.Reward);
        }

        public void SetReward(int rewardInfo, List<DbReward> rewards, bool needRestartStage = false)
        {
            if (!_isInit) Init();

            Set(LocalString.Get(rewardInfo), needRestartStage);
            SetReward(rewards);
        }
        
        public void SetReward(int rewardInfo, List<DbRewardBig> rewards, bool needRestartStage = false)
        {
            if (!_isInit) Init();

            Set(LocalString.Get(rewardInfo), needRestartStage);
            SetReward(rewards);
        }
        
        public void SetReward(int rewardInfo, bool needRestartStage, params DbReward[] rewards)
        {
            if (!_isInit) Init();

            Set(LocalString.Get(rewardInfo), needRestartStage);
            SetReward(rewards);
        }
        
        public void SetReward(string rewardInfo, bool needRestartStage, params DbReward[] rewards)
        {
            if (!_isInit) Init();

            Set(rewardInfo, needRestartStage);
            SetReward(rewards);
        }

        private void SetReward(DbReward[] rewards)
        {
            for (var idx = 0; idx < rewards.Length; ++idx)
            {
                if (_items.Count <= idx) _items.Add(Manager.UI.MakeSubItem<UI_RewardToast_Item>(Get<Transform>((int)Transforms.Rewards)));
                _items[idx].Set(rewards[idx]);
            }

            for (var idx = rewards.Length; idx < _items.Count; ++idx)
            {
                _items[idx].gameObject.SetActive(false);   
            }
        }


        private void SetReward(List<DbReward> rewards)
        {
            for (var idx = 0; idx < rewards.Count; ++idx)
            {
                if (_items.Count <= idx) _items.Add(Manager.UI.MakeSubItem<UI_RewardToast_Item>(Get<Transform>((int)Transforms.Rewards)));
                _items[idx].Set(rewards[idx]);
            }

            for (var idx = rewards.Count; idx < _items.Count; ++idx)
            {
                _items[idx].gameObject.SetActive(false);   
            }
        }

        private void SetReward(List<DbRewardBig> rewards)
        {
            for (var idx = 0; idx < rewards.Count; ++idx)
            {
                if (_items.Count <= idx) _items.Add(Manager.UI.MakeSubItem<UI_RewardToast_Item>(Get<Transform>((int)Transforms.Rewards)));
                _items[idx].Set(rewards[idx]);
            }

            for (var idx = rewards.Count; idx < _items.Count; ++idx)
            {
                _items[idx].gameObject.SetActive(false);   
            }
        }
        private void EnableTouchClose()
        {
            Get<Image>((int) Images.IMG_Dimmed).raycastTarget = true;
        }
        
        private void PlayParticles(int max)
        {

            if (max == 2)
            {
                foreach (var item in _items)
                {
                    item.PlayParticles();
                }
            }
            else
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
            
        }

        private void WhenAnimationDone()
        {
            Manager.Sound.PlaySFX(SFXType.UI_Close);
            Manager.UI.CloseSingleUI(this);
            if (_needRestartStage) Manager.Field.SpawnGame();
        }

        public override bool NeedRaycast()
        {
            return true;
        }
    }
}