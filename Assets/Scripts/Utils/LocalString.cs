using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utils
{
    public static class LocalString
    {
        private static string _table = "Strings";
        public static string Get(int keyName) {
            var localizeString = new LocalizedString { TableReference = _table, TableEntryReference = keyName.ToString()};
            var stringOperation = localizeString.GetLocalizedStringAsync();
    
            if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded) {
                return stringOperation.Result;
            }

            return string.Empty;
        }
        public static LocalizedString GetLocalized(int keyName) {
            var localizeString = new LocalizedString { TableReference = _table, TableEntryReference = keyName.ToString()};
            return localizeString;
            // var stringOperation = localizeString.GetLocalizedStringAsync();
            //
            // if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded) {
            //     return stringOperation .Result;
            // }
            //
            // return string.Empty;
        }
    }
}