using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Play;
using Data.DbShop;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;
using static UIs.FieldMain.UI_MainBottom;

namespace UIs.Etc.Setting
{
    public class UI_Setting: UI_Popup
    {
        enum InputFields
        {
            I_CouponInputField
        }

        enum GameObjects
        {
            B_UseCoupon,
            B_SignInWithGoogle,
            IMG_CompleteLoginGoogle,
            B_SignInWithHive,
            IMG_CompleteLoginHive,
            B_SignInWithApple,
            IMG_CompleteLoginApple,
            T_Guest
        }
        
        enum Images
        {
            B_Save
        }
        
       
        private int _saveWaitTime = 0;
        
        
        private void Start()
        {
            Init();           
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<GameObject>(typeof(GameObjects)); 
            Bind<TMP_InputField>(typeof(InputFields));
            Bind<Image>(typeof(Images));

            //Get<GameObject>((int)GameObjects.B_SignInWithGoogle).BindEvent(Functions.TrueCondition, _ => { });// LinkWith(AuthV4.ProviderType.GOOGLE), UIEffectType.Bounce);
            //Get<GameObject>((int)GameObjects.B_SignInWithHive).BindEvent(Functions.TrueCondition, _ => LinkWith(AuthV4.ProviderType.HIVE), UIEffectType.Bounce);
            //Get<GameObject>((int)GameObjects.B_SignInWithApple).BindEvent(Functions.TrueCondition, _ => LinkWith(AuthV4.ProviderType.SIGNIN_APPLE), UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.B_UseCoupon).BindEvent(Functions.TrueCondition, UseCoupon, UIEffectType.Bounce, false);
            
            Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
            Util.FindChild(gameObject, "B_Copy", true).BindEvent(Functions.TrueCondition, CopyId, UIEffectType.Bounce);
            // Util.FindChild(gameObject, "B_NaverLounge", true).BindEvent(Functions.TrueCondition, _ => OpenLounge(), UIEffectType.Bounce);
            // Util.FindChild(gameObject, "B_PowerSaving", true).BindEvent(Functions.TrueCondition, _ => OnPowerSavingButtonClicked(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Save", true).BindEvent(Functions.TrueCondition, _ => Save(), UIEffectType.Bounce);
            //Util.FindChild(gameObject, "B_AccountManaging", true).BindEvent(Functions.TrueCondition, _ => OpenAccountSetting(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Privacy", true).BindEvent(Functions.TrueCondition, _ => OpenPrivacy(), UIEffectType.Bounce);
            //Util.FindChild(gameObject, "B_Support", true).BindEvent(Functions.TrueCondition, _ => OpenSupport(), UIEffectType.Bounce);
            //Util.FindChild(gameObject, "B_Notice", true).BindEvent(Functions.TrueCondition, _ => OpenNotice(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_Languages", true).BindEvent(Functions.TrueCondition, _ => OpenLanguageSet(), UIEffectType.Bounce);

            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Id", true).text = SettingController.UID;
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_AppVersion", true).text = Application.version;
            
            Util.FindChild(gameObject, "B_BGMToggle", true).GetOrAddComponent<UI_SettingToggle>()
                .Set(SettingController.data.BGMSound.Value > 0.5f, ToggleBGM);
            Util.FindChild(gameObject, "B_SFXToggle", true).GetOrAddComponent<UI_SettingToggle>()
                .Set(SettingController.data.SfxSound.Value > 0.5f, ToggleSfx);
            Util.FindChild(gameObject, "B_CameraShakingToggle", true).GetOrAddComponent<UI_SettingToggle>()
                .Set(SettingController.data.IsCameraShaking.Value, ToggleCameraShaking);
            Util.FindChild(gameObject, "B_AutoPowerSavingToggle", true).GetOrAddComponent<UI_SettingToggle>()
                .Set(SettingController.data.IsAutoPowerSaving.Value, ToggleAutoPowerSaving);
            Util.FindChild(gameObject, "B_PushAlarmToggle", true).GetOrAddComponent<UI_SettingToggle>()
                .Set(SettingController.data.IsPushAlarm.Value, TogglePushAlarm);
            Util.FindChild(gameObject, "B_NightPushAlarmToggle", true).GetOrAddComponent<UI_SettingToggle>()
                .Set(SettingController.data.IsNightPushAlarm.Value, ToggleNightPushAlarm);
            
            SetLogin();

#if GOOGLEPLAY
            //Util.FindChild(gameObject, "Apple", true).SetActive(false);
#endif

#if UNITY_IOS
            Util.FindChild(gameObject, "Coupon", true).SetActive(false);

            Util.FindChild(gameObject, "B_AccountManaging", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "SignInButtons", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "B_Copy", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "B_Close", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "T_Guest", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "T_IdTitle", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "T_Id", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "T_AppVersionTitle", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "T_AppVersion", true).transform.localPosition += Vector3.up * 200f;
            Util.FindChild(gameObject, "IMG_SettingBG", true).transform.localPosition += Vector3.up * 100f;
            Util.FindChild(gameObject, "IMG_SettingBG", true).GetComponent<RectTransform>().sizeDelta += new Vector2(0, -200);


#else
            Util.FindChild(gameObject, "Coupon", true).SetActive(PlayFabManager.Data.CanShowCoupon);
#endif

            return true;
        }

        private void OpenLanguageSet()
        {
            Manager.UI.ShowPopupUI<UI_LanguageSet>();
        }

