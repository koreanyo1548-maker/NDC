using System;
using System.Collections.Generic;
using System.Numerics;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbStage;
using Managers;
using Managers.Base;
using MEC;
using TMPro;
using UIBases;
using UIs.Attend;
using UIs.OfflineReward;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Random = UnityEngine.Random;

namespace UIs.Dungeon.TrainingGround
{
    public class UI_TrainingGroundClear: UI_Scene
    {
        private List<UI_Normal_Item> _items = new ();
        
        private int _prevPlayed = 0;
        enum Texts
        {
            T_CurrentDamageNum,
            T_PreviousDamageNum
        }
        
        enum Particles
        {
            GrPower_Impact_Light_01_1 = 0,
            DustMotesLively = 1
        }

        enum GameObjects
        {
            T_NoReward,
            T_BestRecord
        }

        enum Transforms
        {
            Rewards
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<ParticleSystem>(typeof(Particles));
            Bind<Transform>(typeof(Transforms));
            Bind<GameObject>(typeof(GameObjects));
            
            Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _ => Close());

            transform.GetComponent<Canvas>().sortingOrder = 100;
            return true;
        }

        public void Set(List<DbReward> rewards, int prevLevel, int curLevel, BigInteger prevDamage, BigInteger curDamage)
        {
            Init();
            
            Get<GameObject>((int)GameObjects.T_NoReward).SetActive(rewards.Count == 0);
            Get<GameObject>((int)GameObjects.T_BestRecord).SetActive(curDamage > prevDamage);
            Get<TextMeshProUGUI>((int)Texts.T_PreviousDamageNum).text = prevDamage == 0 ? LocalString.Get(210370) 
                : string.Format(LocalString.Get(210041), prevLevel) + "    " + Define.AddUnit(prevDamage, 3, 2);
            Get<TextMeshProUGUI>((int)Texts.T_CurrentDamageNum).text = curDamage == 0 ? LocalString.Get(210370) 
                    : string.Format(LocalString.Get(210041), curLevel) + "    " + Define.AddUnit(curDamage, 3, 2);
            
            for (var idx = 0; idx < rewards.Count; ++idx)
            {
                if (_items.Count <= idx) _items.Add(Manager.UI.MakeSubItem<UI_Normal_Item>(Get<Transform>((int)Transforms.Rewards)));
                _items[idx].Set(rewards[idx]);
                _items[idx].gameObject.SetActive(true);   
            }

            for (var idx = rewards.Count; idx < _items.Count; ++idx)
            {
                _items[idx].gameObject.SetActive(false);   
            }
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

        public override bool NeedRaycast()
        {
            return true;
        }

        private void Close()
        {
            Manager.UI.CloseSceneUI(this);
        }
    }
}