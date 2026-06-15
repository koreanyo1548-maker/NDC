using System;
using System.Collections.Generic;
using Controller;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbRecord;

using Data.DbUser;
using Data.DbUser.Progress;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;
using Define = Utils.Define;
using Util = Utils.Util;

namespace UIs.Quest
{
    public class UI_Quest_Item: UI_Base, ILanguageSet
    {
        private EventsManager _questEventManager;
        private EventsManager _badgeEventManager;


        private DbUserQuest _quest;

        private Vector3 _progressVector = new(1, 1, 1);

        enum GameObjects
        {
            IMG_Badge,
            B_GetReward,
            IMG_Complete
        }
        enum Transforms
        {
            IMG_Process
        }

        enum Images
        {
            B_GetReward
        }
        
        enum Texts
        {
            T_Name,
            T_Reward,
            T_Progress,
            T_Get
        }
        public override bool Init()
        {
            if (!base.Init()) return false;
            
            Bind<GameObject>(typeof(GameObjects));
            Bind<Transform>(typeof(Transforms));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Get<GameObject>((int)GameObjects.B_GetReward).BindEvent(Condition, OnGetButtonClicked, UIEffectType.Bounce, false);
            Get<GameObject>((int) GameObjects.IMG_Badge).AddComponent<RepeatingScale>();
            
            WhenQuestChanged();
            UpdateBadge();
            return true;
        }

        public void SetInfo(DbQuest quest)
        {
            _quest = DbUserQuest.Get(quest.Id);
            
            if (!_isInit) Init();
            
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_quest.Meta.NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Reward).text = _quest.Meta.RewardCount.ToString("N0");
            
            _questEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenQuestChanged,
                updatedEntity = new[] {DbUserQuest.Get(_quest.Id)}
            });
            
            _badgeEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = UpdateBadge,
                updatedField = new[] {BadgeController.data.Quests.Value[_quest.Id]}
            });

            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenQuestChanged()
        {
            var clear = _quest.Count.Value >= _quest.Meta.Goal;
            _progressVector.x = Mathf.Min(1, 1f * _quest.Count.Value / _quest.Meta.Goal);
            var isDone = clear && _quest.IsRewarded.Value;
            if (isDone) transform.SetAsLastSibling();
            Get<GameObject>((int)GameObjects.B_GetReward).SetActive(!isDone);
            Get<GameObject>((int)GameObjects.IMG_Complete).SetActive(isDone);
            if (_quest.Meta.Cycle == QuestCycleType.Repeat)
            {
                var count = _quest.Count.Value / _quest.Meta.Goal;
                var onceReward = _quest.Meta.RewardCount;
                Get<TextMeshProUGUI>((int) Texts.T_Reward).text = Math.Max(onceReward, count * onceReward).ToString("N0");
            }
            Get<TextMeshProUGUI>((int) Texts.T_Progress).text = _quest.Count.Value + "/" + _quest.Meta.Goal;
            Get<Transform>((int) Transforms.IMG_Process).localScale = _progressVector;
        }

        private void OnGetButtonClicked(PointerEventData eventData)
        {
            if (!_quest.CanRewarded) return;

            var (rewardType, rewardAmount) = QuestController.I.GetReward(_quest);
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            toast.SetReward(210242, false, new DbReward(rewardType, rewardAmount));
        }
        
        private void UpdateBadge()
        {
           // if (_quest.Id == 0) Debug.Log(_quest);
            var condition = Condition();
            Get<GameObject>((int) GameObjects.IMG_Badge).SetActive(BadgeController.data.Quests.Value[_quest.Id].Value);
            Get<Image>((int) Images.B_GetReward).material = Define.GetUIMaterial(!condition);
            Get<TextMeshProUGUI>((int) Texts.T_Get).text = LocalString.Get(condition ? 210147 : 210148);
        }

        private bool Condition()
        {
            return BadgeController.I.IsQuestBadgeOn(_quest.Id);
        }

        private void OnDisable()
        {
            _badgeEventManager.Dispose();
            _questEventManager.Dispose();
        }

        private void OnEnable()
        {
            _badgeEventManager?.Reconnect();
            _questEventManager?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_quest.Meta.NameId);
        }
    }
}