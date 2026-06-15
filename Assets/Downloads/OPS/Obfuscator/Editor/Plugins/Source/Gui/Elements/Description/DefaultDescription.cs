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
    public class DefaultDescription : ADescription
    {
        public DefaultDescription(String _Text)
        {
            this.Text = _Text;
        }

        //Description
        #region Description

        public String Text { get; private set; }

        #endregion

        //Size
        #region Size

        public int Height { get; private set; }

        #endregion

        //Setup
        #region Setup

        /// <summary>
        /// Setup here your styles.
        /// </summary>
        protected override void OnSetupGui()
        {
            base.OnSetupGui();

            //Height
            this.Height = 20;

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
        /// Rendering the gui.
        /// </summary>
        protected override void OnGui()
        {
            base.OnGui();

            GUILayout.BeginHorizontal(Style.AGuiStyle.ActiveStyle.Container_Content_Background);

            //Main Box
            GUILayout.BeginHorizontal();
            GUILayout.Space(this.Border);
            GUILayout.BeginVertical();

            //Description
            GUILayout.BeginVertical(Style.AGuiStyle.ActiveStyle.Container_Description_Background, GUILayout.Height(this.Height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(new GUIContent(this.Text), Style.AGuiStyle.ActiveStyle.Container_Description_Label);
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            //Seperator to Content
            GUILayout.BeginVertical(Style.AGuiStyle.ActiveStyle.Container_Content_Background);
            GUILayout.Space(this.Border);
            GUILayout.EndVertical();

            //End MainBox
            GUILayout.EndVertical();
            GUILayout.Space(this.Border);
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
