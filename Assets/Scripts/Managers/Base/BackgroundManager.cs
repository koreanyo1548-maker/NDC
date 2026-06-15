using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbUser.Progress;
using ThirdParty;
using UIs.FieldMain.MainStage;
using UIs.OfflineReward;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Managers.Base
{
    public class BackgroundManager: MonoBehaviour
    {
        public static List<IBackgroundChecker> _checks;

        private void Awake()
        {
            _checks = new List<IBackgroundChecker>();
            
            CurrencyController.data.BookShelves.ForEach(b =>
            {
                if (b.LeftTime > 0)
                {
                    Add(b);
                }
            });
        }

        
        private DateTime _pauseTime;
        private bool _checkCurrentScene = true;
        private void OnApplicationPause(bool pause)
        {
            if (_checkCurrentScene)
            {
                if (!SceneManager.GetActiveScene().name.Equals(SceneType.Field.ToString())) return;
                if (!Manager.Field.IsInit) return;

                _checkCurrentScene = false;
            }

            if (!pause)
            {
                PlayFabManager.Store.DoWithTime(time =>
                {
                    if (_pauseTime < PlayFabManager.Store.LastUpdatedTime) return;
                    var timeDiff = time.Subtract(_pauseTime);
                    PlayFabManager.Store.SetBackgroundTime(timeDiff);
                    if (timeDiff.TotalSeconds > 100)
                    {
                        var field = Manager.Field.CurField.Value;
                        var reason = field == FieldType.Stage ? GameOverType.StageMove : GameOverType.GiveUp;
                        if (field == FieldType.Training) 
                            Manager.UI.GetSceneUI<UI_MainStage>().RemoveTrainingStage();
                        Manager.Field.GameOver(reason, -1, field);
                        Manager.UI.ShowPopupUI<UI_OfflineReward>().Set(time.Subtract(_pauseTime));
                    }
                    var count = _checks.Count;
                    for (var idx = 0; idx < count; ++idx)
                    {
                        _checks[idx].WhenBackFromBackground(timeDiff, time);
                    }
                    PlayFabManager.Store.ForceSave();
                });
            }
            else
            {
                PlayFabManager.Store.DoWithTime(time =>
                {
                    _pauseTime = time;
                });
            }
        }

        public void Add(IBackgroundChecker update)
        {
            if (_checks.Contains(update)) return;
            _checks.Add(update);
        }

        public void Remove(IBackgroundChecker update)
        {
            _checks.Remove(update);
        }
    }
}