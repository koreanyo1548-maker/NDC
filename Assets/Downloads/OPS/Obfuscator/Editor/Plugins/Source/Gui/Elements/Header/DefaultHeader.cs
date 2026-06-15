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
    public class DefaultHeader : AHeader
    {
        public DefaultHeader(String _HeaderText)
        {
            this.HeaderText = _HeaderText;
        }

        //Header Text
        #region Header Text

        /// <summary>
        /// Text the header shows.
        /// </summary>
        public String HeaderText { get; }

        #endregion

        //Setup
        #region Setup

        /// <summary>
        /// Setup here your styles.
        /// </summary>
        protected override void OnSetupGui()
        {
            base.OnSetupGui();

            // Size
            this.Height = 25;
        }

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Height of the header.
        /// </summary>
        protected int Height { get; private set; }

        /// <summary>
        /// Rendering the gui.
        /// </summary>
        protected override void OnGui()
        {
            base.OnGui();

            //Header
            GUILayout.BeginVertical(Style.AGuiStyle.ActiveStyle.Container_Header_Background, GUILayout.Height(this.Height));

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.Label(new GUIContent(this.HeaderText), Style.AGuiStyle.ActiveStyle.Container_Header_Label);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion
    }
}
