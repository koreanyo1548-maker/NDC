using System;
using System.Collections.Generic;
using System.Linq;
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
using UIs.Shop.DefaultShop;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.EventPackage
{
    public class UI_OncePackages: UI_Popup, IBackgroundChecker
    {
        enum GameObjects
        {
            IMG_OncePackage1,
            IMG_OncePackage2,
            IMG_OncePackage3,
            IMG_OncePackage4,
            B_OncePackage1,
            B_OncePackage2,
            B_OncePackage3,
            B_OncePackage4,
        }

        enum Images
        {
            B_OncePackage1,
            B_OncePackage2,
            B_OncePackage3,
            B_OncePackage4,
        }

        enum Texts
        {
            T_Cost,
            T_ResetTime1,
            T_ResetTime2,
            T_ResetTime3,
            T_ResetTime4
        }

        // private List<int> _shopIds = new() {1014, 1015, 1016};
        private List<int> _shopIds = new() {1014, 1031, 1032, 1033};
        private Sprite[] _tabSprites; // 0: not selected 1: selected
        private int _curOpened;
        
        private CoroutineHandle _resetTimeRoutine;

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            
            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            
            _tabSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

            for (var idx = (int) Images.B_OncePackage1; idx <= (int) Images.B_OncePackage4; ++idx)
            {
                var curIdx = idx;
                Get<Image>(idx).gameObject.BindEvent(() => _curOpened != curIdx, _ => OpenTab(curIdx));
            }
            
            Util.FindChild(gameObject, "B_Buy", true).BindEvent(Functions.TrueCondition, _ => Buy(), UIEffectType.Bounce);

            SetRewards(0);
            SetRewards(1);
            SetRewards(2);
            SetRewards(3);
            SetLimitCount();

            AdjustPackage();
            
            return true;

            void SetRewards(int shopIdIdx)
            {
                var rewards = DbInAppShop.Get(_shopIds[shopIdIdx]).Reward;
                var parent = Get<GameObject>((int) GameObjects.IMG_OncePackage1 + shopIdIdx);
                var rewardParent = parent.transform.Find("G_PackageRewards");
                for (var idx = 0; idx < rewards.Count; ++idx)
                {
                    rewardParent.GetChild(idx).gameObject.GetOrAddComponent<UI_Package_Item>()
                        .Set(DbCurrency.Get(rewards[idx].currencyType).GetResource(), rewards[idx].count);
                }

            }
        }

        private void SetLimitCount()
        {
            for (var idx = 0; idx < 4; ++idx)
            {
                var item = DbInAppShop.Get(_shopIds[idx]);
                var parent = Get<GameObject>((int) GameObjects.IMG_OncePackage1 + idx);
                Util.FindChild<TextMeshProUGUI>(parent.gameObject, "T_Limit", true).text = StringMaker.GetBuyLimitText(item);
            }
        }

        public void AdjustPackage(int not = -1)
        {
            var have = CurrencyController.data.EventPackageTime.Keys.ToList();
            var existing = -1;
            for (var idx = 0; idx < _shopIds.Count; ++idx)
            {
                var exist = have.Contains(_shopIds[idx]);
                if (exist && existing == -1 && not != idx) existing = idx;
                Get<GameObject>((int)GameObjects.B_OncePackage1 + idx).SetActive(exist);
            }

            if (existing != -1)
            {
                OpenTab(existing);
            }
            else
            {
                ClosePopupUI();
            }
        }
        
        private void OpenTab(int idx)
        {
            _curOpened = idx;
            for (var jdx = 0; jdx < 4; ++jdx)
            {
                Get<Image>((int)Images.B_OncePackage1 + jdx).sprite = _tabSprites[_curOpened == jdx ? 1 :0];
                Get<GameObject>((int)GameObjects.IMG_OncePackage1 + jdx).SetActive(_curOpened == jdx);
            }
            
            Get<TextMeshProUGUI>((int)Texts.T_Cost).text = DbInAppShop.Get(_shopIds[idx]).GetDisplayPrice();

            Timing.KillCoroutines(_resetTimeRoutine);
            _resetTimeRoutine = Timing.RunCoroutine(_ResetTimeRoutine(CurrencyController.data.EventPackageTime[_shopIds[_curOpened]]).CancelWith(gameObject));
        }

        IEnumerator<float> _ResetTimeRoutine(DateTime reset)
        {
            var aSecond = new TimeSpan(0, 0, 1);
            var resetTime = reset - DateTime.UtcNow.AddHours(9);
            while (true)
            {
                if (resetTime.TotalSeconds <= 0)
                {
                    var cur = _curOpened;
                    AdjustPackage(_curOpened);
                    Get<GameObject>((int)GameObjects.B_OncePackage1 + cur).SetActive(false);
                    break;
                }
                Get<TextMeshProUGUI>((int) Texts.T_ResetTime1 + _curOpened).text = resetTime.ToString(@"hh\:mm\:ss");
                yield return Timing.WaitForSeconds(1);
                resetTime = resetTime.Subtract(aSecond);
            }
        }
        
        private void Buy()
        {
            IAPManager.I.Buy(DbInAppShop.Get(_shopIds[_curOpened]), () =>
            {
                SetLimitCount();
            });
        }

        private void OnEnable()
        {
            if (_isInit)
            {
                AdjustPackage();
                SetLimitCount();
            }
            Manager.Background.Add(this);
        }

        private void OnDisable()
        {
            Manager.Background.Remove(this);
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }

        public void WhenBackFromBackground(TimeSpan time, DateTime now)
        {
            Timing.KillCoroutines(_resetTimeRoutine);
            if (CurrencyController.data.EventPackageTime.ContainsKey(_shopIds[_curOpened]))
            {
                _resetTimeRoutine = Timing.RunCoroutine(_ResetTimeRoutine(CurrencyController.data.EventPackageTime[_shopIds[_curOpened]]).CancelWith(gameObject));
            }
            else
            {
                AdjustPackage();
            }
        }
    }
}