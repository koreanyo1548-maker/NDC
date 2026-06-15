using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using UnityEngine;
using Utils;

namespace Managers.Base
{
    public class DayDiffManager: MonoBehaviour
    {
        public static List<IDayDiffChecker> _checks;

        private void Awake()
        {
            _checks = new List<IDayDiffChecker>();
            
            Add(QuestController.I);
            Add(NewbieQuestController.I);
            Add(CurrencyController.data);
            Add(AttendController.I);
            Add(LevelController.data);
            Add(SeasonPassController.data);
            Add(EventAttendController.data);
            Add(DropEventController.I);
        }

        public void HandleDayDiff(DateTime now, int dayDiff)
        {
            for (var idx = 0; idx < _checks.Count;)
            {
                var check = _checks[idx];
                _checks[idx].HandleDayDiff(now, dayDiff);
                if (idx < _checks.Count && check == _checks[idx])
                {
                    idx++;
                }
            }
        }

        public void Add(IDayDiffChecker check)
        {
            if (_checks.Contains(check)) return;
            _checks.Add(check);
        }

        public void Remove(IDayDiffChecker check)
        {
            _checks.Remove(check);
        }
    }
}