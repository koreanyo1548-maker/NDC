using UnityEngine.Localization;

namespace Utils
{
    public interface ILanguageSet
    {
        public void OnLanguageChanged(Locale locale);
    }
}