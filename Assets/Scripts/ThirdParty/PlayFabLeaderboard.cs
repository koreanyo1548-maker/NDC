using System;
using System.Collections.Generic;
using System.Numerics;
using Controller;
using Controller.Infos;
using Managers;
using Newtonsoft.Json;
using UIs.FieldMain;
using UIs.Ranking;
using Utils;

namespace ThirdParty
{
    public class PlayFabLeaderboard
    {
        public bool DontRank = false;

        public void StartUpdating() { }

        #region Cheat
        public void UpdateCheat(bool isDia, string log) { }
        public void PostCheatLog(string log) { }
        #endregion

        #region UpdateStage
        private bool _stageUpdated;

        public bool IsStageUpdated()
        {
            if (_stageUpdated == false) return false;
            _stageUpdated = false;
            return true;
        }

        public void UpdateStage() { }
        #endregion

        #region UpdateTraining
        private bool _trainingUpdated;

        public bool IsTrainingUpdated()
        {
            if (_trainingUpdated == false) return false;
            _trainingUpdated = false;
            return true;
        }

        public void UpdateTraining() { }
        #endregion

        #region GetLeaderboard & Info

        public void GetStageLeaderboard(int position)
        {
            Manager.UI.GetPopupUI<UI_Ranking>()?.OnStageLoaded(0, new List<LeaderboardEntry>());
        }

        public void GetTrainingLeaderboard(int position)
        {
            Manager.UI.GetPopupUI<UI_Ranking>()?.OnTrainingLoaded(0, new List<LeaderboardEntry>());
        }

        public void GetRankingInfo(string id, Action<int, int, int, int, int> onSucceed)
        {
            onSucceed(0, 0, 0, 0, 0);
        }

        public void GetRankingOf(string id, Action<LeaderboardEntry> onSucceed)
        {
            onSucceed(new LeaderboardEntry());
        }

        public void CheckIdIsValid(string id, Action ifValid, Action ifNotValid)
        {
            ifNotValid();
        }

        #endregion

        #region GetMy Ranking

        private int _myTrainingRanking = -1;

        public void GetMyTrainingRanking(Action<int> withRanking)
        {
            withRanking(0);
        }

        public void GetMyStageLeaderboard()
        {
            var rankingPopup = Manager.UI.GetPopupUI<UI_Ranking>();
            rankingPopup?.OnMyStageLoaded(new List<LeaderboardEntry> { new() { Position = 9999 } });
            Manager.UI.GetSceneUI<UI_MainTop>()?.SetRanking(10000);
        }

        public void GetMyTrainingLeaderboard(Action<int> withRanking = null)
        {
            _myTrainingRanking = 0;
            if (withRanking != null)
                withRanking(0);
            else
                Manager.UI.GetPopupUI<UI_Ranking>()?.OnMyTrainingLoaded(new List<LeaderboardEntry> { new() { Position = 9999 } });
        }

        #endregion

        private int Encode(BigInteger num)
        {
            return num.ToInt();
        }

        [Serializable]
        public class RankingInfo
        {
            public int level;
            public int title;
            public int profile;
            public int stage;
            public int training;

            public RankingInfo()
            {
                level = LevelController.data.Level.Value;
                title = EquipController.data.Title.Value;
                profile = EquipController.data.Profile.Value;
                stage = LevelController.data.MaxStage.Value;
                training = PlayFabManager.Leaderboard.Encode(LevelController.data.MaxTraining.Value);
            }

            [JsonConstructor]
            public RankingInfo(int level, int title, int profile, int stage, int training)
            {
                if (profile == 0) profile = 1;
                this.level = level;
                this.title = title;
                this.profile = profile;
                this.stage = stage;
                this.training = training;
            }
        }
    }
}
