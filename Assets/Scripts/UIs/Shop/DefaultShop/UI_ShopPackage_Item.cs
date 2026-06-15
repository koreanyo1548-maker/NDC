using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbShop;
using Data.Utils;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_ShopPackage_Item: UI_Base, ILanguageSet, IDayDiffChecker
    {
        private IDbShop _item;
        private Action _afterBuy;

        private CoroutineHandle _timeRoutine;

        enum GameObjects
        {
            IMG_SoldOutBG,
            IMG_NewTag,
            IMG_BestTag,
            IMG_Value,
            IMG_Limited,
            IMG_Timer
        }

        enum Texts
        {
            T_Count,
            T_RemoveTime,
            T_Cost
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            return true;
        }

        public void SetInfo(DateTime now, IDbShop item, Action afterBuy = null)
        {
            if (!_isInit) Init();

            _item = item;
            _afterBuy = afterBuy;
            
            Util.FindChild<Image>(gameObject, "IMG_Bg").sprite = Manager.Resource.Load<Sprite>(item.GetBackgroundResource());
            Util.FindChild<Image>(gameObject, "IMG_Item").sprite = Manager.Resource.Load<Sprite>(item.GetResource());
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = LocalString.Get(item.GetNameId());
            Get<TextMeshProUGUI>((int)Texts.T_Cost).text = item.GetDisplayPrice();
            if (_item.GetValue() > 0) Util.FindChild<TextMeshProUGUI>(gameObject, "T_Value", true).text = string.Format(LocalString.Get(210307), _item.GetValue());

            var rewardParent = Util.FindChild<Transform>(gameObject, "G_PackageRewards");
            
            var rewards = item.GetRewards();
            for (var idx = 0; idx < rewards.Count; ++idx)
            {
                var reward = rewards[idx];
                if (reward.currencyType == CurrencyType.Accessory || reward.currencyType == CurrencyType.Weapon ||
                    reward.currencyType == CurrencyType.Skill)
                {
                    var grade = DbSelector.GetEquipment(reward.currencyType, reward.id).GetGrade();
                    Manager.UI.MakeSubItem<UI_PackageEquip_Item>(rewardParent).Set(DbCurrency.Get(reward.currencyType).GetResource(reward.id),
                        reward.count, grade);
                }
                else
                {
                    Manager.UI.MakeSubItem<UI_Package_Item>(rewardParent).Set(DbCurrency.Get(reward.currencyType).GetResource(reward.id),
                        reward.count);
                }
            }
           
            Util.FindChild(gameObject, "B_Buy").BindEvent(BuyCondition, _ => Buy(), UIEffectType.Bounce);
            SetCount();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            CheckValidate(now);
        }

        private bool BuyCondition()
        {
            return CurrencyController.I.CanBuy(_item);
        }

        private void Buy()
        {
            IAPManager.I.Buy(_item, () =>
            {
                SetCount();
                if (_afterBuy != null) _afterBuy();
            });
        }

        private void CheckValidate(DateTime now)
        {
            gameObject.SetActive(true);
            if (_item.GetDuration() > 0)
            {
                Manager.DayDiff.Add(this);
                if (now < _item.GetStartTime())
                {
                    gameObject.SetActive(false);
                    return;
                }

                if (now > _item.GetStartTime().AddDays(_item.GetDuration()))
                {
                    gameObject.SetActive(false);
                    Manager.DayDiff.Remove(this);
                    return;
                }
            }
            else
            {
                Manager.DayDiff.Remove(this);
            }
            SetTime();
        }
        
        private void SetTime()
        {
            Timing.KillCoroutines(_timeRoutine);
            
            var haveTime = _item.GetDuration() > 0;
            Get<GameObject>((int) GameObjects.IMG_Timer).SetActive(haveTime);
            Get<TextMeshProUGUI>((int) Texts.T_RemoveTime).text = !haveTime
                ? string.Empty
                : StringMaker.GetDayTimeString(_item.GetStartTime().AddDays(_item.GetDuration())
                    .Subtract(DateTime.UtcNow.AddHours(9)));

            if (haveTime)
            {
                var now = DateTime.UtcNow.AddHours(9);
                var timeDiff = _item.GetStartTime().AddDays(_item.GetDuration()).Subtract(now);
                if (timeDiff.TotalSeconds <= 0)
                {
                    Manager.DayDiff.Remove(this);
                    Manager.Resource.Destroy(gameObject);
                    return;
                }
                else if (timeDiff.Days > 0)
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
        }

        private IEnumerator<float> _TimeRoutine(int seconds)
        {
            yield return Timing.WaitForSeconds(seconds);
            SetTime();
        }

        private void SetCount()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = StringMaker.GetBuyLimitText(_item);

            var canBuy = BuyCondition();
            Get<GameObject>((int)GameObjects.IMG_SoldOutBG).SetActive(!canBuy);
            Get<GameObject>((int)GameObjects.IMG_NewTag).SetActive(_item.GetIsNew() && canBuy);
            Get<GameObject>((int)GameObjects.IMG_BestTag).SetActive(_item.GetIsBest());
            Get<GameObject>((int)GameObjects.IMG_Value).SetActive(_item.GetValue() > 0 && canBuy);
            Get<GameObject>((int)GameObjects.IMG_Limited).SetActive(_item.GetIsLimited() && canBuy);
            
            if (!canBuy) Timing.CallDelayed(0.1f, () => transform.SetAsLastSibling());
        }

        private void OnEnable()
        {
            if (_isInit) 
            {
                SetTime();
                SetCount();
                Get<TextMeshProUGUI>((int)Texts.T_Cost).text = _item.GetDisplayPrice();
            }
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = LocalString.Get(_item.GetNameId());
            if (_item.GetValue() > 0) Util.FindChild<TextMeshProUGUI>(gameObject, "T_Value", true).text = string.Format(LocalString.Get(210307), _item.GetValue());
            Get<TextMeshProUGUI>((int)Texts.T_Count).text = StringMaker.GetBuyLimitText(_item);

        }

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff > 0)
            {
                CheckValidate(now);
            }
        }
    }
}