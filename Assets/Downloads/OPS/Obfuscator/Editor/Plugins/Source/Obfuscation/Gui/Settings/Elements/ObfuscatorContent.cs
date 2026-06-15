using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Gui
using OPS.Editor.Gui;
using OPS.Editor.Gui.Style;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui.Style;

namespace OPS.Obfuscator.Editor.Gui
{
    public class ObfuscatorContent : AContent
    {
        // Row
        #region Row

        private Dictionary<int, bool> rowToPro_Dictionary = new Dictionary<int, bool>();

        public int AddRow(IRow _Row, bool _IsPro)
        {
            int var_Index = base.AddRow(_Row);

            this.rowToPro_Dictionary[var_Index] = _IsPro;

            return var_Index;
        }

        #endregion

        //Setup
        #region Setup

        /// <summary>
        /// Setup here your styles.
        /// </summary>
        protected override void OnSetupGui()
        {
            base.OnSetupGui();

            //Border
            this.Border = 2f;
        }

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Border around the content.
        /// </summary>
        public float Border { get; private set; }

        /// <summary>
        /// Renders the row at _Index.
        /// </summary>
        /// <param name="_Index"></param>
        private void GuiRow(int _Index)
        {
            bool var_Enabled = GUI.enabled;

            AGuiStyle var_CurrentStyle = AGuiStyle.ActiveStyle;

            if (this.rowToPro_Dictionary.ContainsKey(_Index) && this.rowToPro_Dictionary[_Index])
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle_Pro.Style;

                if (!AccessHelper.IsProActive())
                {
                    GUI.enabled = false;
                }
            }
            else
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle.Style;
            }

            this.RowList[_Index].Gui(_Index);

            AGuiStyle.ActiveStyle = var_CurrentStyle;

            GUI.enabled = var_Enabled;
        }

        /// <summary>
        /// Rendering the gui.
        /// </summary>
        protected override void OnGui()
        {
            base.OnGui();

            GUILayout.BeginHorizontal(Style.ObfuscatorStyle.ActiveStyle.Container_Content_Background);

            // Main Box
            GUILayout.BeginHorizontal();
            GUILayout.Space(this.Border);
            GUILayout.BeginVertical();

            // Content
            GUILayout.BeginVertical(Style.ObfuscatorStyle.ActiveStyle.Container_Description_Background);

            // Box
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            for (int i = 0; i < this.RowList.Count; i++)
            {
                this.GuiRow(i);
            }

            // End Box
            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            // End Content
            GUILayout.EndVertical();

            // End MainBox
            GUILayout.Space(this.Border);
            GUILayout.EndVertical();
            GUILayout.Space(this.Border);
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
