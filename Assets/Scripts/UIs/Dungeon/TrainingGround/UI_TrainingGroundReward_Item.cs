using System;
using Controller.Infos;
using Data;
using Data.DbDungeon;
using dynamicscroll;
using TMPro;
using UIBases;
using UIs.Summon;
using UnityEngine;
using Utils;

namespace UIs.Dungeon.TrainingGround
{
    public class UI_TrainingGroundReward_Item : DynamicScrollObject<TrainingGroundRewardItem>
    {
        private UI_Normal_Item _reward;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _levelInfoText;
        private GameObject _isReceived;
        
        private bool _isInit;
        
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _reward = transform.Find("UI_Normal_Item").gameObject.GetOrAddComponent<UI_Normal_Item>();
            _levelText = transform.Find("Gift").Find("T_LevelTitle").GetComponent<TextMeshProUGUI>();
            _levelInfoText = transform.Find("T_LevelInfo").GetComponent<TextMeshProUGUI>();
            _isReceived = transform.Find("IMG_Received").gameObject;
            _isInit = true;
        }

        public override void UpdateScrollObject(TrainingGroundRewardItem rewardItem, int index)
        {
            base.UpdateScrollObject(rewardItem, index);

            var level = LevelController.data.TrainingGroundStage.Value;
            _reward.Set(rewardItem.reward.RewardType, rewardItem.reward.RewardCount, rewardItem.reward.RewardId);
            _levelText.text = string.Format(LocalString.Get(210041), rewardItem.reward.Id);
            _levelInfoText.text = string.Format(LocalString.Get(210377),
                Define.AddUnit(DbTrainingGroundLevel.Get(rewardItem.reward.Id).Damage, 3, 2));
            _isReceived.SetActive(index < level);
        }
    }
    public class TrainingGroundRewardItem
    {
        public DbTrainingGroundReward reward;

        public TrainingGroundRewardItem(DbTrainingGroundReward reward)
        {
            this.reward = reward;
        }
    }
}