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
    public class Row_Button : ARow<System.Object>
    {
        public Row_Button(String _Text, Action _OnButtonPress)
            : base("", null)
        {
            this.buttonText = _Text;
            this.buttonPress = _OnButtonPress;
        }

        //Gui
        #region Gui

        private String buttonText;

        private Action buttonPress;

        protected override void OnGui(int _RowIndex)
        {
            base.OnGui(_RowIndex);

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if(GUILayout.Button(this.buttonText))
            {
                if(this.buttonPress != null)
                {
                    this.buttonPress();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
