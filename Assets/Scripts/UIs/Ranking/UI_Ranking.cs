using System;
using System.Collections.Generic;

using dynamicscroll;
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

namespace UIs.Ranking
{
    public class UI_Ranking: UI_Popup, ILanguageSet
    {
        private DynamicScroll<RankingItem, UI_Ranking_Item> _stageRankingRect;
        private DynamicScroll<RankingItem, UI_Ranking_Item> _trainingRankingRect;
        private Images? _curOpened = null;
        
        private Sprite[] _tabSprites; // 0: not selected 1: selected

        private Dictionary<int, List<LeaderboardEntry>> _stageRankings = new();
        private Dictionary<int, List<LeaderboardEntry>> _trainingRankings = new();
        
        private List<RankingItem> _stageData = new();
        private List<RankingItem> _trainingData = new();

        private List<RankingItem> _stageDataSave = new();
        private List<RankingItem> _trainingDataSave = new();

        private UI_MyRanking_Item _myRanking;

        private Vector3 _positionSetter = new Vector3();
        
        private int _myStageRanking = 0;
        private int _myTrainingRanking = 0;


        enum GameObjects
        {
            V_Stage,
            V_Training,
            T_Loading
        }

        enum DynamicScrollRects
        {
            V_Stage,
            V_Training
        }
        
        enum Images
        {
            B_StageTab,
            B_TrainingTab
        }

        enum Transforms
        {
            B_StageTab,
            B_TrainingTab
        }

        enum Texts
        {
            T_Stat
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
                Bind<Transform>(typeof(Transforms));
                Bind<GameObject>(typeof(GameObjects));
                Bind<Image>(typeof(Images));
                Bind<DynamicScrollRect>(typeof(DynamicScrollRects));
                Bind<TextMeshProUGUI>(typeof(Texts));

                _myRanking = Util.FindChild(gameObject, "My_Ranking_Item", true).GetOrAddComponent<UI_MyRanking_Item>();
                _tabSprites = new[]
                    {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

                for (var idx = 0; idx < 200; ++idx)
                {
                    _stageDataSave.Add(new RankingItem {type = RankingType.Stage});
                    _trainingDataSave.Add(new RankingItem {type = RankingType.Training});
                }

                _stageData = _stageDataSave.Clone();
                _trainingData = _trainingDataSave.Clone();

                _stageRankingRect = new DynamicScroll<RankingItem, UI_Ranking_Item>();
                _stageRankingRect.Initiate(Get<DynamicScrollRect>((int) DynamicScrollRects.V_Stage), _stageData,
                    -1, "Prefabs/UI/SubItem/UI_Ranking_Item");

                _trainingRankingRect = new DynamicScroll<RankingItem, UI_Ranking_Item>();
                _trainingRankingRect.Initiate(Get<DynamicScrollRect>((int) DynamicScrollRects.V_Training),
                    _trainingData,
                    -1, "Prefabs/UI/SubItem/UI_Ranking_Item");

                foreach (Images tab in Enum.GetValues(typeof(Images)))
                {
                    Get<Image>((int) tab).GetComponent<Button>().onClick.AddListener(() => OnTabClicked(tab));
                }

                Get<GameObject>((int) GameObjects.T_Loading).SetActive(false);
                Get<GameObject>((int) GameObjects.V_Stage).SetActive(false);
                Get<GameObject>((int) GameObjects.V_Training).SetActive(false);
                Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(),
                    UIEffectType.None, false);
                OnTabClicked(Images.B_StageTab);

                LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

