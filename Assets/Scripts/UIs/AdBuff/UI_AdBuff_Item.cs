using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbShop;

using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Data.DbUser.Currency;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace UIs.AdBuff
{
    public class UI_AdBuff_Item: UI_Base, ILanguageSet
    {
        private DbAdBuff _buffMeta;
        private DbUserAdBuff _buff;
        
        private EventsManager _adBuffUsingHandler;
        private CoroutineHandle _timeRoutine;

        enum GameObjects
        {
            IMG_Ad,
            IMG_TimeLeft
        }
        
        enum Texts
        {
            T_UseCount,
            T_WatchAd
        }

        enum Images
        {
            B_WatchAd
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));

            return true;
        }

        public void SetInfo(DbAdBuff adBuff)
        {
            Init();
            
            _buffMeta = adBuff;
            _buff = CurrencyController.I.GetAdBuff(adBuff.Id);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = LocalString.Get(adBuff.NameId);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Buff", true).text = string.Format(LocalString.Get(adBuff.DescriptionId), adBuff.Id == AdBuffType.GoldBuff ? adBuff.Buff * 0.01f :adBuff.Buff);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Time", true).text = (adBuff.Duration/60) + "m";
            Util.FindChild<Image>(gameObject, "IMG_Buff", true).sprite = Manager.Resource.Load<Sprite>(adBuff.Resource);
            Util.FindChild<Image>(gameObject, "IMG_BuffReward", true).sprite = Manager.Resource.Load<Sprite>(adBuff.RewardResource);

            Get<Image>((int)Images.B_WatchAd).gameObject.BindEvent(UseCondition, _=>WatchAd(), UIEffectType.Bounce, false);
            
            _adBuffUsingHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenUsingChanged,
                updatedField = new[] {_buff.IsUsing}
            });
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            
            WhenUsingChanged();
        }

        private bool UseCondition()
        {
            return !_buff.IsUsing.Value && _buff.UseCount < _buffMeta.LimitPerDay;
        }

        private void WatchAd()
        { 
            Manager.Ad.ShowAd(eAdType.Buff, BuffWithDelay);
        }

        private void BuffWithDelay()
        {
            QuestController.I.DoQuests(QuestType.CheckBuffAdWatch);
            Timing.CallDelayed(0.1f, UseBuff);
        }

        private void UseBuff()
        {
            PlayFabManager.Store.DoWithTime(time =>
            {
                _buff.Use(time);
            });
        }

        private void WhenUsingChanged()
        {
            var canUse = UseCondition();
            Get<TextMeshProUGUI>((int)Texts.T_UseCount).text = CurrencyController.I.Have(CurrencyType.AdSkip) ? string.Empty : string.Format(LocalString.Get(210132), _buffMeta.LimitPerDay - _buff.UseCount, _buffMeta.LimitPerDay);
            Get<Image>((int) Images.B_WatchAd).material = Define.GetUIMaterial(!canUse);
            Get<GameObject>((int) GameObjects.IMG_Ad).SetActive(canUse);
            Get<GameObject>((int) GameObjects.IMG_TimeLeft).SetActive(!canUse);

            if (!canUse)
            {
                if (_buff.IsUsing.Value)
                {
                    if (CurrencyController.I.Have(CurrencyType.AdSkip))
                    {
                        Get<TextMeshProUGUI>((int) Texts.T_WatchAd).text = LocalString.Get(210325);
                    }
                    else
                    {
                        Timing.KillCoroutines(_timeRoutine);
                        _timeRoutine = Timing.RunCoroutine(_AdBuffTimeRoutine().CancelWith(gameObject));
                    }
                }
                else Get<TextMeshProUGUI>((int) Texts.T_WatchAd).text = LocalString.Get(210131);
            }
            else
            {
                Get<TextMeshProUGUI>((int) Texts.T_WatchAd).text = LocalString.Get(210130);
            }
        }
        
        private IEnumerator<float> _AdBuffTimeRoutine()
        {
            var time = _buff.LeftTime;
            while (time > 0)
            {
                Get<TextMeshProUGUI>((int) Texts.T_WatchAd).text = time/60 + ":" + (time%60).ToString("D2");
                yield return Timing.WaitForSeconds(1);
                time--;
            }
        }
        private void OnDisable()
        {
            _adBuffUsingHandler.Dispose();
        }

        private void OnEnable()
        {
            _adBuffUsingHandler?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name", true).text = LocalString.Get(_buffMeta.NameId);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Buff", true).text = string.Format(LocalString.Get(_buffMeta.DescriptionId), _buffMeta.Id == AdBuffType.GoldBuff ? _buffMeta.Buff * 0.01f : _buffMeta.Buff);
        }
    }
}