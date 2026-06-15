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
    public class Row_Multi_Boolean : ARow<bool[]>
    {
        public Row_Multi_Boolean(String _Text, String[] _Text_Array, bool[] _Value_Array)
            : base(_Text, _Value_Array)
        {
            this.tableTextArray = _Text_Array;
        }

        public Row_Multi_Boolean(String _Text, String[] _Text_Array, ASettings _Settings, String[] _SettingsElementKey_Array)
            : base(_Text, _Settings)
        {
            this.tableTextArray = _Text_Array;

            // Settings
            this.settingsElementKey_Array = _SettingsElementKey_Array;
        }

        // Settings
        #region Settings

        /// <summary>
        /// Key array for the settings elements.
        /// </summary>
        private String[] settingsElementKey_Array;

        /// <summary>
        /// Load from settings as string to bool array.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <returns></returns>
        protected override bool[] Load(ASettings _Settings)
        {
            bool[] var_Result = new bool[this.settingsElementKey_Array.Length];

            for (int i = 0; i < this.settingsElementKey_Array.Length; i++)
            {
                var_Result[i] = _Settings.Get_Setting_AsBool(this.settingsElementKey_Array[i]);
            }

            return var_Result;
        }

        /// <summary>
        /// Store in settings as string to bool array.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <param name="_RowContent"></param>
        protected override void Save(ASettings _Settings, bool[] _RowContent)
        {
            for (int i = 0; i < this.settingsElementKey_Array.Length; i++)
            {
                _Settings.Add_Or_UpdateSettingElement(this.settingsElementKey_Array[i], this.RowContent[i]);
            }
        }

        #endregion

        // Table
        #region Table

        private GUIStyle tableHeaderGUIStyle;

        private int tableTextWidth;
        private int tableTextHeight;

        // Table - Checkbox
        #region Checkbox

        private String[] tableTextArray;

        #endregion

        // Table - Checkbox
        #region Checkbox

        private Checkbox[] tableCheckboxArray;

        #endregion

        #endregion

        //Setup
        #region Setup

        protected override void OnSetupGui()
        {
            base.OnSetupGui();

            //Table
            this.tableHeaderGUIStyle = new GUIStyle("label");
            this.tableHeaderGUIStyle.alignment = TextAnchor.UpperLeft; //MiddleLeft
            this.tableHeaderGUIStyle.fontSize = 12;

            this.tableTextWidth = 100;
            this.tableTextHeight = 20;

            //Checkbox
            this.tableCheckboxArray = new Checkbox[this.RowContent.Length];

            for (int i = 0; i < this.RowContent.Length; i++)
            {
                this.tableCheckboxArray[i] = new Checkbox(false);
            }
        }

        #endregion

        //Gui
        #region Gui

        protected override void OnGui(int _RowIndex)
        {
            base.OnGui(_RowIndex);

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            for (int i = 0; i < this.RowContent.Length; i++)
            {
                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical();

                GUILayout.FlexibleSpace();

                GUILayout.Label(this.tableTextArray[i], this.tableHeaderGUIStyle, GUILayout.Width(this.tableTextWidth), GUILayout.Height(this.tableTextHeight));

                //GUILayout.BeginHorizontal();
                //GUILayout.FlexibleSpace();

                this.RowContent[i] = this.tableCheckboxArray[i].OnGui(this.RowContent[i]);

                //GUILayout.FlexibleSpace();
                //GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion
    }
}