        public void SetLogin()
        {
            var isGuest = SettingController.data.IsGuest.Value;
            var isGoogle = SettingController.data.IsGoogle.Value;
            var isHive = SettingController.data.IsHive.Value;
            var isApple = SettingController.data.IsApple.Value;
            
            //Get<GameObject>((int)GameObjects.T_Guest).SetActive(isGuest);
            //Get<GameObject>((int)GameObjects.B_SignInWithGoogle).SetActive(!isGoogle);
            //Get<GameObject>((int)GameObjects.IMG_CompleteLoginGoogle).SetActive(isGoogle);
            //Get<GameObject>((int)GameObjects.B_SignInWithHive).SetActive(!isHive);
            //Get<GameObject>((int)GameObjects.IMG_CompleteLoginHive).SetActive(isHive);
            
            #if APPSTORE
            Get<GameObject>((int)GameObjects.B_SignInWithApple).SetActive(!isApple);
            Get<GameObject>((int)GameObjects.IMG_CompleteLoginApple).SetActive(isApple);
            #endif
        }

        private void UseCoupon(PointerEventData eventData)
        {
            Get<GameObject>((int)GameObjects.B_UseCoupon).SetActive(false);
            var coupon = Get<TMP_InputField>((int) InputFields.I_CouponInputField).text;
            if (DbCoupon.Get(coupon) == null)
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200000);
                SetOnUseButton();
                return;
            }

            if (!CurrencyController.I.CanUse(coupon))
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(200001);
                SetOnUseButton();
                return;
            }
            
            PlayFabManager.Data.CheckCoupon(coupon, SetOnUseButton);

            void SetOnUseButton()
            {
                Get<GameObject>((int)GameObjects.B_UseCoupon).SetActive(true);
            }
        }

        //private void LinkWith(AuthV4.ProviderType providerType)
        //{
        //    var btn = Get<GameObject>(providerType == AuthV4.ProviderType.GOOGLE ? (int) GameObjects.B_SignInWithGoogle
        //        : providerType == AuthV4.ProviderType.SIGNIN_APPLE ? (int)GameObjects.B_SignInWithApple : (int)GameObjects.B_SignInWithHive);
        //    btn.SetActive(false);
        //    HiveManager.ConnectWith(providerType, SetLogin);
        //}
        
        private void CopyId(PointerEventData eventData)
        {
            var editor = new TextEditor
            {
                text = SettingController.UID
            };
            editor.SelectAll();
            editor.Copy();
            Manager.UI.ShowSingleUI<UI_Toast>().SetText(200003);
        }
        
        private void OpenLounge()
        {
            Application.OpenURL("https://game.naver.com/lounge/Death_Crow_idle/home");
        }

        private void OpenNotice()
        {
            //HiveManager.I.ShowPromotion(PromotionType.NOTICE, true);
        }
        
        private void Save()
        {
            if (CanSave())
            {
                _saveWaitTime = 10;
                SetSaveBlock();
                PlayFabManager.Store.ForceSave(() =>
                {
                    Manager.UI.ShowSingleUI<UI_Toast>().SetText(200004);
                });

                Timing.RunCoroutine(_SaveWaitRoutine());
            }
            else
            {
                Manager.UI.ShowSingleUI<UI_Toast>().SetText(string.Format(LocalString.Get(200005), _saveWaitTime));
            }
        }

        IEnumerator<float> _SaveWaitRoutine()
        {
            while (_saveWaitTime > 0)
            {
                yield return Timing.WaitForSeconds(1);
                _saveWaitTime--;
            }
            SetSaveBlock();
        }

        private void SetSaveBlock()
        {
            Get<Image>((int) Images.B_Save).material = Define.GetUIMaterial(!CanSave());
        }
        private bool CanSave()
        {
            return _saveWaitTime == 0;
        }
        // private void OnPowerSavingButtonClicked()
        // {
        //     Manager.UI.SetPowerSavingTimer(true);
        //     Manager.UI.StartPowerSaving();
        //     ClosePopupUI();
        // }

        
        private void OpenAccountSetting()
        {
            Manager.UI.ShowPopupUI<UI_AccountSetting>();
        }

        private void OpenPrivacy()
        {
            //HiveManager.I.ShowTerms();
            Application.OpenURL("https://ndolphinconnect.blogspot.com/2022/06/blog-post.html");
        }

        private void OpenSupport()
        {
           // HiveManager.I.ShowInquiry();
        }

        private void ToggleSfx()
        {
            SettingController.I.SetSfxSound(SettingController.data.SfxSound.Value == 0 ? 1 : 0);
        }
        

        private void ToggleBGM()
        {
            SettingController.I.SetBGMSound(SettingController.data.BGMSound.Value == 0 ? 1 : 0);
        }

        private void ToggleCameraShaking()
        {
            SettingController.data.IsCameraShaking.Value = !SettingController.data.IsCameraShaking.Value;
        }

        private void ToggleAutoPowerSaving()
        {
            SettingController.data.IsAutoPowerSaving.Value = !SettingController.data.IsAutoPowerSaving.Value;
            Manager.UI.SetPowerSavingTimer(!SettingController.data.IsAutoPowerSaving.Value);
        }

        private void TogglePushAlarm()
        {
            //HiveManager.I.SetRemotePushSetting(true);
            // SettingController.data.IsPushAlarm.Value = !SettingController.data.IsPushAlarm.Value;
        }

        private void ToggleNightPushAlarm()
        {
            //HiveManager.I.SetRemotePushSetting(false);
            // SettingController.data.IsNightPushAlarm.Value = !SettingController.data.IsNightPushAlarm.Value;
        }

        public override bool NeedRaycast()
        {
            return true;
        }

        public override void WhenPopupClosed()
        {
        }
    }
}