using System;
using Controller.Infos;
using Data;
using Data.DbCharacter;
using Data.DbDefinition;
using Managers;
using TMPro;
using UIBases;
using UIs.Lock;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace UIs.Character.Stat
{
    public class UI_StatLocked : UI_Base, ILanguageSet
    {
        private IDbStatCondition _stat;
        private Action _whenUnlocked;

        private EventsManager _lockEventsManager;

        public void Set(IDbStatCondition stat, Action whenUnlocked)
        {
            _stat = stat;

            _whenUnlocked = whenUnlocked;

            if (LevelController.I.CheckIsLocked(_stat.GetCondition(), _stat.GetGoal()))
            {
                _lockEventsManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = CheckLocked,
                    updatedField = LevelController.I.GetUpdatedFieldForLock(_stat.GetCondition())
                });

                transform.Find("T_Goal").GetComponent<TextMeshProUGUI>().text =
                    string.Format(LocalString.Get(stat.GetDescriptionId()), stat.GetGoal());
                LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            }
            else CheckLocked();
        }

        private void CheckLocked()
        {
            var isLocked = LevelController.I.CheckIsLocked(_stat.GetCondition(), _stat.GetGoal());

            if (!isLocked)
            {
                gameObject.SetActive(false);
                _lockEventsManager?.Dispose();
                _whenUnlocked();

                // if (_lockEventsManager != null) Manager.UI.ShowSingleUI<UI_Toast>().SetText(_lock.NameId);
                Destroy(transform.GetComponent<UI_StatLocked>());
            
                LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
            }
        }

        public void OnLanguageChanged(Locale locale)
        {
            transform.Find("T_Goal").GetComponent<TextMeshProUGUI>().text =
                string.Format(LocalString.Get(_stat.GetDescriptionId()), _stat.GetGoal());
        }
    }
}