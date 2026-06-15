using System;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbRecord;
using Data.DbUser.Progress;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Lock;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.NewbieQuest
{
    public class UI_NewbieQuest_Item: UI_Base
    {
        private EventsManager _questEventManager;


        private DbUserNewbieQuest _quest;

        private Vector3 _progressVector = new(1, 1, 1);

        enum GameObjects
        {
            B_GetReward,
            IMG_Complete
        }
        enum Transforms
        {
            IMG_Process
        }

        enum Images
        {
            IMG_Reward,
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
            Get<GameObject>((int)GameObjects.B_GetReward).BindEvent(() => _quest.CanRewarded, OnGetButtonClicked, UIEffectType.Bounce);
            Util.FindChild(gameObject, "IMG_Badge", true).GetOrAddComponent<UI_Badge>()
                .Set(new DbField[]{_quest.IsRewarded, _quest.Count}, () => _quest.CanRewarded);
                        
            WhenQuestChanged();
            return true;
        }

        public void SetInfo(DbNewbieQuest quest)
        {
            _quest = DbUserNewbieQuest.Get(quest.Id);
            
            if (!_isInit) Init();
            
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_quest.Meta.NameId);
            Get<TextMeshProUGUI>((int) Texts.T_Reward).text = _quest.Meta.RewardCount.ToString("N0");
            Get<Image>((int) Images.IMG_Reward).sprite = DbCurrency.Get(_quest.Meta.RewardType).GetResource(_quest.Meta.RewardId);
            
            _questEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenQuestChanged,
                updatedEntity = new[] {DbUserNewbieQuest.Get(_quest.Id)}
            });
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        private void WhenQuestChanged()
        {
            _progressVector.x = Mathf.Min(1, 1f * _quest.Count.Value / _quest.Meta.Goal);
            var isDone = _quest.IsRewarded.Value;
            if (isDone) transform.SetAsLastSibling();
            Get<GameObject>((int)GameObjects.B_GetReward).SetActive(!isDone);
            Get<GameObject>((int)GameObjects.IMG_Complete).SetActive(isDone);
            Get<TextMeshProUGUI>((int) Texts.T_Progress).text = _quest.Count.Value + "/" + _quest.Meta.Goal;
            Get<Transform>((int) Transforms.IMG_Process).localScale = _progressVector;
            
            var condition = _quest.CanRewarded;
            Get<Image>((int) Images.B_GetReward).material = Define.GetUIMaterial(!condition);
            Get<TextMeshProUGUI>((int) Texts.T_Get).text = LocalString.Get(condition ? 210147 : 210148);
        }

        public void SetSiblingIndex()
        { 
            if (_quest.IsRewarded.Value) transform.SetAsLastSibling();
        }

        private void OnGetButtonClicked(PointerEventData eventData)
        {
            if (!_quest.CanRewarded) return;

            var (rewardType, rewardAmount, rewardId) = NewbieQuestController.I.GetReward(_quest);
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            toast.SetReward(210242, false, new DbReward(rewardType, rewardAmount, rewardId));
        }

        private void OnDisable()
        {
            _questEventManager.Dispose();
        }

        private void OnEnable()
        {
            _questEventManager?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(_quest.Meta.NameId);
        }
    }
}