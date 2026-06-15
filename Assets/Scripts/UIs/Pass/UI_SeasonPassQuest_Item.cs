using System;
using Controller.Infos;
using Data.DbEvent;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Pass
{
    public class UI_SeasonPassQuest_Item: UI_Base, ILanguageSet
    {
        private EventsManager _questEventManager;
        private EventsManager _badgeEventManager;

        private DbSeasonPassQuest _quest;

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
            return true;
        }

        public void SetInfo(DbSeasonPassQuest quest)
        {
            _quest = quest;
            
            if (!_isInit) Init();
            
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_quest.NameId);
            WhenQuestChanged();

            if (SeasonPassController.data.CurrentId.Value != 0)
            {
                _questEventManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenQuestChanged,
                    updatedField = new[] {SeasonPassController.data.Quest[quest.Id]}
                });
            }

            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenQuestChanged()
        {
            if (SeasonPassController.data.CurrentId.Value == 0) return;
            var doCount = SeasonPassController.data.Quest[_quest.Id].Value;
            var isDone = doCount == -1;
            if (isDone) doCount = _quest.Goal;
            _progressVector.x = Mathf.Min(1, 1f * doCount / _quest.Goal);
            
            Get<GameObject>((int)GameObjects.IMG_Complete).SetActive(isDone);
            Get<GameObject>((int)GameObjects.B_GetReward).SetActive(!isDone);
            
            var onceReward = _quest.Point;
            Get<TextMeshProUGUI>((int) Texts.T_Reward).text = Math.Max(onceReward, onceReward).ToString("N0");
            Get<TextMeshProUGUI>((int) Texts.T_Progress).text = doCount + "/" + _quest.Goal;
            Get<Transform>((int) Transforms.IMG_Process).localScale = _progressVector;

            if (!isDone)
            {
                var canGet = doCount != -1 && doCount >= _quest.Goal;
                Get<GameObject>((int) GameObjects.IMG_Badge).SetActive(canGet);
                Get<Image>((int) Images.B_GetReward).material = Define.GetUIMaterial(!canGet);
                Get<TextMeshProUGUI>((int) Texts.T_Get).text = LocalString.Get(canGet ? 210147 : 210148);
            }
        }

        private void OnGetButtonClicked(PointerEventData eventData)
        {
            SeasonPassController.I.GetPoint(_quest.Id);
        }
        
        private bool Condition()
        {
            return SeasonPassController.data.Quest[_quest.Id].Value >= _quest.Goal;
        }

        private void OnDisable()
        {
            _badgeEventManager?.Dispose();
            _questEventManager?.Dispose();
        }

        private void OnEnable()
        {
            _badgeEventManager?.Reconnect();
            _questEventManager?.Reconnect();
        }

        public void UnuseIt()
        {
            _badgeEventManager?.Dispose();
            _questEventManager?.Dispose();
        }
        
        public void OnLanguageChanged(Locale locale)
        {
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_quest.NameId);
        }
    }
}