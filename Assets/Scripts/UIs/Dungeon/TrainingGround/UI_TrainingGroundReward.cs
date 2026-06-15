using System.Collections.Generic;
using Controller.Infos;
using Data;
using Data.DbDungeon;
using Data.DbSummon;
using dynamicscroll;
using Managers;
using TMPro;
using UIBases;
using UIs.Summon;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Dungeon.TrainingGround
{
    public class UI_TrainingGroundReward: UI_Popup
    {
        private DynamicScroll<TrainingGroundRewardItem, UI_TrainingGroundReward_Item> _rewards;

        enum DynamicScrollRects
        {
            V_TrainingGroundReward
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<DynamicScrollRect>(typeof(DynamicScrollRects));
            
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            var rewards = new List<TrainingGroundRewardItem>();
            DbTrainingGroundReward.ForEach(reward =>
            {
                rewards.Add(new TrainingGroundRewardItem(reward));
            });
            
            _rewards = new DynamicScroll<TrainingGroundRewardItem, UI_TrainingGroundReward_Item>();
            _rewards.Initiate(Get<DynamicScrollRect>((int)DynamicScrollRects.V_TrainingGroundReward), rewards,
                -1,"Prefabs/UI/SubItem/UI_TrainingGroundReward_Item");
                                              
            return true;
        }
        
        public void Set()
        {
            if (!_isInit) Init();
            
            _rewards.MoveToIndex(LevelController.data.TrainingGroundStage.Value, 0.1f);
            _rewards.ForceUpdateList();
        }
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}