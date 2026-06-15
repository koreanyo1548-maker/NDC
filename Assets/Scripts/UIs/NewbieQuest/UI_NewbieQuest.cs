using System;
using System.Collections.Generic;
using Controller.Infos;
using Data;
using Data.DbCommon;
using Data.DbRecord;
using Managers;
using Managers.Base;
using TMPro;
using UIBases;
using UIs.Lock;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.NewbieQuest
{
    public class UI_NewbieQuest: UI_Popup, ILanguageSet
    {
        private Images? _curOpened = null;
        
        private Sprite[] _tabSprites; // 0: not selected 1: selected
        
        private Vector3 _positionSetter = new Vector3();

        enum GameObjects
        {
            Day1,
            Day2,
            Day3,
            Day4,
            Day5,
            Day6,
            Day7
        }

        enum Images
        {
            B_Day1Tab,
            B_Day2Tab,
            B_Day3Tab,
            B_Day4Tab,
            B_Day5Tab,
            B_Day6Tab,
            B_Day7Tab,
            B_GetAll
        }

        enum Transforms
        {
            B_Day1Tab,
            B_Day2Tab,
            B_Day3Tab,
            B_Day4Tab,
            B_Day5Tab,
            B_Day6Tab,
            B_Day7Tab
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
            
            Get<Image>((int) Images.B_GetAll).gameObject.BindEvent(RewardCondition, _ => GetAllRewards(), UIEffectType.Bounce, false);
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            foreach (Images tab in Enum.GetValues(typeof(Images)))
            {
                if (tab == Images.B_Day1Tab)
                {
                    Get<Image>((int) tab).gameObject.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce);
                }
                else if (tab <= Images.B_Day7Tab)
                {
                    var obj = Get<Image>((int) tab);
                    obj.gameObject.GetOrAddComponent<UI_Locked>().Set(LockType.NewbieQuestDay2 + (int)tab - (int)Images.B_Day2Tab,
                        obj, Util.FindChild(obj.gameObject, "IMG_LockIcon"), null,
                        () => obj.gameObject.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce));
                }
            }
            
            var parents = new List<Transform>();
            for (var idx = 1; idx <= 7; ++idx)
            {
                var day = idx;
                parents.Add(Util.FindChild<Transform>(gameObject, $"G_Day{idx}Parent", true));
                Util.FindChild(gameObject, $"IMG_Day{idx}Badge", true).GetOrAddComponent<UI_ControllerBadge>()
                    .Set(new [] {NewbieQuestController.I.IsAnyRewardInDay[day]}, () => NewbieQuestController.I.IsAnyRewardInDay[day].Value,
                        idx == 1 ? null : LockType.NewbieQuestDay2 + idx-2);
                Util.FindChild<TextMeshProUGUI>(gameObject, "T_Day" + idx, true).text =
                    string.Format(LocalString.Get(210342), idx);
            }

            for (var idx = (int)GameObjects.Day2; idx <= (int)GameObjects.Day7; ++idx)
            {
                Get<GameObject>(idx).SetActive(false);
            }

            _tabSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

            DbNewbieQuest.ForEach(q =>
            {
                UI_NewbieQuest_Item item;
                if (q.ToDo == QuestType.NewbieDayQuestClear) 
                    item = parents[q.Day-1].parent.parent.parent.GetChild(0).gameObject.GetOrAddComponent<UI_NewbieQuest_Item>();
                else item = Manager.UI.MakeSubItem<UI_NewbieQuest_Item>(parents[q.Day-1], "UI_Quest_Item");
                item.SetInfo(q);
            });

            foreach (var parent in parents)
            {
                for (var idx = 0; idx < parent.childCount; ++idx)
                {
                    parent.GetChild(idx).GetComponent<UI_NewbieQuest_Item>().SetSiblingIndex();
                }
            }
            
            OnTabClicked(Images.B_Day1Tab);

            return true;
        }

        private void GetAllRewards()
        {
            var rewards = NewbieQuestController.I.GetAllRewards((int)_curOpened-(int)Images.B_Day1Tab+1);
            var toast = Manager.UI.ShowSingleUI<UI_RewardToast>();
            
            var rewardsForToast = new List<DbReward>();

            foreach (var (rewardType, reward) in rewards)
            {
                foreach (var (rewardId, rewardCount) in reward)
                {
                    rewardsForToast.Add(new DbReward(rewardType, rewardCount, rewardId));
                }
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
                Get<GameObject>((int)tab).SetActive(true);
            }

            void CloseTab(Images? tab)
            {
                Get<GameObject>((int)tab).SetActive(false);
            }
        }

        private bool RewardCondition()
        {
            return NewbieQuestController.I.CanGetReward((int)_curOpened - (int) Images.B_Day1Tab + 1);
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public void OnLanguageChanged(Locale locale)
        {
            for (var idx = 1; idx <= 7; ++idx)
            {
                Util.FindChild<TextMeshProUGUI>(gameObject, "T_Day" + idx, true).text =
                    string.Format(LocalString.Get(210342), idx);
            }
        }
    }
}