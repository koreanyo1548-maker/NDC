using Data.DbShop;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace UIs.Shop.DefaultShop
{
    public class UI_ShopDiaSale_Item: UI_Shop_Item, ILanguageSet
    {
        private IDbShop _item;
        public override void SetInfo(IDbShop item)
        {
            base.SetInfo(item);
            _item = item;
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Sale", true).text = string.Format(LocalString.Get(210307), item.GetValue());
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        public void OnLanguageChanged(Locale locale)
        {
            Util.FindChild<TextMeshProUGUI>(gameObject, "T_Sale", true).text = string.Format(LocalString.Get(210307), _item.GetValue());
        }
    }
}