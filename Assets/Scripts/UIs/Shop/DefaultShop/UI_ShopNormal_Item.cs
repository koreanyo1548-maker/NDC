using System;
using System.Collections.Generic;
using Controller.Currency;
using Data;
using Data.DbCommon;
using Data.DbDefinition;
using Data.DbShop;
using Managers;
using MEC;
using TMPro;
using UIs.Dungeon.TrainingGround;
using UIs.Toast;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_ShopNormal_Item: UI_Shop_Item, IDayDiffChecker, ILanguageSet
    {
        private bool _isCostume;
        private DbReward _costume;

        private GameObject _timeObj;
        private TextMeshProUGUI _removeTime;
        private CoroutineHandle _timeRoutine;
        
        
        public void SetInfo(DateTime now, IDbShop item)
        {
            base.SetInfo(item);
            
            var rewards = item.GetRewards();
            _isCostume = rewards[0].currencyType == CurrencyType.Costume;
            Util.FindChild(gameObject, "IMG_Item", true).SetActive(!_isCostume);
            Util.FindChild(gameObject, "UI_Normal_Item", true).SetActive(_isCostume);
            Util.FindChild(gameObject, "IMG_Info", true).SetActive(_isCostume);
            Util.FindChild<Image>(gameObject, "IMG_Cost", true).sprite = 
                Manager.Resource.Load<Sprite>(DbCurrency.Get(item.GetPriceType()).Resource);

            Util.FindChild(gameObject, "IMG_Increase", true).SetActive(_item.GetIncreasePrice() > 0);
            Get<TextMeshProUGUI>((int)Texts.T_Count).gameObject.SetActive(!_isCostume);
            _timeObj = Util.FindChild(gameObject, "Time", true);
            _removeTime = Util.FindChild<TextMeshProUGUI>(gameObject, "T_Time", true);
            if (_isCostume)
            {
                _costume = rewards[0];
                Util.FindChild(gameObject, "UI_Normal_Item", true).GetOrAddComponent<UI_Normal_Item>().Set(rewards[0]);
                Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text =
                    LocalString.Get(DbCostume.Get(rewards[0].id).NameId);
            }
            
            SetCostForDynamicPrice();
            CheckValidate(now);
            
            if (_item.GetIncreasePrice() > 0) LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        protected override void WhenCurrencyChanged()
        {
            SetCostForDynamicPrice();
            base.WhenCurrencyChanged();
        }
        
        private void SetCostForDynamicPrice()
        {
            if (_item.GetIncreasePrice() > 0)
            {
                Get<TextMeshProUGUI>((int)Texts.T_Cost).text = 
                    (_item.GetPrice() + CurrencyController.I.GetBuyCount(_item.GetId()) * _item.GetIncreasePrice()).ToString("N0");
            }
        }
        protected override void Buy()
        {
            if (_isCostume)
            {
                Manager.UI.ShowPopupUI<UI_BuyPopup>().Set(_costume, RealBuy, _item.GetPrice());
            }
            else
            {
                RealBuy();
            }
        }

        private void RealBuy()
        {
            base.Buy();
            if (_item.GetIncreasePrice() > 0)
            {
                Timing.CallDelayed(1f, () => Manager.UI.ShowSingleUI<UI_Toast>().SetText(string.Format(LocalString.Get(210380), _item.GetIncreasePrice().ToString("N0"))));
            }
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
            var isIncreasePrice = _item.GetIncreasePrice() > 0;
            _timeObj.SetActive(haveTime || isIncreasePrice);
            _removeTime.text = haveTime
                ? StringMaker.GetDayTimeString(_item.GetStartTime().AddDays(_item.GetDuration())
                    .Subtract(DateTime.UtcNow.AddHours(9)))
                : isIncreasePrice ? StringMaker.GetResetTime(210381)
                : string.Empty;

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

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_isInit) 
            {
                SetTime();
            }
        }
        
        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            if (dayDiff > 0)
            {
                CheckValidate(now);
            }
        }

        public void OnLanguageChanged(Locale locale)
        {
            if (_item.GetIncreasePrice() > 0)
            {
                _removeTime.text = StringMaker.GetResetTime(210381);
            }
        }
    }
}