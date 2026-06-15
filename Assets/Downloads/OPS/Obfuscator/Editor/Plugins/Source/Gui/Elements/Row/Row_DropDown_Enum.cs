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
    public class Row_DropDown_Enum<TEnum> : ARow<TEnum>
        where TEnum : Enum
    {
        public Row_DropDown_Enum(String _Text, TEnum _DropDownValue, Action _OnValueChanged)
            : base(_Text, _DropDownValue)
        {
            this.onValueChanged = _OnValueChanged;
        }

        public Row_DropDown_Enum(String _Text, ASettings _Settings, String _SettingsElementKey, Action _OnValueChanged)
            : base(_Text, _Settings)
        {
            this.onValueChanged = _OnValueChanged;

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
        /// Load from settings as enum.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <returns></returns>
        protected override TEnum Load(ASettings _Settings)
        {
            return _Settings.Get_Setting_AsEnum<TEnum>(this.settingsElementKey);
        }

        /// <summary>
        /// Store in settings as enum.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <param name="_RowContent"></param>
        protected override void Save(ASettings _Settings, TEnum _RowContent)
        {
            _Settings.Add_Or_UpdateSettingElement(this.settingsElementKey, (Enum)_RowContent);
        }

        #endregion

        //Setup
        #region Setup

        protected override void OnSetupGui()
        {
            base.OnSetupGui();

            // First call, to notify listener!
            this.CallOnValueChanged();
        }

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Action when the drop down value changed.
        /// </summary>
        private Action onValueChanged;

        /// <summary>
        /// Call onValueChanged if it is not null!
        /// </summary>
        private void CallOnValueChanged()
        {
            if (this.onValueChanged != null)
            {
                this.onValueChanged();
            }
        }

        protected override void OnGui(int _RowIndex)
        {
            base.OnGui(_RowIndex);

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();

            this.RowContent = (TEnum)EditorGUILayout.EnumPopup((Enum)this.RowContent);

            if (EditorGUI.EndChangeCheck())
            {
                this.CallOnValueChanged();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
