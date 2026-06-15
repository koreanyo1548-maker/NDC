using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbShop;
using Data.Utils;
using Exceptions;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Dungeon.LevelEntrance;
using UIs.Dungeon.StageEntrance;
using UIs.Dungeon.TrainingGround;
using UIs.Lock;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Dungeon
{
    public class UI_Dungeon_Item : UI_Base, ILanguageSet, IBackgroundChecker
    {
        private EventsManager _ticketEventManager;
        private CoroutineHandle _resetTimeRoutine;
        
        private DbField<int> _ticket;
        private DbField<int> _adTicket;
        private DbDungeonMeta _dungeon;


        enum GameObjects
        {
            IMG_NewTag
        }
        
        enum Texts
        {
            T_TicketCount
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;
            
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<GameObject>(typeof(GameObjects));
            
            return true;
        }

        public void Set(DbDungeonMeta dungeon)
        {
            _dungeon = dungeon;
            
            if (!_isInit) Init();
            
            Util.FindChild<Image>(gameObject, "IMG_DungeonGate", true).sprite =
                Manager.Resource.Load<Sprite>(dungeon.Resource);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_DungeonName", true).text = LocalString.Get(dungeon.NameId);

            Get<GameObject>((int)GameObjects.IMG_NewTag).SetActive(dungeon.Id == FieldType.Training);

            if (dungeon.Id == FieldType.Awakening || dungeon.Id == FieldType.Pet
                || dungeon.Id == FieldType.BlackMarket || dungeon.Id == FieldType.Dia
                || dungeon.Id == FieldType.Training)
            {
                gameObject.GetOrAddComponent<UI_Locked>().Set(GetLockType(dungeon.Id), transform.GetComponent<Image>(), Util.FindChild(gameObject, "IMG_LockIcon"), null,
                    () => gameObject.BindEvent(Functions.TrueCondition, _ => OpenDungeonEntrance(), UIEffectType.Bounce));
            }
            else
            {
                Util.FindChild(gameObject, "IMG_LockIcon").SetActive(false);
                gameObject.BindEvent(Functions.TrueCondition, _ => OpenDungeonEntrance(), UIEffectType.Bounce);
            }

            if (dungeon.Id == FieldType.Training)
            {
                Util.FindChild<Image>(gameObject, "IMG_Glow", true).gameObject.SetActive(false);
                Util.FindChild<Image>(gameObject, "IMG_PortalFX", true).gameObject.SetActive(false);
                Util.FindChild<Image>(gameObject, "IMG_Ticket", true).sprite = Manager.Resource.Load<Sprite>("Icon_Timer");
                
                _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
                Manager.Background.Add(this);
            }
            else
            {
                Util.FindChild<Image>(gameObject, "IMG_Glow", true).color = Define.GetDungeonColor(dungeon.Id);
                _ticket = CurrencyController.I.GetTicketModel(dungeon.Use);
                _adTicket = CurrencyController.I.GetTicketModel(dungeon.AdUse);
                _ticketEventManager = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenTicketCountChanged,
                    updatedField = new [] {_ticket, _adTicket}
                });
                WhenTicketCountChanged();
            }
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            LockType GetLockType(FieldType field)
            {
                if (field == FieldType.Awakening) return LockType.AwakeningDungeon;
                if (field == FieldType.Pet) return LockType.PetDungeon;
                if (field == FieldType.BlackMarket) return LockType.BlackMarketDungeon;
                if (field == FieldType.Dia) return LockType.DiaDungeon;
                if (field == FieldType.Training) return LockType.TrainingGround;
                throw new NotDefinedFieldException(field);
            }
        }

        private void OpenDungeonEntrance()
        {
            Manager.UI.OpenDungeonEntrance(_dungeon.Id, false);
        }

        private void WhenTicketCountChanged()
        {
            var useTicket = _ticket.Value > 0;
            var currency = DbCurrency.Get(useTicket ? _dungeon.Use : _dungeon.AdUse);
            Util.FindChild<Image>(gameObject, "IMG_Ticket", true).sprite =
                Manager.Resource.Load<Sprite>(currency.Resource);
            Get<TextMeshProUGUI>((int) Texts.T_TicketCount).text = (useTicket ? _ticket.Value : _adTicket.Value) + "/" + currency.DailyCharge;
        }
        
        private IEnumerator<float> _ResetLeftTimeRoutine()
        {
            var dayDiff = (DbPlay.Get(PlayType.TrainingGroundResetDay).Value - (int) DateTime.UtcNow.AddHours(9).DayOfWeek + 7) % 7;
            if (dayDiff == 0) dayDiff = 7;
            var nextResetDate = DateTime.UtcNow.AddHours(9).Subtract(DateTime.UtcNow.AddHours(9).TimeOfDay)
                .AddDays(dayDiff) - DateTime.UtcNow.AddHours(9);
            while (nextResetDate.TotalSeconds > 0)
            {
                Get<TextMeshProUGUI>((int) Texts.T_TicketCount).text = string.Format(LocalString.Get(210367), StringMaker.GetTimeString(nextResetDate));
                yield return Timing.WaitForSeconds(1);
                nextResetDate -= Define.ASecond;
            }

            yield return Timing.WaitForSeconds(1);
            _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
        }


        protected virtual void OnEnable()
        {
            _ticketEventManager?.Reconnect();
            if (_isInit && _dungeon.Id == FieldType.Training)
            {
                Manager.Background.Add(this);
                Timing.KillCoroutines(_resetTimeRoutine);
                _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
            }
        }

        private void OnDisable()
        {
            _ticketEventManager?.Dispose();
            if (_isInit && _dungeon.Id == FieldType.Training)
            {
                Manager.Background.Remove(this);
            }
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_DungeonName", true).text = LocalString.Get(_dungeon.NameId);
        }

        public void WhenBackFromBackground(TimeSpan time, DateTime now)
        {
            if (_isInit && _dungeon.Id == FieldType.Training)
            {
                Timing.KillCoroutines(_resetTimeRoutine);
                _resetTimeRoutine = Timing.RunCoroutine(_ResetLeftTimeRoutine().CancelWith(gameObject));
            }
        }
    }
}