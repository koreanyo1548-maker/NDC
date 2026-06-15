using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// OPS - Json
using OPS.Serialization.Json;
using OPS.Serialization.Json.Attributes;

namespace OPS.Editor.Settings.File
{
    /// <summary>
    /// Abstract json serializeable settings.
    /// </summary>
    [JsonObject]
    public abstract class ASettings
    {
        // Constructor
        #region Constructor

        public ASettings()
        {
        }

        #endregion

        // Helper
        #region Helper

        /// <summary>
        /// Helper: Parse String to bool.
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        private bool ParseToBool(String _Value)
        {
            return _Value == "True" || _Value == "true";
        }

        /// <summary>
        /// Helper: Parse bool to String.
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        private String ParseToString(bool _Value)
        {
            return _Value ? "True" : "False";
        }

        /// <summary>
        /// Helper: Parse String to Int32.
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        private int ParseToInt(String _Value)
        {
            return Int32.Parse(_Value);
        }

        /// <summary>
        /// Helper: Parse Int32 to String.
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        private String ParseToString(int _Value)
        {
            return _Value.ToString();
        }

        #endregion

        //Settings
        #region Settings

        /// <summary>
        /// Key, Value
        /// </summary>
        [JsonProperty(Name = "SettingElement_Array")]
        private SettingsElement[] settingsElement_Array { get; set; } = new SettingsElement[0];

        /// <summary>
        /// Returns true if the SettingElementArray contains an element with _Key.
        /// </summary>
        /// <param name="_Key"></param>
        /// <returns></returns>
        private bool Contains_SettingElement(String _Key)
        {
            for (int i = 0; i < this.settingsElement_Array.Length; i++)
            {
                if (this.settingsElement_Array[i].Key == _Key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the SettingElement belonging to _Key.
        /// </summary>
        /// <param name="_Key"></param>
        /// <returns></returns>
        private SettingsElement Get_SettingElement(String _Key)
        {
            for (int i = 0; i < this.settingsElement_Array.Length; i++)
            {
                if (this.settingsElement_Array[i].Key == _Key)
                {
                    return this.settingsElement_Array[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Does not check if the element is already added. Just appends it to the array!
        /// </summary>
        /// <param name="_SettingsElement"></param>
        /// <returns></returns>
        private bool Add_SettingElement(SettingsElement _SettingsElement)
        {
            List<SettingsElement> var_SettingElement_List = new List<SettingsElement>(this.settingsElement_Array);
            var_SettingElement_List.Add(_SettingsElement);
            this.settingsElement_Array = var_SettingElement_List.ToArray();

            return true;
        }

        /// <summary>
        /// Add or update a setting element.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Value"></param>
        public void Add_Or_UpdateSettingElement(String _Key, String _Value)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                var_SettingElement = new SettingsElement();
                var_SettingElement.Key = _Key;

                this.Add_SettingElement(var_SettingElement);
            }

            var_SettingElement.Type = ESettingsElementType.String;
            var_SettingElement.Value = _Value;
        }

        /// <summary>
        /// Add or update a setting element.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Value"></param>
        public void Add_Or_UpdateSettingElement(String _Key, bool _Value)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                var_SettingElement = new SettingsElement();
                var_SettingElement.Key = _Key;

                this.Add_SettingElement(var_SettingElement);
            }

            var_SettingElement.Type = ESettingsElementType.Bool;
            var_SettingElement.Value = this.ParseToString(_Value);
        }

        /// <summary>
        /// Add or update a setting element.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Value"></param>
        public void Add_Or_UpdateSettingElement(String _Key, int _Value)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                var_SettingElement = new SettingsElement();
                var_SettingElement.Key = _Key;

                this.Add_SettingElement(var_SettingElement);
            }

            var_SettingElement.Type = ESettingsElementType.Number;
            var_SettingElement.Value = this.ParseToString(_Value);
        }

        /// <summary>
        /// Add or update a setting element.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Value"></param>
        public void Add_Or_UpdateSettingElement(String _Key, Enum _Value)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                var_SettingElement = new SettingsElement();
                var_SettingElement.Key = _Key;

                this.Add_SettingElement(var_SettingElement);
            }

            var_SettingElement.Type = ESettingsElementType.Enum;
            var_SettingElement.Value = _Value.ToString();
        }

        /// <summary>
        /// Add or update a setting element.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Values"></param>
        public void Add_Or_UpdateSettingElement(String _Key, String[] _Values)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                var_SettingElement = new SettingsElement();
                var_SettingElement.Key = _Key;

                this.Add_SettingElement(var_SettingElement);
            }

            var_SettingElement.Type = ESettingsElementType.Array_String;
            var_SettingElement.Values = _Values;
        }

        /// <summary>
        /// Get a setting. If this does not exist _Default will be returned.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        public String Get_Setting_AsString(String _Key, String _Default = "")
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                return _Default;
            }
            else
            {
                return var_SettingElement.Value;
            }
        }

        /// <summary>
        /// Get a setting. If this does not exist _Default will be returned.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        public bool Get_Setting_AsBool(String _Key, bool _Default = false)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                return _Default;
            }
            else
            {
                return this.ParseToBool(var_SettingElement.Value);
            }
        }

        /// <summary>
        /// Get a setting. If this does not exist _Default will be returned.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        public int Get_Setting_AsInt(String _Key, int _Default = 0)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                return _Default;
            }
            else
            {
                return this.ParseToInt(var_SettingElement.Value);
            }
        }

        /// <summary>
        /// Get a setting. If this does not exist _Default will be returned.
        /// </summary>
        /// <param name="_Key"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        public TEnum Get_Setting_AsEnum<TEnum>(String _Key, TEnum _Default = default(TEnum))
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                return _Default;
            }
            else
            {
                try
                {
                    return (TEnum)Enum.Parse(typeof(TEnum), var_SettingElement.Value);
                }
#pragma warning disable
                catch (Exception e)
                {
                    return _Default;
                }
#pragma warning restore
            }
        }

        /// <summary>
        /// Get a setting. If setting with _Key does not exist, an empty array will be returned.
        /// </summary>
        /// <param name="_Key"></param>
        /// <returns></returns>
        public String[] Get_Setting_AsArray(String _Key)
        {
            SettingsElement var_SettingElement = this.Get_SettingElement(_Key);

            if (var_SettingElement == null)
            {
                return new string[0];
            }
            else
            {
                return var_SettingElement.Values;
            }
        }

        #endregion
    }
}
