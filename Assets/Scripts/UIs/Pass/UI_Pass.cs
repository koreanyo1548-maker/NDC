using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Utils;
using Data;
using Data.DbDefinition;
using Data.DbEvent;
using Data.DbShop;
using Data.Utils;
using DG.Tweening;
using dynamicscroll;
using Exceptions;
using Managers;
using MEC;
using ThirdParty;
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

namespace UIs.Pass
{
    public class UI_Pass: UI_Popup, ILanguageSet
    {
        private Sprite[] _tabSprites; // 0: not selected 1: selected
        private Sprite[] _categoryTabSprites; // 0: not selected 1: selected
        private Images? _curOpenedCategory = null;
        private Images? _curOpened = null;
        
        private EventsManager _seasonPassCheckEventManager;
        private EventsManager _basicProgressEventsManager;
        private EventsManager _seasonProgressEventsManager;
        private EventsManager _levelPassBadgeEventsManager;
        private EventsManager _stagePassBadgeEventsManager;
        private EventsManager _soulPassBadgeEventsManager;
        private EventsManager _seasonPassBadgeEventManager;
        private EventsManager _seasonPassNextEventManager;
        private EventsManager _canGetAnyBasicRewardEventsManager;
        private EventsManager _canGetAnySeasonRewardEventsManager;
        private DynamicScroll<PassItem, UI_Pass_Item> _basicPasses;
        private DynamicScroll<SeasonPassRewardItem, UI_SeasonPassReward_Item> _seasonPasses;
        
        private PassType _passType = PassType.StagePass;
        private CurrencyType _specificType = CurrencyType.StagePass1;
        private List<List<PassItem>> _levelPassData = new();
        private List<List<PassItem>> _stagePassData = new();
        private List<PassItem> _soulPassData = new();

        private string[] _passNum = {"1", "2", "3", "4", "5"};

        enum Texts
        {
            T_Title,
            T_Time,
            T_BasicGauge,
            T_SeasonGauge,
            T_Buy,
            T_BasicCurrentLevel,
            T_BasicNextLevel,
            T_SeasonCurrentLevel,
            T_PassIndex,
            T_Pass1,
            T_Pass2,
            T_Pass3,
            T_Pass4,
            T_Pass5
        }

        enum GameObjects
        {
            IMG_BadgePass1,
            IMG_BadgePass2,
            IMG_BadgePass3,
            IMG_BadgePass4,
            IMG_BadgePass5,
            IMG_BadgeSeasonPassReward,
            IMG_BadgeSeasonPassQuest,
            IMG_BadgeLevelPass,
            IMG_BadgeStagePass,
            IMG_BadgeSoulPass,
            IMG_BadgeSeasonPass,
            B_Buy,
            IMG_Purchased,
            IMG_LockBG,
            IMG_LockIcon,
            IMG_TimeBG,
            IMG_Top,
            IMG_TopPassBg,
            BasicPassView,
            SeasonPassRewardView,
            SeasonPassQuestView,
            BasicPassTabs,
            SeasonPassTabs,
            IMG_BasicPassTopBG,
            IMG_SeasonPassTopBG,
            B_SeasonGetAll,
            IMG_SeasonPassFinalRewardGrade
        }
        
        enum Images
        {
            B_LevelPass,
            B_StagePass,
            B_SoulPass,
            B_SeasonPass,
            B_Pass1,
            B_Pass2,
            B_Pass3,
            B_Pass4,
            B_Pass5,
            B_SeasonPassReward,
            B_SeasonPassQuest,
            IMG_PassIcon,
            IMG_BasicGauge,
            IMG_SeasonGauge,
            B_Buy,
            B_BasicGetAll,
            B_SeasonGetAll,
            IMG_Top,
            IMG_TopPassBg,
            IMG_Pass1,
            IMG_Pass2,
            IMG_Pass3,
            IMG_Pass4,
            IMG_Pass5
        }

        enum DynamicScrollRects
        {
            BasicPassView,
            SeasonPassRewardView
        }

        enum Buttons
        {
            B_Buy
        }

        private void Start()
        {
            Init();
        }