                PlayFabManager.Leaderboard.GetMyStageLeaderboard();
                PlayFabManager.Leaderboard.GetMyTrainingLeaderboard();
            });
            return true;
        }

        private void OnEnable()
        {
            if (_isInit && PlayFabManager.Leaderboard.IsStageUpdated())
            {
                Timing.CallDelayed(Timing.DeltaTime, () =>
                {
                    PlayFabManager.Leaderboard.GetMyStageLeaderboard();
                    _stageRankings.Clear();
                    _stageData = _stageDataSave.Clone();
                    _stageRankingRect.ChangeList(_stageData, 0);
                });
            }
            else if (_isInit && PlayFabManager.Leaderboard.IsTrainingUpdated())
            {
                Timing.CallDelayed(Timing.DeltaTime, () =>
                {
                    PlayFabManager.Leaderboard.GetMyTrainingLeaderboard();
                    _trainingRankings.Clear();
                    _trainingData = _trainingDataSave.Clone();
                    _trainingRankingRect.ChangeList(_trainingData, 0);
                });
            }
        }

        public LeaderboardEntry GetStageOf(int idx)
        {
            var dicIdx = idx / 20;
            if (_stageRankings.ContainsKey(dicIdx))
            {
                return _stageRankings[dicIdx][idx - dicIdx * 20];
            }
            
            if (idx % 20 == 0 && (dicIdx == 0 || _stageRankings.ContainsKey(dicIdx-1)))
            {
                Get<GameObject>((int)GameObjects.T_Loading).SetActive(true);
                PlayFabManager.Leaderboard.GetStageLeaderboard(idx);
            }
            return null;
        }

        public LeaderboardEntry GetTrainingOf(int idx)
        {
            var dicIdx = idx / 20;
            if (_trainingRankings.ContainsKey(dicIdx))
            {
                return _trainingRankings[dicIdx][idx - dicIdx * 20];
            }
            
            if (idx % 20 == 0 && (dicIdx == 0 || _trainingRankings.ContainsKey(dicIdx-1)))
            {
                Get<GameObject>((int)GameObjects.T_Loading).SetActive(true);
                PlayFabManager.Leaderboard.GetTrainingLeaderboard(idx);
            }
            return null;
        }

        public void OnMyStageLoaded(List<LeaderboardEntry> rankings)
        {
            _myStageRanking = rankings[0].Position + 1;
            if (_curOpened == Images.B_StageTab) _myRanking.Set(_myStageRanking, RankingType.Stage);
        }

        public void OnMyTrainingLoaded(List<LeaderboardEntry> rankings)
        {
            _myTrainingRanking = rankings[0].Position + 1;
            if (_curOpened == Images.B_TrainingTab) _myRanking.Set(_myTrainingRanking, RankingType.Training);
        }

        public void OnStageLoaded(int idx, List<LeaderboardEntry> rankings)
        {
            _stageRankings.Add(idx, rankings);
            if (rankings.Count < 20)
            {
                var removed = idx * 20 + rankings.Count;
                _stageData.RemoveRange(removed, _stageData.Count - removed);
                var center = _stageRankingRect.GetCentralizedObject().CurrentIndex;
                _stageRankingRect.ChangeList(_stageData, 0);
                _stageRankingRect.MoveToIndex(center, 0.1f);
            }
            Get<GameObject>((int)GameObjects.T_Loading).SetActive(false);
            if (gameObject.activeSelf && _stageData.Count > 0 && rankings.Count >= 20)
            {
                _stageRankingRect.ForceUpdateList();
            }
        }
        
        public void OnTrainingLoaded(int idx, List<LeaderboardEntry> rankings)
        {
            _trainingRankings.Add(idx, rankings);
            if (rankings.Count < 20)
            {
                var removed = idx * 20 + rankings.Count;
                _trainingData.RemoveRange(removed, _trainingData.Count - removed);
                var center = _trainingRankingRect.GetCentralizedObject().CurrentIndex;
                _trainingRankingRect.ChangeList(_trainingData, 0);
                _trainingRankingRect.MoveToIndex(center, 0.1f);
            }
            Get<GameObject>((int)GameObjects.T_Loading).SetActive(false);
            if (gameObject.activeSelf && _trainingData.Count > 0 && rankings.Count >= 20)
            {
                _trainingRankingRect.ForceUpdateList();
            }
        }

        
        private void OnTabClicked(Images clicked)
        {
            if (_curOpened == clicked)
            {
                return;
            }

            if (_curOpened != null)
            {
                _positionSetter = Get<Transform>((int) _curOpened).localPosition;
                _positionSetter.y = 22.83f;
                Get<Transform>((int)_curOpened).localPosition = _positionSetter;
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
                CloseTab(_curOpened);
            }

            _curOpened = clicked; 
            _positionSetter = Get<Transform>((int) _curOpened).localPosition;
            _positionSetter.y = 7.92f;
            Get<Transform>((int) clicked).localPosition = _positionSetter;
            Get<Image>((int) clicked).sprite = _tabSprites[1];
            OpenTab(clicked);
            Get<TextMeshProUGUI>((int)Texts.T_Stat).text = LocalString.Get(_curOpened == Images.B_TrainingTab ? 210383 : 210159);

            void OpenTab(Images tab)
            {
                switch (tab)
                {
                    case Images.B_StageTab:
                        Get<GameObject>((int) GameObjects.V_Stage).SetActive(true);
                        Get<GameObject>((int)GameObjects.T_Loading).SetActive(false);
                        _myRanking.Set(_myStageRanking, RankingType.Stage);
                        break;
                    case Images.B_TrainingTab:
                        Get<GameObject>((int) GameObjects.V_Training).SetActive(true);
                        Get<GameObject>((int)GameObjects.T_Loading).SetActive(false);
                        _myRanking.Set(_myTrainingRanking, RankingType.Training);
                        break;
                }
            }

            void CloseTab(Images? tab)
            {
                switch (tab)
                {
                    case Images.B_StageTab:
                        Get<GameObject>((int) GameObjects.V_Stage).SetActive(false);
                        break;
                    case Images.B_TrainingTab:
                        Get<GameObject>((int) GameObjects.V_Training).SetActive(false);
                        break;
                }
            }
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }

        public void OnLanguageChanged(Locale locale)
        {            
            Get<TextMeshProUGUI>((int)Texts.T_Stat).text = LocalString.Get(_curOpened == Images.B_TrainingTab ? 210383 : 210159);
        }
    }
}