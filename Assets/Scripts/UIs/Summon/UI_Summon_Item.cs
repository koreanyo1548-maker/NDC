using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbSummon;
using Data.Stores;
using Data.Utils;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Utils;
using UIs.Lock;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Summon
{
    public class UI_Summon_Item: UI_Base, ILanguageSet
    {
        private EventsManager _currencyEventHandler;
        private EventsManager _ticketEventHandler;
        private EventsManager _expEventHandler;
        private SummonType _summonType;

        private List<SummonCurrency> _summonCurrencies = new() {SummonCurrency.None, SummonCurrency.None};

        enum Animators
        {
            B_Reward
        }
        
        enum Images
        {
            B_Summon1,
            B_Summon2,
            B_SummonAd,
            B_Reward,
            IMG_Exp,
            IMG_Cost1,
            IMG_Cost2
        }
        
        enum Texts
        {
            T_CostSummon1,
            T_CostSummon2,
            T_CostSummonAd,
            T_Summon1Count,
            T_Summon2Count,
            T_SummonAdCount,
            T_Level,
            T_Exp
        }

        enum Shinies
        {
            B_Reward
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Animator>(typeof(Animators));
            Bind<UIShiny>(typeof(Shinies));

            if (_summonType == SummonType.Weapon)
            {
                Get<Image>((int)Images.B_SummonAd).gameObject.BindEvent(() => Condition(-1), _ => Summon(-1), UIEffectType.Bounce);
                Get<Image>((int)Images.B_Summon1).gameObject.BindEvent(() => Condition(0), _ => Summon(0), UIEffectType.Bounce);
                Get<Image>((int)Images.B_Summon2).gameObject.BindEvent(() => Condition(1), _ => Summon(1), UIEffectType.Bounce);
                
                SetButtonEnable();
            }
            else
            {
                var lockType = GetLockType();

                var summonAd = Get<Image>((int) Images.B_SummonAd).gameObject;
                summonAd.GetOrAddComponent<UI_Locked>().Set(lockType, summonAd.GetComponent<Image>(), Util.FindChild(summonAd, "IMG_LockIcon"), null,
                    () => summonAd.BindEvent(() => Condition(-1), _ => Summon(-1), UIEffectType.Bounce));
                var summon1 = Get<Image>((int) Images.B_Summon1).gameObject;
                summon1.GetOrAddComponent<UI_Locked>().Set(lockType, summon1.GetComponent<Image>(), Util.FindChild(summon1, "IMG_LockIcon"), null,
                    () => summon1.BindEvent(() => Condition(0), _ => Summon(0), UIEffectType.Bounce));
                var summon2 = Get<Image>((int) Images.B_Summon2).gameObject;
                summon2.GetOrAddComponent<UI_Locked>().Set(lockType, summon2.GetComponent<Image>(), Util.FindChild(summon2, "IMG_LockIcon"), null,
                    () =>
                    {
                        summon2.BindEvent(() => Condition(1), _ => Summon(1), UIEffectType.Bounce);
                        SetButtonEnable();
                    });
            }

            if (HaveBonus())
            {
                Get<Image>((int)Images.B_Reward).gameObject.BindEvent(RewardCondition, _ => GetReward(), UIEffectType.Bounce, false);
                Util.FindChild(gameObject, "B_Bonus", true).BindEvent(Functions.TrueCondition, _ => OpenBonusInfo(), UIEffectType.Bounce);
            }
            Util.FindChild(gameObject, "B_Probability", true).BindEvent(Functions.TrueCondition, _ => OpenProbabilityInfo(), UIEffectType.Bounce);
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            return true;

            void SetButtonEnable()
            {
                WhenCurrencyChanged();
                _currencyEventHandler = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenCurrencyChanged,
                    updatedField = new DbField[] {CurrencyController.I.GetMoneyModel(CurrencyType.Dia), 
                        CurrencyController.I.GetTicketModel(Define.SummonTypeToTicket(_summonType)),
                        CurrencyController.I.GetTicketModel(Define.SummonTypeToAdTicket(_summonType))
                    }
                });
            }

            LockType GetLockType()
            {
                switch (_summonType)
                {
                    case SummonType.Skill : return LockType.SkillSummon;
                    case SummonType.Accessory : return LockType.AccessorySummon;
                    case SummonType.Relic : return LockType.Relic;
                    case SummonType.Necklace: return LockType.Necklace;
                    default: throw new Exception("소환타입 " + _summonType + " 에 정의된 LockType이 없습니다.");
                } 
            }
        }

        private bool HaveBonus()
        {
            return _summonType == SummonType.Weapon || _summonType == SummonType.Accessory ||
                   _summonType == SummonType.Skill;
        }

        private bool Condition(int summonTypeIdx)
        {
            return CurrencyController.I.CheckSummonCurrency(_summonType, summonTypeIdx == -1 ? SummonCurrency.None : _summonCurrencies[summonTypeIdx], summonTypeIdx);
        }

        private bool RewardCondition()
        {
            return LevelController.I.CanGetSummonReward(_summonType);
        }

        private void GetReward()
        {
            LevelController.I.GetSummonLevelReward(_summonType);
        }

        public void SetInfo(SummonType summonType)
        {
            _summonType = summonType;
            
            if (!_isInit) Init();
            
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text = StringMaker.GetSummonName(_summonType);
            
            var summon1 = DbSummonProduct.Get(_summonType, 0);
            var summon2 = DbSummonProduct.Get(_summonType, 1);
            Get<TextMeshProUGUI>((int)Texts.T_Summon1Count).text = string.Format(LocalString.Get(210097), summon1.Counts);
            Get<TextMeshProUGUI>((int)Texts.T_Summon2Count).text = string.Format(LocalString.Get(210097),summon2.Counts);
            Get<TextMeshProUGUI>((int) Texts.T_SummonAdCount).text = string.Format(LocalString.Get(210097), DbSummonProduct.Get(_summonType, 4).Counts);

            if (summonType != SummonType.Relic)
            {
                _expEventHandler = new EventsManager(this, new EventsManager.Config
                {
                    handler = WhenSummonExpChanged,
                    updatedField = ExpField()
                });
                WhenSummonExpChanged();
            }

            DbField[] ExpField()
            {
                switch (_summonType)
                {
                    case SummonType.Weapon: return new DbField[] {LevelController.data.WeaponSummonExp,  LevelController.data.WeaponSummonReward};
                    case SummonType.Accessory: return new DbField[] {LevelController.data.AccessorySummonExp,  LevelController.data.AccessorySummonReward};
                    case SummonType.Skill: return new DbField[] {LevelController.data.SkillSummonExp,  LevelController.data.SkillSummonReward};
                    case SummonType.Necklace: return new DbField[] {LevelController.data.NecklaceSummonExp};
                    default: throw new Exception("소환타입 " + _summonType + " 에 정의된 경험치가 없습니다.");
                }
            }
        }

        private void CheckSummonCurrency()
        {
            for (var idx = 0; idx < 2; ++idx)
            {
                var canTicket = CurrencyController.I.CheckSummonCurrency(_summonType, SummonCurrency.Ticket, idx);
                if (canTicket != (_summonCurrencies[idx] == SummonCurrency.Ticket) || _summonCurrencies[idx] == SummonCurrency.None)
                {
                    _summonCurrencies[idx] = canTicket ? SummonCurrency.Ticket : _summonType == SummonType.Necklace ? SummonCurrency.BeadsOre : SummonCurrency.Dia;
                    var summon = DbSummonProduct.Get(_summonType, idx);
                    Get<TextMeshProUGUI>((int)Texts.T_CostSummon1 + idx).text = 
                        summon.GetNeed(_summonCurrencies[idx] != SummonCurrency.Ticket).ToString("N0");
                    Get<Image>((int) Images.IMG_Cost1 + idx).sprite = Manager.Resource.Load<Sprite>(DbCurrency.Get(
                        Define.SummonCurrencyToCurrency(_summonCurrencies[idx],_summonType)).Resource);
                }
            }
        }
        
        private void WhenCurrencyChanged()
        {
            CheckSummonCurrency();
            Get<Image>((int) Images.B_SummonAd).material = Define.GetUIMaterial(!Condition(-1));
            Get<Image>((int)Images.B_Summon1).material = Define.GetUIMaterial(!Condition(0));
            Get<Image>((int)Images.B_Summon2).material = Define.GetUIMaterial(!Condition(1));

            var adCost = Define.SummonTypeToAdTicket(_summonType);
            Get<TextMeshProUGUI>((int) Texts.T_CostSummonAd).text =
                CurrencyController.I.GetTicketModel(adCost).Value + "/" + DbCurrency.Get(adCost).MaxHave;
        }

        private void WhenSummonExpChanged()
        {
            var need = GetNeed();

            var cur = LevelController.I.GetSummonExp(_summonType);
            
            Get<TextMeshProUGUI>((int) Texts.T_Level).text = string.Format(LocalString.Get(210041), LevelController.I.GetSummonLevel(_summonType));
            if (need == 0)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Exp).text = string.Empty;
                Get<Image>((int) Images.IMG_Exp).fillAmount = 1;
            }
            else
            {
                Get<TextMeshProUGUI>((int) Texts.T_Exp).text = Define.AddUnit(cur, 3, 2) + "/" + Define.AddUnit(need, 3, 2);
                Get<Image>((int) Images.IMG_Exp).fillAmount = (float)cur / need;
            }
            
            if (_summonType != SummonType.Necklace)
            {
                var can = RewardCondition();
                Get<Image>((int) Images.B_Reward).material = Define.GetUIMaterial(!can);
                Get<Animator>((int) Animators.B_Reward).enabled = can;
                Get<UIShiny>((int) Shinies.B_Reward).enabled = can;
            }
            
            int GetNeed()
            {
                switch (_summonType)
                {
                    case SummonType.Weapon:
                    case SummonType.Accessory: return LevelController.I.GetNextSummonLevelMeta(_summonType).NeedExp;
                    case SummonType.Skill: return LevelController.I.GetNextSummonLevelMeta(_summonType).SkillNeedExp;
                    case SummonType.Necklace: return LevelController.I.GetNextSummonLevelMeta(_summonType).NecklaceNeedExp;
                    default: throw new Exception("소환타입 " + _summonType + " 에 정의된 경험치가 없습니다.");
                }
            }
        }

        private void Summon(int summonNumber)
        {
            var currency = summonNumber == -1 ? SummonCurrency.None : _summonCurrencies[summonNumber];
            if (currency == SummonCurrency.None)
            {
                Manager.Ad.ShowAd(eAdType.Summon, () => Timing.CallDelayed(0.1f, DoSummon));
            }
            else
            {
                DoSummon();
            }

            void DoSummon()
            {
                CurrencyController.I.TrySummon(_summonType, currency, summonNumber);
            }
        }
        
        private void OpenBonusInfo()
        {
            if (_summonType == SummonType.Relic) return;
            Manager.UI.ShowPopupUI<UI_SummonBonus>().Set(_summonType);
        }

        private void OpenProbabilityInfo()
        {
            Manager.UI.ShowPopupUI<UI_SummonProbability>().Set(_summonType);
        }
        
        private void OnDisable()
        {
            _currencyEventHandler?.Dispose();
            _expEventHandler?.Dispose();
        }

        private void OnEnable()
        {
            _currencyEventHandler?.Reconnect();
            _expEventHandler?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Name").text = StringMaker.GetSummonName(_summonType);
            var summon1 = DbSummonProduct.Get(_summonType, 0);
            var summon2 = DbSummonProduct.Get(_summonType, 1);
            Get<TextMeshProUGUI>((int)Texts.T_Summon1Count).text = string.Format(LocalString.Get(210097), summon1.Counts);
            Get<TextMeshProUGUI>((int)Texts.T_Summon2Count).text = string.Format(LocalString.Get(210097),summon2.Counts);
        }
    }
}