        public override bool Init()
        {
            if (!base.Init()) return false;

            Timing.CallDelayed(Timing.DeltaTime, () =>
            {
                Bind<TextMeshProUGUI>(typeof(Texts));
                Bind<Image>(typeof(Images));
                Bind<GameObject>(typeof(GameObjects));
                Bind<DynamicScrollRect>(typeof(DynamicScrollRects));
                Bind<Button>(typeof(Buttons));
                
                
                #region Set View
                
                SetStagePassData();
                SetLevelPassData();
                SetSoulPassData();
                
                _basicPasses = new DynamicScroll<PassItem, UI_Pass_Item>();
                _basicPasses.spacing = 0;
                _basicPasses.Initiate(Get<DynamicScrollRect>((int)DynamicScrollRects.BasicPassView), _stagePassData[0],
                    -1,"Prefabs/UI/SubItem/UI_Pass_Item");
                
                var seasonPassRewardData = new List<SeasonPassRewardItem>();
                DbSeasonPassReward.ForEach(s =>
                {
                    seasonPassRewardData.Add(new SeasonPassRewardItem { rewardId = s.Id });
                });
                _seasonPasses = new DynamicScroll<SeasonPassRewardItem, UI_SeasonPassReward_Item>();
                _seasonPasses.spacing = 0;
                _seasonPasses.Initiate(Get<DynamicScrollRect>((int)DynamicScrollRects.SeasonPassRewardView), seasonPassRewardData,
                    -1, "Prefabs/UI/SubItem/UI_SeasonPassReward_Item");

                // var finalReward = DbCostume.Get(DbSeasonPassReward.Get(DbSeasonPassReward.Count).RewardId);
                // Util.FindChild<Image>(gameObject, "IMG_SeasonPassFinalReward", true).sprite = finalReward.GetResource();
                // Get<GameObject>((int)GameObjects.IMG_SeasonPassFinalRewardGrade).GetComponent<Image>().sprite = Manager.Resource.Load<Sprite>(finalReward.Grade.ToString());
                // Util.FindChild<TextMeshProUGUI>(gameObject, "T_SeasonPassFinalRewardGrade", true).text
                //     = LocalString.Get(DbGrade.Get(finalReward.Grade).NameId);
                // Get<GameObject>((int)GameObjects.IMG_SeasonPassFinalRewardGrade).BindEvent(Functions.TrueCondition, _ => OpenSeasonPassFinalRewardInfo(), UIEffectType.Bounce);

                if (SeasonPassController.data.CurrentId.Value != 0)
                {
                    var questParent = Get<GameObject>((int)GameObjects.SeasonPassQuestView).transform.Find("Viewport").Find("Content");
                    DbSeasonPassQuest.ForEach(q =>
                    {
                        var item = Manager.UI.MakeSubItem<UI_SeasonPassQuest_Item>(questParent);
                        item.SetInfo(q);
                    });

                }
                
                #endregion
                
                
                #region TabClicked
                
                _tabSprites = new[]
                    {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

                _categoryTabSprites = new[]
                    {Manager.Resource.Load<Sprite>(Define.EmptySprite), Manager.Resource.Load<Sprite>("UI_MainBG_160x160 1")};
                
                for (var tab = (int)Images.B_LevelPass; tab <= (int)Images.B_SeasonPass; ++tab)
                {
                    var curTab = tab;
                    Get<Image>(tab).gameObject.BindEvent(Functions.TrueCondition, _ => OnCategoryTabClicked((Images)curTab), UIEffectType.Bounce);
                }
                
                for (var tab = (int)Images.B_Pass1; tab <= (int)Images.B_Pass5; ++tab)
                {
                    var curTab = tab;
                    Get<Image>(tab).gameObject.BindEvent(Functions.TrueCondition, _ => OnBasicPassTabClicked((Images)curTab), UIEffectType.Bounce);
                }
                
                for (var tab = (int)Images.B_SeasonPassReward; tab <= (int)Images.B_SeasonPassQuest; ++tab)
                {
                    var curTab = tab;
                    Get<Image>(tab).gameObject.BindEvent(Functions.TrueCondition, _ => OnSeasonPassTabClicked((Images)curTab), UIEffectType.Bounce);
                }

                for (var badge = (int) GameObjects.IMG_BadgePass1; badge <= (int) GameObjects.IMG_BadgeSeasonPassQuest; ++badge)
                {
                    Get<GameObject>(badge).AddComponent<RepeatingScale>();
                }
                
                #endregion

                
                Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
                Get<Image>((int)Images.B_BasicGetAll).gameObject.BindEvent(CanGetAnyBasicReward, _ => GetAllBasicRewards(), UIEffectType.Bounce, false);
                Get<Image>((int)Images.B_SeasonGetAll).gameObject.BindEvent(CanGetAnySeasonReward, _ => GetAllSeasonRewards(), UIEffectType.Bounce, false);
                
                Get<GameObject>((int)GameObjects.B_Buy).BindEvent(Functions.TrueCondition, _ => BuyWhenCannotBuy(), UIEffectType.Bounce);
               
               
                #region Handler
                
                _seasonPassCheckEventManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenSeasonPassChanged,
                    updatedField = new[] {SeasonPassController.data.CurrentId}
                });
                _basicProgressEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = SetBasicPassProgress,
                    updatedField = new[] {CurrencyController.data.Pass}
                });

