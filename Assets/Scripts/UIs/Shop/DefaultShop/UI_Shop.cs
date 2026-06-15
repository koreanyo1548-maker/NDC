using System;
using Controller;
using Controller.Currency;
using Data;
using Data.DbShop;
using Managers;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.FieldMain;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_Shop: UI_Popup, ILanguageSet
    {

        private Sprite[] _tabSprites; // 0: not selected 1: selected

        private Images? _curOpened = null;

        private Vector3 _positionSetter = new Vector3();
        private EventsManager _mileageEventHandler;
        
        enum Transforms
        {
            B_PackageTab,
            B_TicketTab,
            B_DiaTab,
            B_MileageTab,
            B_NormalTab
        }

        enum GameObjects
        {
            V_Package,
            V_Ticket,
            V_Dia,
            V_Mileage,
            V_Normal
            // B_Detail,
            // T_Detail
        }

        enum Images
        {
            B_PackageTab,
            B_TicketTab,
            B_DiaTab,
            B_MileageTab,
            B_NormalTab
        }

        enum Texts
        {
            T_TitleShop,
            T_Info,
            T_Mileage
        }


        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            
            
            Bind<Transform>(typeof(Transforms));
            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
                
            _tabSprites = new[]
                {Manager.Resource.Load<Sprite>("UI_TAB_Btn"), Manager.Resource.Load<Sprite>("UI_TAB_Btn_Selected")};

            foreach (Images tab in Enum.GetValues(typeof(Images)))
            {
                Get<Image>((int)tab).GetComponent<Button>().onClick.AddListener(() => OnTabClicked(tab));
            }
                
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_ResetTime", true).text = StringMaker.GetResetTime(210112);
            var packageParent = Util.FindChild<Transform>(gameObject, "G_PackageParent", true);
            var ticketParent = Util.FindChild<Transform>(gameObject, "G_TicketParent", true);
            var diaParent = Util.FindChild<Transform>(gameObject, "G_DiaParent", true);
            var mileageParent = Util.FindChild<Transform>(gameObject, "G_MileageParent", true);
            var normalParent = Util.FindChild<Transform>(gameObject, "G_NormalParent", true);
                
            // Util.FindChild(gameObject, "B_Detail", true).BindEvent(Functions.TrueCondition, _ => OpenDetail(), UIEffectType.Bounce);

            Get<GameObject>((int)GameObjects.V_Dia).SetActive(false);
            Get<GameObject>((int)GameObjects.V_Ticket).SetActive(false);
            Get<GameObject>((int)GameObjects.V_Mileage).SetActive(false);
            Get<GameObject>((int)GameObjects.V_Normal).SetActive(false);
            Get<TextMeshProUGUI>((int) Texts.T_Info).text = LocalString.Get(210110);
            OnTabClicked(Images.B_PackageTab);

            _mileageEventHandler = new EventsManager(this, new EventsManager.Config
            {
                handler = WhenMileageChanged,
                updatedField = new[] {CurrencyController.I.GetMoneyModel(CurrencyType.Mileage)}
            });
            WhenMileageChanged();
            
            PlayFabManager.Store.DoWithTime(Set);
            
            void Set(DateTime now)
            {
                DbInGameShop.ForEach(SetItem);
                DbInAppShop.ForEach(SetItem);
                
                void SetItem(IDbShop shopItem)
                {
                    var category = shopItem.GetCategory();
                    if (category == ShopCategoryType.Package)
                    {
                        var item = Manager.UI.MakeSubItem<UI_ShopPackage_Item>(packageParent);
                        item.SetInfo(now, shopItem);
                    }
                    else if (category == ShopCategoryType.Costume)
                    {
                        var item = Manager.UI.MakeSubItem<UI_ShopCostume_Item>(packageParent);
                        item.SetInfo(now, shopItem);
                    }
                    else if (category == ShopCategoryType.Ticket)
                    {
                        var item = Manager.UI.MakeSubItem<UI_ShopPackage_Item>(ticketParent);
                        item.SetInfo(now, shopItem);
                    }
                    else if (category == ShopCategoryType.Dia)
                    {
                        var item = shopItem.GetBuyLimit() == 0 ? Manager.UI.MakeSubItem<UI_ShopDiaNormal_Item>(diaParent) as UI_Shop_Item
                                : Manager.UI.MakeSubItem<UI_ShopDiaSale_Item>(diaParent);
                        item.SetInfo(shopItem);
                    }
                    else if (category == ShopCategoryType.Mileage)
                    {
                        var item = Manager.UI.MakeSubItem<UI_ShopMileage_Item>(mileageParent);
                        item.SetInfo(shopItem);
                    }
                    else if (category == ShopCategoryType.Normal)
                    {
                        var item = Manager.UI.MakeSubItem<UI_ShopNormal_Item>(normalParent);
                        item.SetInfo(now, shopItem);
                    }
                }

                LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            
            }
            return true;

        }
        public override void WhenPopupClosed()
        {
            Manager.UI.GetSceneUI<UI_MainBottom>().CloseInnerPopup();
        }

        private void WhenMileageChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Mileage).text =
                Define.AddUnit(CurrencyController.I.GetMoneyModel(CurrencyType.Mileage).Value, 4, 0);
        }

        public void OpenTab(ShopCategoryType type)
        {
            if (!_isInit) Init();
            
            switch (type)
            {
                case ShopCategoryType.Package: 
                case ShopCategoryType.Costume: OnTabClicked(Images.B_PackageTab);
                    break;
                case ShopCategoryType.Ticket: OnTabClicked(Images.B_TicketTab);
                    break;
                case ShopCategoryType.Dia: OnTabClicked(Images.B_DiaTab);
                    break;
                case ShopCategoryType.Mileage: OnTabClicked(Images.B_MileageTab);
                    break;
                case ShopCategoryType.Normal: OnTabClicked(Images.B_NormalTab);
                    break;
            }
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
                _positionSetter.y = 5.917f;
                Get<Transform>((int)_curOpened).localPosition = _positionSetter;
                Get<Image>((int) _curOpened).sprite = _tabSprites[0];
                CloseTab(_curOpened);
            }
            
            
            _curOpened = clicked; 
            _positionSetter = Get<Transform>((int) _curOpened).localPosition;
            _positionSetter.y = -5.683f;
            Get<Transform>((int) clicked).localPosition = _positionSetter;
            Get<Image>((int) clicked).sprite = _tabSprites[1];
            OpenTab(clicked);
            Get<TextMeshProUGUI>((int) Texts.T_TitleShop).text = string.Format(LocalString.Get(210108),
                LocalString.Get(_curOpened == Images.B_PackageTab ? 210105 :
                    _curOpened == Images.B_TicketTab ? 210409 :
                    _curOpened == Images.B_DiaTab ? 210107 : 
                    _curOpened == Images.B_MileageTab ? 210292 : 210106));
            
            void OpenTab(Images tab)
            {
                switch (tab)
                {
                    case Images.B_PackageTab:
                        Get<GameObject>((int)GameObjects.V_Package).SetActive(true);
                        break;
                    case Images.B_TicketTab:
                        Get<GameObject>((int)GameObjects.V_Ticket).SetActive(true);
                        break;
                    case Images.B_DiaTab:
                        Get<GameObject>((int)GameObjects.V_Dia).SetActive(true);
                        break;
                    case Images.B_MileageTab:
                        Get<GameObject>((int)GameObjects.V_Mileage).SetActive(true);
                        // Get<GameObject>((int)GameObjects.B_Detail).SetActive(false);
                        // Get<GameObject>((int)GameObjects.T_Detail).SetActive(false);
                        Get<TextMeshProUGUI>((int) Texts.T_Info).text = LocalString.Get(210293);
                        break;
                    case Images.B_NormalTab:
                        Get<GameObject>((int)GameObjects.V_Normal).SetActive(true);
                        Get<TextMeshProUGUI>((int) Texts.T_Info).text = string.Empty;
                        break;
                }
            }
            
            void CloseTab(Images? tab)
            {
                switch (tab)
                {
                    case Images.B_PackageTab:
                        Get<GameObject>((int)GameObjects.V_Package).SetActive(false);
                        break;
                    case Images.B_TicketTab:
                        Get<GameObject>((int)GameObjects.V_Ticket).SetActive(false);
                        break;
                    case Images.B_DiaTab:
                        Get<GameObject>((int)GameObjects.V_Dia).SetActive(false);
                        break;
                    case Images.B_MileageTab:
                        Get<GameObject>((int)GameObjects.V_Mileage).SetActive(false);
                        // Get<GameObject>((int)GameObjects.B_Detail).SetActive(true);
                        // Get<GameObject>((int)GameObjects.T_Detail).SetActive(true);
                        Get<TextMeshProUGUI>((int) Texts.T_Info).text = LocalString.Get(210110);
                        break;
                    case Images.B_NormalTab:
                        Get<GameObject>((int)GameObjects.V_Normal).SetActive(false);
                        Get<TextMeshProUGUI>((int) Texts.T_Info).text = LocalString.Get(210110);
                        break;
                }
            }
        }
        
        // private void OpenDetail()
        // {
        //     Application.OpenURL("http://rainbowrabbit.co.kr/terms-of-service/");
        // }
        
        public override bool NeedRaycast()
        {
            return true;
        }
        
        private void OnDisable()
        {
            _mileageEventHandler?.Dispose();
        }

        private void OnEnable()
        {
            _mileageEventHandler?.Reconnect();
        }

        public void OnLanguageChanged(Locale locale)
        {
            Get<TextMeshProUGUI>((int) Texts.T_Info).text = LocalString.Get(_curOpened == Images.B_MileageTab ? 210293 : 210110);
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_ResetTime", true).text = StringMaker.GetResetTime(210112);
        }
    }
}