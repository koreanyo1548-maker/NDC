using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Gui
using OPS.Editor.Gui;
using OPS.Editor.Gui.Style;

// OPS - Settings
using OPS.Editor.Settings.File;

namespace OPS.Obfuscator.Editor.Gui
{
    public class ObfuscatorHeader : AHeader
    {
        public ObfuscatorHeader(String _HeaderText, ComponentSettings _ComponentSettings, String _ActivateAbleSettingId)
        {
            this.HeaderText = _HeaderText;

            this.ComponentSettings = _ComponentSettings;
            this.ActivateAbleSettingId = _ActivateAbleSettingId;

            this.FoldOut = true;

            if (!this.IsActivateAble)
            {
                this.IsActive = true;
            }
        }

        public ObfuscatorHeader(String _HeaderText)
            : this(_HeaderText, null, null)
        {
        }

        // Header Text
        #region Header Text

        /// <summary>
        /// Text the header shows.
        /// </summary>
        public String HeaderText { get; }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// Settings.
        /// </summary>
        protected ComponentSettings ComponentSettings { get; private set; }

        /// <summary>
        /// Load content by settings.
        /// </summary>
        private void Load()
        {
            this.IsActive = this.ComponentSettings.Get_Setting_AsBool(this.ActivateAbleSettingId);
        }

        /// <summary>
        /// Store content to settings.
        /// </summary>
        private void Save()
        {
            this.ComponentSettings.Add_Or_UpdateSettingElement(this.ActivateAbleSettingId, this.IsActive);
        }

        #endregion

        // ActivateAble
        #region ActivateAble

        /// <summary>
        /// Is activate able header.
        /// </summary>
        public bool IsActivateAble
        {
            get
            {
                return this.ActivateAbleSettingId != null;
            }
        }

        /// <summary>
        /// Settings used to active/deactive.
        /// </summary>
        public String ActivateAbleSettingId { get; }

        #endregion

        // IsActive
        #region IsActive

        /// <summary>
        /// The header checkbox value.
        /// </summary>
        public bool IsActive { get; private set; }

        #endregion

        // Setup
        #region Setup

        /// <summary>
        /// Setup here your styles.
        /// </summary>
        protected override void OnSetupGui()
        {
            base.OnSetupGui();

            // Settings - Load
            if (this.IsActivateAble)
            {
                this.Load();
            }

            // Texture
            this.FoldOut_Texture_In = AGuiStyle.ActiveStyle.FoldOut_In;
            this.FoldOut_Texture_Out = AGuiStyle.ActiveStyle.FoldOut_Out;

            // Size
            this.Height = 25;

            // CheckBox
            this.checkbox = new Checkbox(true);
        }

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Height of the header.
        /// </summary>
        protected int Height { get; private set; }

        /// <summary>
        /// Is foldout or not.
        /// </summary>
        public bool FoldOut { get; protected set; }

        /// <summary>
        /// Foldin texture.
        /// </summary>
        protected Texture FoldOut_Texture_In { get; private set; }

        /// <summary>
        /// Foldout texture.
        /// </summary>
        protected Texture FoldOut_Texture_Out { get; private set; }

        /// <summary>
        /// Checkbox for the activation.
        /// </summary>
        private Checkbox checkbox;

        /// <summary>
        /// Rendering the gui.
        /// </summary>
        protected override void OnGui()
        {
            base.OnGui();

            //Header
            GUILayout.BeginVertical(AGuiStyle.ActiveStyle.Container_Header_Background, GUILayout.Height(this.Height));

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(20));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(this.FoldOut ? this.FoldOut_Texture_Out : this.FoldOut_Texture_In, AGuiStyle.ActiveStyle.Button, GUILayout.Height(this.Height), GUILayout.Width(this.Height)))
            {
                this.FoldOut = !this.FoldOut;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.Label(new GUIContent(this.HeaderText), AGuiStyle.ActiveStyle.Container_Header_Label);
            GUILayout.EndVertical();

            if (this.IsActivateAble)
            {
                GUILayout.FlexibleSpace();

                // Begin check content modification.
                EditorGUI.BeginChangeCheck();

                this.IsActive = this.checkbox.OnGui(this.IsActive);

                // End check content modification.
                if (EditorGUI.EndChangeCheck())
                {
                    // Has settings assigned.
                    if (this.ComponentSettings != null)
                    {
                        // Store them.
                        this.Save();
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion
    }
}
