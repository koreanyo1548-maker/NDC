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
    public class EmptyHeader : AHeader
    {
        public EmptyHeader()
        {
        }

        //Gui
        #region Gui

        /// <summary>
        /// Rendering the gui.
        /// </summary>
        protected override void OnGui()
        {
            base.OnGui();
        }

        #endregion
    }
}
