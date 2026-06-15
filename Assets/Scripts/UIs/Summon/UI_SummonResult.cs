using System.Collections.Generic;
using Coffee.UIEffects;
using Controller;
using Controller.Currency;
using Data;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbNecklaceInfo;
using Data.DbRelicInfo;
using Data.DbSummon;
using DG.Tweening;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Summon
{
    public class UI_SummonResult: UI_Popup
    {
        private List<UI_SummonResult_Item> _summoned;

        private Transform _shakeT;
        private Animator _animator;

        private bool _isContinue;
        private bool _isSummoning;
        private SummonType _summonType;
        private int _recentSummonNumber;
        private bool _isSummonWithMoney;
        
        private List<SummonCurrency> _summonCurrencies = new() {SummonCurrency.None, SummonCurrency.None, SummonCurrency.None, SummonCurrency.None};

        
        private static string _summon11 = "Summon11";
        private static string _summon33 = "Summon33";
        
        enum GameObjects
        {
            IMG_CheckIcon
        }

        enum Texts
        {
            T_CostSummon1,
            T_CostSummon2,
            T_CostSummon3,
            T_CostSummon4,
            T_Summon1Count,
            T_Summon2Count,
            T_Summon3Count,
            T_Summon4Count,
            T_Currency,
            T_Ticket
        }

        enum Images
        {
            B_Close,
            B_Summon1,
            B_Summon2,
            B_Summon3,
            B_Summon4,
            IMG_Ticket,
            IMG_Currency,
            IMG_Cost1,
            IMG_Cost2,
            IMG_Cost3,
            IMG_Cost4
        }

        enum UIShines
        {
            B_Summon3,
            B_Summon4
        }
        
        public override bool CanClose()
        {
            return !_isSummoning && !_isContinue;
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            Bind<UIShiny>(typeof(UIShines));

            Get<Image>((int)Images.B_Close).gameObject.BindEvent(CanClose, _ => ClosePopupUI(), UIEffectType.None, false);
            Get<Image>((int)Images.B_Summon1).gameObject.BindEvent(Summon1Condition, _ => TrySummon(0));
            Get<Image>((int)Images.B_Summon2).gameObject.BindEvent(Summon2Condition, _ => TrySummon(1));
            Get<Image>((int)Images.B_Summon3).gameObject.BindEvent(Summon3Condition, _ => TrySummon(2));
            Get<Image>((int)Images.B_Summon4).gameObject.BindEvent(Summon4Condition, _ => TrySummon(3));
            Util.FindChild(gameObject, "B_Continue", true).BindEvent(Functions.TrueCondition, _ => ChangeContinue());
            _shakeT = Util.FindChild<Transform>(gameObject, "SafeArea", true);
            
            
            Get<GameObject>((int)GameObjects.IMG_CheckIcon).SetActive(false);
            var summonParent = Util.FindChild<Transform>(gameObject, "Summoned", true);
            var count = DbWeapon.Count;

            _animator = transform.GetComponent<Animator>();
            _animator.enabled = false;
            _summoned = new List<UI_SummonResult_Item>(count);
            while (count-- > 0)
            {
                _summoned.Add(Manager.UI.MakeSubItem<UI_SummonResult_Item>(summonParent));
            }

            return true;
        }

        private void ChangeContinue()
        {
            if (_isSummoning && !_isContinue) return;
            _isContinue = !_isContinue;
            Get<GameObject>((int)GameObjects.IMG_CheckIcon).SetActive(_isContinue);
            EnablingButton();
        }

        private void AdjustSummonCurrency()
        {
            for (var idx = 0; idx < 4; ++idx)
            {
                var canTicket = CurrencyController.I.CheckSummonCurrency(_summonType, SummonCurrency.Ticket, idx);
                if (canTicket != (_summonCurrencies[idx] == SummonCurrency.Ticket) || _summonCurrencies[idx] == SummonCurrency.None)
                {
                    _summonCurrencies[idx] = canTicket ? SummonCurrency.Ticket : _summonType == SummonType.Necklace ? SummonCurrency.BeadsOre : SummonCurrency.Dia;
                    var currency = Define.SummonCurrencyToCurrency(_summonCurrencies[idx], _summonType);
                    Get<Image>((int)Images.IMG_Cost1 + idx).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(currency).Resource);
                }
            }
        }
        
        private void TrySummon(int summonNumber)
        {
            CurrencyController.I.TrySummon(_summonType, _summonCurrencies[summonNumber], summonNumber);
            SetCurrency();
        }

        private void SetButtonInfo()
        {
            for (var idx = 0; idx < 4; ++idx)
            {
                var summon = DbSummonProduct.Get(_summonType, idx);
                Get<TextMeshProUGUI>((int)Texts.T_CostSummon1 + idx).text = summon.GetNeed(_summonCurrencies[idx] != SummonCurrency.Ticket).ToString("N0");
                Get<TextMeshProUGUI>((int)Texts.T_Summon1Count + idx).text = string.Format(LocalString.Get(210097), summon.Counts);
                Get<Image>((int)Images.B_Summon1 + idx).gameObject.SetActive(true);
            }
        }
        
        private void EnablingButton()
        {
            Get<Image>((int)Images.B_Summon1).material = Define.GetUIMaterial(!Summon1Condition());
            Get<Image>((int)Images.B_Summon2).material = Define.GetUIMaterial(!Summon2Condition());
            var canSummon3 = Summon3Condition();
            var canSummon4 = Summon4Condition();
            Get<Image>((int)Images.B_Summon3).material = Define.GetUIMaterial(!canSummon3);
            Get<Image>((int)Images.B_Summon4).material = Define.GetUIMaterial(!canSummon4);
            Get<UIShiny>((int) UIShines.B_Summon4).enabled = canSummon4;
            Get<UIShiny>((int) UIShines.B_Summon3).enabled = canSummon3;
            Get<Image>((int) Images.B_Close).material = Define.GetUIMaterial(!CanClose());
        }

        private bool Summon1Condition()
        {
            return !_isSummoning && !_isContinue && Condition(0);
        }

        private bool Summon2Condition()
        {
            return !_isSummoning && Condition(1);
        }

        private bool Summon3Condition()
        {
            return !_isSummoning && Condition(2);
        }

        private bool Summon4Condition()
        {
            return !_isSummoning && Condition(3);
        }
        
        private bool Condition(int idx)
        {
            return CurrencyController.I.CheckSummonCurrency(_summonType, _summonCurrencies[idx], idx);
        }

        public void SetResult(List<IDbCanSummon> summons, int summonNumber, SummonType summonType, SummonCurrency summonCurrency)
        {
            if (!_isInit) Init();

            _isSummoning = true;
            _recentSummonNumber = summonNumber;
            _summonType = summonType;
            _isSummonWithMoney = summonCurrency != SummonCurrency.Ticket;
            Get<Image>((int) Images.IMG_Ticket).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(Define.SummonTypeToTicket(_summonType)).Resource);
            Get<Image>((int) Images.IMG_Currency).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get
                (_summonType == SummonType.Necklace ? CurrencyType.BeadsOre : CurrencyType.Dia).Resource);
            AdjustSummonCurrency();
            EnablingButton();
            SetButtonInfo();
            SetCurrency();
            _animator.Play(summonNumber == 0 ? _summon11 : _summon33);
            var useCount = summons.Count;
            var time = 0f;
            if (summonNumber < 2)
            {
                for (var idx = 0; idx < useCount; ++idx)
                {
                    time = _summoned[idx].SetInfo(summons[idx], summonType, 0, time);
                }
            }
            else
            {
                var dic = ListToDictionary(summons);
                useCount = dic.Count;
                var idx = 0;
                foreach (var d in dic)
                {
                    time = _summoned[idx].SetInfo(d.Key, summonType, d.Value, time);
                    idx++;
                }
            }
            Disabling(useCount, time);
        }
        
        private void SetCurrency()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Currency).text = Define.AddUnit(
                CurrencyController.I.GetMoneyModel(_summonType == SummonType.Necklace ? CurrencyType.BeadsOre : CurrencyType.Dia).Value, 3, 2);
            Get<TextMeshProUGUI>((int) Texts.T_Ticket).text = Define.AddUnit(
                CurrencyController.I.GetTicketModel(Define.SummonTypeToTicket(_summonType)).Value, 3, 2);
        }

        private void Disabling(int from, float time)
        {
            Timing.CallDelayed(time + 1, () =>
            {
                _isSummoning = false;
                if (_isContinue)
                {
                    var isSuccess = CurrencyController.I.TrySummon(_summonType, _isSummonWithMoney ? 
                        _summonType == SummonType.Necklace ? SummonCurrency.BeadsOre : SummonCurrency.Dia : SummonCurrency.Ticket, _recentSummonNumber);
                    if (!isSuccess)
                    {
                        ChangeContinue();
                    }
                    else
                    {
                        EnablingButton();
                    }
                }
                else EnablingButton();
            });

            var totalCount = _summoned.Count;
            for (var idx = from; idx < totalCount; ++idx)
            {
                _summoned[idx].gameObject.SetActive(false);
            }
        }

        private Dictionary<T, int> ListToDictionary<T>(List<T> list)
        {
            var dic = new Dictionary<T, int>();
            foreach (var element in list)
            {
                if (dic.ContainsKey(element)) dic[element]++;
                else dic.Add(element, 1);
            }

            return dic;
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }

        private Vector3 _shakePower = new Vector3(50, 50, 0);
        public void Shake(float time)
        {
            _shakeT.DOKill();
            _shakeT.localPosition = Define.Zero3;
            _shakeT.DOShakePosition(time, _shakePower, 50, 10).OnComplete(() => _shakeT.localPosition = Define.Zero3).SetEase(Ease.Linear);
        }
    }
}