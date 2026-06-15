using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;

namespace OPS.Editor.Gui.Style
{
    public abstract class AGuiStyle
    {
        public static AGuiStyle ActiveStyle { get; set; } = new DefaultStyle();

        // Container
        #region Container

        // Container - Header
        #region Container - Header

        public abstract GUIStyle Container_Header_Background { get; }

        public abstract GUIStyle Container_Header_Label { get; }

        #endregion

        // Container - Description
        #region Container - Description

        public abstract GUIStyle Container_Description_Background { get; }

        public abstract GUIStyle Container_Description_Label { get; }

        #endregion

        // Container - Content
        #region Container - Content

        public abstract GUIStyle Container_Content_Background { get; }

        public abstract GUIStyle Container_Content_Label { get; }

        #endregion

        #endregion

        // Row
        #region Row

        public abstract GUIStyle Row_Background_Light { get; }

        public abstract GUIStyle Row_Background_Dark { get; }

        public abstract GUIStyle Row_Label { get; }

        public abstract GUIStyle Row_Label_Big { get; }

        #endregion

        // Button
        #region Button

        public abstract GUIStyle Button { get; }

        #endregion

        // Toolbar
        #region Toolbar

        public abstract GUIStyle Toolbar { get; }

        #endregion

        // Checkbox
        #region Checkbox

        public abstract Texture CheckBox_On { get; }

        public abstract Texture CheckBox_Off { get; }

        #endregion

        // FoldOut
        #region FoldOut

        public abstract Texture FoldOut_In { get; }

        public abstract Texture FoldOut_Out { get; }

        #endregion
    }
}
