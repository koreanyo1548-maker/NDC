using System.Collections.Generic;
using Controller;
using Controller.Play;
using Managers;
using MEC;
using ThirdParty;
using UIBases;
using UIs.Toast;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc.Setting
{
    public class UI_AccountSetting: UI_Popup
    {

        private int _saveWaitTime = 0;

        enum GameObjects
        {
            UI_DeleteAccount,
            UI_Logout,
            B_SignInWithGoogle,
            B_SignInWithHive,
            B_SignInWithApple,
            IMG_CompleteLoginGoogle,
            IMG_CompleteLoginHive,
            IMG_CompleteLoginApple,
            T_Guest,
            B_Logout
        }
        
        enum Images
        {
            B_Save
        }
        
        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            
            Bind<Image>(typeof(Images));
            Bind<GameObject>(typeof(GameObjects));
            
            Get<Image>((int)Images.B_Save).gameObject.BindEvent(Functions.TrueCondition, SaveData, UIEffectType.Bounce);
            Get<GameObject>((int)GameObjects.UI_DeleteAccount).GetOrAddComponent<UI_DeleteAccount>();
            Get<GameObject>((int)GameObjects.UI_Logout).GetOrAddComponent<UI_Logout>();

            //Get<GameObject>((int)GameObjects.B_SignInWithGoogle).BindEvent(Functions.TrueCondition, _ => { });// LinkWith(AuthV4.ProviderType.GOOGLE), UIEffectType.Bounce);
            //Get<GameObject>((int)GameObjects.B_SignInWithHive).BindEvent(Functions.TrueCondition, _ => LinkWith(AuthV4.ProviderType.HIVE), UIEffectType.Bounce);
            //Get<GameObject>((int)GameObjects.B_SignInWithApple).BindEvent(Functions.TrueCondition, _ => LinkWith(AuthV4.ProviderType.SIGNIN_APPLE), UIEffectType.Bounce);
            Util.FindChild(gameObject, "IMG_Dimmed").BindEvent(Functions.TrueCondition, _ => ClosePopupUI());
            Get<GameObject>((int)GameObjects.B_Logout).BindEvent(Functions.TrueCondition, _ => OpenLogoutPopup(), UIEffectType.Bounce);
            Util.FindChild(gameObject, "B_DeleteAccount", true).BindEvent(Functions.TrueCondition, _ => OpenDeleteAccountPopup(), UIEffectType.Bounce);
            
            Get<GameObject>((int)GameObjects.UI_DeleteAccount).SetActive(false);
            Get<GameObject>((int)GameObjects.UI_Logout).SetActive(false);

            SetLogin();
            
            SetSaveBlock();
            
            #if GOOGLEPLAY
            
            //Util.FindChild(gameObject, "Apple", true).SetActive(false);
            
            #endif
            
            return true;
        }

        //private void LinkWith(AuthV4.ProviderType providerType)
        //{
        //    var btn = Get<GameObject>(providerType == AuthV4.ProviderType.GOOGLE ? (int) GameObjects.B_SignInWithGoogle
        //        : providerType == AuthV4.ProviderType.SIGNIN_APPLE ? (int)GameObjects.B_SignInWithApple : (int)GameObjects.B_SignInWithHive);
        //    btn.SetActive(false);
        //    HiveManager.ConnectWith(providerType, () =>
        //    {
        //        SetLogin();
        //        Manager.UI.GetPopupUI<UI_Setting>().SetLogin();
        //    });
        //}

        private void SetLogin()
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
            Get<GameObject>((int)GameObjects.B_Logout).SetActive(!isGuest);
        }

        private void OpenLogoutPopup()
        {
            Get<GameObject>((int) GameObjects.UI_Logout).SetActive(true);
        }

        private void OpenDeleteAccountPopup()
        {
            Get<GameObject>((int) GameObjects.UI_DeleteAccount).SetActive(true);
        }


        private void SaveData(PointerEventData eventData)
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

        private void OnEnable()
        {
            if (_isInit) SetLogin();
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