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
    public class DefaultContent : AContent
    {
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
        /// Rendering the gui.
        /// </summary>
        protected override void OnGui()
        {
            base.OnGui();

            GUILayout.BeginHorizontal(Style.AGuiStyle.ActiveStyle.Container_Content_Background);

            // Main Box
            GUILayout.BeginHorizontal();
            GUILayout.Space(this.Border);
            GUILayout.BeginVertical();

            // Content
            GUILayout.BeginVertical(Style.AGuiStyle.ActiveStyle.Container_Description_Background);

            // Box
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            for (int i = 0; i < this.RowList.Count; i++)
            {
                this.RowList[i].Gui(i);
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
