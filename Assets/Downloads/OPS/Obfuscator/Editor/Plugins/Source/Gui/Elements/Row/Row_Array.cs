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
    public class Row_Array : ARow<String[]>
    {
        public Row_Array(String _Text, String[] _RowArray)
            : base(_Text, _RowArray)
        {

        }

        public Row_Array(String _Text, ASettings _Settings, String _SettingsElementKey)
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
        /// Load from settings as array.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <returns></returns>
        protected override string[] Load(ASettings _Settings)
        {
            return _Settings.Get_Setting_AsArray(this.settingsElementKey);
        }

        /// <summary>
        /// Store in settings as array.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <param name="_RowContent"></param>
        protected override void Save(ASettings _Settings, string[] _RowContent)
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

            GUILayout.Space(2);

            GUILayout.FlexibleSpace();

            for (int i = 0; i < this.RowContent.Length; i++)
            {
                this.RowContent[i] = GUILayout.TextField(this.RowContent[i], GUILayout.MaxWidth(9999), GUILayout.Height(18));
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Line"))
            {
                List<String> var_TempList = new List<string>(this.RowContent);
                var_TempList.Add("");
                this.RowContent = var_TempList.ToArray();

                GUI.changed = true;
            }
            if (GUILayout.Button("Remove Line"))
            {
                List<String> var_TempList = new List<string>(this.RowContent);
                if (var_TempList.Count > 0)
                {
                    var_TempList.RemoveAt(var_TempList.Count - 1);
                }
                this.RowContent = var_TempList.ToArray();

                GUI.changed = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.Space(2);

            GUILayout.EndVertical();
        }

        #endregion
    }
}
