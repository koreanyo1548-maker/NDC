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
    public class Row_Text : ARow<System.Object>
    {
        public Row_Text(String _Text)
            : base(_Text, null)
        {

        }

        //Setup
        #region Setup

        protected override void OnSetupGui()
        {
            base.OnSetupGui();
        }

        #endregion

        //Gui
        #region Gui

        protected override void OnGui(int _RowIndex)
        {
            base.OnGui(_RowIndex);
        }

        #endregion
    }
}
