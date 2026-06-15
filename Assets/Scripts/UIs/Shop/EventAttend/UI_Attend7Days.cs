using System;
using System.Collections.Generic;
using Controller;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbEvent;
using Data.DbShop;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Shop.DefaultShop;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace UIs.Shop.EventAttend
{
    public class UI_Attend7Days: UI_Popup, ILanguageSet
    {
        private List<UI_Attend7Days_Item> _items = new();
        private UI_PackageEquip_Item _costume;

        private CoroutineHandle _timeRoutine;
        private EventsManager _eventIdChanged;


        enum GameObjects
        {
            UI_ItemInfo
        }
        enum Texts
        {
            T_RemoveTime,
            T_ItemInfo
        }
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            
            var day = 1;
            var itemParent = Util.FindChild<Transform>(gameObject, "G_Attend", true);
            var attendEvent = DbAttendEvent.Get(EventAttendController.data.CurrentId.Value).RewardIds;
            while (day <= 7)
            {
                _items.Add(itemParent.GetChild(day - 1).gameObject.GetOrAddComponent<UI_Attend7Days_Item>());
                _items[day-1].Set(day, attendEvent[day-1]);
                day++;
            }

            Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            Util.FindChild(gameObject, "ANIM_Attend", true).GetOrAddComponent<AnimationEventSetter>().SetAction(CheckAttend);
            
            _costume = Util.FindChild<UI_PackageEquip_Item>(gameObject, "UI_PackageEquip_Item_Info", true);
            SetInfoText();
            Get<GameObject>((int)GameObjects.UI_ItemInfo).BindEvent(Functions.TrueCondition, _ => OnOffInfoPopup(false));
            Get<GameObject>((int)GameObjects.UI_ItemInfo).SetActive(false);

            _eventIdChanged = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenEventChanged,
                updatedField = new[] { EventAttendController.data.CurrentId }
            });
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            // Util.FindChild(gameObject, "B_Move", true).BindEvent(Functions.TrueCondition, _ =>
            // {
            //     EventAttendController.data.LastRewardedDate = EventAttendController.data.LastRewardedDate.AddDays(-1);
            //     EventAttendController.data.CanRewarded.Value = true;
            // }, UIEffectType.Bounce);
            // Util.FindChild(gameObject, "B_DateOver", true).BindEvent(Functions.TrueCondition, _ =>
            // {
            //     EventAttendController.data.CurrentId.Value = 0;
            // });

            
            SetTime();
            
            return true;
        }
        
        private void WhenEventChanged()
        {
            _eventIdChanged.Dispose();
            Manager.Resource.Destroy(gameObject);
        }

        private void OnOffInfoPopup(bool isOn)
        {
            Get<GameObject>((int) GameObjects.UI_ItemInfo).transform.position = _items[costumeIdx].transform.position;
            Get<GameObject>((int) GameObjects.UI_ItemInfo).SetActive(isOn);
        }

        private int costumeIdx;
        private void SetInfoText()
        {
            var rewards = DbAttendEvent.Get(EventAttendController.data.CurrentId.Value).RewardIds;
            for (var idx = 0; idx < rewards.Count; ++idx)
            {
                var reward = DbAttendEventReward.Get(rewards[idx]).Items[0];
                if (reward.currencyType == CurrencyType.Costume)
                {
                    costumeIdx = idx;
                    _items[idx].gameObject.BindEvent(Functions.TrueCondition, _ => OnOffInfoPopup(true));
                    var costume = DbCostume.Get(reward.id);
                    Get<TextMeshProUGUI>((int) Texts.T_ItemInfo).text =
                        "<color=#ffd25f>" + LocalString.Get(costume.NameId) + "</color><br>" +
                        StringMaker.GetCostumeOptionString(costume);
                    _costume.Set(DbCurrency.Get(reward.currencyType).GetResource(reward.id), reward.count, DbCostume.Get(reward.id).Grade, false);
                    return;
                }
            }
            
        }
        
        private void SetTime()
        {
            Timing.KillCoroutines(_timeRoutine);

            var item = DbAttendEvent.Get(EventAttendController.data.CurrentId.Value);
            Get<TextMeshProUGUI>((int) Texts.T_RemoveTime).text = 
                    StringMaker.GetDayTimeString(item.StartDateCal.AddDays(item.Duration)
                    .Subtract(DateTime.UtcNow.AddHours(9)));

            var now = DateTime.UtcNow.AddHours(9);
            var timeDiff = item.StartDateCal.AddDays(item.Duration).Subtract(now);
            if (timeDiff <= TimeSpan.Zero)
            {
                return;
            }
            if (timeDiff.Days > 0)
            {
                var nextDay = now.AddDays(1).Subtract(now.TimeOfDay);
                _timeRoutine = Timing.RunCoroutine(_TimeRoutine((int)nextDay.Subtract(now).TotalSeconds+1).CancelWith(gameObject));
            }
            else
            {
                var nowMinutes = new TimeSpan(0, now.Minute, now.Second);
                var nextHour = now.AddHours(1).Subtract(nowMinutes);
                _timeRoutine = Timing.RunCoroutine(_TimeRoutine((int)nextHour.Subtract(now).TotalSeconds+1).CancelWith(gameObject));
            }
        }
        
        private IEnumerator<float> _TimeRoutine(int seconds)
        {
            yield return Timing.WaitForSeconds(seconds);
            SetTime();
        }

        private void CheckAttend()
        {
            if (EventAttendController.data.CanRewarded.Value)
            {
                if (EventAttendController.data.RewardedCount.Value >= 7) return;
                EventAttendController.I.Attend(rewardPop =>
                {
                    _items[EventAttendController.data.RewardedCount.Value-1].SetStatus(true, true, rewardPop);
                });
            }
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        private void OnEnable()
        {
            if (_isInit) SetTime();
        }


        public override void WhenPopupClosed()
        {
            OnOffInfoPopup(false);
        }

        public void OnLanguageChanged(Locale locale)
        {
            SetInfoText();
        }
    }
}