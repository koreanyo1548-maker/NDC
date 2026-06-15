using System;
using System.Collections.Generic;
using Data.DbCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace Data.Stores
{
    [Serializable]
    public class MailInfo: ILanguageSet
    {
        public int id;
        public MailType type;
        public string title;
        public string contents;
        public List<string> titleL;
        public List<string> contentsL;
        public List<DbReward> rewards;
        public DateTime startTime;
        public DateTime endTime;
        public DateTime resetTime;
        public List<string> users;

        ~MailInfo()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        }
        
        public MailInfo(int id, MailType type, string title, string contents, 
            List<string> titleL, List<string> contentsL,
            List<string> rewards, DateTime startTime, DateTime endTime, DateTime resetTime, List<string> users)
        {
            this.id = id;
            this.type = type;
            this.titleL = titleL;
            this.contentsL = contentsL;
            var idx = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
            this.title = titleL == null ? title : titleL.Count > idx ? titleL[idx] : titleL[0];
            this.contents = contentsL == null ? contents: contentsL.Count > idx ? contentsL[idx] : contentsL[0];
            this.rewards = new ();
            if (rewards != null)
            {
                foreach (var str in rewards)
                {
                    this.rewards.Add(new DbReward(str));
                }
            }
            this.startTime = startTime;
            this.endTime = endTime;
            this.resetTime = resetTime;
            this.users = users;
            

            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        public void OnLanguageChanged(Locale locale)
        {
            var idx = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
            title = titleL == null ? title : titleL.Count > idx ? titleL[idx] : titleL[0];
            contents = contentsL == null ? contents: contentsL.Count > idx ? contentsL[idx] : contentsL[0];
        }
    }
    
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MailType
    {
        Everyday,
        Once,
        Permanent,
        Shop
    }
}