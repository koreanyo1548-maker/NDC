using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Gui
using UnityEngine;
using UnityEditor;

// OPS - Gui
using OPS.Editor.Gui;
using OPS.Editor.Gui.Style;

namespace OPS.Obfuscator.Editor.Gui.Style
{
    public class ObfuscatorStyle : DefaultStyle
    {
        /// <summary>
        /// Static ObfuscatorStyle reference.
        /// </summary>
        public static ObfuscatorStyle Style { get; } = new ObfuscatorStyle();

        // Container
        #region Container

        // Container - Header
        #region Container - Header

        public override GUIStyle Container_Header_Background
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(0.0f, 0.51f, 0.67f, 1.0f));
                return var_GUIStyle;
            }
        }

        public override GUIStyle Container_Header_Label
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.normal.textColor = Color.white;
                var_GUIStyle.fontSize = 15;
                return var_GUIStyle;
            }
        }

        #endregion

        #endregion
    }
}
