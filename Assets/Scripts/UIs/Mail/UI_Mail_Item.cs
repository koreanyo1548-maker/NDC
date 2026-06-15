using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Data;
using Data.DbShop;

using Data.DbUser.Currency;
using Data.Stores;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Mail
{
    public class UI_Mail_Item: UI_Base
    {
        private EventsManager _rewardEventManager;
        private CoroutineHandle _timeRoutine;
        
        private Transform _rewardParent;

        private DbUserMail _mail;

        enum GameObjects
        {
            IMG_Badge
        }
        
        enum Texts
        {
            T_Title,
            T_Time            
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));

            _rewardParent = Util.FindChild(gameObject, "RewardParent", true).transform;
            
            Util.FindChild(gameObject, "B_GetReward", true).BindEvent(Functions.TrueCondition, _ => OpenInfoPopup(), UIEffectType.Bounce);

            return true;
        }

        public void Set(DbUserMail mail)
        {
            if (!_isInit) Init();

            _mail = mail;
            _rewardEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenMailRewardedChanged,
                updatedField = new[] {mail.IsRewarded}
            });
            
            
            if (mail.IsHide.Value)
            {
                RemoveMail();
                return;
            }

            mail.IsHide.ValueChanged += (_, _) =>
            {
                if (mail.IsHide.Value)
                {
                    RemoveMail();
                }
            };

            var rewards = Define.SmallToBigReward(mail.Rewards);
            
            var rewardCount = rewards.Count;
            for (var idx = 0; idx < rewardCount; ++idx)
            {
                if (_rewardParent.childCount <= idx)
                {
                    var item = Manager.UI.MakeSubItem<UI_MailReward_Item>(_rewardParent);
                    item.transform.localScale = Vector3.one;
                    item.Set(rewards[idx]);
                }
                else
                {
                    _rewardParent.GetChild(idx).GetComponent<UI_MailReward_Item>().Set(rewards[idx]);
                }
            }
            var listCount = _rewardParent.childCount;
            for (var idx = listCount-1; idx >= rewardCount; --idx)
            {
                Manager.Resource.Destroy(_rewardParent.GetChild(idx).gameObject);
            }

            Get<TextMeshProUGUI>((int) Texts.T_Title).text = mail.IsShop ? LocalString.Get(mail.InAppShop.NameId) : mail.MailInfo.title;
            
            _timeRoutine = Timing.RunCoroutine(_TimeRoutine().CancelWith(gameObject));
            WhenMailRewardedChanged();
        }

        IEnumerator<float> _TimeRoutine()
        {
            if (_mail.IsShop || _mail.MailInfo.type == MailType.Permanent)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Time).text = LocalString.Get(210262);
                yield break;
            }
            
            SetTime();
            switch (_mail.MailInfo.type)
            {
                case MailType.Once: yield return Timing.WaitForSeconds(_mail.MailInfo.endTime.Subtract(DateTime.UtcNow.AddHours(9)).Seconds);
                    break;
                case MailType.Everyday:
                    var now = DateTime.UtcNow.AddHours(9);
                    var todayResetTime = now.Subtract(now.TimeOfDay).Add(_mail.MailInfo.resetTime.TimeOfDay);
                    var diff = now < todayResetTime ? todayResetTime.Subtract(now) : todayResetTime.AddDays(1).Subtract(now);
                    yield return Timing.WaitForSeconds(diff.Seconds);
                    break;
            }
            
            while (true)
            {
                yield return Timing.WaitForSeconds(60); 
                SetTime();
            }
        }
        
        private void SetTime()
        {
            var timeStr = string.Empty;
            switch (_mail.MailInfo.type)
            {
                case MailType.Once: timeStr = 
                    string.Format(LocalString.Get(210260), StringMaker.GetTimeString(_mail.MailInfo.endTime.Subtract(DateTime.UtcNow.AddHours(9))));
                    break;
                case MailType.Everyday:
                    var now = DateTime.UtcNow.AddHours(9);
                    var todayResetTime = now.Subtract(now.TimeOfDay).Add(_mail.MailInfo.resetTime.TimeOfDay);
                    var diff = now < todayResetTime ? todayResetTime.Subtract(now) : todayResetTime.AddDays(1).Subtract(now);
                    timeStr = string.Format(LocalString.Get(210260), StringMaker.GetTimeString(diff));
                    break;
            }
            Get<TextMeshProUGUI>((int) Texts.T_Time).text = timeStr;
        }

        private void WhenMailRewardedChanged()
        {
            var isRewarded = _mail.IsRewarded.Value;
            Get<GameObject>((int)GameObjects.IMG_Badge).SetActive(!isRewarded);
            var rewardCount = _rewardParent.childCount;
            for (var idx = 0; idx < rewardCount; ++idx)
            {
                _rewardParent.GetChild(idx).GetComponent<UI_MailReward_Item>().SetRewarded(isRewarded);
            }

            Timing.KillCoroutines(_timeRoutine);
            _timeRoutine = Timing.RunCoroutine(_TimeRoutine().CancelWith(gameObject)); 
        }

        private void OpenInfoPopup()
        {
            Manager.UI.ShowPopupUI<UI_MailInfo>().Set(_mail, Get<TextMeshProUGUI>((int)Texts.T_Time).text);
        }
        
        private void RemoveMail()
        {
            _rewardEventManager.Dispose();
            Manager.Resource.Destroy(gameObject);
        }
        
        private void OnEnable()
        {
            _rewardEventManager?.Reconnect();
            if (_mail != null)
            {
                _timeRoutine = Timing.RunCoroutine(_TimeRoutine().CancelWith(gameObject));
            }
        }

        private void OnDisable()
        {
            _rewardEventManager.Dispose();
        }
    }
}