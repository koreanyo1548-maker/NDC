using System;
using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Play;
using Data;
using Data.DbPetInfo;
using Data.Utils;
using Managers;
using UIBases;
using UIs.FieldMain;
using UIs.Lock;
using UIs.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Pet
{
    public class UI_PetInventory: UI_Popup
    {
        private UI_EquipPet _equipUI;
        public UI_EquipPet EquipUI => _equipUI;
        
        private Images? _curOpened = null;
        
        private Sprite[] _tabSprites; // 0: not selected 1: selected
        private Vector3 _positionSetter = new();

        private Dictionary<CurrencyType, UI_Book_Item> _books = new();

        private Transform _petParent;
        
        public UIField<bool> Check = new(false);

        private EventsManager _bookEventManager;
        
        enum GameObjects
        {
            V_Pet,
            V_Book,
            T_NoBook
        }

        enum Images
        {
            B_PetTab,
            B_BookTab
        }
        

        enum Transforms
        {
            B_PetTab,
            B_BookTab,
            G_BookParent
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

            Util.FindChild(gameObject, "Bible", true).GetOrAddComponent<UI_Bible>();
            Util.FindChild(gameObject, "Bookshelf", true).GetOrAddComponent<UI_Bookshelf>();
            _equipUI = Util.FindChild(gameObject, "UI_EquipPet", true).GetOrAddComponent<UI_EquipPet>();
            _equipUI.SetOrder(transform.GetComponent<Canvas>().sortingOrder);
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            
            _tabSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

            _petParent = Util.FindChild<Transform>(gameObject, "G_PetParent", true);
            var bookParent = Get<Transform>((int)Transforms.G_BookParent);
            
            Get<GameObject>((int)GameObjects.V_Book).SetActive(false);
            
            DbPet.ForEach(
                p =>
                {
                    var item = Manager.UI.MakeSubItem<UI_Pet_Item>(_petParent, "UI_Inventory_Item");
                    item.Set(p);
                });
            
            DbBook.ForEach(
                b =>
                {
                    if (CurrencyController.I.GetBookModel(b.Id).Value > 0)
                    {
                        var item = Manager.UI.MakeSubItem<UI_Book_Item>(bookParent);
                        item.Set(b);
                        _books.Add(b.Id, item);
                    }
                });
                
            Get<GameObject>((int)GameObjects.T_NoBook).SetActive(_books.Count == 0);
            
            _bookEventManager = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenBookCountChanged,
                updatedField = CurrencyController.I.GetAllBookModels()
            });

            Util.FindChild(gameObject, "IMG_BookBadge", true).AddComponent<UI_Badge>()
                .Set(new DbField[] {BadgeController.data.Book}, () => BadgeController.data.Book.Value);
                
            foreach (Images tab in Enum.GetValues(typeof(Images)))
            {
                Get<Image>((int)tab).gameObject.BindEvent(Functions.TrueCondition, _ => OnTabClicked(tab), UIEffectType.Bounce);
            }
            OnTabClicked(Images.B_PetTab);
            return true;
        }
        
        private void WhenBookCountChanged()
        {
            DbBook.ForEach(
                b =>
                {
                    var bookCount = CurrencyController.I.GetBookModel(b.Id).Value;
                    if (bookCount > 0 && !_books.ContainsKey(b.Id))
                    {
                        var item = Manager.UI.MakeSubItem<UI_Book_Item>(Get<Transform>((int)Transforms.G_BookParent));
                        item.transform.localScale = Vector3.one;
                        item.Set(b);
                        _books.Add(b.Id, item);
                    }
                    else if (bookCount == 0 && _books.ContainsKey(b.Id))
                    {
                        Manager.Resource.Destroy(_books[b.Id].gameObject);
                        _books.Remove(b.Id);
                    }
                });
            Get<GameObject>((int)GameObjects.T_NoBook).SetActive(_books.Count == 0);
        }

        private void OnTabClicked(Images clicked)
        {
            if (_curOpened == clicked)
            {
                return;
            }
            if (_curOpened != null)
            {
                _positionSetter = Get<Transform>((int) _curOpened).localPosition;
                _positionSetter.y = 22.83f;
                Get<Transform>((int)_curOpened).localPosition = _positionSetter;
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
                CloseTab(_curOpened);
            }
            
            Check.Value = !Check.Value;
            
            _curOpened = clicked; 
            _positionSetter = Get<Transform>((int) _curOpened).localPosition;
            _positionSetter.y = 7.92f;
            Get<Transform>((int) clicked).localPosition = _positionSetter;
            Get<Image>((int) clicked).sprite = _tabSprites[1];
            OpenTab(clicked);
            
            void OpenTab(Images tab)
            {
                switch (tab)
                {
                    case Images.B_PetTab:
                        Get<GameObject>((int)GameObjects.V_Pet).SetActive(true);
                        _equipUI.gameObject.SetActive(true);
                        break;
                    case Images.B_BookTab:
                        Get<GameObject>((int)GameObjects.V_Book).SetActive(true);
                        _equipUI.gameObject.SetActive(false);
                        break;
                }
            }
            
            void CloseTab(Images? tab)
            {
                switch (tab)
                {
                    case Images.B_PetTab:
                        Get<GameObject>((int)GameObjects.V_Pet).SetActive(false);
                        break;
                    case Images.B_BookTab:
                        Get<GameObject>((int)GameObjects.V_Book).SetActive(false);
                        break;
                }
            }
        }
        
        public void SetOrder(int order)
        {
            if (_equipUI != null)
            {
                _equipUI.SetOrder(order);
            }
        }


        private void OnEnable()
        {
            _bookEventManager?.Reconnect();
        }

        private void OnDisable()
        {
            _bookEventManager?.Dispose();
        }
        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
            Manager.UI.GetSceneUI<UI_MainBottom>().CloseInnerPopup();
        }

        public void MoveToTop(bool isPet)
        {
            if (isPet)
            {
                var position = _petParent.localPosition;
                position.y = 0;
                _petParent.localPosition = position;
            }
            else
            {
                var position = Get<Transform>((int)Transforms.G_BookParent).localPosition;
                position.y = 0;
                Get<Transform>((int)Transforms.G_BookParent).localPosition = position;
            }
        }
    }
}