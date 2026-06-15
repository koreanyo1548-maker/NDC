using System;
using Controller;
using Controller.Play;
using Data;
using Data.Utils;
using UIBases;
using UIs.Character;
using UIs.Skill;
using UIs.Summon;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using Managers.Base;
using UIs.Etc;
using UIs.Lock;
using UIs.Pet;
using UIs.Shop;
using UIs.Shop.DefaultShop;
using UIs.Toast;
using UIs.Utils;
using Utils;
using UI_Inventory = UIs.Inventory.UI_Inventory;

namespace UIs.FieldMain
{
    public class UI_MainBottom: UI_Scene
    {
        private Canvas _canvas;
        
        private GameObjects? _curOpened = null;
        

        public enum GameObjects
        {
            B_Shop,
            B_Inventory,
            B_Character,
            B_Summon,
            B_Skill,
            B_Pet,
            B_Close
        }

        enum Transforms
        {
            B_Shop,
            B_Inventory,
            B_Character,
            B_Summon,
            B_Skill,
            B_Pet,
            B_Close
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            _canvas = GetComponent<Canvas>();
            Bind<GameObject>(typeof(GameObjects));
            Bind<Transform>(typeof(Transforms));
            

            CheckLock(GameObjects.B_Character, LockType.Character, "T_CharacterName");
            CheckLock(GameObjects.B_Inventory, LockType.Inventory, "T_InventoryName");
            CheckLock(GameObjects.B_Summon, LockType.Summon, "T_SummonName");
            CheckLock(GameObjects.B_Skill, LockType.Skill, "T_SkillName");
            CheckLock(GameObjects.B_Pet, LockType.Pet, "T_PetName");

            Get<GameObject>((int) GameObjects.B_Shop).BindEvent(Functions.TrueCondition,
                _ => OnButtonClicked(GameObjects.B_Shop, Get<Transform>((int) GameObjects.B_Shop)),
                UIEffectType.Bounce, false);

            void CheckLock(GameObjects btn, LockType lockType, string unlock)
            {
                var btnObj = Get<GameObject>((int)btn);
                btnObj.GetOrAddComponent<UI_Locked>().Set(lockType, btnObj.transform.GetComponent<Image>(), Util.FindChild(btnObj, "IMG_Lock"), Util.FindChild(btnObj, unlock),
                    () => btnObj.BindEvent(Functions.TrueCondition, _ => OnButtonClicked(btn, Get<Transform>((int)btn)), UIEffectType.Bounce, false));
            }
            
            Get<GameObject>((int)GameObjects.B_Close).GetComponent<Button>().onClick.AddListener(ClosePopup);
            Get<GameObject>((int)GameObjects.B_Close).SetActive(false);
            
            SetBadges();
            
            void ClosePopup()
            { 
                Manager.UI.CloseAllPopupUI();
                CloseInnerPopup();
            }

            void SetBadges()
            {
                Util.FindChild(gameObject, "IMG_InventoryBadge", true).GetOrAddComponent<UI_Badge>().
                    Set(new DbField[] {BadgeController.data.Weapons ,BadgeController.data.Accessories, BadgeController.data.Necklace},
                        () => BadgeController.I.IsWeaponBadgeOn() || BadgeController.I.IsAccessoryBadgeOn() || BadgeController.data.Necklace.Value,
                        LockType.Inventory);
                Util.FindChild(gameObject, "IMG_CharacterBadge", true).GetOrAddComponent<UI_Badge>().
                    Set(new DbField[] {BadgeController.data.Stats, BadgeController.data.LevelPoint, BadgeController.data.LevelUp, 
                            BadgeController.data.Title, BadgeController.data.Promotion}, 
                        () => BadgeController.I.IsStatBadgeOn() || BadgeController.data.LevelPoint.Value 
                                || BadgeController.data.LevelUp.Value || BadgeController.data.Title.Value,
                        LockType.Character);
                Util.FindChild(gameObject, "IMG_SummonBadge", true).GetOrAddComponent<UI_Badge>().
                    Set(new[] {BadgeController.data.Summon}, 
                        () => BadgeController.data.Summon.Value, LockType.Summon);
                Util.FindChild(gameObject, "IMG_SkillBadge", true).GetOrAddComponent<UI_Badge>().
                    Set(new[] {BadgeController.data.Skills}, 
                        () => BadgeController.I.IsSkillBadgeOn(), LockType.Skill);
                Util.FindChild(gameObject, "IMG_PetBadge", true).GetOrAddComponent<UI_Badge>().
                    Set( new[] {BadgeController.data.Book}, 
                        () => BadgeController.data.Book.Value, LockType.Pet);

            }

            return true;
        }

        public void CloseInnerPopup()
        {
            _curOpened = null;
            Get<GameObject>((int)GameObjects.B_Close).SetActive(false);
            Manager.UI.SetTmpOrder(0);
            SetSortingOrder(0);
        }

        public void OpenPopup(GameObjects btn)
        {
            OnButtonClicked(btn, Get<Transform>((int)btn));
        }
        
        private void OnButtonClicked(GameObjects btn, Transform btnT)
        {
            // if same button is clicked, close the popup
            // if (_curOpened == btn)
            // {
            //     Manager.UI.ClosePopupUI();
            //     Get<GameObject>((int)GameObjects.CloseButton).SetActive(false);
            //     _curOpened = null;
            //     return;
            // }
            
            if (_curOpened != null)
            {
                Manager.UI.CloseAllPopupUI(false);
                Manager.UI.SetTmpOrder(0);
            }
            
            Manager.Sound.PlaySFX(SFXType.UI_Tab);
            
            Get<GameObject>((int)GameObjects.B_Close).SetActive(true);
            Get<Transform>((int)Transforms.B_Close).position = btnT.position;

            int order = 0;
            switch (btn)
            {
                case GameObjects.B_Shop:
                    order = Manager.UI.ShowPopupUI<UI_Shop>().GetComponent<Canvas>().sortingOrder;
                    break;
                case GameObjects.B_Character:
                    order = Manager.UI.ShowPopupUI<UI_Character>().GetComponent<Canvas>().sortingOrder;
                    break;
                case GameObjects.B_Inventory:
                    order = Manager.UI.ShowPopupUI<UI_Inventory>().GetComponent<Canvas>().sortingOrder;
                    break;
                case GameObjects.B_Skill:
                    var skillPopup = Manager.UI.ShowPopupUI<UI_SkillInventory>();
                    order = skillPopup.GetComponent<Canvas>().sortingOrder;
                    skillPopup.SetOrder(order+1);
                    break;
                case GameObjects.B_Summon:
                    order = Manager.UI.ShowPopupUI<UI_Summon>().GetComponent<Canvas>().sortingOrder;
                    break;
                case GameObjects.B_Pet:
                    var petPopup = Manager.UI.ShowPopupUI<UI_PetInventory>();
                    order = petPopup.GetComponent<Canvas>().sortingOrder;
                    petPopup.SetOrder(order);
                    break;
                default: return;
            }
            _curOpened = btn;
            
            SetSortingOrder(order+1);
            Manager.UI.SetTmpOrder(btn == GameObjects.B_Pet ? 2 : 1);
        }

        public void SetSortingOrder(int order)
        {
            _canvas.sortingOrder = order;
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }
    }
}