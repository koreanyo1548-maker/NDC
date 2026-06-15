using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Gui
using OPS.Editor.Gui.Style;

namespace OPS.Editor.Gui
{
    public class Checkbox
    {
        public Checkbox(bool _Header)
        {
            this.header = _Header;
        }

        //Header
        #region Header

        private bool header;

        #endregion

        //Texture
        #region Texture

        private Texture texture_On;
        private Texture texture_Off;

        #endregion

        //Size
        #region Size

        private int width;
        private int height;

        #endregion

        //Setup
        #region Setup

        private void SetupGui()
        {
            //Texture
            this.texture_On = AGuiStyle.ActiveStyle.CheckBox_On;

            this.texture_Off = AGuiStyle.ActiveStyle.CheckBox_Off;

            //Size
            if (this.header)
            {
                this.width = 80;
                this.height = 26;
            }
            else
            {
                this.width = 70;
                this.height = 25;
            }
        }

        #endregion

        //Gui
        #region Gui

        private bool firstCall_Gui = true;

        public bool OnGui(bool _CurrentValue)
        {
            //Setup
            if (this.firstCall_Gui)
            {
                this.SetupGui();

                this.firstCall_Gui = false;
            }

            bool var_Result = _CurrentValue;

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_CurrentValue ? this.texture_On : this.texture_Off, AGuiStyle.ActiveStyle.Button, GUILayout.Width(this.width), GUILayout.Height(this.height)))
            {
                var_Result = !_CurrentValue;

                GUI.changed = true;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            return var_Result;
        }

        #endregion
    }
}
