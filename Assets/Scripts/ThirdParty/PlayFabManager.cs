using System;
using System.Collections.Generic;
using Controller.Play;
using Data.Utils;
using UnityEngine;
using Utils;
using UIs.Etc.Login;

namespace ThirdParty
{
    public static class PlayFabManager
    {
        private static PlayFabStore _store;
        private static PlayFabLeaderboard _leaderboard;
        private static PlayFabTitleData _data;
        public static PlayFabStore Store => _store;
        public static PlayFabLeaderboard Leaderboard => _leaderboard;
        public static PlayFabTitleData Data => _data;

        public static void Init()
        {
            _store = new PlayFabStore();
            _leaderboard = new PlayFabLeaderboard();
            _data = new PlayFabTitleData();
        }

        public static void ManualGuestLogin(string id)
        {
            AfterLogin();
        }

        public static void LoginWithGooglePlayGamesServices(string authCode) => AfterLogin();
        public static void LoginWithGoogle(string authCode) => AfterLogin();
        public static void LoginWithApple(string token) => AfterLogin();

        private static void AfterLogin()
        {
            SettingController.UID = SystemInfo.deviceUniqueIdentifier;
            Data.CheckDontRank();

            UI_Login.I.StartLoading();
            _store.LoadData();

            SettingController.Nickname = LocalSaveManager.GetOrCreateNickname();
            DbLoadChecker.I.Check();

            Data.CheckAppStoreReview();
        }

        public static void TrySetDisplayName(string name, Action<string> onSuccess)
        {
            LocalSaveManager.SetNickname(name);
            SettingController.Nickname = name;
            onSuccess(name);
        }

        public static void LogError(string bodyName, string msg) { }
        public static void LogCheat(string bodyName, Dictionary<string, string> dic) { }
    }
}
