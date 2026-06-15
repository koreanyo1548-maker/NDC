using System;
using System.Collections.Generic;
using Controller;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbEquipment;
using Data.DbStage;

using dynamicscroll;
using Managers;
using TMPro;
using UIs.Dungeon;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.StageMove
{
    public class UI_StageMove_Item: DynamicScrollObject<StageMoveItem>
    {
        private EventsManager _maxStageEventsManager;
        
        private Transform _rewardParent;
        
        private TextMeshProUGUI _recommendPowerText;
        private TextMeshProUGUI _expText;
        private TextMeshProUGUI _goldText;
        private TextMeshProUGUI _weaponStoneText;
        private TextMeshProUGUI _accessoryStoneText;
        private TextMeshProUGUI _stageText;
        private TextMeshProUGUI _moveText;

        private Image _stageMoveBtn;
        
        private GameObject _currentStageMark1;
        private GameObject _currentStageMark2;

        private UI_StageMove _stageMoveUI;
        
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _stageMoveUI = Manager.UI.GetPopupUI<UI_StageMove>();

            _rewardParent = Util.FindChild<Transform>(gameObject, "RewardParent", true);
            _recommendPowerText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_RecommendPower", true);
            _expText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Exp", true);
            _goldText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Gold", true);
            _weaponStoneText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_WeaponStone", true);
            _accessoryStoneText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_AccessoryStone", true);
            _moveText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Moving", true);
            _stageText = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Stage", true);
            _currentStageMark1 = Util.FindChild(gameObject, "IMG_CurrentStage", true);
            _currentStageMark2 = Util.FindChild(gameObject, "IMG_CurrentEmoji", true);
            _stageMoveBtn = Util.FindChild(gameObject, "B_StageMove", true).GetComponent<Image>();
            
            _stageMoveBtn.gameObject.BindEvent(MoveCondition, MoveStage);
            
            
            _maxStageEventsManager = new EventsManager(this, new EventsManager.Config
            {
                handler = () => UpdateScrollObject(null, CurrentIndex),
                updatedField = new[] {LevelController.data.MaxStage}
            });

            _maxStageEventsManager.Dispose();
        }

        private void MoveStage(PointerEventData eventData)
        {
            LevelController.I.MoveStage(GetStage());
            _stageMoveUI.ClosePopupUI();
            SettingController.I.SetAutoProgress(CurrentIndex == 0);
        }
        
        public override void UpdateScrollObject(StageMoveItem stageMove, int index)
        {
            base.UpdateScrollObject(stageMove, index);
            CurrentIndex = index;

            var stage = GetStage();
            var stageMeta = DbStageLevel.Get(stage);
            var rewardMeta = DbStageReward.Get(stage);
            _expText.text = Define.AddUnit(rewardMeta.Exp, 3, 2);
            _goldText.text = Define.AddUnit(rewardMeta.Gold, 3, 2);
            _recommendPowerText.text = Define.AddUnit(stageMeta.Power, 3, 2);
            _weaponStoneText.text = Define.AddUnit(rewardMeta.GrowthStone, 3, 2);
            _accessoryStoneText.text = Define.AddUnit(rewardMeta.GrowthStone, 3, 2);
            _stageText.text = string.Format(LocalString.Get(210017), stage);

            var count = 4;
            for (var idx = 0; idx < rewardMeta.Weapons.Count; ++idx)
            {
                var weapon = DbWeapon.Get(rewardMeta.Weapons[idx]);
                SetEquip(count, weapon.Resource, weapon.Grade);
                count++;
            }
            for (var idx = 0; idx < rewardMeta.Accessories.Count; ++idx)
            {
                var accessory = DbAccessory.Get(rewardMeta.Accessories[idx]);
                SetEquip(count, accessory.Resource, accessory.Grade);
                count++;
            }

            for (var idx = count; idx < _rewardParent.childCount; ++idx)
            {
                Manager.Resource.Destroy(_rewardParent.GetChild(idx).gameObject);
            }

            SetMoveButton();
            if (CurrentIndex == 0)
            {
                _maxStageEventsManager.Reconnect();
                _moveText.text = LocalString.Get(210156);
            }
            else
            {
                _maxStageEventsManager.Dispose();
                _moveText.text = LocalString.Get(210155);
            }

            void SetEquip(int idx, string resource, GradeType grade)
            {
                if (_rewardParent.childCount > idx) _rewardParent.GetChild(idx).GetComponent<UI_StageMoveReward_Item>().Set(resource, grade);
                else Manager.UI.MakeSubItem<UI_StageMoveReward_Item>(_rewardParent).Set(resource,grade);
            }
        }

        private void OnEnable()
        {
            SetMoveButton();
        }

        private void SetMoveButton()
        {
            var isCurrentStage = GetStage() == LevelController.data.Stage.Value;
            _currentStageMark1.SetActive(isCurrentStage);
            _currentStageMark2.SetActive(isCurrentStage);
            _stageMoveBtn.material = Define.GetUIMaterial(isCurrentStage);
        }
        
        private bool MoveCondition()
        {
            return GetStage() != LevelController.data.Stage.Value;
        }
        
        private int GetStage()
        {
            var maxStage = LevelController.data.MaxStage.Value;
            return CurrentIndex == 0 ? maxStage + 1 : (maxStage + 9) / 10 * 10 + 1 - 10 * CurrentIndex;
        }
    }

    public class StageMoveItem
    {
    }
}