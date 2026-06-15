using System;
using System.Collections.Generic;
using Controller;
using Controller.Play;
using Data;
using Data.DbCharacter;
using Data.DbDefinition;
using Data.DbPromote;
using Data.DbRecord;
using Data.DbUser;
using Data.DbUser.Progress;
using Data.Utils;
using Managers;
using TMPro;
using UIBases;
using UIs.Character.Ability;
using UIs.Character.Level;
using UIs.Character.Promotion;
using UIs.Character.Stat;
using UIs.Character.Title;
using UIs.Etc;
using UIs.FieldMain;
using UIs.Lock;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Character
{
    public class UI_Character: UI_Popup
    {
        private Images? _curOpened = null;
        
        private Sprite[] _tabSprites; // 0: not selected 1: selected
        
        private UI_Ability _abilityUI;

        public UIField<bool> Check = new(false);
        public int CurOpened => (int) _curOpened;

        private Vector3 _positionSetter = new Vector3();
        public bool IsChangingAbility() => _abilityUI.isChanging;

        enum GameObjects // GameObjects
        {
            V_Stat,
            V_Level,
            V_Title,
            V_Promotion,
            V_Ability
        }

        public enum Images
        {
            B_StatTab,
            B_LevelTab,
            B_TitleTab,
            B_PromotionTab,
            B_AbilityTab
        }
        

        enum Transforms
        {
            B_StatTab,
            B_LevelTab,
            B_TitleTab,
            B_PromotionTab,
            B_AbilityTab
        }

        void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            Bind<Transform>(typeof(Transforms));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));


            Util.FindChild(gameObject, "Level", true).GetOrAddComponent<UI_Level>();
            Util.FindChild(gameObject, "MyTitle", true).GetOrAddComponent<UI_Title>();
            Util.FindChild(gameObject, "V_Promotion", true).GetOrAddComponent<UI_Promotion>();
            _abilityUI = Util.FindChild(gameObject, "V_Ability", true).GetOrAddComponent<UI_Ability>();
            Util.FindChild(gameObject, "UI_StatUpCount", true).GetOrAddComponent<UI_StatUpCount>();
            Util.FindChild(gameObject, "UI_LevelUpCount", true).GetOrAddComponent<UI_LevelUpCount>();
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            
            _tabSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

            var statParent = Util.FindChild<Transform>(gameObject, "StatParent", true);
            var levelParent = Util.FindChild<Transform>(gameObject, "LevelParent", true);
            var titleParent = Util.FindChild<Transform>(gameObject, "TitleParent", true);
            var promotionParent = Util.FindChild<Transform>(gameObject, "PromotionParent", true);
            
            
            Get<GameObject>((int)GameObjects.V_Level).SetActive(false);
            Get<GameObject>((int)GameObjects.V_Title).SetActive(false);
            Get<GameObject>((int)GameObjects.V_Promotion).SetActive(false);
            Get<GameObject>((int)GameObjects.V_Ability).SetActive(false);
            
            DbGoldStat.ForEach(
                stat =>
                {
                    var item = Manager.UI.MakeSubItem<UI_Stat_Item>(statParent);
                    item.SetInfo(stat);
                    item.gameObject.name += "_" + stat.Id;
                });
            
            DbLevelPoint.ForEach(
                levelPoint =>
                {
                    var item = Manager.UI.MakeSubItem<UI_LevelPoint_Item>(levelParent);
                    item.SetInfo(levelPoint);
                });

            DbTitle.ForEach(
                title => DbUserTitle.Get(title.Value.Id).GetOrder(),
                title =>
                {
                    var item = Manager.UI.MakeSubItem<UI_Title_Item>(titleParent);
                    item.SetInfo(title);
                });
            
            DbPromotion.ForEach(p => p.IsOnUI,
                p =>
                {
                    var item = Manager.UI.MakeSubItem<UI_Promotion_Item>(promotionParent);
                    item.SetInfo(p);
                });
            
            
            foreach (Images tab in Enum.GetValues(typeof(Images)))
            {
                if (tab == Images.B_LevelTab)
                {
                    var obj = Get<Image>((int)tab).gameObject;
                    obj.GetOrAddComponent<UI_Locked>().Set(LockType.Level, obj.GetComponent<Image>(), Util.FindChild(obj, "IMG_LockIcon"), null,
                        () => obj.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce));
                }
                else if (tab == Images.B_AbilityTab)
                {
                    var obj = Get<Image>((int)tab).gameObject;
                    obj.GetOrAddComponent<UI_Locked>().Set(LockType.Ability, obj.GetComponent<Image>(), Util.FindChild(obj, "IMG_LockIcon"), null,
                        () => obj.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce));
                }
                else
                {
                    Get<Image>((int)tab).gameObject.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce);
                }
            }
            
            SetBadges();
            OnTabClicked(Images.B_StatTab);

            void SetBadges()
            {
                Util.FindChild(gameObject, "IMG_StatBadge", true).AddComponent<UI_Badge>()
                    .Set(new DbField[] {BadgeController.data.Stats}, BadgeController.I.IsStatBadgeOn);
                Util.FindChild(gameObject, "IMG_LevelBadge", true).AddComponent<UI_Badge>()
                    .Set(new DbField[] {BadgeController.data.LevelPoint, BadgeController.data.LevelUp},
                        () => BadgeController.data.LevelPoint.Value || BadgeController.data.LevelUp.Value, LockType.Level);
                Util.FindChild(gameObject, "IMG_TitleBadge", true).AddComponent<UI_Badge>()
                    .Set( new[] {BadgeController.data.Title}, () => BadgeController.data.Title.Value);
                Util.FindChild(gameObject, "IMG_PromotionBadge", true).AddComponent<UI_Badge>()
                    .Set( new[] {BadgeController.data.Promotion}, () => BadgeController.data.Promotion.Value);
            }
            
            return true;
        }

        public void OpenTab(Images btn)
        {
            if (!_isInit) Init();
            OnTabClicked(btn);
        }

        private void OnTabClicked(Images clicked)
        {
            if (_curOpened == clicked)
            {
                return;
            }

            Check.Value = !Check.Value;

            if (_curOpened != null)
            {
                _positionSetter = Get<Transform>((int) _curOpened).localPosition;
                _positionSetter.y = 22.62f;
                Get<Transform>((int)_curOpened).localPosition = _positionSetter;
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
                CloseTab(_curOpened);
            }
            
            _curOpened = clicked; 
            _positionSetter = Get<Transform>((int) _curOpened).localPosition;
            _positionSetter.y = 9.82f;
            Get<Transform>((int) clicked).localPosition = _positionSetter;
            Get<Image>((int) clicked).sprite = _tabSprites[1];
            OpenTab(clicked);
            
            void OpenTab(Images tab)
            {
                switch (tab)
                {
                    case Images.B_StatTab:
                        Get<GameObject>((int)GameObjects.V_Stat).SetActive(true);
                        break;
                    case Images.B_LevelTab:
                        Get<GameObject>((int)GameObjects.V_Level).SetActive(true);
                        break;
                    case Images.B_TitleTab:
                        Get<GameObject>((int)GameObjects.V_Title).SetActive(true);
                        break;
                    case Images.B_PromotionTab:
                        Get<GameObject>((int)GameObjects.V_Promotion).SetActive(true);
                        break;
                    case Images.B_AbilityTab:
                        Get<GameObject>((int)GameObjects.V_Ability).SetActive(true);
                        break;
                }
            }
            
            void CloseTab(Images? tab)
            {
                switch (tab)
                {
                    case Images.B_StatTab:
                        Get<GameObject>((int)GameObjects.V_Stat).SetActive(false);
                        break;
                    case Images.B_LevelTab:
                        Get<GameObject>((int)GameObjects.V_Level).SetActive(false);
                        break;
                    case Images.B_TitleTab:
                        Get<GameObject>((int)GameObjects.V_Title).SetActive(false);
                        break;
                    case Images.B_PromotionTab:
                        Get<GameObject>((int)GameObjects.V_Promotion).SetActive(false);
                        break;
                    case Images.B_AbilityTab:
                        Get<GameObject>((int)GameObjects.V_Ability).SetActive(false);
                        break;
                }
            }
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
            Manager.UI.GetSceneUI<UI_MainBottom>().CloseInnerPopup();
        }
    }
}