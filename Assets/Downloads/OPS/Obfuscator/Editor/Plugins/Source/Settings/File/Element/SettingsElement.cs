using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OPS.Editor.Settings.File
{
    /// <summary>
    /// Element inside a ASetting.
    /// </summary>
    public class SettingsElement
    {
        // Constructor
        #region Constructor

        public SettingsElement()
        {
        }

        public SettingsElement(String _Key, String _Value)
        {
            this.Key = _Key;
            this.Value = _Value;
        }

        public SettingsElement(String _Key, String[] _Values)
        {
            this.Key = _Key;
            this.Values = _Values;
        }

        #endregion

        //Settings
        #region Settings

        /// <summary>
        /// Type of the Setting.
        /// </summary>
        public ESettingsElementType Type { get; set; }

        /// <summary>
        /// Key of the Setting.
        /// </summary>
        public String Key { get; set; }

        /// <summary>
        /// Value of the Setting as String.
        /// </summary>
        public String Value { get; set; }

        /// <summary>
        /// Value Array of the Setting as String Array.
        /// </summary>
        public String[] Values { get; set; }

        #endregion
    }
}
