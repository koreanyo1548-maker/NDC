using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbUser.Currency;
using Managers;
using Managers.Base;
using MEC;
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
    public class UI_Bookshelf_Item: UI_Base, IBackgroundChecker
    {
        private DbUserBookshelf _bookshelf;
        private CoroutineHandle _timeRoutine;
        
        private EventsManager _diaChangeHandler;
        private EventsManager _bookChangeHandler;

        enum GameObjects
        {
            Have,
            Locked,
            Empty,
            B_DiaOpen,
            B_AdReduceTime,
            B_Unseal,
            Unsealing
        }

        enum Texts
        {
            T_Time,
            T_DiaOpenCost,
        }

        enum Images
        {
            B_DiaOpen,
            B_AdReduceTime,
            IMG_Book,
            IMG_BookshelfBg
        }

        enum Particles
        {
            Effect_UnsealingStart,
            Effect_UnsealingLoop
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));    
            Bind<TextMeshProUGUI>(typeof(Texts));    
            Bind<Image>(typeof(Images));
            Bind<ParticleSystem>(typeof(Particles));

            Util.FindChild<TextMeshProUGUI>(gameObject, "T_AdReduceTime", true).text = StringMaker.SecondsToTimeFull((int)DbPlay.Get(PlayType.BookReduceTimeByAd).Value);
            
            Get<Image>((int)Images.B_DiaOpen).gameObject.BindEvent(DiaOpenCondition, _ => OpenWithDia(), UIEffectType.Bounce);
            Get<Image>((int)Images.B_AdReduceTime).gameObject.BindEvent(() => !_bookshelf.UseAd, _ => ReduceWithAd(), UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_Unseal).BindEvent(Functions.TrueCondition, _ => Unseal(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Add", true).BindEvent(Functions.TrueCondition, _ => AddToShelf(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Unlock", true).BindEvent(Functions.TrueCondition, _ => UnlockShelf(), UIEffectType.Bounce);
            
            _diaChangeHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenDiaChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Dia)}
            });
            
            _bookChangeHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = () => SetStatus(true),
                updatedField = new[] {_bookshelf.HaveBook}
            });
            
            return true;
        }

        public void Set(int idx)
        {
            _bookshelf = CurrencyController.data.BookShelves.Value[idx].Value;
            if (!_isInit) Init();
            SetStatus();
        }

        private void SetStatus(bool isHaveChanged = false)
        {
            Timing.KillCoroutines(_timeRoutine);
            var isUnlocked = _bookshelf.Index == 0 || 
                             (_bookshelf.Index == 1 && CurrencyController.I.Have(CurrencyType.Bookshelf2)) ||
                             (_bookshelf.Index == 2 && CurrencyController.I.Have(CurrencyType.Bookshelf3));

            var have = _bookshelf.HaveBook.Value;
            Get<Image>((int) Images.IMG_BookshelfBg).material = Define.GetUIMaterial(!isUnlocked);
            Get<GameObject>((int)GameObjects.Locked).SetActive(!isUnlocked);
            Get<GameObject>((int)GameObjects.Empty).SetActive(isUnlocked && !have);
            Get<GameObject>((int)GameObjects.Have).SetActive(isUnlocked && have);
            Get<ParticleSystem>((int)Particles.Effect_UnsealingLoop).gameObject.SetActive(have);
            Get<ParticleSystem>((int)Particles.Effect_UnsealingStart).gameObject.SetActive(have && isHaveChanged);

            if (have)
            {
                SetHave(isHaveChanged);
            }
            else
            {
                _diaChangeHandler.Dispose();
            }
        }

        private void WhenDiaChanged()
        {
            Get<Image>((int) Images.B_DiaOpen).material = Define.GetUIMaterial(!DiaOpenCondition());
        }

        private void SetHave(bool isNewHave)
        {
            var canOpen = _bookshelf.LeftTime == 0;
            Get<GameObject>((int)GameObjects.B_Unseal).SetActive(canOpen);
            Get<GameObject>((int)GameObjects.B_DiaOpen).SetActive(!canOpen);
            Get<GameObject>((int)GameObjects.B_AdReduceTime).SetActive(!canOpen);
            Get<GameObject>((int)GameObjects.Unsealing).SetActive(!canOpen);

            Get<Image>((int)Images.IMG_Book).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(_bookshelf.BookType).Resource);

            Get<ParticleSystem>((int)Particles.Effect_UnsealingLoop).Simulate( 0.0f, true, false );
            Get<ParticleSystem>((int)Particles.Effect_UnsealingLoop).Play();

            if (isNewHave)
            {
                Get<ParticleSystem>((int)Particles.Effect_UnsealingStart).Simulate( 0.0f, true, true );
                Get<ParticleSystem>((int)Particles.Effect_UnsealingStart).Play();
            }
            
            if (canOpen)
            {
                _diaChangeHandler.Dispose();
            }
            else
            {
                _diaChangeHandler.Reconnect();
                _timeRoutine = Timing.RunCoroutine(_BookshelfTimeRoutine().CancelWith(gameObject));
                SetAdReduceTime();
            }
        }

        private void OpenWithDia()
        {
            var prev = CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value;
            if (CurrencyController.I.TryUse(CurrencyType.Dia, _needDia))
            {
                CurrencyController.I.SetDiaLog($"책 열기 {_bookshelf.LeftTime}초", -_needDia, prev);
                _bookshelf.ResetTime();
                SetStatus();
                PlayFabManager.Store.ForceSave();
            }
        }

        private void ReduceWithAd()
        {
            Manager.Ad.ShowAd(eAdType.Bookshelf, () => Timing.CallDelayed(0.1f, () =>
            {
                _bookshelf.ReduceTimeWithAd();
                SetAdReduceTime();
                SetStatus();
            }));
        }

        private void Unseal()
        {
            Manager.UI.ShowSceneUI<UI_PetSummonEffect>().Set(_bookshelf);
            QuestController.I.DoQuests(QuestType.BookUnseal);
            QuestController.I.DoQuests(QuestType.SealedBookUnseal);
        }

        private void AddToShelf()
        {
            Manager.UI.ShowSingleUI<UI_Toast>().SetText(200022);
        }

        private void UnlockShelf()
        {
            var currency = _bookshelf.Index switch
            {
                1 => CurrencyType.Bookshelf2,
                2 => CurrencyType.Bookshelf3,
                _ => throw new Exception(_bookshelf.Index + "번 책장은 잠금 해제가 필요하지 않습니다.")
            };
            Manager.UI.ShowPopupUI<UI_UnlockBookshelf>().Set(currency, () => SetStatus());
        }

        private void SetAdReduceTime()
        {
            Get<Image>((int) Images.B_AdReduceTime).material = Define.GetUIMaterial(_bookshelf.UseAd);
        }

        private bool DiaOpenCondition()
        {
            return CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value >= _needDia;
        }
        
        private int _needDia;
        private IEnumerator<float> _BookshelfTimeRoutine()
        {
            var time = _bookshelf.LeftTime;
            while (time > 0)
            {
                var hours = time / 3600;
                var minutes = (time - hours * 3600) / 60;
                var seconds = time - hours * 3600 - minutes * 60;
                Get<TextMeshProUGUI>((int) Texts.T_Time).text = 
                    hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
                
                var totalMinutes = hours * 60 + minutes + (seconds > 0 ? 1 : 0);
                _needDia = DbCost.Get(CostType.BookUnsealingDia).Cost * totalMinutes;
                Get<TextMeshProUGUI>((int) Texts.T_DiaOpenCost).text = _needDia.ToString("N0");
                Get<Image>((int) Images.B_DiaOpen).material = 
                    Define.GetUIMaterial(CurrencyController.I.GetMoneyModel(CurrencyType.Dia).Value < _needDia);
                    
                yield return Timing.WaitForSeconds(1);
                time--;
            }
            
            SetStatus();
        }
        
        private void OnEnable()
        {
            if (_isInit)
            {
                SetStatus();
            }
            _bookChangeHandler?.Reconnect(false);
            Manager.Background.Add(this);
            // _diaChangeHandler?.Reconnect();
        }      
        
        private void OnDisable()
        {
            _bookChangeHandler.Dispose();
            Manager.Background.Remove(this);
            // _diaChangeHandler.Dispose();
        }

        public void WhenBackFromBackground(TimeSpan time, DateTime now)
        {
            SetStatus();
        }
    }
}