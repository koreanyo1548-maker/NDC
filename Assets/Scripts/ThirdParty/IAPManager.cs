using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//using AppsFlyerSDK;
using Controller.Currency;
using Data.DbShop;
using Managers;
using Newtonsoft.Json;
using UIs.Etc.Warning;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace ThirdParty
{
    public class BuyingItem
    {
        public IDbShop shopItem;
        public DbPassShop passItem;
        public UI_Loading showLoading;
        public Action onCompleteBuy;
        public BuyingItem(IDbShop item, UI_Loading loading, Action act)
        {
            shopItem = item;
            showLoading = loading;
            onCompleteBuy = act;
        }
        public BuyingItem(DbPassShop item, UI_Loading loading, Action act)
        {
            passItem = item;
            showLoading = loading;
            onCompleteBuy = act;
        }
    }
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        public static IAPManager I;

        /// <summary>
        /// 구매 과정을 제어하는 함수 제공자
        /// </summary>
        [Header("Cache")]
        private IStoreController storeController;
        /// <summary>
        /// 여러 플랫폼을 위한 확장 처리 제공자
        /// </summary>
        private IExtensionProvider storeExtensionProvider;

        private bool _isRestoreInit = false;
        private bool _isConnected = false;

        /// <summary>
        /// 구매중인 아이템 정보
        /// </summary>
        BuyingItem _buyingItem = null;

        private void Awake()
        {
            I = this;
            DontDestroyOnLoad(gameObject);

        }
        private void Start()
        {
            // Unity IAP 초기화 시작
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // 구글 플레이 상품들 추가
            DbInAppShop.ForEach((product) => {
                builder.AddProduct(product.ProductId, ProductType.Consumable, new IDs() { { product.ProductId, GooglePlay.Name } });
            });
            DbPassShop.ForEach((product) => {
                builder.AddProduct(product.ProductId, ProductType.Consumable, new IDs() { { product.ProductId, GooglePlay.Name } });
            });

            Debug.Log("IAP 초기화에 시작");
            UnityPurchasing.Initialize(this, builder);

        }
        /// <summary>
        /// Unity IAP 초기화 성공
        /// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP 초기화에 성공했습니다");

            storeController = controller;
            storeExtensionProvider = extensions;

            DbInAppShop.ForEach((product) => {
                var val = storeController.products.WithID(product.ProductId);
                if (product != null)
                    product.DisplayPrice = val.metadata.localizedPriceString;
            });
            DbPassShop.ForEach((product) => {
                var val = storeController.products.WithID(product.ProductId);
                if (product != null)
                    product.DisplayPrice = val.metadata.localizedPriceString;
            });
            Debug.Log("IAP 상품 가격 설정 완료");
        }
        /// <summary>
        /// Unity IAP 초기화 실패
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"Unity IAP 초기화 실패: {error}");
        }
        /// <summary>
        /// Unity IAP 초기화 실패
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"Unity IAP 초기화 실패: {error}, {message}");
        }
        /// <summary>
        /// Unity IAP 구매 성공
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Debug.Log($"결제 완료: {purchaseEvent.purchasedProduct}");
            if (_buyingItem.shopItem != null)
            {
                CurrencyController.I.Buy(_buyingItem.shopItem, string.Empty, () =>
                {
                    Manager.UI.CloseSingleUI(_buyingItem.showLoading);
                });
                _buyingItem.onCompleteBuy();
                _buyingItem = null;
            }
            else
            {
                CurrencyController.I.Buy(_buyingItem.passItem);
                PlayFabManager.Store.ForceSave(() =>
                {
                    _buyingItem.onCompleteBuy();
                    _buyingItem = null;
                });
            }

            AppsFlyerEven_Purchase(
                    purchaseEvent.purchasedProduct.metadata.isoCurrencyCode,
                    purchaseEvent.purchasedProduct.metadata.localizedPriceString);

            return PurchaseProcessingResult.Complete;
        }
        /// <summary>
        /// Unity IAP 구매 실패
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            if (_buyingItem != null)
                Manager.UI.CloseSingleUI(_buyingItem.showLoading);
            _buyingItem = null;
            Debug.LogError($"Unity IAP 구매 실패: {failureDescription.productId}, {failureDescription.message}, {product.receipt}");
        }
        /// <summary>
        /// Unity IAP 구매 실패
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            if (_buyingItem != null)
                Manager.UI.CloseSingleUI(_buyingItem.showLoading);
            _buyingItem = null;
            Debug.LogError($"Unity IAP 구매 실패: {failureReason}, {product.receipt}");
        }

        #region 구매
        /// <summary>
        /// 아이템 구매
        /// </summary>
        public void Buy(IDbShop item, Action afterSuccess)
        {
            if (_buyingItem != null)
            {
                Debug.LogError("현재 구매 진행중: " + JsonConvert.SerializeObject(item));
                return;
            }

             Debug.Log(">>>>>>>> [구매시작] " + JsonConvert.SerializeObject(item));
            
            var loading = Manager.UI.ShowSingleUI<UI_Loading>();

#if UNITY_EDITOR
            Debug.Log("에디터 결제");
            CurrencyController.I.Buy(item, string.Empty, () =>
            {
                Manager.UI.CloseSingleUI(loading);
            });
            afterSuccess();
#else
            Debug.Log($"구매 IAP - IDbShop {item.GetProductId()}");
            // 구매 상품
            _buyingItem = new BuyingItem(item, loading, afterSuccess);
            Product product = storeController.products.WithID(item.GetProductId());
            // 상품이 존재하며 구매 가능하면
            if (product != null && product.availableToPurchase)
                storeController.InitiatePurchase(product);
            else
                Debug.LogError("상품이 없거나 현재 구매가 불가능합니다");
#endif
        }
        /// <summary>
        /// 패스 구매
        /// </summary>
        public void Buy(DbPassShop item, Action afterSuccess)
        {
            if (_buyingItem != null)
            {
                Debug.LogError("현재 구매 진행중: " + JsonConvert.SerializeObject(item));
                return;
            }

            var loading = Manager.UI.ShowSingleUI<UI_Loading>();
#if UNITY_EDITOR
            Debug.Log("에디터 결제");
            CurrencyController.I.Buy(item, () =>
            {
                Manager.UI.CloseSingleUI(loading);
            });
            afterSuccess();
#else
            Debug.Log($"구매 IAP - IDbShop {item.ProductId}");
            // 구매 상품
            _buyingItem = new BuyingItem(item, loading, afterSuccess);
            Product product = storeController.products.WithID(item.ProductId);
            // 상품이 존재하며 구매 가능하면
            if (product != null && product.availableToPurchase)
                storeController.InitiatePurchase(product);
            else
                Debug.LogError("상품이 없거나 현재 구매가 불가능합니다");
#endif
        }
        #endregion

        void AppsFlyerEven_Purchase(string currency, string value)
        {
#if APPSFLYER_ENBALE
            string result = Regex.Replace(value, @"[^.0-9\s]", "");
            // 구매 이벤트
            AppsFlyer.sendEvent("af_purchase", new()
            {
                { AFInAppEvents.CURRENCY, currency },
                { AFInAppEvents.REVENUE, result }
            });

            // 첫 구매 이벤트
            // 플레이팹 서버에서 첫구매했는지 가져온다.
            PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
                Keys = new List<string> { "FirstPurchase" }
            }, result => {
                if (result.Data.Count == 0)
                {
                    AppsFlyer.sendEvent("unique_pu", new());

                    // 첫 구매 저장
                    PlayFabClientAPI.UpdateUserData(new() {
                        Data = new() { { "FirstPurchase", "true" } }
                    }, result => {}, err => {});
                }
            }, err => {});
#endif
        }
    }
}