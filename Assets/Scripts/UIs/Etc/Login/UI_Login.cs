using System;
using Controller;
using Controller.Play;
using Data;
using Managers.Base;
using ThirdParty;
using TMPro;
using UIs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc.Login
{
    public enum LoginProgress
    {
        HiveSetUp,
        HiveLogin,
        PlayFabLogin
    }
    public class UI_Login: MonoBehaviour
    {
        public bool ShowPrologue = true;
        private static UI_Login _instance;
        public static UI_Login I => _instance;

        private GameObject _loadingText;
        private TextMeshProUGUI _loadingPercentText;
        private SlicedFilledImage _loadingPercentImage;
        private GameObject _touchToStart;
        private GameObject _touchToLogin;
        private TextMeshProUGUI _errorText;
        private TextMeshProUGUI _ver;

        private GameObject _networkError;
        
        private LoginProgress _progress;
        
        // private GameObject _googleLoginButton;
        // private GameObject _guestLoginButton;

        private void Awake()
        {
            _instance = this;
            
            _loadingText = transform.Find("LoadingText").gameObject;
            _loadingPercentText = _loadingText.transform.Find("T_LoadingPercent").GetComponent<TextMeshProUGUI>();
            _loadingPercentImage = _loadingText.transform.Find("IMG_LoadingGauge").GetComponent<SlicedFilledImage>();
            _errorText = transform.Find("T_Error").GetComponent<TextMeshProUGUI>();

            _ver = transform.Find("Text Ver").GetComponent<TextMeshProUGUI>();
            _ver.text = "Ver " + Application.version.ToString();

            _touchToStart = transform.Find("TouchToStart").gameObject;
            _touchToLogin = transform.Find("TouchToLogin").gameObject;

            _networkError = transform.parent.Find("UI_Network").gameObject;
            _networkError.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                TryLogin(null);
            });

            _loadingPercentText.text = "0%";
            _loadingPercentImage.fillAmount = 0;
            
            _touchToStart.BindEvent(Functions.TrueCondition, LoadScene, UIEffectType.None, false);
            _touchToLogin.BindEvent(Functions.TrueCondition, TryLogin, UIEffectType.None, false);
            WhenLoggingIn();
        }

        public void NetworkCheck(bool isSetUpError)
        {
            if (isSetUpError) _progress = LoginProgress.HiveSetUp;
            _networkError.SetActive(true);
        }

        public void MultiDevice(Action whenContinue, Action whenExit)
        {
            var multiDevice = transform.parent.Find("UI_MultiDevice");
            multiDevice.Find("SafeArea").Find("B_Yes").GetComponent<Button>().onClick.AddListener(() => whenContinue());
            multiDevice.Find("SafeArea").Find("B_No").GetComponent<Button>().onClick.AddListener(() => whenExit());
            multiDevice.gameObject.SetActive(true);
        }

        public void Kick()
        {
            var kick = transform.parent.Find("UI_Kick").gameObject;
            kick.SetActive(true);
            kick.transform.Find("SafeArea").Find("B_Yes").GetComponent<Button>().onClick.AddListener(() => 
            {
                
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private void LoadScene(PointerEventData eventData)
        {
            GameObject.Find("@Sound").GetComponent<SoundManager>().PlaySFX(SFXType.TouchToStart);
            Instantiate(Resources.Load<UI_FadeOnScene>("Prefabs/UI/Scene/UI_FadeOnScene"), transform).FadeIn(() =>
            {
                if (!ShowPrologue && PlayerPrefs.HasKey(SettingType.DoPrologue1.ToString()))
                {
                    SceneManager.LoadScene(SceneType.Field.ToString());
                }
                else
                {
                    SceneManager.LoadScene(SceneType.Prologue.ToString());
                }
            });
        }

        private void TryLogin(PointerEventData eventData)
        {
            //if (_progress == LoginProgress.HiveSetUp)
            //{
            //    HiveManager.I.SDKReset();
            //}
            //else if (_progress == LoginProgress.HiveLogin)
            //{
            //    HiveManager.I.ShowLogInOption();
            //}
            //else 
            if (_progress == LoginProgress.PlayFabLogin)
            {
                PlayFabManager.ManualGuestLogin(SettingController.Id);
            }

            WhenLoggingIn();
        }

        private void WhenLoggingIn()
        {
            _loadingText.SetActive(false);
            _touchToStart.SetActive(false);
            _touchToLogin.SetActive(false);
            _errorText.text = string.Empty;
        }

        public void FailLogin(bool isPlayFabLoginFailed)
        {
            _progress = isPlayFabLoginFailed ? LoginProgress.PlayFabLogin : LoginProgress.HiveLogin;
            _loadingText.SetActive(false);
            _touchToStart.SetActive(false);
            _touchToLogin.SetActive(true);
            _errorText.text = isPlayFabLoginFailed ? "P" : "H";
        }
        
        public void StartLoading()
        {
            _loadingText.SetActive(true);
            _touchToStart.SetActive(false);
            _touchToLogin.SetActive(false);
        }

        public void SetLoadingPercent(float percent)
        {
            _loadingPercentText.text = percent.ToString("p0");
            _loadingPercentImage.fillAmount = percent;
        }
        public void FinishLoading()
        {
            _loadingText.SetActive(false);
            _touchToStart.SetActive(true);
            _touchToLogin.SetActive(false);
        }
    }
}