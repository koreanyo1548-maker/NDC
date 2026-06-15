using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Settings
using OPS.Editor.Settings.File;

namespace OPS.Editor.Gui
{
    public abstract class AContent
    {
        // Constructor
        #region Constructor

        public AContent()
        {

        }

        #endregion

        // Row
        #region Row

        /// <summary>
        /// Renderer rows in the content.
        /// </summary>
        public List<IRow> RowList { get; } = new List<IRow>();

        /// <summary>
        ///  Adds the _Row to the row list.
        /// </summary>
        /// <param name="_Row"></param>
        /// <returns></returns>
        public int AddRow(IRow _Row)
        {
            // Add to row list.
            this.RowList.Add(_Row);

            // Custom post process.
            this.OnAddRow(_Row);

            // Return the count -1, is the index.
            return this.RowList.Count - 1;
        }

        /// <summary>
        /// Called after _Row got add to the row list.
        /// </summary>
        /// <param name="_Row"></param>
        protected virtual void OnAddRow(IRow _Row)
        {

        }

        #endregion

        // Setup
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

        // Gui
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