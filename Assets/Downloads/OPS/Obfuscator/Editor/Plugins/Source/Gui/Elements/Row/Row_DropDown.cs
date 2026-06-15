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
    public class Row_DropDown : ARow<String>
    {
        public Row_DropDown(String _Text, String[] _DropDownValues, String _ActiveValue, Action _OnValueChanged)
            : base(_Text, _ActiveValue)
        {
            this.DropDownValues = _DropDownValues;
            this.onValueChanged = _OnValueChanged;
        }

        public Row_DropDown(String _Text, String[] _DropDownValues, ASettings _Settings, String _SettingsElementKey, Action _OnValueChanged)
            : base(_Text, _Settings)
        {
            this.DropDownValues = _DropDownValues;
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
        /// Load from settings as string.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <returns></returns>
        protected override String Load(ASettings _Settings)
        {
            int var_Index = this.GetIdByString(_Settings.Get_Setting_AsString(this.settingsElementKey));

            return this.GetStringById(var_Index);
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
        /// Selected Index in DropDownValues.
        /// </summary>
        public int Selected { get; private set; }

        /// <summary>
        /// Valid options for the DropDown.
        /// </summary>
        public String[] DropDownValues { get; private set; }

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

            this.Selected = EditorGUILayout.Popup(this.GetIdByString(this.RowContent), this.DropDownValues);

            if (EditorGUI.EndChangeCheck())
            {
                this.RowContent = this.GetStringById(this.Selected);

                this.CallOnValueChanged();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion

        // Helper
        #region Helper

        private String GetStringById(int _Id)
        {
            return this.DropDownValues[_Id];
        }

        private int GetIdByString(String _Value)
        {
            for (int i = 0; i < this.DropDownValues.Length; i++)
            {
                if (this.DropDownValues[i] == _Value)
                {
                    return i;
                }
            }
            return 0;
        }

        #endregion
    }
}
