using System;
using System.Collections.Generic;
using System.Numerics;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbEvent;
using Data.DbShop;
using Data.DbStage;
using Data.Stores;
using Data.Utils;
using Exceptions;
using Managers;
using TMPro;
using UIBases;
using UIs.Attend;
using UIs.Dungeon;
using UIs.Etc;
using UIs.Etc.Setting;
using UIs.FieldMain.MainStage;
using UIs.Friend;
using UIs.Lock;
using UIs.Mail;
using UIs.Pass;
using UIs.Quest;
using UIs.Ranking;
using UIs.Shop.EventAttend;
using UIs.Shop.BlackMarket;
using UIs.StageMove;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;
using Define = Utils.Define;
using MEC;

namespace UIs.FieldMain
{
    public class UI_MainRight: UI_Scene, ILanguageSet
    {
        //private bool isCheatOpen = false;

        
        private Sprite[] _dungeonSprite; // 0: enter dungeon 1: exit dungeon

        private bool _canAttendRewarded;
        private bool _canMailRewarded;

        private bool _isDungeonLocked = true;

        GameObject UICamera;

        enum GameObjects
        {
            IMG_AutoProgressOff,
            IMG_AutoProgressOn,
            IMG_AutoSkillOn,
            IMG_DungeonLockIcon,
            IMG_BlackMarketLockIcon,
            IMG_QuestBadge,
            IMG_MenuBadge,
            IMG_StageMoveLock,
            IMG_MailBadge,
            IMG_AttendBadge,
            P_SubMenu,
            B_AttendEvent,
            B_BlackMarket,
            IMG_AttendEventBadge
        }

        enum Images
        {
            B_StageMove,
            B_Dungeon
        }

        enum Texts
        {
            T_ProgressOption,
            T_AutoSkill,
            T_Dungeon
        }

        private void Start()
        {
            Init();
        }
        private void Update()
        {
            if (UICamera != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    UICamera.SetActive(true);
                    UICamera = null;
                }
            }
        }
        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<GameObject>(typeof(GameObjects));
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Image>(typeof(Images));
            transform.GetComponent<Canvas>().sortingOrder = 2;
            
            Get<GameObject>((int) GameObjects.IMG_QuestBadge).AddComponent<RepeatingScale>();
            Get<GameObject>((int) GameObjects.IMG_MenuBadge).AddComponent<RepeatingScale>();
            Get<GameObject>((int) GameObjects.IMG_MailBadge).AddComponent<RepeatingScale>();
            Get<GameObject>((int) GameObjects.IMG_AttendBadge).AddComponent<RepeatingScale>();
            Get<GameObject>((int) GameObjects.IMG_AttendEventBadge).AddComponent<RepeatingScale>();

            _dungeonSprite = new[]
                {Manager.Resource.Load<Sprite>("Icon_Dungeon"), Manager.Resource.Load<Sprite>("Icon_DungeonExit")};

            Util.FindChild(gameObject, "B_AutoProgressOnOff", true).BindEvent(Functions.TrueCondition, OnChangeAutoProgress, UIEffectType.Bounce);
            var autoSkill = Util.FindChild(gameObject, "B_AutoSkillOnOff", true);
            autoSkill.GetOrAddComponent<UI_Locked>().Set(LockType.AutoSkill, null,
                autoSkill.transform.Find("IMG_AutoSkillLockIcon").gameObject, null,
                () => autoSkill.BindEvent(Functions.TrueCondition, OnChangeAutoSkill, UIEffectType.Bounce));
            
            var dungeon = Util.FindChild(gameObject, "B_Dungeon", true);
            dungeon.GetOrAddComponent<UI_Locked>().Set(LockType.Dungeon, dungeon.GetComponent<Image>(), Get<GameObject>((int)GameObjects.IMG_DungeonLockIcon), null, 
                () =>
                {
                    dungeon.BindEvent(Functions.TrueCondition, OnDungeonButtonClicked, UIEffectType.Bounce);
                    _isDungeonLocked = false;
                });
            
            Util.FindChild(gameObject, "IMG_DungeonBadge", true).GetOrAddComponent<UI_Badge>()
                .Set(new[] {BadgeController.data.Dungeon}, () => BadgeController.data.Dungeon.Value, LockType.Dungeon);
            
