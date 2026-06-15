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
    public class ObfuscatorStyle_Pro : ObfuscatorStyle
    {
        /// <summary>
        /// Static ObfuscatorStyle_Pro reference.
        /// </summary>
        public static new ObfuscatorStyle_Pro Style { get; } = new ObfuscatorStyle_Pro();

        // Container
        #region Container

        // Container - Header
        #region Container - Header

        public override GUIStyle Container_Header_Background
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(0.90f, 0.75f, 0.06f, 0.80f));
                return var_GUIStyle;
            }
        }


        public override GUIStyle Container_Header_Label
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.normal.textColor = Color.black;
                var_GUIStyle.fontSize = 15;
                return var_GUIStyle;
            }
        }

        #endregion

        #endregion

        // Row
        #region Row

        public override GUIStyle Row_Background_Light
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(0.94f, 0.77f, 0.06f, 0.3f));
                return var_GUIStyle;
            }
        }

        public override GUIStyle Row_Background_Dark
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(0.94f, 0.77f, 0.06f, 0.45f));
                return var_GUIStyle;
            }
        }

        #endregion

        // Checkbox
        #region Checkbox

        public override Texture CheckBox_On
        {
            get
            {
                return (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Button/On_Button_Pro.png");
            }
        }

        #endregion


        // FoldOut
        #region FoldOut

        public override Texture FoldOut_In
        {
            get
            {
                return (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Button/Foldout_In_Pro.png");
            }
        }

        public override Texture FoldOut_Out
        {
            get
            {
                return (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Button/Foldout_Out_Pro.png");
            }
        }

        #endregion
    }
}
