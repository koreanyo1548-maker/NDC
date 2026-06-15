using System;
using System.Collections.Generic;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Managers;
using MEC;
using TMPro;
using UIs.Dungeon.Entrance;
using UIs.Utils;
using UnityEngine.EventSystems;
using Utils;

namespace UIs.Dungeon.TrainingGround
{
    public class UI_DungeonTrainingGround: UI_DungeonEntrance, IBackgroundChecker
    {
        private EventsManager _recordEventHandler;
        private EventsManager _levelEventHandler;
        private CoroutineHandle _resetTimeRoutine;
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            
            Util.FindChild(gameObject, "B_RewardInfo", true).BindEvent(Functions.TrueCondition, _ => OpenRewardInfo(), UIEffectType.Bounce);
            _recordEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenRecordChanged,
                updatedField = new [] {LevelController.data.MaxTraining}
            });
            _levelEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenRecordChanged,
                updatedField = new [] {LevelController.data.TrainingGroundStage}
            });
            return true;
        }

        public override void SetFieldType(FieldType fieldType, bool isForceRefresh = false)
        {
            if (!_isInit) Init();
            _fieldType = fieldType;
            
            _dungeonMeta = DbDungeonMeta.Get(_fieldType);
            
            _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
            Manager.Background.Add(this);
            
            Get<TextMeshProUGUI>((int) Texts.T_Title).text = LocalString.Get(_dungeonMeta.NameId);
            WhenRecordChanged();
            _preset.Value = SettingController.I.GetSkillPresetFor(_fieldType);
            SetSkillPreset();
        }

        private void WhenRecordChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_PrevRecord).text = 
                LevelController.data.MaxTraining.Value == 0 ? LocalString.Get(210370)
                // : LevelController.data.TrainingGroundStage.Value == 0 ? Define.AddUnit(LevelController.data.MaxTraining.Value, 3, 2)
                : string.Format(LocalString.Get(210041), LevelController.data.TrainingGroundStage.Value) 
                  + "    " + Define.AddUnit(LevelController.data.MaxTraining.Value, 3, 2);
        }

        protected override int GetStage()
        {
            return 0;
        }

        protected override void EnterStage()
        {
            Manager.Field.EnterDungeon(FieldType.Training, 1);
            Manager.UI.CloseAllPopupUI();
            Manager.UI.ShowSceneUI<UI_TrainingGroundCount>().Count(3, () => Manager.Field.SpawnGame());
        }

        protected override void ClearStage()
        {
            
        }

        private void OpenRewardInfo()
        {
            Manager.UI.ShowPopupUI<UI_TrainingGroundReward>().Set();
        }
        
        private IEnumerator<float> _ResetLeftTimeRoutine()
        {
            var dayDiff = (DbPlay.Get(PlayType.TrainingGroundResetDay).Value - (int) DateTime.UtcNow.AddHours(9).DayOfWeek + 7) % 7;
            if (dayDiff == 0) dayDiff = 7;
            var nextResetDate = DateTime.Now.Subtract(DateTime.Now.TimeOfDay)
                .AddDays(dayDiff) - DateTime.Now;
            while (nextResetDate.TotalSeconds > 0)
            {
                Get<TextMeshProUGUI>((int) Texts.T_ResetTime).text = string.Format(LocalString.Get(210367), StringMaker.GetTimeString(nextResetDate));
                yield return Timing.WaitForSeconds(1);
                nextResetDate -= Define.ASecond;
            }

            yield return Timing.WaitForSeconds(1);
            _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
        }

        private void OnEnable()
        {
            if (_isInit)
            {
                _recordEventHandler.Reconnect();
                _levelEventHandler.Reconnect();
                Manager.Background.Add(this);
                Timing.KillCoroutines(_resetTimeRoutine);
                _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
            }
        }

        private void OnDisable()
        {
            if (_isInit)
            {
                _recordEventHandler.Dispose();
                _levelEventHandler.Dispose();
                Manager.Background.Remove(this);
            }
        }

        public void WhenBackFromBackground(TimeSpan time, DateTime now)
        {
            Timing.KillCoroutines(_resetTimeRoutine);
            _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
        }
    }
}