                _seasonProgressEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = SetSeasonPassProgress,
                    updatedField = new[] {SeasonPassController.data.Point}
                });

                var levelPass = CurrencyController.I.GetPassInfo(PassType.LevelPass);
                _levelPassBadgeEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = () => SetBadge(PassType.LevelPass),
                    updatedController = levelPass.CanGetNormal.Concat(levelPass.CanGetPremium).ToArray()
                });
                var stagePass = CurrencyController.I.GetPassInfo(PassType.StagePass);
                _stagePassBadgeEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = () => SetBadge(PassType.StagePass),
                    updatedController = stagePass.CanGetNormal.Concat(stagePass.CanGetPremium).ToArray()
                });
                var soulPass = CurrencyController.I.GetPassInfo(PassType.SoulPass);
                _soulPassBadgeEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = () => SetBadge(PassType.SoulPass),
                    updatedController = soulPass.CanGetNormal.Concat(soulPass.CanGetPremium).ToArray()
                });
                _seasonPassBadgeEventManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = () => SetBadge(PassType.SeasonPass),
                    updatedController = new[] {SeasonPassController.I.CanGetPoint, SeasonPassController.I.CanGetReward}
                });
                _canGetAnyBasicRewardEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenCanGetAnyBasicRewardChanged,
                    updatedController = new[] {stagePass.CanGetNormal[0], stagePass.CanGetPremium[0]}
                });
                _canGetAnySeasonRewardEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenCanGetAnySeasonRewardChanged,
                    updatedController = new[] {SeasonPassController.I.CanGetReward, SeasonPassController.I.CanGetPoint}
                });
                _seasonPassNextEventManager =  new EventsManager(this, new EventsManager.Config
                {
                    handler = UpdateSeasonPass,
                    updatedController = new[] {SeasonPassController.I.NextRewarded}
                });
                
                #endregion
                
                LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

                SetSeasonPassProgress();
                OpenCategory(_initCategoryType);
                
                SetBadge(PassType.LevelPass);
                SetBadge(PassType.StagePass);
                SetBadge(PassType.SoulPass);
                SetBadge(PassType.SeasonPass);

                WhenSeasonPassChanged();
            });
           
            return true;
        }

        
        #region Buy
        
        private void BuyWhenCannotBuy()
        {
            if (!BuyCondition())
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200020);
            }
            else
            {
                Buy();
            }
        }
        
        private bool BuyCondition()
        {
            if (_passType == PassType.SeasonPass)
            {
                return !SeasonPassController.I.IsSeasonPassPurchased();
            }
            
            var passInfo = CurrencyController.I.GetPassInfo(_passType);
            if (passInfo != null)
            {
                var meta = DbSelector.GetPass(_passType, passInfo.LastFreeRewarded + 1);
                if (meta == null) meta = DbSelector.GetPass(_passType, passInfo.LastFreeRewarded);
                // for (var idx = 0; idx < Define.passToCurrency[_passType].IndexOf(_specificType); ++idx)
                // {
                //     if (!CurrencyController.I.Have(Define.passToCurrency[_passType][idx])) return false;
                // }
                return _specificType <= meta.GetSpecificPassType() && 
                       !CurrencyController.I.Have(_specificType);
            }

            return false;
        }

        private void Buy()
        {
            IAPManager.I.Buy(DbPassShop.Get(p => p.PassType == _specificType), () =>
            {
                SetIsBought();
                Get<Button>((int)Buttons.B_Buy).enabled = false;
                if (_passType == PassType.SeasonPass)
                {
                    SeasonPassController.I.WhenBuySeasonPass();
                    _seasonPasses.ForEach(item => item.SetStatus());
                }
                else
                {
                    _basicPasses.ForEach(item => item.SetStatus());
                }
            });
        }

        private void SetIsBought()
        {
            var passShop = DbPassShop.Get(p => p.PassType == _specificType);
            var isBought = false;
            var isLocked = false;
            var isSeasonPass = _passType == PassType.SeasonPass;
            
            if (isSeasonPass)
            {
                isBought = !BuyCondition();
                isLocked = !isBought;
                WhenCanGetAnySeasonRewardChanged();
            }
            else
            {
                isBought = CurrencyController.I.Have(_specificType);
                isLocked =  !BuyCondition() && !isBought;
                WhenCanGetAnyBasicRewardChanged();
            }
            
            var canBuy = BuyCondition();
            Get<Button>((int)Buttons.B_Buy).enabled = canBuy;
            Get<Image>((int) Images.B_Buy).material = Define.GetUIMaterial(!canBuy);
            
            Get<TextMeshProUGUI>((int) Texts.T_Buy).text = !isBought ? passShop.DisplayPrice : LocalString.Get(210144);//isLocked ? 210143 : 210144);
            Get<TextMeshProUGUI>((int) Texts.T_Buy).color = !isBought ? Color.white : Define.ColorFFF8AA;//(isLocked || canBuy) ? Color.white : Define.ColorFFF8AA;
            Get<GameObject>((int)GameObjects.IMG_Purchased).SetActive(isBought);
            Get<GameObject>((int)GameObjects.IMG_LockBG).SetActive(isLocked);
            Get<GameObject>((int)GameObjects.IMG_LockIcon).SetActive(isLocked);
            Get<GameObject>((int) GameObjects.B_Buy).SetActive(!isBought);// && !isLocked);

            
            var checkTime = passShop.Condition == PassCondition.CheckDate || isSeasonPass;
            Get<GameObject>((int)GameObjects.IMG_TimeBG).SetActive(checkTime);
            if (checkTime)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Time).text =
                    //isLocked ? LocalString.GetLocalizedString(210140) :
                    LocalString.Get(210136) + (isSeasonPass ? DbSeasonPass.Get(SeasonPassController.data.CurrentId.Value).GetLeftTime() : passShop.GetLeftTime());
            }
        }

        #endregion
        
        
        #region Update Status

        private bool _isPrevSeasonPass = true;
        private void WhenSeasonPassChanged()
        {
            var isSeasonPass = SeasonPassController.data.CurrentId.Value != 0;
            if (isSeasonPass == _isPrevSeasonPass) return;
            _isPrevSeasonPass = isSeasonPass;
            
            if (!isSeasonPass)
            {
                Get<DynamicScrollRect>((int) DynamicScrollRects.SeasonPassRewardView).enabled = false;
                Get<Image>((int)Images.B_SeasonPass).gameObject.SetActive(false);
                Get<GameObject>((int)GameObjects.IMG_BadgeSeasonPass).SetActive(false);
                _seasonPassBadgeEventManager?.Dispose();
                _seasonProgressEventsManager?.Dispose();
                _canGetAnySeasonRewardEventsManager?.Dispose();
                _seasonPassNextEventManager?.Dispose();
                OpenCategory(_initCategoryType == PassType.SoulPass ? PassType.SoulPass : PassType.LevelPass);
                if (_seasonPasses != null)
                {
                    // _seasonPasses.ForEach(item => item.UnuseIt());
                    // _seasonPasses = null;
                
                    var questParent = Get<GameObject>((int)GameObjects.SeasonPassQuestView).transform.Find("Viewport").Find("Content");
                    while (questParent.childCount > 0)
                    {
                        Manager.Resource.Destroy(questParent.GetChild(0).gameObject);
                    }
                }
            }
            else
            {
                Get<DynamicScrollRect>((int) DynamicScrollRects.SeasonPassRewardView).enabled = true;
                Get<Image>((int)Images.B_SeasonPass).gameObject.SetActive(true);
                _seasonPassBadgeEventManager?.Reconnect();
                _seasonProgressEventsManager?.Reconnect();
                _canGetAnySeasonRewardEventsManager?.Reconnect();
                _seasonPassNextEventManager?.Reconnect();
                
                var questParent = Get<GameObject>((int)GameObjects.SeasonPassQuestView).transform.Find("Viewport").Find("Content");
                DbSeasonPassQuest.ForEach(q =>
                {
                    var item = Manager.UI.MakeSubItem<UI_SeasonPassQuest_Item>(questParent);
                    item.SetInfo(q);
                });

                UpdateSeasonPass();
            }

        }
        
        public void UpdateBasicPass()
        {
            _basicPasses.ForEach(item => item.SetStatus());
            SetBasicPassProgress();
        }

        public void UpdateSeasonPass()
        {
            _seasonPasses.ForEach(item => item.SetStatus());
            SetSeasonPassProgress();
        }

        private void SetBasicPassProgress()
        {
            if (_passType == PassType.SeasonPass) return;
            Get<TextMeshProUGUI>((int) Texts.T_Title).text = LocalString.Get(DbCurrency.Get(_specificType).NameId);
            
            var pass = CurrencyController.I.GetPassInfo(_passType);
            var nextMeta = DbSelector.GetPass(_passType, pass.LastFreeRewarded + 1);
            var isLast = nextMeta == null;
            if (isLast) nextMeta = DbSelector.GetPass(_passType, pass.LastFreeRewarded);
            
            // 완료한 경우
            if (_specificType < nextMeta.GetSpecificPassType())
            {
                var lastId = DbSelector.GetFirstId(_passType, Define.passToCurrency[_passType][Define.passToCurrency[_passType].IndexOf(_specificType) + 1])-1;
                var prevMeta = DbSelector.GetPass(_passType, lastId);
                Set(prevMeta.GetGoal(), prevMeta.GetGoal(), lastId, lastId);
            }
            // 아직 안간 경우
            else if (_specificType > nextMeta.GetSpecificPassType())
            {
                Set(0, DbSelector.GetPass(_passType, DbSelector.GetFirstId(_passType, _specificType)).GetGoal(), 0, 1);
            }
            // 진행 중인 경우
            else
            {
                var cur = pass.LastFreeRewarded - DbSelector.GetFirstId(_passType, _specificType);
                if (cur == -1) cur = 0;
                Set(pass.Progress, nextMeta.GetGoal(), cur, isLast ? cur : cur + 1);
            }

            void Set(int curProgress, int goalProgress, int curLevel, int nextLevel)
            {
                Get<TextMeshProUGUI>((int) Texts.T_BasicGauge).text = curProgress + "/" + goalProgress;
                Get<Image>((int) Images.IMG_BasicGauge).fillAmount = 1f * curProgress / goalProgress;
                Get<TextMeshProUGUI>((int) Texts.T_BasicCurrentLevel).text = curLevel.ToString();
                Get<TextMeshProUGUI>((int) Texts.T_BasicNextLevel).text = nextLevel.ToString();
            }
        }

        private void SetSeasonPassProgress()
        {
            if (_passType != PassType.SeasonPass) return;
            var nextMeta = DbSeasonPassReward.Get(SeasonPassController.I.NextRewarded.Value);
            
            
            Set(SeasonPassController.data.Point.Value, nextMeta.NeedPoint, Math.Max(1, nextMeta.Id-1));

            void Set(long curProgress, int goalProgress, int nextLevel)
            {
                Get<TextMeshProUGUI>((int) Texts.T_SeasonGauge).text = curProgress + "/" + goalProgress;
                Get<Image>((int) Images.IMG_SeasonGauge).DOFillAmount(goalProgress == 0 ? 1 : 1f * curProgress / goalProgress, 0.5f);
                Get<TextMeshProUGUI>((int) Texts.T_SeasonCurrentLevel).text = nextLevel.ToString();
            }
        }

        private bool CanGetAnyBasicReward()
        {
            return Get<Image>((int) Images.B_BasicGetAll).material == Define.GetUIMaterial(false);
        }

        private bool CanGetAnySeasonReward()
        {
            return Get<Image>((int) Images.B_SeasonGetAll).material == Define.GetUIMaterial(false);
        }

        private void WhenCanGetAnyBasicRewardChanged()
        {
            if (_passType == PassType.SeasonPass) return;
            var passInfo = CurrencyController.I.GetPassInfo(_passType);

            var idx = Define.passToCurrency[_passType].IndexOf(_specificType);
            Get<Image>((int)Images.B_BasicGetAll).material = 
                Define.GetUIMaterial(!(passInfo.CanGetNormal[idx].Value || passInfo.CanGetPremium[idx].Value));
        }

        private void WhenCanGetAnySeasonRewardChanged()
        {
            if (_passType != PassType.SeasonPass) return;
            Get<Image>((int)Images.B_SeasonGetAll).material = 
                Define.GetUIMaterial(!SeasonPassController.I.CanGetReward.Value);
        }
        
        private void SetBadge(PassType passType)
        {
            var isActive = false;

            if (passType == PassType.SeasonPass)
            {
                isActive = SeasonPassController.I.CanGetPoint.Value || SeasonPassController.I.CanGetReward.Value;
                
                Get<GameObject>((int)GameObjects.IMG_BadgeSeasonPassReward).SetActive(SeasonPassController.I.CanGetReward.Value);
                Get<GameObject>((int)GameObjects.IMG_BadgeSeasonPassQuest).SetActive(SeasonPassController.I.CanGetPoint.Value);
            }
            else
            {
                var passInfo = CurrencyController.I.GetPassInfo(passType);
                foreach (var list in passInfo.CanGetNormal)
                {
                    if (list.Value)
                    {
                        isActive = true;
                        break;
                    }
                }
                if (!isActive)
                {
                    foreach (var list in passInfo.CanGetPremium)
                    {
                        if (list.Value)
                        {
                            isActive = true;
                            break;
                        }
                    }
                }
                
                if (passType != PassType.SoulPass && passType == ImagesToPassType(_curOpenedCategory))
                {
                    var jdx = 0;
                    for (var idx = (int)GameObjects.IMG_BadgePass1; idx <= (int)GameObjects.IMG_BadgePass5; ++idx)
                    {
                        Get<GameObject>(idx).SetActive(passInfo.CanGetNormal[jdx].Value || passInfo.CanGetPremium[jdx].Value);
                        jdx++;
                    }
                }
            }
            
            Get<GameObject>((int)PassTypeToBadge(passType)).SetActive(isActive);


            GameObjects PassTypeToBadge(PassType pass)
            {
                switch (pass)
                {
                    case PassType.LevelPass: return GameObjects.IMG_BadgeLevelPass;
                    case PassType.StagePass: return GameObjects.IMG_BadgeStagePass;
                    case PassType.SoulPass: return GameObjects.IMG_BadgeSoulPass;
                    case PassType.SeasonPass : return GameObjects.IMG_BadgeSeasonPass;
                    default: throw new Exception($"{pass} is not defined pass category");
                }
            }
        }
        
        #endregion
        
        
        #region Reward
        
        private void GetAllBasicRewards()
        { 
            CurrencyController.I.GetAllPassRewards(_passType, _specificType);
            UpdateBasicPass();
        }

        private void GetAllSeasonRewards()
        {
            SeasonPassController.I.GetAllRewards();
            UpdateSeasonPass();
        }
        
        #endregion
        
        
        #region Set Contents
        
        private int _prevSoulPassSeason;
        private void SetBasicPassType(PassType passType, CurrencyType specificType, bool needRefresh = true)
        {
            if (!needRefresh)
            {
                if (passType == PassType.LevelPass && _passType != PassType.SoulPass) return;
            }

            if (_specificType != specificType)
            {
                ChangeList();
            }
            if (passType == PassType.SoulPass &&
                _prevSoulPassSeason != CurrencyController.I.GetPassInfo(_passType).Season.Value)
            {
                UpdateBasicPass();
                _prevSoulPassSeason = CurrencyController.I.GetPassInfo(_passType).Season.Value;
            }
            
            Get<GameObject>((int) GameObjects.IMG_Top).SetActive(passType == PassType.StagePass);
            Get<GameObject>((int)GameObjects.IMG_TopPassBg).SetActive(passType != PassType.StagePass);
            if (passType == PassType.StagePass)
            {
                var id = specificType == CurrencyType.StagePass1 ? 1 : specificType == CurrencyType.StagePass2 ? 2 : 3;
                Get<Image>((int) Images.IMG_Top).sprite = Manager.Resource.Load<Sprite>("Chapter" + id);
            }
            else
            {
                Get<Image>((int) Images.IMG_TopPassBg).sprite = Manager.Resource.Load<Sprite>(
                    passType == PassType.LevelPass ? "IMG_levelPass" : "IMG_SoulPass");
            }

            var passShop = DbPassShop.Get(p => p.PassType == _specificType);
            Get<Image>((int) Images.IMG_PassIcon).sprite = Manager.Resource.Load<Sprite>(passShop.Resource);
            Get<TextMeshProUGUI>((int) Texts.T_PassIndex).text = passType == PassType.LevelPass ? _passNum[Define.passToCurrency[passType].IndexOf(_specificType)] : string.Empty;
            SetBasicPassProgress();
            SetIsBought();
            
            _basicPasses.ForceUpdateList();
            _basicPasses.MoveToIndex(CurrencyController.I.GetFreePassIndex(passType, specificType), 0.1f);

            
            void ChangeList()
            {
                _specificType = specificType;
                _passType = passType;
                var diff = Define.passToCurrency[passType].IndexOf(_specificType);
                _basicPasses.ChangeList(passType == PassType.StagePass ? _stagePassData[diff]
                    : passType == PassType.LevelPass ? _levelPassData[diff] 
                    : _soulPassData, 0);
                    
                var info = CurrencyController.I.GetPassInfo(_passType);
                ControllerField[] field = {info.CanGetNormal[diff], info.CanGetPremium[diff]};
                _canGetAnyBasicRewardEventsManager.Set(WhenCanGetAnyBasicRewardChanged, field);
                WhenCanGetAnyBasicRewardChanged();
            }
        }
        
        #endregion


        #region Selection

        private void OnBasicPassTabClicked(Images clicked, bool force = false)
        {
            if (!force && _curOpened == clicked)
            {
                return;
            }

            if (_curOpened != null)
            {
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
            }
            
            _curOpened = clicked; 
            Get<Image>((int) clicked).sprite = _tabSprites[1];
            var pass = ImagesToPassType(_curOpenedCategory);
            SetBasicPassType(pass, Define.passToCurrency[pass][clicked - Images.B_Pass1]);
        }

        private void OnSeasonPassTabClicked(Images clicked)
        {
            _specificType = CurrencyType.SeasonPass;
            _passType = PassType.SeasonPass;
            if (_curOpened != null)
            {
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
            }
            
            _curOpened = clicked; 
            Get<Image>((int) clicked).sprite = _tabSprites[1];

            var isRewardView = clicked == Images.B_SeasonPassReward;
            Get<GameObject>((int)GameObjects.SeasonPassRewardView).SetActive(isRewardView);
            Get<GameObject>((int)GameObjects.SeasonPassQuestView).SetActive(!isRewardView);
            Get<GameObject>((int)GameObjects.B_SeasonGetAll).SetActive(isRewardView);
            SetSeasonPassProgress();
            
            if (isRewardView)
            {
                _seasonPasses.MoveToIndex(SeasonPassController.I.NextRewarded.Value, 0.1f);
            }
            
            Get<TextMeshProUGUI>((int) Texts.T_Title).text = LocalString.Get(DbSeasonPass.Get(SeasonPassController.data.CurrentId.Value).NameId);

            SetIsBought();
            Get<GameObject>((int)GameObjects.IMG_TopPassBg).SetActive(true);
            Get<Image>((int) Images.IMG_TopPassBg).sprite = Manager.Resource.Load<Sprite>("IMG_AfterlifePass");
            var passShop = DbPassShop.Get(p => p.PassType == CurrencyType.SeasonPass);
            Get<Image>((int) Images.IMG_PassIcon).sprite = Manager.Resource.Load<Sprite>(passShop.Resource);
            Get<TextMeshProUGUI>((int) Texts.T_PassIndex).text = string.Empty;
        }

        private void OnCategoryTabClicked(Images clicked)
        {
            if (_curOpenedCategory == clicked)
            {
                return;
            }

            if (_curOpenedCategory != null)
            {
                Get<Image>((int) _curOpenedCategory).sprite = _categoryTabSprites[0];
            }
            
            _curOpenedCategory = clicked; 
            Get<Image>((int) clicked).sprite = _categoryTabSprites[1];

            Get<GameObject>((int) GameObjects.BasicPassTabs)
                .SetActive(clicked == Images.B_LevelPass || clicked == Images.B_StagePass);

            var isSeasonPass = clicked == Images.B_SeasonPass;
            Get<GameObject>((int)GameObjects.IMG_BasicPassTopBG).SetActive(!isSeasonPass);
            Get<GameObject>((int)GameObjects.IMG_SeasonPassTopBG).SetActive(isSeasonPass);
            Get<GameObject>((int)GameObjects.SeasonPassTabs).SetActive(isSeasonPass);

            Get<GameObject>((int)GameObjects.BasicPassView).SetActive(!isSeasonPass);
            if (!isSeasonPass)
            {
                Get<GameObject>((int)GameObjects.SeasonPassRewardView).SetActive(false);
                Get<GameObject>((int)GameObjects.SeasonPassQuestView).SetActive(false);
            }
            
            OpenTab(clicked);
            
            void OpenTab(Images tab)
            {
                switch (tab)
                {
                    case Images.B_LevelPass:
                        var targetLevelPass = DbLevelPass.Get(CurrencyController.I.GetPassInfo(PassType.LevelPass).LastFreeRewarded+1);
                        if (targetLevelPass == null) targetLevelPass = DbLevelPass.Get(CurrencyController.I.GetPassInfo(PassType.LevelPass).LastFreeRewarded);
                        var curLevelPass = targetLevelPass.GetSpecificPassType();
                        OnBasicPassTabClicked(Images.B_Pass1 + Define.passToCurrency[PassType.LevelPass].IndexOf(curLevelPass), true);
                        SetNamesAndImages(PassType.LevelPass);
                        SetBadge(PassType.LevelPass);
                        break;
                    case Images.B_StagePass:
                        var targetStagePass = DbStagePass.Get(CurrencyController.I.GetPassInfo(PassType.StagePass).LastFreeRewarded+1);
                        if (targetStagePass == null) targetStagePass = DbStagePass.Get(CurrencyController.I.GetPassInfo(PassType.StagePass).LastFreeRewarded);
                        var curStagePass = targetStagePass.GetSpecificPassType();
                        OnBasicPassTabClicked(Images.B_Pass1 + Define.passToCurrency[PassType.StagePass].IndexOf(curStagePass), true);
                        SetNamesAndImages(PassType.StagePass);
                        SetBadge(PassType.StagePass);
                        break;
                    case Images.B_SoulPass:
                        SetBasicPassType(PassType.SoulPass, CurrencyType.SoulPass);
                        break;
                    case Images.B_SeasonPass:
                        OnSeasonPassTabClicked(Images.B_SeasonPassReward);
                        break;
                }

                void SetNamesAndImages(PassType passType)
                {
                    var passes = Define.passToCurrency[passType];
                    for (var idx = 0; idx < passes.Count; ++idx)
                    {
                        var curIdx = idx;
                        var pass = DbPassShop.Get(p => p.PassType == passes[curIdx]);
                        Get<TextMeshProUGUI>((int)Texts.T_Pass1 + idx).text = LocalString.Get(pass.NameId);
                        Get<Image>((int) Images.IMG_Pass1 + idx).sprite = Manager.Resource.Load<Sprite>(pass.Resource);
                    }
                }
            }
        }

        #endregion
       
        
        #region Open

        private PassType _initCategoryType = PassType.LevelPass;
        public void OpenCategory(PassType category)
        {
            _initCategoryType = category;
            if (!_isInit)
            {
                Init();
                return;
            }
            switch (category)
            {
                case PassType.LevelPass: OnCategoryTabClicked(Images.B_LevelPass);
                    break;
                case PassType.StagePass: OnCategoryTabClicked(Images.B_StagePass);
                    break;
                case PassType.SoulPass: OnCategoryTabClicked(Images.B_SoulPass);
                    break;
                case PassType.SeasonPass: OnCategoryTabClicked(Images.B_SeasonPass);
                    break;
                default: throw new Exception(category + " is not defined pass category");
            }
        }
        
        
        #endregion
        
        
        #region Set Pass Data
        
        private void SetStagePassData()
        {
            var passes = Define.passToCurrency[PassType.StagePass];
            for (var idx = 0; idx < passes.Count; ++idx)
            {
                _stagePassData.Add(new List<PassItem>());
                var pass = passes[idx];
                var curIdx = idx;
                var count = 0;
                DbStagePass.ForEach(s => s.PassType == pass, s =>
                {
                    _stagePassData[curIdx].Add(new PassItem 
                        { index = count, passId = s.Id, passType = PassType.StagePass, specificPassType = pass});
                    count++;
                });
            }
        }

        private void SetLevelPassData()
        {
            var passes = Define.passToCurrency[PassType.LevelPass];
            for (var idx = 0; idx < passes.Count; ++idx)
            {
                _levelPassData.Add(new List<PassItem>());
                var pass = passes[idx];
                var curIdx = idx;
                var count = 0;
                DbLevelPass.ForEach(l => l.PassType == pass, l =>
                {
                    _levelPassData[curIdx].Add(new PassItem
                        {index = count, passId = l.Id, passType = PassType.LevelPass, specificPassType = pass});
                    count++;
                });
            }
        }

        private void SetSoulPassData()
        {
            var idx = 0;
            DbSoulPass.ForEach(s =>
            {
                _soulPassData.Add(new PassItem { index = idx, passId = s.Id, passType = PassType.SoulPass, specificPassType = CurrencyType.SoulPass});
                idx++;
            });
        }
        
        #endregion
        
        
        public override void WhenPopupClosed()
        {
            
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        private PassType ImagesToPassType(Images? image)
        {
            switch (image)
            {
                case Images.B_LevelPass: return PassType.LevelPass;
                case Images.B_StagePass: return PassType.StagePass;
                case Images.B_SoulPass: return PassType.SoulPass;
                case Images.B_SeasonPass : return PassType.SeasonPass;
                default: throw new Exception($"{image} is not defined pass category button");
            }
        }

        private void OpenSeasonPassFinalRewardInfo()
        {
            Manager.UI.ShowPopupUI<UI_CostumeInfo>().Set(DbCostume.Get(DbSeasonPassReward.Get(DbSeasonPassReward.Count).RewardId),
                Get<GameObject>((int)GameObjects.IMG_SeasonPassFinalRewardGrade).transform.position, Define.PivotLeftHigh2);
        }
        
        private void OnDisable()
        {
            _seasonPassCheckEventManager?.Dispose();
            _basicProgressEventsManager?.Dispose();
            _seasonProgressEventsManager?.Dispose();
            _levelPassBadgeEventsManager?.Dispose();
            _stagePassBadgeEventsManager?.Dispose();
            _soulPassBadgeEventsManager?.Dispose();
            _seasonPassBadgeEventManager?.Dispose();
            _canGetAnyBasicRewardEventsManager?.Dispose();
            _canGetAnySeasonRewardEventsManager?.Dispose();
            _seasonPassNextEventManager?.Dispose();
        }

        private void OnEnable()
        {
            _seasonPassCheckEventManager?.Reconnect();
            _basicProgressEventsManager?.Reconnect();
            _seasonProgressEventsManager?.Reconnect();
            _levelPassBadgeEventsManager?.Reconnect();
            _stagePassBadgeEventsManager?.Reconnect();
            _soulPassBadgeEventsManager?.Reconnect();
            _seasonPassBadgeEventManager?.Reconnect();
            _canGetAnyBasicRewardEventsManager?.Reconnect();
            _canGetAnySeasonRewardEventsManager?.Reconnect();
            _seasonPassNextEventManager?.Reconnect();
        }

        
        public void OnLanguageChanged(Locale locale)
        {
            var passShop = DbPassShop.Get(p => p.PassType == _specificType);
            var canBuy = !CurrencyController.I.Have(_specificType);
            //var info = CurrencyController.I.GetPassInfo(_passType);
            //var isLocked =  info != null && info.IsLocked;
            Get<TextMeshProUGUI>((int) Texts.T_Buy).text = canBuy ? passShop.DisplayPrice : LocalString.Get(210144); //isLocked ? 210143 : 210144);

            if (_passType == PassType.SeasonPass)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Title).text = LocalString.Get(DbSeasonPass.Get(SeasonPassController.data.CurrentId.Value).NameId);
            }
            else
            {
                Get<TextMeshProUGUI>((int) Texts.T_Title).text = LocalString.Get(DbCurrency.Get(_specificType).NameId);
            }

            if (SeasonPassController.data.CurrentId.Value != 0)
            {
                var finalReward = DbCostume.Get(DbSeasonPassReward.Get(DbSeasonPassReward.Count).RewardId);
                Util.FindChild<TextMeshProUGUI>(gameObject, "T_SeasonPassFinalRewardGrade", true).text
                    = LocalString.Get(DbGrade.Get(finalReward.Grade).NameId);
            }

        }
    }
}