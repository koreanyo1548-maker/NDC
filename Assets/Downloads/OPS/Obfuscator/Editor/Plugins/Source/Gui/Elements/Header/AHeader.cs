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
    public abstract class AHeader
    {
        //Setup
        #region Setup

        /// <summary>
        /// Setup the content.
        /// </summary>
        private void SetupGui()
        {
            // Custom Setup.
            this.OnSetupGui();
        }

        /// <summary>
        /// Override for custom content setup.
        /// </summary>
        protected virtual void OnSetupGui()
        {

        }

        #endregion

        //Gui
        #region Gui

        private bool firstCall_Gui = true;

        /// <summary>
        /// Rendering the gui.
        /// </summary>
        public void Gui()
        {
            //Setup
            if (this.firstCall_Gui)
            {
                this.SetupGui();

                this.firstCall_Gui = false;
            }

            this.OnGui();
        }

        /// <summary>
        /// Custom rendering of the gui.
        /// </summary>
        protected virtual void OnGui()
        {
        }

        #endregion
    }
}
