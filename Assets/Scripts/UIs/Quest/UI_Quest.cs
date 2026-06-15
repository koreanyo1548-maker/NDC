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
using Data.Utils;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Inventory;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Quest
{
    public class UI_Quest : UI_Popup
    {
        private EventsManager _badgeEventManager;
        private Images? _curOpened = null;
        
        private Sprite[] _tabSprites; // 0: not selected 1: selected
        
        private Vector3 _positionSetter = new Vector3();

        private string _dailyReset;
        private string _weeklyReset;

        enum GameObjects
        {
            V_Repeat,
            V_Daily,
            V_Weekly,
            IMG_RepeatBadge,
            IMG_DailyBadge,
            IMG_WeeklyBadge
        }

        enum Images
        {
            B_RepeatTab,
            B_DailyTab,
            B_WeeklyTab,
            B_GetAll
        }

        enum Transforms
        {
            B_RepeatTab,
            B_DailyTab,
            B_WeeklyTab
        }
        
        enum Texts
        {
            T_ResetTime
        }


        void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Transform>(typeof(Transforms));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            Get<GameObject>((int) GameObjects.IMG_RepeatBadge).AddComponent<RepeatingScale>();
            Get<GameObject>((int) GameObjects.IMG_DailyBadge).AddComponent<RepeatingScale>();
            Get<GameObject>((int) GameObjects.IMG_WeeklyBadge).AddComponent<RepeatingScale>();
            
            Get<Image>((int) Images.B_GetAll).gameObject.BindEvent(RewardCondition, _ => GetAllRewards(), UIEffectType.Bounce, false);
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            _dailyReset = StringMaker.GetResetTime(210146);
            _weeklyReset = StringMaker.GetResetTime(210328);
            Get<TextMeshProUGUI>((int)Texts.T_ResetTime).text = string.Empty;
            

            foreach (Images tab in Enum.GetValues(typeof(Images)))
            {
                if (tab <= Images.B_WeeklyTab)
                {
                    Get<Image>((int) tab).GetComponent<Button>().onClick.AddListener(() => OnTabClicked(tab));
                }
            }

            var repeatParent = Util.FindChild<Transform>(gameObject, "G_RepeatParent", true);
            var dailyParent = Util.FindChild<Transform>(gameObject, "G_DailyParent", true);
            var weeklyParent = Util.FindChild<Transform>(gameObject, "G_WeeklyParent", true);

            Get<GameObject>((int) GameObjects.V_Daily).SetActive(false);
            Get<GameObject>((int) GameObjects.V_Weekly).SetActive(false);

            _tabSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

            var quests = DbQuest.GetAll(q => q.Cycle == QuestCycleType.Repeat);
            for (var idx = 0; idx < quests.Count; ++idx)
            {
                var item = Manager.UI.MakeSubItem<UI_Quest_Item>(repeatParent);
                item.SetInfo(quests[idx]);
            }

            quests = DbQuest.GetAll(q => q.Cycle == QuestCycleType.Daily);
            for (var idx = 0; idx < quests.Count; ++idx)
            {
                var item = Manager.UI.MakeSubItem<UI_Quest_Item>(dailyParent);
                item.SetInfo(quests[idx]);
            }

            quests = DbQuest.GetAll(q => q.Cycle == QuestCycleType.Weekly);
            for (var idx = 0; idx < quests.Count; ++idx)
            {
                var item = Manager.UI.MakeSubItem<UI_Quest_Item>(weeklyParent);
                item.SetInfo(quests[idx]);
            }
            
            _badgeEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = UpdateBadge,
                updatedField = new[] {BadgeController.data.Quests}
            });

            OnTabClicked(Images.B_RepeatTab);
            UpdateBadge();

            return true;
        }

        private void GetAllRewards()
        {
            var questType = _curOpened == Images.B_DailyTab ? QuestCycleType.Daily :
                _curOpened == Images.B_RepeatTab ? QuestCycleType.Repeat : QuestCycleType.Weekly;
            var rewards = QuestController.I.GetAllReward(questType);
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            
            var rewardsForToast = new List<DbReward>();

            foreach (var reward in rewards)
            { 
                rewardsForToast.Add(new DbReward(reward.Key, reward.Value));
            }
            toast.SetReward(210242, rewardsForToast);
        }
        
        public override void WhenPopupClosed()
        {
        }

        private void OnTabClicked(Images clicked)
        {
            if (_curOpened == clicked)
            {
                return;
            }

            if (_curOpened != null)
            {
                _positionSetter = Get<Transform>((int) _curOpened).localPosition;
                _positionSetter.y = 22.83f;
                Get<Transform>((int)_curOpened).localPosition = _positionSetter;
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
                CloseTab(_curOpened);
            }

            Manager.Sound.PlaySFX(SFXType.UI_Button);
            
            _curOpened = clicked; 
            _positionSetter = Get<Transform>((int) _curOpened).localPosition;
            _positionSetter.y = 7.92f;
            Get<Transform>((int) clicked).localPosition = _positionSetter;
            Get<Image>((int) clicked).sprite = _tabSprites[1];
            Get<Image>((int) Images.B_GetAll).material = Define.GetUIMaterial(!RewardCondition());
            OpenTab(clicked);

            void OpenTab(Images tab)
            {
                switch (tab)
                {
                    case Images.B_RepeatTab:
                        Get<GameObject>((int) GameObjects.V_Repeat).SetActive(true);
                        Get<TextMeshProUGUI>((int)Texts.T_ResetTime).text = string.Empty;
                        break;
                    case Images.B_DailyTab:
                        Get<GameObject>((int) GameObjects.V_Daily).SetActive(true);
                        Get<TextMeshProUGUI>((int)Texts.T_ResetTime).text = _dailyReset;
                        break;
                    case Images.B_WeeklyTab:
                        Get<GameObject>((int) GameObjects.V_Weekly).SetActive(true);
                        Get<TextMeshProUGUI>((int)Texts.T_ResetTime).text = _weeklyReset;
                        break;
                }
            }

            void CloseTab(Images? tab)
            {
                switch (tab)
                {
                    case Images.B_RepeatTab:
                        Get<GameObject>((int) GameObjects.V_Repeat).SetActive(false);
                        break;
                    case Images.B_DailyTab:
                        Get<GameObject>((int) GameObjects.V_Daily).SetActive(false);
                        break;
                    case Images.B_WeeklyTab:
                        Get<GameObject>((int) GameObjects.V_Weekly).SetActive(false);
                        break;
                }
            }
        }

        private void UpdateBadge()
        {
            var repeat = BadgeController.I.IsQuestBadgeOn(QuestCycleType.Repeat);
            var daily = BadgeController.I.IsQuestBadgeOn(QuestCycleType.Daily);
            var weekly = BadgeController.I.IsQuestBadgeOn(QuestCycleType.Weekly);
            Get<GameObject>((int)GameObjects.IMG_RepeatBadge).SetActive(repeat);
            Get<GameObject>((int)GameObjects.IMG_DailyBadge).SetActive(daily);
            Get<GameObject>((int)GameObjects.IMG_WeeklyBadge).SetActive(weekly);
            Get<Image>((int) Images.B_GetAll).material = Define.GetUIMaterial(_curOpened == Images.B_DailyTab ? !daily :
                _curOpened == Images.B_RepeatTab ? !repeat : !weekly);
        }

        private bool RewardCondition()
        {
            if (_curOpened == Images.B_DailyTab) return BadgeController.I.IsQuestBadgeOn(QuestCycleType.Daily);
            if (_curOpened == Images.B_RepeatTab) return BadgeController.I.IsQuestBadgeOn(QuestCycleType.Repeat);
            return BadgeController.I.IsQuestBadgeOn(QuestCycleType.Weekly);
        }

        private void OnDisable()
        {
            _badgeEventManager.Dispose();
        }

        private void OnEnable()
        {
            _badgeEventManager?.Reconnect();
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

    }
}