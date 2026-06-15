using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

namespace OPS.Editor.Gui.Style
{
    public class DefaultStyle : AGuiStyle
    {
        // Container
        #region Container

        // Container - Header
        #region Container - Header

        public override GUIStyle Container_Header_Background
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
                return var_GUIStyle;
            }
        }

        public override GUIStyle Container_Header_Label
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.fontSize = 15;
                return var_GUIStyle;
            }
        }

        #endregion

        // Container - Description
        #region Container - Description

        public override GUIStyle Container_Description_Background
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
                return var_GUIStyle;
            }
        }

        public override GUIStyle Container_Description_Label
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.fontSize = 14;
                return var_GUIStyle;
            }
        }

        #endregion

        // Container - Content
        #region Container - Content

        public override GUIStyle Container_Content_Background
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
                return var_GUIStyle;
            }
        }

        public override GUIStyle Container_Content_Label
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.fontSize = 12;
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
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
                return var_GUIStyle;
            }
        }

        public override GUIStyle Row_Background_Dark
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle();
                var_GUIStyle.normal.background = TextureHelper.MakeTexture(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.2f));
                return var_GUIStyle;
            }
        }

        public override GUIStyle Row_Label
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.fontSize = 12;
                return var_GUIStyle;
            }
        }

        public override GUIStyle Row_Label_Big
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.fontStyle = FontStyle.Bold;
                var_GUIStyle.fontSize = 14;
                return var_GUIStyle;
            }
        }

        #endregion

        // Button
        #region Button

        public override GUIStyle Button
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("label");
                var_GUIStyle.normal.background = null;
                var_GUIStyle.active.background = null;
                return var_GUIStyle;
            }
        }

        #endregion

        // Toolbar
        #region Toolbar

        public override GUIStyle Toolbar
        {
            get
            {
                GUIStyle var_GUIStyle = new GUIStyle("button");
                var_GUIStyle.fontSize = 20;
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
                return (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Button/On_Button.png");
            }
        }

        public override Texture CheckBox_Off
        {
            get
            {
                return (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Button/Off_Button.png");
            }
        }

        #endregion

        // FoldOut
        #region FoldOut

        public override Texture FoldOut_In
        {
            get
            {
                return (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Button/Foldout_In.png");
            }
        }

        public override Texture FoldOut_Out
        {
            get
            {
                return (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Button/Foldout_Out.png");
            }
        }

        #endregion
    }
}