            var blackMarket = Get<GameObject>((int)GameObjects.B_BlackMarket);
                blackMarket.GetOrAddComponent<UI_Locked>().Set(LockType.BlackMarket, blackMarket.GetComponent<Image>(), Get<GameObject>((int)GameObjects.IMG_BlackMarketLockIcon), null,
                    () =>
                    {
                        blackMarket.BindEvent(Functions.TrueCondition, OnBlackMarketButtonClicked, UIEffectType.Bounce);
                    });
            
            Util.FindChild(gameObject, "B_SubMenu", true).BindEvent(Functions.TrueCondition, _ => OpenMenu(true));
            Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _ => OpenMenu(false));
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => OpenMenu(false));
            Util.FindChild(gameObject, "B_Quest", true).BindEvent(Functions.TrueCondition, OnQuestButtonClicked);
            Util.FindChild(gameObject, "B_Setting", true).BindEvent(Functions.TrueCondition, OnSettingButtonClicked);
            Util.FindChild(gameObject, "B_PowerSaving", true).BindEvent(Functions.TrueCondition, OnPowerSavingButtonClicked);
            Util.FindChild(gameObject, "B_Ranking", true).BindEvent(Functions.TrueCondition, OnRankingButtonClicked, UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_StageMove", true).BindEvent(StageMoveCondition, OnStageButtonClicked, UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Mail", true).BindEvent(Functions.TrueCondition, OnMailButtonClicked, UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Attend", true).BindEvent(Functions.TrueCondition, OnAttendButtonClicked, UIEffectType.Bounce);
#if CHEAT
            Util.FindChild(gameObject, "B_Friend", true).BindEvent(Functions.TrueCondition, OnFriendButtonClicked, UIEffectType.Bounce); // <- 임시
            Util.FindChild(gameObject, "B_Cheat", true).BindEvent(Functions.TrueCondition, OnCheatButtonClicked, UIEffectType.Bounce); // <- 임시
#else
            Destroy(Util.FindChild(gameObject, "B_Friend", true));
            Destroy(Util.FindChild(gameObject, "B_Cheat", true));
#endif
            Get<GameObject>((int)GameObjects.B_AttendEvent).BindEvent(Functions.TrueCondition, OnAttendEventButtonClicked, UIEffectType.Bounce);


            SettingController.data.IsAutoProgress.ValueChanged += WhenAutoProgressChanged;
            SettingController.data.IsAutoSkill.ValueChanged += WhenAutoSkillChanged;
            Manager.Field.CurField.ValueChanged += WhenCurFieldChanged;
            CurrencyController.data.Mail.ValueChanged += WhenCanMailChanged;
            BadgeController.data.Attend.ValueChanged += WhenAttendChanged;

            WhenAutoProgressChanged(null, null);
            WhenAutoSkillChanged(null, null);
            WhenCurFieldChanged(null, null);
            WhenCurFieldChanged(null, null);
            WhenBadgeChanged(null, null);
            WhenCanMailChanged(null, null);
            WhenAttendChanged(null, null);
            
            BadgeController.data.Quests.ValueChanged += WhenBadgeChanged;
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            WhenAttendEventIdChanged();
            EventAttendController.data.CurrentId.ValueChanged += (_, _) => WhenAttendEventIdChanged();
            WhenAttendEventBadgeChanged();
            EventAttendController.data.CanRewarded.ValueChanged += (_, _) => WhenAttendEventBadgeChanged();
            

            OpenMenu(false);

            return true;
        }
        
        #region Attend Event

        private void WhenAttendEventIdChanged()
        {
            Get<GameObject>((int) GameObjects.B_AttendEvent).SetActive(EventAttendController.data.CurrentId.Value != 0);
        }

        private void WhenAttendEventBadgeChanged()
        {
            Get<GameObject>((int) GameObjects.IMG_AttendEventBadge).SetActive(EventAttendController.data.CanRewarded.Value);
        }

        private void OnAttendEventButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_Attend7Days>();
            OpenMenu(false);
        }
        
        #endregion
        
        
        // #region Drop Event
        //
        // private DbField<BigInteger> _dropEventMoney;
        // private int _dropShopMinNeed;
        // private void WhenDropCurrencyChanged()
        // {
        //     Get<GameObject>((int)GameObjects.IMG_DropEventBadge).SetActive(_dropEventMoney.Value >= _dropShopMinNeed);
        // }
        //
        // private void WhenDropEventChanged()
        // {
        //     Get<GameObject>((int) GameObjects.B_DropEvent).SetActive(DropEventController.I.CanUseShop.Value);
        // }
        //
        // private void OnDropEventButtonClicked(PointerEventData eventData)
        // {
        //     Manager.UI.ShowPopupUI<UI_EventShop>();
        //     OpenMenu(false);
        // }
        //
        // #endregion

        
        #region BlackMarket

        private void OnBlackMarketButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_BlackMarket>();
            OpenMenu(false);
        }

        #endregion
        
        
        
        #region Mail
        
        private void WhenCanMailChanged(object sender, EventArgs eventArgs)
        {
            CurrencyController.data.Mail.ForEach(m =>
            {
                m.IsRewarded.ValueChanged -= WhenCanMailChanged;
                m.IsRewarded.ValueChanged += WhenCanMailChanged;
            });
            _canMailRewarded = CurrencyController.I.CanGetAnyMailReward();
            Get<GameObject>((int)GameObjects.IMG_MailBadge).SetActive(_canMailRewarded);
            Get<GameObject>((int) GameObjects.IMG_MenuBadge).SetActive(_canAttendRewarded || _canMailRewarded);
        }
        private void OnMailButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_Mail>();
            OpenMenu(false);
        }
        
        #endregion
        
        
        #region Attend
        
        private void WhenAttendChanged(object sender, EventArgs eventArgs)
        {
            _canAttendRewarded = BadgeController.data.Attend.Value;
            Get<GameObject>((int)GameObjects.IMG_AttendBadge).SetActive(_canAttendRewarded);
            Get<GameObject>((int) GameObjects.IMG_MenuBadge).SetActive(_canAttendRewarded || _canMailRewarded);
        }
        

        private void OnAttendButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_Attend>();
            OpenMenu(false);
        }

        #endregion


        #region Friend

        private void OnFriendButtonClicked(PointerEventData eventData)
        {
            //Manager.UI.ShowPopupUI<UI_Friend>();

            if (UICamera == null)
            {
                // 모든 UI hide
                foreach (var c in Camera.allCameras)
                {
                    if (c.name == "UI Camera")
                    {
                        UICamera = c.gameObject;
                        c.gameObject.SetActive(false);
                        break;
                    }
                }
            }

            OpenMenu(false);
        }

        #endregion

        #region Friend

        private void OnCheatButtonClicked(PointerEventData eventData)
        {
            Debug.Log("OnCheatButtonClicked");

            // 스테이지 업
            LevelController.I.OnStageClearAll();

            OpenMenu(false);
        }

        #endregion

        #region Stage & Dungeon Move


        private void OnStageButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_StageMove>().Open();
            OpenMenu(false);
        }
        
        private bool StageMoveCondition()
        {
             return Manager.Field.CurField.Value == FieldType.Stage;
        }
        

        private void WhenCurFieldChanged(object sender, EventArgs eventArgs)
        {
            var curField = Manager.Field.CurField.Value;
            var isInStage = curField == FieldType.Stage;
            var isInDungeon = !isInStage && curField != FieldType.Promotion;
            
            Get<Image>((int)Images.B_Dungeon).material = Define.GetUIMaterial(curField == FieldType.Promotion || _isDungeonLocked);
            Get<Image>((int) Images.B_Dungeon).sprite = _dungeonSprite[isInDungeon ? 1 : 0];
            Get<TextMeshProUGUI>((int) Texts.T_Dungeon).text = LocalString.Get(isInDungeon ? 210251 : 210013);
            
            Get<Image>((int)Images.B_StageMove).material = Define.GetUIMaterial(!isInStage);
            Get<GameObject>((int)GameObjects.IMG_StageMoveLock).SetActive(!isInStage);
        }
        
        private void OnDungeonButtonClicked(PointerEventData eventData)
        {
            var curField = Manager.Field.CurField.Value;
            switch (curField)
            {
                case FieldType.Stage:
                    Manager.UI.ShowPopupUI<UI_Dungeon>();
                    OpenMenu(false);
                    break;
                case FieldType.Promotion:
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200040);
                    break;
                case FieldType.BlackMarket: case FieldType.Dia:
                case FieldType.Awakening: case FieldType.Pet: case FieldType.SkillGrowth: 
                    Manager.UI.ShowPopupUI<UI_DefaultPopup>().Set(210252, () =>
                    {
                        var fade = Manager.UI.ShowSceneUI<UI_Fade>();
                        fade.FadeIn(() =>
                         {
                             Manager.Field.GiveUpDungeon();
                             fade.FadeOut();
                         });
                    });
                    break;
                case FieldType.Training:
                    Manager.UI.ShowPopupUI<UI_DefaultPopup>().Set(210252, () =>
                    {
                        var fade = Manager.UI.ShowSceneUI<UI_Fade>();
                        fade.FadeIn(() =>
                        {
                            Manager.Field.GiveUpDungeon();
                            Manager.UI.GetSceneUI<UI_MainStage>().RemoveTrainingStage();
                            fade.FadeOut();
                        });
                    });
                break;
                default: throw new NotDefinedFieldException(curField);
            }
        }
        
        #endregion
        
        
        #region Auto Progress
        
        private void OnChangeAutoProgress(PointerEventData eventData)
        {
            if (Manager.Field.CurField.Value == FieldType.Stage)
            {
                if (LevelController.data.Stage.Value == DbStageLevel.Count)
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200057);
                }
                else
                {
                    SettingController.I.SetAutoProgress(!SettingController.data.IsAutoProgress.Value);
                }
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200041);
            }
        }

        private void WhenAutoProgressChanged(object sender, EventArgs eventArgs)
        {
            var isAuto = SettingController.data.IsAutoProgress.Value;
            Get<GameObject>((int)GameObjects.IMG_AutoProgressOn).SetActive(isAuto);
            Get<GameObject>((int)GameObjects.IMG_AutoProgressOff).SetActive(!isAuto);
            Get<TextMeshProUGUI>((int) Texts.T_ProgressOption).text = LocalString.Get(isAuto ? 210015 : 210014);
        }
        
        #endregion
        
        
        #region AutoSkill

        private void OnChangeAutoSkill(PointerEventData eventData)
        {
            SettingController.I.ChangeAutoSkill();
        }


        private void WhenAutoSkillChanged(object sender, EventArgs eventArgs)
        {
            var isAuto = SettingController.data.IsAutoSkill.Value;
            Get<GameObject>((int)GameObjects.IMG_AutoSkillOn).SetActive(isAuto);
            Get<TextMeshProUGUI>((int) Texts.T_AutoSkill).color = isAuto ? Define.ColorFFF8AA : Define.ColorE4E4F1;
        }
        #endregion
        

        #region Menu
        
        private void OpenMenu(bool isOpen)
        {
            Get<GameObject>((int)GameObjects.P_SubMenu).SetActive(isOpen);
        }
        
        private void OnQuestButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_Quest>();
            OpenMenu(false);
        }

        private void OnSettingButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_Setting>();
            OpenMenu(false);
        }
        
        private void OnRankingButtonClicked(PointerEventData eventData)
        {
            Manager.UI.ShowPopupUI<UI_Ranking>();
            OpenMenu(false);
        }
        
        private void WhenBadgeChanged(object sender, EventArgs eventArgs)
        {
            Get<GameObject>((int) GameObjects.IMG_QuestBadge).SetActive(BadgeController.I.IsQuestBadgeOn());
            //Get<GameObject>((int) GameObjects.IMG_MenuBadge).SetActive(BadgeController.I.IsQuestBadgeOn());
        }
        
        private void OnPowerSavingButtonClicked(PointerEventData eventData)
        {
            Manager.UI.SetPowerSavingTimer(true);
            Manager.UI.StartPowerSaving();
        }

        #endregion

        

        public override bool NeedRaycast()
        {
            return true;
        }

        public void OnLanguageChanged(Locale locale)
        {
            WhenCurFieldChanged(this, null);
            WhenAutoProgressChanged(this, null);
        }
    }
}