using System;

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
    public class ObfuscatorContainer : AContainer
    {
        public ObfuscatorContainer(ObfuscatorHeader _Header, ObfuscatorDescription _Description, ObfuscatorContent _Content, bool _IsPro)
            : base(_Header, _Description, _Content)
        {
            this.IsPro = _IsPro;

            if (this.IsPro && !AccessHelper.IsProActive())
            {
                this.EnableHeader = false;
                this.EnableDescription = false;
                this.EnableContent = false;
            }
        }

        // Settings
        #region Settings

        /// <summary>
        /// The settings belonging to this component.
        /// </summary>
        public ComponentSettings ComponentSettings { get; }

        #endregion

        //Pro
        #region Pro

        /// <summary>
        /// Is a pro setting.
        /// </summary>
        public bool IsPro { get; }

        #endregion

        // Header
        #region Header

        public new ObfuscatorHeader Header
        {
            get
            {
                return base.Header as ObfuscatorHeader;
            }
        }

        #endregion

        // Description
        #region Description

        public new ObfuscatorDescription Description
        {
            get
            {
                return base.Description as ObfuscatorDescription;
            }
        }

        #endregion

        // Content
        #region Content

        public new ObfuscatorContent Content
        {
            get
            {
                return base.Content as ObfuscatorContent;
            }
        }

        #endregion

        //Update
        #region Update

        public void Update()
        {
            // Show
            this.ShowDescription = this.Header.FoldOut;
            this.ShowContent = this.Header.FoldOut;

            // Enable
            this.EnableContent = this.Header.IsActive;
        }

        #endregion

        // Gui
        #region Gui

        protected override void GuiHeader()
        {
            AGuiStyle var_CurrentStyle = AGuiStyle.ActiveStyle;

            if (this.IsPro)
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle_Pro.Style;
            }
            else
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle.Style;
            }

            base.GuiHeader();

            AGuiStyle.ActiveStyle = var_CurrentStyle;
        }

        protected override void GuiDescription()
        {
            AGuiStyle var_CurrentStyle = AGuiStyle.ActiveStyle;

            if (this.IsPro)
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle_Pro.Style;
            }
            else
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle.Style;
            }

            base.GuiDescription();

            AGuiStyle.ActiveStyle = var_CurrentStyle;
        }

        protected override void GuiContent()
        {
            AGuiStyle var_CurrentStyle = AGuiStyle.ActiveStyle;

            if (this.IsPro)
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle_Pro.Style;
            }
            else
            {
                AGuiStyle.ActiveStyle = ObfuscatorStyle.Style;
            }

            base.GuiContent();

            AGuiStyle.ActiveStyle = var_CurrentStyle;
        }

        #endregion
    }
}