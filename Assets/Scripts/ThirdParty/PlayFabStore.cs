using System;
using System.Collections.Generic;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbUser.Currency;
using Data.DbUser.Equipment;
using Data.DbUser.Etc;
using Data.DbUser.Progress;
using Data.Stores;
using Data.Utils;
using Managers;
using MEC;
using Newtonsoft.Json;
using UIs.FieldMain;
using UnityEngine;
using Utils;

namespace ThirdParty
{
    public class PlayFabStore
    {
        public DateTime LastUpdatedTime;
        public TimeSpan OfflineTime { get; private set; }

        private Dictionary<string, string> _saves = new()
        {
            {"Time", string.Empty},
            {"Equipments", string.Empty},
            {"Records", string.Empty},
            {"Currency", string.Empty},
            {"Info", string.Empty},
            {"Ability", string.Empty},
            {"Relic", string.Empty}
        };

        #region Time Checker

        public void CheckTime()
        {
            DoWithTime(now =>
            {
                Manager.DayDiff.HandleDayDiff(now, Define.GetDayDiff(LastUpdatedTime, now));
                LastUpdatedTime = now;
            });
        }

        public void DoWithTime(Action<DateTime> toDo)
        {
            toDo(LocalSaveManager.Now());
        }

        #endregion


        #region Logging

        private string _log = string.Empty;

