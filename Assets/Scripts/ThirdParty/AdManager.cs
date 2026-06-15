using System;
using System.Collections.Generic;
using System.Configuration;
//using AppsFlyerSDK;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data;
using GoogleMobileAds.Api;
using Managers;
using UIs.Toast;
using UnityEngine;

namespace ThirdParty
{
    public class AdManager : MonoBehaviour
    {
        public Dictionary<eAdType, RewardedAd> dicRewardedAd = new Dictionary<eAdType, RewardedAd>();
        //private RewardedAd _rewardedAd;

        //private Action _afterAd;

        private void Start()
        {
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(initStatus =>
            {
                LoadRewardedAd(eAdType.Summon);
                LoadRewardedAd(eAdType.Dungeon);
                LoadRewardedAd(eAdType.Buff);
                LoadRewardedAd(eAdType.OfflineReward);
                LoadRewardedAd(eAdType.Bookshelf);
                LoadRewardedAd(eAdType.Shop);
            });
        }

        private void LoadRewardedAd(eAdType e)
        {
            // Clean up the old ad before loading a new one.
            if (dicRewardedAd.ContainsKey(e))
            {
                if (dicRewardedAd[e] != null)
                {
                    dicRewardedAd[e].Destroy();
                    dicRewardedAd.Remove(e);
                }
            }
            Debug.Log($"Loading the rewarded ad. {e}");

            // create our request used to load the ad.
            //var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(GetAdId(e), new AdRequest(),
                (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"Rewarded ad failed to load an ad with error : {error}");
                        return;
                    }
                    //Debug.Log($"광고 로딩 완료 {e}");
                    dicRewardedAd[e] = ad;
                    //_rewardedAd = ad;
                    RegisterReloadHandler(e);
                });
        }
        private void RegisterReloadHandler(eAdType e)// RewardedAd ad)
        {
            if (dicRewardedAd.ContainsKey(e))
            {
                // Raised when the ad closed full screen content.
                dicRewardedAd[e].OnAdFullScreenContentClosed += () =>
                {
                    Manager.Sound.MuteSound(false);
                    // Reload the ad so that we can show another as soon as possible.
                    LoadRewardedAd(e);
                };
                // Raised when the ad failed to open full screen content.
                dicRewardedAd[e].OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError("Rewarded ad failed to open full screen content " +
                                   "with error : " + error);

                    // Reload the ad so that we can show another as soon as possible.
                    LoadRewardedAd(e);
                };
            }
        }
        /// <summary>
        /// 광고 보여주고 보상주기
        /// </summary>
        public void ShowAd(eAdType e, Action afterAd)
        {
            if (CurrencyController.I.Have(CurrencyType.AdSkip))
            {
                afterAd();
                QuestController.I.DoQuests(QuestType.AdWatch);
                return;
            }

            if (dicRewardedAd.ContainsKey(e))
            {
                var ad = dicRewardedAd[e];
                if (ad != null && ad.CanShowAd())
                {
                    Manager.Sound.MuteSound(true);
                    ad.Show((Reward reward) =>
                    {
                        afterAd();
#if APPSFLYER_ENBALE
                        AppsFlyer.sendEvent("rv_finish", new());
#endif
                        QuestController.I.DoQuests(QuestType.AdWatch);
                    });
                }
                else
                {
                    Debug.LogError("CanShowAd");
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200058);
                    LoadRewardedAd(e);
                }
            }
        }
        string GetAdId(eAdType e)
        {
            // TODO: 계정 확정 후 실제 Ad Unit ID로 교체
            // Android test rewarded ad unit: ca-app-pub-3940256099942544/5224354917
            // iOS test rewarded ad unit:     ca-app-pub-3940256099942544/1712485313
#if UNITY_ANDROID
            return "ca-app-pub-3940256099942544/5224354917";
#else
            return "ca-app-pub-3940256099942544/1712485313";
#endif
        }
    }
}