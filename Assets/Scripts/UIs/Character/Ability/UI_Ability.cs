using System;
using System.Collections.Generic;
using Controller.Currency;
using Controller.Have;
using Data;
using Data.DbAbility;
using Data.DbDefinition;
using Data.DbUser.Equipment;
using Managers;
using MEC;
using TMPro;
using UIBases;
using UIs.Etc;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Ability
{
    public class UI_Ability: UI_Base
    {
        private Sprite[] _autoChangeBtnSprites; // 0: 불가능, 1: 가능

        private List<UI_Ability_Item> _items = new();
        private bool _hasNothing;
        public bool isChanging;

        private CoroutineHandle _rechangeHandler;
        
        private EventsManager _passionChangeEventManager;
        private EventsManager _presetChangeEventManager;
        
        enum GameObjects
        {
            G_Ability,
            T_NoAbility,
            IMG_TimeIcon,
            T_AutoChanging,
            BlockTouchWhenAutoChange
        }

        enum Animators
        {
            B_FreePassion
        }
        enum Images
        {
            B_AutoChange,
            B_Change,
            IMG_IconGauge
        }

        enum Texts
        {
            T_Time,
            T_Count,
            T_AutoChange
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<Animator>(typeof(Animators));
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));

            _autoChangeBtnSprites = new []
            {
                Manager.Resource.Load<Sprite>("UI_NoButton_round"),
                Manager.Resource.Load<Sprite>("UI_HighlightButton_round")
            };

            _passionChangeEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = SetPassionCount,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Passion)},
                updatedController = new[] {AbilityController.I.changeCost}
            });

            _presetChangeEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = SetNothing,
                updatedController = new[] {AbilityController.I.preset}
            });
            
            
            for (var idx = 1; idx <= 5; ++idx)
            {
                var curIdx = idx -1;
                Util.FindChild(gameObject,"B_AbilityPreset" + idx, true).GetOrAddComponent<UI_AbilityPreset_Item>().Set(curIdx);
            }
            
            var abilityItemParent = Util.FindChild<Transform>(gameObject, "G_Ability", true);
            for (var idx = 0; idx < 5; ++idx)
            {
                _items.Add(abilityItemParent.GetChild(idx).gameObject.GetOrAddComponent<UI_Ability_Item>());
                _items[idx].Set(idx);
            }

            var runeParent = Util.FindChild<Transform>(gameObject, "G_Rune", true);
            runeParent.gameObject.BindEvent(Functions.TrueCondition, _ => OpenRuneInfo(), UIEffectType.Bounce);
            for (var idx = 0; idx < 5; ++idx)
            {
                var rune = DbAbilityRune.GetAtIndex(idx);
                var levelT = runeParent.GetChild(idx).Find("T_Level");
                AbilityController.I.runeLevel[rune].ValueChanged += (_, _) =>
                {
                    SetLevelText();
                };
                SetLevelText();

                void SetLevelText()
                {
                    levelT.GetComponent<TextMeshProUGUI>().text = 
                        string.Format(LocalString.Get(210041), AbilityController.I.runeLevel[rune].Value);
                }
            }
            
            Get<Image>((int)Images.B_Change).gameObject.BindEvent(CanChange, _ => StartChangeOnce(), UIEffectType.Bounce);
            Get<Image>((int)Images.B_AutoChange).gameObject.BindEvent(CanChange, _ => AutoChange(), UIEffectType.Bounce);
            
            Get<GameObject>((int)GameObjects.BlockTouchWhenAutoChange).SetActive(false);
            Get<GameObject>((int)GameObjects.BlockTouchWhenAutoChange).transform.Find("Block").gameObject.BindEvent(Functions.TrueCondition, _ => ShowBlockToast());

            Get<Animator>((int)Animators.B_FreePassion).gameObject.BindEvent(GetPassionCondition, _ => GetPassion(), UIEffectType.Bounce);
            
            Util.FindChild(gameObject, "B_Info", true).BindEvent(Functions.TrueCondition, _ =>
            {
                Manager.UI.ShowPopupUI<UI_AbilityProbability>();
            });
            
            Set();
            
            return true;
        }

        private void Set()
        {
            SetNothing();
            SetPassion();
            SetPassionCount();
        }

        private void SetNothing()
        {
            var curPreset = AbilityController.I.preset.Value;
            _hasNothing = DbUserAbility.Get(curPreset * 5).Option.Value == StatType.None;
            Get<GameObject>((int)GameObjects.G_Ability).SetActive(!_hasNothing);
            Get<GameObject>((int) GameObjects.T_NoAbility).SetActive(_hasNothing);
        }

        private void GetPassion()
        {
            CurrencyController.I.GetFreePassion();
            SetPassion();
        }

        private bool GetPassionCondition()
        {
            return CurrencyController.data.PassionLeftTime <= 0;
        }

        private void SetPassion()
        {
            var shouldWaitForPassion = CurrencyController.data.PassionLeftTime > 0;
            Get<GameObject>((int)GameObjects.IMG_TimeIcon).SetActive(shouldWaitForPassion);
            Get<Animator>((int) Animators.B_FreePassion).enabled = !shouldWaitForPassion;
            Get<Image>((int) Images.IMG_IconGauge).color = shouldWaitForPassion ? Define.ColorA1A1A1 : Color.white;
            if (shouldWaitForPassion)
            {
                Timing.RunCoroutine(_PassionTimeRoutine().CancelWith(gameObject));
            }
            else
            {
                Get<TextMeshProUGUI>((int) Texts.T_Time).text = string.Empty;
                Get<Image>((int) Images.IMG_IconGauge).fillAmount = 1;
            }
        }

        private IEnumerator<float> _PassionTimeRoutine()
        {
            var data = CurrencyController.data;
            while (data.PassionLeftTime > 0)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Time).text = StringMaker.GetTimeString(data.PassionLeftTime);
                var percent = 1 - data.PassionLeftTime / 28800f;
                Get<Image>((int) Images.IMG_IconGauge).fillAmount = percent;
                yield return Timing.WaitForSeconds(1);
            }
        }

        private void SetPassionCount()
        {
            var have = CurrencyController.I.GetMoneyModel(CurrencyType.Passion).Value;
            var need = AbilityController.I.changeCost.Value;
            Get<TextMeshProUGUI>((int) Texts.T_Count).text = Define.AddUnit(have, 3, 2) + "/" + need;
            Get<Image>((int) Images.B_AutoChange).material = Define.GetUIMaterial(have < need);
            Get<Image>((int) Images.B_Change).material = Define.GetUIMaterial(have < need);
        }

        private void AutoChange()
        {
            if (isChanging) FinishAutoChange();
            else StartAutoChange();
        }

        private void StartAutoChange(bool checkCondition = true)
        {
            if (checkCondition && !CheckChangeCondition(true)) return;
            isChanging = true;
            Manager.Input.blockBackKey = true;
            Get<GameObject>((int)GameObjects.BlockTouchWhenAutoChange).SetActive(true);
            Get<Image>((int)Images.B_AutoChange).transform.SetParent(Get<GameObject>((int)GameObjects.BlockTouchWhenAutoChange).transform);
            Get<Image>((int) Images.B_AutoChange).sprite = _autoChangeBtnSprites[0];
            Get<TextMeshProUGUI>((int) Texts.T_AutoChange).text = LocalString.Get(210355);
            Get<GameObject>((int)GameObjects.T_AutoChanging).SetActive(true);
            Get<Image>((int) Images.B_Change).material = Define.GetUIMaterial(true);

            Change(true);
        }

        private void FinishAutoChange()
        {
            Timing.KillCoroutines(_rechangeHandler);
            isChanging = false;
            Manager.Input.blockBackKey = false;
            Get<GameObject>((int)GameObjects.BlockTouchWhenAutoChange).SetActive(false);
            Get<Image>((int)Images.B_AutoChange).transform.SetParent(transform);
            Get<Image>((int) Images.B_AutoChange).sprite = _autoChangeBtnSprites[1];
            Get<TextMeshProUGUI>((int) Texts.T_AutoChange).text = LocalString.Get(210349);
            Get<GameObject>((int)GameObjects.T_AutoChanging).SetActive(false);
            Get<Image>((int) Images.B_Change).material = Define.GetUIMaterial(!CanChange());
        }

        private void StartChangeOnce(bool checkCondition = true)
        {
            if (checkCondition && !CheckChangeCondition(false)) return;
            Change(false);
        }

        /// <returns> true: can change </returns>
        private bool CheckChangeCondition(bool isAuto)
        {
            if (isChanging) return false;
            
            if (AbilityController.I.IsAllLocked())
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200060);
                return false;
            }
            
            if (!isAuto && AbilityController.I.HaveUnlockedHigh())
            {
                Manager.UI.ShowPopupUI<UI_AbilityChangeCheckPopup>().Set(210356, () => StartChangeOnce(false));
                return false;
            }

            if (isAuto)
            {
                Manager.UI.ShowPopupUI<UI_AbilityChangeCheckPopup>().Set(210354, () => StartAutoChange(false));
                return false;
            }

            return true;
        }
        
        private void Change(bool isAuto)
        {
            if (CurrencyController.I.TryUse(CurrencyType.Passion, AbilityController.I.changeCost.Value))
            {
                var doAgain = AbilityController.I.ChangeAbilityAndCheckContinue(isAuto);
                if (_hasNothing)
                {
                    SetNothing();
                    _hasNothing = false;
                }
                foreach(var item in _items) item.Change(isAuto);
                if (doAgain && isChanging && CanChange()) _rechangeHandler = Timing.CallDelayed(0.1f, () => Change(true));
                else if (isChanging && isAuto)
                {
                    Timing.CallDelayed(1.14f, FinishAutoChange);
                }
            }
        }

        private bool CanChange()
        {
            return CurrencyController.I.GetMoneyModel(CurrencyType.Passion).Value >= AbilityController.I.changeCost.Value;
        }

        private void ShowBlockToast()
        {
            Manager.UI.ShowSingleUI<UI_Toast>().SetText(210357);
        }

        private void OpenRuneInfo()
        {
            Manager.UI.ShowPopupUI<UI_Rune>();
        }
        
        private void OnDisable()
        {
            if (_isInit) SetPassion();
            _passionChangeEventManager?.Dispose();
            _presetChangeEventManager?.Dispose();
        }

        private void OnEnable()
        {
            _passionChangeEventManager?.Reconnect();
            _presetChangeEventManager?.Reconnect();
        }
    }
}