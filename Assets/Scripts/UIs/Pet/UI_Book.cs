using System;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbPetInfo;

using Data.DbUser.Equipment;
using Managers;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Pet
{
    public class UI_Book: UI_Popup
    {
        private DbBook _book;
        
        enum Texts
        {
            T_PetCount,
            T_PetGrade,
            T_StoneCount,
            T_Name,
            T_Count,
            T_Time,
            T_Unseal
        }

        enum GameObjects
        {
            IMG_TimeBg
        }

        enum Images
        {
            IMG_Book
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));
            
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Util.FindChild(gameObject, "B_Unseal", true).BindEvent(Functions.TrueCondition, _ => StartUnseal());
            
            return true;
        }

        public void Set(DbBook book)
        {
            if (!_isInit) Init();

            _book = book;
            Get<Image>((int) Images.IMG_Book).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(book.Id).Resource);
            Get<TextMeshProUGUI>((int) Texts.T_Name).text = LocalString.Get(book.NameId);
            Get<TextMeshProUGUI>((int) Texts.T_PetCount).text = $"{book.MinPetCount}~{book.MaxPetCount}";
            Get<TextMeshProUGUI>((int)Texts.T_PetGrade).text = $"{book.GetMinGradeName()}~{book.GetMaxGradeName()}";
            Get<TextMeshProUGUI>((int)Texts.T_StoneCount).text = $"{book.MinStoneCount}~{book.MaxStoneCount}";
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = CurrencyController.I.GetBookModel(book.Id).Value.ToString();

            var needTime = book.Time > 0;
            Get<GameObject>((int)GameObjects.IMG_TimeBg).SetActive(needTime);
            Get<TextMeshProUGUI>((int) Texts.T_Unseal).text = LocalString.Get(needTime ? 210228 : 210229);
            if (needTime) Get<TextMeshProUGUI>((int) Texts.T_Time).text = StringMaker.SecondsToTimeFull(book.Time);
        }

        private bool _isUnsealing = false;
        private void StartUnseal()
        {
            if (_isUnsealing) return;
            if (_book.Time > 0)
            {
                var bookshelf = CurrencyController.I.GetEmptyBookshelf();
                
                if (bookshelf == null)
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200021);
                }
                else
                {
                    _isUnsealing = true;
                    PlayFabManager.Store.DoWithTime(time =>
                    {
                        if (CurrencyController.I.TryUse(_book.Id, 1))
                        {
                            bookshelf.Use(_book.Id, time);
                            ClosePopupUI();
                            _isUnsealing = false;
                        }
                    });
                }
            }
            else
            {
                var count = CurrencyController.I.GetBookModel(_book.Id).Value;
                if (CurrencyController.I.TryUse(_book.Id, count))
                {
                    QuestController.I.DoQuests(QuestType.BookUnseal, count);
                    CurrencyController.I.GetReward(_book, count);
                    ClosePopupUI();
                }
            }
        }

        private void OnEnable()
        {
            _isUnsealing = false;
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}