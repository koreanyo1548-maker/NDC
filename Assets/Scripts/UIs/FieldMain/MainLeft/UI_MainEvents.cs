using System;
using System.Collections.Generic;
using System.Linq;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbCommon;
using Data.DbEvent;
using Data.DbShop;
using Exceptions;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Lock;
using UIs.NewbieQuest;
using UIs.Pass;
using UIs.Shop.AdShop;
using UIs.Shop.EventPackage;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace UIs.FieldMain.MainLeft
{
    public class UI_MainEvents: UI_Base
    {
        private CoroutineHandle _soulPassRoutine;
        private CoroutineHandle _seasonPassRoutine;
        private CoroutineHandle _oncePackageRoutine;
        enum GameObjects
        {
            IMG_SeasonPassBadge,
            IMG_PassBadge,
            SeasonPass
        }

        enum Texts
        {
            T_SeasonPassTime
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));

            Util.FindChild(gameObject, "B_Pass", true).BindEvent(Functions.TrueCondition, OnPassButtonClicked, UIEffectType.Bounce);
            
            var newbieQuest = Util.FindChild(gameObject, "B_NewbieQuest", true);
            var isNewbieQuestAllClear = NewbieQuestController.I.IsAllCleared.Value;
            
            newbieQuest.SetActive(!isNewbieQuestAllClear);

            if (!isNewbieQuestAllClear)
            { 
                newbieQuest.BindEvent(Functions.TrueCondition, OnNewbieQuestButtonClicked, UIEffectType.Bounce);
                Util.FindChild(gameObject, "IMG_NewbieQuestBadge", true).GetOrAddComponent<UI_ControllerBadge>()
                    .Set(NewbieQuestController.I.IsAnyRewardInDay.Values.ToArray(), () => NewbieQuestController.I.CanGetReward(0));
                NewbieQuestController.I.IsAllCleared.ValueChanged += (_, _) =>
                {
                    newbieQuest.SetActive(!NewbieQuestController.I.IsAllCleared.Value);
                };
            }
           
                       
            Get<GameObject>((int) GameObjects.IMG_PassBadge).AddComponent<RepeatingScale>(); 
            
            var levelPass = CurrencyController.I.GetPassInfo(PassType.LevelPass);
            var stagePass = CurrencyController.I.GetPassInfo(PassType.StagePass);
            for (var idx = 0; idx < 3; ++idx)
            {
                levelPass.CanGetNormal[idx].ValueChanged += WhenCanPassChanged;
                levelPass.CanGetPremium[idx].ValueChanged += WhenCanPassChanged;
                stagePass.CanGetNormal[idx].ValueChanged += WhenCanPassChanged;
                stagePass.CanGetPremium[idx].ValueChanged += WhenCanPassChanged;
            }
            var soulPass = CurrencyController.I.GetPassInfo(PassType.StagePass);
            soulPass.CanGetNormal[0].ValueChanged += WhenCanPassChanged;
            soulPass.CanGetPremium[0].ValueChanged += WhenCanPassChanged;

            WhenCanPassChanged(null, null);
            return true;
        }

        #region SoulPass

        // private void OnSoulPassSeasonChanged()
        // {
        //     Timing.KillCoroutines(_soulPassRoutine);
        //     _soulPassRoutine = Timing.RunCoroutine(_PassLeftTimeRoutine());
        // }
        //
        // private IEnumerator<float> _PassLeftTimeRoutine()
        // {
        //     var pass = DbPassShop.Get(p => p.PassType == CurrencyType.SoulPass);
        //     while (true)
        //     {
        //         Get<TextMeshProUGUI>((int) Texts.T_SoulPassTime).text = pass.GetLeftTime();
        //         yield return Timing.WaitForSeconds(pass.GetNextUpdateSeconds());
        //     }
        // }
        //
        // private void OpenSoulPass()
        // {
        //     Manager.UI.ShowPopupUI<UI_Pass>().OpenCategory(PassType.SoulPass);
        // }
        //
        //
        // private void SetSoulPassBadge()
        // {
        //     var passInfo = CurrencyController.I.GetPassInfo(PassType.SoulPass);
        //     Get<GameObject>((int)GameObjects.IMG_SoulPassBadge).SetActive(passInfo.CanGetNormal[0].Value || passInfo.CanGetPremium[0].Value);
        // }
        //
        #endregion

        #region Pass
        
        private void WhenCanPassChanged(object sender, EventArgs eventArgs)
        {
            var levelPass = CurrencyController.I.GetPassInfo(PassType.LevelPass);
            var stagePass = CurrencyController.I.GetPassInfo(PassType.StagePass);
            var canPassRewarded = levelPass.CanGetNormal[0].Value ||  levelPass.CanGetPremium[0].Value || stagePass.CanGetNormal[0].Value || stagePass.CanGetPremium[0].Value
                                  ||  levelPass.CanGetPremium[1].Value || stagePass.CanGetNormal[1].Value || stagePass.CanGetPremium[1].Value
                                  ||  levelPass.CanGetPremium[2].Value || stagePass.CanGetNormal[2].Value || stagePass.CanGetPremium[2].Value;
            Get<GameObject>((int)GameObjects.IMG_PassBadge).SetActive(canPassRewarded);
        }
        
        private void OnPassButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_Pass>();
        }
        
        #endregion
        
        #region NewbieQuest
        
        private void OnNewbieQuestButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_NewbieQuest>();
        }
        
        #endregion

        #region Season Pass

        private void OnSeasonPassChanged()
        {
            Timing.KillCoroutines(_seasonPassRoutine);

            var curSeasonPass = SeasonPassController.data.CurrentId.Value;
            var isSeasonPassExists = curSeasonPass != 0;
            Get<GameObject>((int)GameObjects.SeasonPass).SetActive(isSeasonPassExists);
            if (!isSeasonPassExists) return;
            
            _seasonPassRoutine = Timing.RunCoroutine(_SeasonPassTimeRoutine(curSeasonPass));
        }
        
        private IEnumerator<float> _SeasonPassTimeRoutine(int curSeasonPass)
        {
            var pass = DbSeasonPass.Get(curSeasonPass);
            while (true)
            {
                Get<TextMeshProUGUI>((int) Texts.T_SeasonPassTime).text = pass.GetLeftTime();
                yield return Timing.WaitForSeconds(pass.GetNextUpdateSeconds());
            }
        }
        
        private void OpenSeasonPass()
        {
            Manager.UI.ShowPopupUI<UI_Pass>().OpenCategory(PassType.SeasonPass);
        }


        private void SetSeasonPassBadge()
        {
            Get<GameObject>((int)GameObjects.IMG_SeasonPassBadge)
                .SetActive(SeasonPassController.I.CanGetPoint.Value || SeasonPassController.I.CanGetReward.Value);
        }
        
        #endregion

        
        // #region Once Package
        // public void AdjustEventPackages()
        // {
        //     var have = CurrencyController.data.EventPackageTime.Count > 0;
        //     Get<GameObject>((int)GameObjects.OncePackages).SetActive(have);
        //     if (Manager.UI.GetPopupUI<UI_OncePackages>() != null)
        //     {
        //         Manager.UI.GetPopupUI<UI_OncePackages>().AdjustPackage();
        //     }
        //
        //     Timing.KillCoroutines(_oncePackageRoutine);
        //     if (have) _oncePackageRoutine = Timing.RunCoroutine(_OncePackageTimeRoutine());
        //     // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
        // }
        //
        // private IEnumerator<float> _OncePackageTimeRoutine()
        // {
        //     var time = DateTime.MaxValue;
        //     foreach (var p in CurrencyController.data.EventPackageTime)
        //     {
        //         if (p.Value < time) time = p.Value;
        //     }
        //
        //     while (true)
        //     {
        //         Get<TextMeshProUGUI>((int) Texts.T_OncePackageTime).text =
        //             StringMaker.GetTimeString(time.Subtract(DateTime.UtcNow.AddHours(9)));
        //         yield return Timing.WaitForSeconds(1);
        //     }
        // }
        //
        // private void OpenOncePackages()
        // {
        //     Manager.UI.ShowPopupUI<UI_OncePackages>();
        // }
        //
        // #endregion

        // private void GetOneStoreGift()
        // {
        //     Get<GameObject>((int)GameObjects.OneStoreGift).SetActive(false);
        //
        //     if (CurrencyController.I.HaveCostume(7))
        //     {
        //         CurrencyController.I.UseCoupon("OneStoreCostumeEvent_0");
        //         return;
        //     }
        //     
        //     CurrencyController.I.GetReward(CurrencyType.Costume, 1, 7);
        //     CurrencyController.I.GetReward(CurrencyType.Costume, 1, 9);
        //     CurrencyController.I.UseCoupon("OneStoreCostumeEvent_0");
        //     Manager.UI.ShowSingleUI<UI_RewardToast>().SetReward(210327, new List<DbReward>
        //     {
        //         new(CurrencyType.Costume, 1, 7),
        //         new(CurrencyType.Costume, 1, 9)
        //     });
        // }
        //
        // private void OpenAds()
        // {
        //     Manager.UI.ShowPopupUI<UI_AdShop>().Set();
        // }


        // public void OnLanguageChanged(Locale locale)
        // {
        //     Get<TextMeshProUGUI>((int) Texts.T_SoulPassTime).text = DbPassShop.Get(p => p.PassType == CurrencyType.SoulPass).GetLeftTime();
        //     var curSeasonPass = SeasonPassController.data.CurrentId.Value;
        //     if (curSeasonPass != 0)
        //     {
        //         Get<TextMeshProUGUI>((int) Texts.T_SeasonPassTime).text = DbSeasonPass.Get(curSeasonPass).GetLeftTime();
        //     }
        //
        // }
    }
}