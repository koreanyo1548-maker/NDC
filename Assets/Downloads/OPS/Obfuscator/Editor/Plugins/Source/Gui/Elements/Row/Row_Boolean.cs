using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Settings
using OPS.Editor.Settings.File;

namespace OPS.Editor.Gui
{
    public class Row_Boolean : ARow<bool>
    {
        public Row_Boolean(String _Text, bool _RowContent)
            : base(_Text, _RowContent)
        {

        }

        public Row_Boolean(String _Text, ASettings _Settings, String _SettingsElementKey)
            : base(_Text, _Settings)
        {
            // Settings
            this.settingsElementKey = _SettingsElementKey;
        }

        // Settings
        #region Settings

        /// <summary>
        /// Key for the settings element.
        /// </summary>
        private String settingsElementKey;

        /// <summary>
        /// Load from settings as bool.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <returns></returns>
        protected override bool Load(ASettings _Settings)
        {
            return _Settings.Get_Setting_AsBool(this.settingsElementKey);
        }

        /// <summary>
        /// Store in settings as bool.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <param name="_RowContent"></param>
        protected override void Save(ASettings _Settings, bool _RowContent)
        {
            _Settings.Add_Or_UpdateSettingElement(this.settingsElementKey, _RowContent);
        }

        #endregion

        //Checkbox
        #region Checkbox

        private Checkbox checkbox;

        #endregion

        //Setup
        #region Setup

        protected override void OnSetupGui()
        {
            base.OnSetupGui();

            // Checkbox
            this.checkbox = new Checkbox(false);
        }

        #endregion

        //Gui
        #region Gui

        protected override void OnGui(int _RowIndex)
        {
            base.OnGui(_RowIndex);

            GUILayout.FlexibleSpace();

            this.RowContent = this.checkbox.OnGui(this.RowContent);
        }

        #endregion
    }
}
