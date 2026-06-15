using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Gui
using OPS.Editor.Gui;
using OPS.Editor.Gui.Style;

namespace OPS.Editor.Gui
{
    public interface IRow
    {
        // Enabled
        #region Enabled

        /// <summary>
        /// Is this Row enabled or disabled.
        /// </summary>
        bool Enabled { get; set; }

        #endregion

        // Content
        #region Content

        /// <summary>
        /// The row content.
        /// </summary>
        System.Object RowContent { get; set; }

        #endregion

        // Gui
        #region Gui

        /// <summary>
        /// Render the row with _RowIndex.
        /// </summary>
        /// <param name="_RowIndex"></param>
        void Gui(int _RowIndex);

        #endregion
    }
}
