using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Play;
using Data.DbDefinition;
using Data.DbPetInfo;
using Data.DbShop;
using Data.Stores;
using Data.Utils;
using Managers;
using Managers.Base;
using MEC;
using Newtonsoft.Json;
using ThirdParty;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Data.DbUser.Currency
{
    [Serializable]
    public class DbUserBookshelf: IBackgroundChecker
    {
        public int Index;
        public DbField<bool> HaveBook;
        public CurrencyType BookType;
        public DateTime StartTime;
        public int LeftTime;
        public bool UseAd;
        public bool UseDia;

        public DbField<bool> CanOpen;

        private CoroutineHandle _timeRoutine;
        
        public DbUserBookshelf(int idx, DbUserModel parent)
        {
            Index = idx;
            HaveBook = new DbField<bool>(false, 0, parent);
            CanOpen = new DbField<bool>(false, 0, parent);
        }
        
        [JsonConstructor]
        public DbUserBookshelf(int index, bool haveBook, CurrencyType bookType, DateTime startTime, int leftTime, 
            bool useAd, bool useDia, DbUserModel parent)
        {
            Index = index;
            HaveBook = new DbField<bool>(haveBook, 0, parent);
            BookType = bookType;
            StartTime = startTime;
            LeftTime = leftTime;
            UseAd = useAd;
            UseDia = useDia;

            CanOpen = new DbField<bool>(HaveBook.Value, 0, parent);
        }

        public void Use(CurrencyType bookType, DateTime time)
        {
            BookType = bookType;
            LeftTime = DbBook.Get(BookType).Time;
            StartTime = time;
            UseAd = false;
            UseDia = false;
            HaveBook.Value = true;

            _timeRoutine = Timing.RunCoroutine(_BookshelfTimeRoutine());
            Manager.Background.Add(this);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
        }

        public void StartBookShelfTimeRoutine()
        {
            CanOpen.Value = false;
            _timeRoutine = Timing.RunCoroutine(_BookshelfTimeRoutine());
        }

        private IEnumerator<float> _BookshelfTimeRoutine()
        {
            while (LeftTime > 0)
            {
                yield return Timing.WaitForSeconds(1);
                LeftTime--;
            }
            BadgeController.I.OnBookUpdated(null, null);
            CanOpen.Value = true;
            if (SceneManager.GetActiveScene().name.Equals(SceneType.Field.ToString())) Manager.Background.Remove(this);
        }
        
        public void ResetTime()
        {
            UseDia = true;
            LeftTime = 0;
            CanOpen.Value = true;
            Timing.KillCoroutines(_timeRoutine);
            BadgeController.I.OnBookUpdated(null, null);
        }

        public void ReduceTimeWithAd()
        {
            LeftTime = Math.Max(0, LeftTime - (int)DbPlay.Get(PlayType.BookReduceTimeByAd).Value);
            UseAd = true;
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
        }

        public void Unseal()
        {
            CurrencyController.I.GetReward(DbBook.Get(BookType));
            UseAd = false;
            HaveBook.Value = false;
            CanOpen.Value = false;
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency);
        }

        public void WhenBackFromBackground(TimeSpan time, DateTime now)
        {
            LeftTime = Math.Max(0, LeftTime - (int)time.TotalSeconds);
        }
    }
}