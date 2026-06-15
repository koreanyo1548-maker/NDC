using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbShop;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_ShopCostume_Item: UI_Base, ILanguageSet, IDayDiffChecker
    {
        private IDbShop _item;

        private CoroutineHandle _timeRoutine;

        enum GameObjects
        {
            IMG_SoldOutBG,
            IMG_CostumeInfo,
            IMG_Limited,
            IMG_NewTag,
            IMG_BestTag,
            IMG_Timer
        }

        enum Texts
        {
            T_RemoveTime,
            T_Cost
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            return true;
        }

        public void SetInfo(DateTime now, IDbShop item)
        {
            if (!_isInit) Init();

            _item = item;

            Util.FindChild<Image>(gameObject, "IMG_Bg").sprite = Manager.Resource.Load<Sprite>(item.GetBackgroundResource());
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = LocalString.Get(item.GetNameId());
            Get<TextMeshProUGUI>((int)Texts.T_Cost).text = item.GetDisplayPrice();            
            
            Util.FindChild(gameObject, "B_Info").BindEvent(Functions.TrueCondition, _ => OpenInfo());

            var rewardParent = Util.FindChild<Transform>(gameObject, "G_PackageRewards");
            var rewards = item.GetRewards();
            var costumeIdx = 0;
            for (var idx = 0; idx < rewards.Count; ++idx)
            {
                var reward = rewards[idx];
                if (reward.currencyType == CurrencyType.Costume)
                {
                    var costume = DbCostume.Get(reward.id);
                    var itemUI = Get<GameObject>((int) GameObjects.IMG_CostumeInfo).transform.Find("UI_PackageEquip_Item" + costumeIdx++);
                    itemUI.gameObject.GetOrAddComponent<UI_PackageEquip_Item>()
                        .Set(DbCurrency.Get(reward.currencyType).GetResource(reward.id), reward.count, costume.Grade);
                    
                    itemUI.Find("T_Info").GetComponent<TextMeshProUGUI>().text = 
                        $"<color=#ffd25f><size=28>{LocalString.Get(costume.NameId)}</size></color><br>{StringMaker.GetCostumeOptionString(costume)}";
                        
                    Manager.UI.MakeSubItem<UI_PackageEquip_Item>(rewardParent)
                        .Set(DbCurrency.Get(reward.currencyType).GetResource(reward.id), reward.count, costume.Grade);
                        
                    // if (costume.Position == CostumePositionType.Body) 
                    //     Util.FindChild<Image>(gameObject, "IMG_Costume_Body").sprite = costume.GetResource();
                    // if (costume.Position == CostumePositionType.Weapon) 
                    //     Util.FindChild<Image>(gameObject, "IMG_Costume_Weapon").sprite = costume.GetResource();
                }
                else
                {
                    Manager.UI.MakeSubItem<UI_Package_Item>(rewardParent)
                        .Set(DbCurrency.Get(reward.currencyType).GetResource(reward.id), reward.count);
                }
            }

            Get<GameObject>((int) GameObjects.IMG_CostumeInfo).SetActive(false);
            Util.FindChild(gameObject, "B_Buy").BindEvent(BuyCondition, _ => Buy(), UIEffectType.Bounce);
            SetCount();
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            CheckValidate(now);
            
        }

        private void OpenInfo()
        {
            var info = Get<GameObject>((int) GameObjects.IMG_CostumeInfo);
            info.SetActive(!info.activeSelf);
        }

        private bool BuyCondition()
        {
            return CurrencyController.I.CanBuy(_item);
        }

        private void Buy()
        {
            IAPManager.I.Buy(_item, SetCount);
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
            var canBuy = BuyCondition();
            Get<GameObject>((int)GameObjects.IMG_SoldOutBG).SetActive(!canBuy);
            Get<GameObject>((int)GameObjects.IMG_Limited).SetActive(_item.GetIsLimited());
            Get<GameObject>((int)GameObjects.IMG_NewTag).SetActive(_item.GetIsNew());
            Get<GameObject>((int)GameObjects.IMG_BestTag).SetActive(_item.GetIsBest());
            
            if (!canBuy) Timing.CallDelayed(0.1f, () => transform.SetAsLastSibling());
        }

        private void OnEnable()
        {
            if (_isInit)
            {
                SetTime();
                Get<TextMeshProUGUI>((int)Texts.T_Cost).text = _item.GetDisplayPrice();            
            }
        }
        
        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = LocalString.Get(_item.GetNameId());
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