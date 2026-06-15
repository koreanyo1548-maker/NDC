using System;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Settings
using OPS.Editor.Settings.File;

namespace OPS.Editor.Gui
{
    public abstract class AContainer
    {
        public AContainer(AHeader _Header, ADescription _Description, AContent _Content)
        {
            this.Header = _Header;
            this.Description = _Description;
            this.Content = _Content;

            this.ShowHeader = true;
            this.ShowDescription = true;
            this.ShowContent = true;

            this.EnableHeader = true;
            this.EnableDescription = true;
            this.EnableContent = true;
        }

        // Header
        #region Header

        /// <summary>
        /// Render the header.
        /// </summary>
        public AHeader Header { get; private set; }

        /// <summary>
        /// Render the header or dont.
        /// </summary>
        public bool ShowHeader { get; set; }

        /// <summary>
        /// Enable the header or dont.
        /// </summary>
        public bool EnableHeader { get; set; }

        #endregion

        // Description
        #region Description

        /// <summary>
        /// Render the description.
        /// </summary>
        public ADescription Description { get; private set; }

        /// <summary>
        /// Render the description or dont.
        /// </summary>
        public bool ShowDescription { get; set; }

        /// <summary>
        /// Enable the description or dont.
        /// </summary>
        public bool EnableDescription { get; set; }

        #endregion

        //Content
        #region Content

        /// <summary>
        /// Render the content.
        /// </summary>
        public AContent Content { get; private set; }

        /// <summary>
        /// Render the content or dont.
        /// </summary>
        public bool ShowContent { get; set; }

        /// <summary>
        /// Enable the content or dont.
        /// </summary>
        public bool EnableContent { get; set; }

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Rendering of the gui.
        /// </summary>
        public void Gui()
        {
            // Box
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Space(7);

            // Header
            if (this.ShowHeader)
            {
                bool var_Enabled = GUI.enabled;

                GUI.enabled = this.EnableHeader && var_Enabled;

                this.GuiHeader();

                GUI.enabled = var_Enabled;
            }

            // Description
            if (this.ShowDescription)
            {
                bool var_Enabled = GUI.enabled;

                GUI.enabled = this.EnableDescription && var_Enabled;

                this.GuiDescription();

                GUI.enabled = var_Enabled;
            }

            // Content
            if (this.ShowContent)
            {
                bool var_Enabled = GUI.enabled;

                GUI.enabled = this.EnableContent && var_Enabled;

                this.GuiContent();

                GUI.enabled = var_Enabled;
            }

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }

        protected virtual void GuiHeader()
        {
            this.Header.Gui();
        }

        protected virtual void GuiDescription()
        {
            this.Description.Gui();
        }

        protected virtual void GuiContent()
        {
            this.Content.Gui();
        }

        #endregion
    }
}