        IEnumerator<float> _PostLogRoutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(5);
                _log = string.Empty;
            }
        }

        public void SetLog(string log)
        {
            _log += log;
        }

        public void SaveLog()
        {
            LocalSaveManager.SaveKeys(new Dictionary<string, string>
            {
                {"ZLog", JsonConvert.SerializeObject(UserInfo.saved.log)}
            });
        }

        #endregion


        #region Device Check

        public void CheckDevice(Action whenPass, Action whenConflict)
        {
            whenPass();
        }

        #endregion


        #region Mail

        IEnumerator<float> _CheckMailRoutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(60);
                DbUserCurrency.Get(0).CheckNewMail();
            }
        }

        #endregion


        #region Data Saving

        private TimeSpan _backgroundTime = TimeSpan.Zero;

        public void SetBackgroundTime(TimeSpan time)
        {
            _backgroundTime = time;
        }

        public void ForceSave(Action toDo = null, bool exitGame = false, bool needRestart = false)
        {
            TrySave(exitGame, needRestart, toDo);
        }

        private void TrySave(bool exitGame, bool needRestart, Action toDo = null)
        {
            if (!DbLoadChecker.I.IsAllLoaded) return;
            DoWithTime(Set);

            void Set(DateTime now)
            {
                Manager.DayDiff.HandleDayDiff(now, Define.GetDayDiff(LastUpdatedTime, now));
                UserInfo.saved.info.playTime += now - LastUpdatedTime - _backgroundTime;
                _backgroundTime = TimeSpan.Zero;
                LastUpdatedTime = now;
                _saves["Time"] = JsonConvert.SerializeObject(now);
                _saves["Equipments"] = UserInfo.GetEquipments();
                _saves["Records"] = UserInfo.GetRecords();
                _saves["Currency"] = UserInfo.GetCurrency();
                _saves["Info"] = UserInfo.GetInfo();
                _saves["Ability"] = UserInfo.GetAbility();
                _saves["Relic"] = UserInfo.GetRelic();

                LocalSaveManager.SaveAll(_saves);
                toDo?.Invoke();

                if (exitGame)
                {
                    if (needRestart) RestartAndroid();
                    else Exit();
                }
            }
        }

        IEnumerator<float> _AutoSaveRoutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(5);
                ForceSave();
            }
        }

        public void SaveAndExit()
        {
            ForceSave(() => { }, true, false);
        }

        public void SaveAndRestart()
        {
#if APPSTORE
            ForceSave(() => { }, true, false);
#else
            ForceSave(() => { }, true, true);
#endif
        }

        public void ResetUserData()
        {
            LocalSaveManager.DeleteAll();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion


        #region Data Loading

        public void LoadData()
        {
            DoWithTime(Load);

            void Load(DateTime now)
            {
                LastUpdatedTime = now;

                var data = LocalSaveManager.LoadAll();

                if (data != null && data.ContainsKey("Equipments"))
                {
                    UserInfo userInfo;
                    if (!data.ContainsKey("Records"))
                    {
                        userInfo = new UserInfo(
                            JsonConvert.DeserializeObject<Equipment>(data["Equipments"]),
                            0, null,
                            JsonConvert.DeserializeObject<List<Quest>>(data["Quests"]),
                            JsonConvert.DeserializeObject<List<Title>>(data["Titles"]),
                            data.TryGetValue("Ability", out var ability)
                                ? JsonConvert.DeserializeObject<List<Ability>>(ability)
                                : new(),
                            data.TryGetValue("Relic", out var relic)
                                ? JsonConvert.DeserializeObject<List<Relic>>(relic)
                                : new(),
                            JsonConvert.DeserializeObject<CurrencyInfo>(data["Currency"]),
                            JsonConvert.DeserializeObject<Info>(data["Info"]),
                            data.TryGetValue("ZLog", out var log)
                                ? JsonConvert.DeserializeObject<UserLog>(log)
                                : new UserLog());
                    }
                    else
                    {
                        userInfo = new UserInfo(
                            JsonConvert.DeserializeObject<Equipment>(data["Equipments"]),
                            JsonConvert.DeserializeObject<Record>(data["Records"]),
                            data.TryGetValue("Ability", out var ability)
                                ? JsonConvert.DeserializeObject<List<Ability>>(ability)
                                : new(),
                            data.TryGetValue("Relic", out var relic)
                                ? JsonConvert.DeserializeObject<List<Relic>>(relic)
                                : new(),
                            JsonConvert.DeserializeObject<CurrencyInfo>(data["Currency"]),
                            JsonConvert.DeserializeObject<Info>(data["Info"]),
                            data.TryGetValue("ZLog", out var log)
                                ? JsonConvert.DeserializeObject<UserLog>(log)
                                : new UserLog());
                    }

                    new DbUserWeapon().Set(userInfo.ConvertToWeapon());
                    new DbUserAccessory().Set(userInfo.ConvertToAccessory());
                    new DbUserSkill().Set(userInfo.ConvertToSkill());
                    new DbUserNecklace().Set(userInfo.ConvertToNecklaces());
                    var prevTime = JsonConvert.DeserializeObject<DateTime>(data["Time"]);
                    var dayDiff = Define.GetDayDiff(prevTime, now);
                    var lastDay = prevTime.DayOfWeek == DayOfWeek.Sunday ? 7 : (int) prevTime.DayOfWeek;
                    var nowDay = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int) now.DayOfWeek;
                    new DbUserQuest().Set(userInfo.ConvertToQuest(dayDiff, nowDay < lastDay || dayDiff > 6));
                    new DbUserTitle().Set(userInfo.ConvertToTitle());
                    new DbUserPet().Set(userInfo.ConvertToPet());
                    new DbUserAbility().Set(userInfo.ConvertToAbility());
                    new DbUserRelic().Set(userInfo.ConvertToRelic());
                    new DbUserNewbieQuest().Set(userInfo.ConvertToNewbieQuest());
                    NewbieQuestController.I.SetCurDay(userInfo.GetNewbieQuestDay());

                    var info = userInfo.info;
                    new DbUserEquip().Set(info?.ConvertToEquip());
                    new DbUserLevel().Set(info?.ConvertToLevel());
                    new DbUserLevelPoint().Set(info?.ConvertToLevelPoint());
                    new DbUserStat().Set(info?.ConvertToStat());
                    new DbUserSeasonPass().Set(info?.ConvertToSeasonPass());
                    new DbUserMainQuest().Set(info?.ConvertToMainQuest());
                    new DbUserAttend().Set(info?.ConvertToAttend());
                    new DbUserEventAttend().Set(info?.ConvertToEventAttend());
                    new DbUserCurrency().Set(userInfo.currency.ConvertToCurrency(now));
                    FriendController.I.Set(info.friends, info.friendRewarded);

                    if (UserInfo.saved.info.questResetDate == new DateTime())
                    {
                        UserInfo.saved.info.questResetDate = prevTime;
                        UserInfo.saved.info.currencyResetDate = prevTime;
                    }

                    AttendController.I.ForceHandleDayDiff(now, dayDiff);
                    DbUserEventAttend.Get(0).ForceHandleDayDiff(now, dayDiff);
                    DbUserSeasonPass.Get(0).ForceHandleDayDiff(now, dayDiff);
                    DropEventController.I.ForceHandleDayDiff(now, dayDiff);
                    NewbieQuestController.I.HandleDayDiff(now, dayDiff);

                    DbUserCurrency.Get(0).HandleDayDiff(now, 0);
                    DbUserLevel.Get(0).HandleDayDiff(now, 0);
                    DbUserLevel.Get(0).HandleDayDiff(now, 0);

                    DbUserCurrency.Get(0).SetPass(now);
                    DbUserCurrency.Get(0).SetAdBuff(now, dayDiff > 0);
                    DbUserCurrency.Get(0).SetBookshelves(now);
                    DbUserCurrency.Get(0).SetPassion(now);

                    OfflineTime = now - prevTime;
                }
                else
                {
                    if (PlayerPrefs.HasKey(SettingType.Setting.ToString()))
                    {
                        DbUserSetting.Get(0).Reset();
                    }

                    new DbUserWeapon().Set(null);
                    new DbUserAccessory().Set(null);
                    new DbUserSkill().Set(null);
                    new DbUserNecklace().Set(null);
                    new DbUserQuest().Set(null);
                    new DbUserTitle().Set(null);
                    new DbUserPet().Set(null);
                    new DbUserAbility().Set(null);
                    new DbUserRelic().Set(null);
                    new DbUserCurrency().Set(null);
                    new DbUserEquip().Set(null);
                    new DbUserLevel().Set(null);
                    new DbUserLevelPoint().Set(null);
                    new DbUserStat().Set(null);
                    new DbUserMainQuest().Set(null);
                    new DbUserAttend().Set(null);
                    new DbUserEventAttend().Set(null);
                    new DbUserSeasonPass().Set(null);
                    new DbUserNewbieQuest().Set(null);
                    FriendController.I.Set(new(), 0);

                    new UserInfo().WhenCreated(now);

                    AttendController.I.HandleDayDiff(now, 1);
                    DbUserEventAttend.Get(0).HandleDayDiff(now, 1);
                    DbUserSeasonPass.Get(0).HandleDayDiff(now, 1);
                    DropEventController.I.HandleDayDiff(now, 1);
                    NewbieQuestController.I.HandleDayDiff(now, 1);

                    DbUserEventAttend.Get(0).HandleDayDiff(now, 1);
                    DbUserSeasonPass.Get(0).HandleDayDiff(now, 1);

                    DbUserCurrency.Get(0).SetPass(now);
                    DbUserCurrency.Get(0).SetAdBuff(now, true);
                    DbUserCurrency.Get(0).SetBookshelves(now);
                    DbUserCurrency.Get(0).SetPassion(now);
                    ForceSave();
                }

                new DbUserBadge().Set(null);
                new DbUserPlay().Set(null);

                DbUserCurrency.Get(0).CheckMail();
                Timing.RunCoroutine(_AutoSaveRoutine());
                Timing.RunCoroutine(_CheckMailRoutine());
                Timing.RunCoroutine(_PostLogRoutine());
                DbLoadChecker.I.Check();
            }
        }

        #endregion


        #region Nickname

        public void SetAutoNickname()
        {
            if (!string.IsNullOrEmpty(SettingController.Nickname)) return;
            string nickname = LocalSaveManager.GetOrCreateNickname();
            SettingController.Nickname = nickname;
            Manager.UI.GetSceneUI<UI_MainTop>()?.SetNickname();
        }

        #endregion

        #region Exit

        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void RestartAndroid()
        {
#if UNITY_EDITOR
#else
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
                const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

                var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

                intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
                currentActivity.Call("startActivity", intent);
                currentActivity.Call("finish");
                var process = new AndroidJavaClass("android.os.Process");
                int pid = process.CallStatic<int>("myPid");
                process.CallStatic("killProcess", pid);
            }
#endif
        }

        #endregion
    }
}
