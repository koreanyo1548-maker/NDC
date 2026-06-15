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
    public class Row_TextArea : ARow<String>
    {
        public Row_TextArea(String _Text, String _Value)
            : base(_Text, _Value)
        {

        }

        public Row_TextArea(String _Text, ASettings _Settings, String _SettingsElementKey)
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
        /// Load from settings as string.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <returns></returns>
        protected override String Load(ASettings _Settings)
        {
            return _Settings.Get_Setting_AsString(this.settingsElementKey);
        }

        /// <summary>
        /// Store in settings as string.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <param name="_RowContent"></param>
        protected override void Save(ASettings _Settings, String _RowContent)
        {
            _Settings.Add_Or_UpdateSettingElement(this.settingsElementKey, _RowContent);
        }

        #endregion

        //Gui
        #region Gui

        protected override void OnGui(int _RowIndex)
        {
            base.OnGui(_RowIndex);

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            this.RowContent = EditorGUILayout.TextArea(this.RowContent, GUILayout.MaxWidth(9999), GUILayout.Height(18 * 10));

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
