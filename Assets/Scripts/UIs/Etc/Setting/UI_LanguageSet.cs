using System.Collections.Generic;
using Managers;
using UIBases;
using UIs.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc.Setting
{
    public class UI_LanguageSet: UI_Popup, ILanguageSet
    {
        private Sprite[] _btnSprites;
        private List<Image> _languageBtns = new();
        private void Start()
        {
            Init();
        }
        
        public override bool Init()
        {
            if (!base.Init()) return false;

            transform.Find("IMG_Dimmed").gameObject.BindEvent(Functions.TrueCondition, _ => ClosePopupUI());

            _btnSprites = new[]
            {
                Manager.Resource.Load<Sprite>("UI_DefaultButton_round"),
                Manager.Resource.Load<Sprite>("UI_DefaultButton_round2")
            };
            
            var parent = Util.FindChild<Transform>(gameObject, "Languages", true);
            for (var idx = 0; idx < parent.childCount; ++idx)
            {
                var curIdx = idx;
                var obj = parent.GetChild(idx);
                obj.gameObject.BindEvent(() => !IsCurrentSelected(curIdx), _ => SetLanguage(curIdx), UIEffectType.Bounce);
                _languageBtns.Add(obj.GetComponent<Image>());
            }
            
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
            OnLanguageChanged(LocalizationSettings.SelectedLocale);
            return true;
        }

        private void SetLanguage(int idx)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[idx];

            //Configuration.updateGameLanguage(Define.LanguageIndexToCode(idx));
        }

        private bool IsCurrentSelected(int idx)
        {
            return LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[idx];
        }

        public void OnLanguageChanged(Locale locale)
        {
            var selected = LocalizationSettings.AvailableLocales.Locales.IndexOf(locale);
            for (var idx = 0; idx < _languageBtns.Count; ++idx)
            {
                _languageBtns[idx].sprite = _btnSprites[selected == idx ? 1 : 0];
            }
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