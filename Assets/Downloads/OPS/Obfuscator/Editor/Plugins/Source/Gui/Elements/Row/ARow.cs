using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Gui
using OPS.Editor.Gui;
using OPS.Editor.Gui.Style;

namespace OPS.Editor.Gui
{
    public abstract class ARow<T> : IRow
    {
        // Constructor
        #region Constructor

        public ARow(String _Text, T _RowContent)
        {
            // Text
            this.Text = _Text;

            // Content
            this.RowContent = _RowContent;
        }

        public ARow(String _Text, ASettings _Settings)
        {
            // Text
            this.Text = _Text;

            // Settings
            this.settings = _Settings;
        }

        #endregion

        // Settings
        #region Settings

        private ASettings settings;

        /// <summary>
        /// If no row content got assigned, but a setting, you can use it here to load the row content.
        /// </summary>
        /// <param name="_Settings"></param>
        protected virtual T Load(ASettings _Settings)
        {
            return default(T);
        }

        /// <summary>
        /// Store row content to settings.
        /// </summary>
        /// <param name="_Settings"></param>
        /// <param name="_RowContent"></param>
        protected virtual void Save(ASettings _Settings, T _RowContent)
        {

        }

        #endregion

        // Enabled
        #region Enabled

        /// <summary>
        /// Is this Row enabled or disabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        #endregion

        //Notification
        #region Notification

        /// <summary>
        /// Error notification.
        /// </summary>
        public String Notification_Error { get; set; }

        /// <summary>
        /// Warning notification.
        /// </summary>
        public String Notification_Warning { get; set; }

        /// <summary>
        /// Info notification.
        /// </summary>
        public String Notification_Info { get; set; }

        #endregion

        //Size
        #region Size

        /// <summary>
        /// Min Height of this row.
        /// </summary>
        public float MinHeight { get; protected set; }

        #endregion

        // Setup
        #region Setup

        /// <summary>
        /// Setup the row.
        /// </summary>
        private void SetupGui()
        {
            // Settings - Load
            if (this.settings != null)
            {
                this.RowContent = this.Load(this.settings);
            }

            // Size - MinHeight
            this.MinHeight = 25f;

            // Custom Setup.
            this.OnSetupGui();
        }

        /// <summary>
        /// Override for custom row setup.
        /// </summary>
        protected virtual void OnSetupGui()
        {

        }

        #endregion

        //Text
        #region Text

        /// <summary>
        /// Text describing the row.
        /// </summary>
        public String Text { get; private set; }

        /// <summary>
        /// Set if the Text should be bold.
        /// </summary>
        public bool Bold { get; set; }

        #endregion

        // Content
        #region Content

        /// <summary>
        /// The row content.
        /// </summary>
        System.Object IRow.RowContent
        {
            get
            {
                return this.RowContent;
            }
            set
            {
                this.RowContent = (T)value;
            }
        }

        /// <summary>
        /// The row content as T.
        /// </summary>
        public T RowContent { get; set; }

        #endregion

        // Gui
        #region Gui

        private bool firstCall_Gui = true;

        /// <summary>
        /// Render the row with _RowIndex.
        /// </summary>
        /// <param name="_RowIndex"></param>
        public void Gui(int _RowIndex)
        {
            //Setup
            if (this.firstCall_Gui)
            {
                this.SetupGui();

                this.firstCall_Gui = false;
            }

            bool var_Enabled = GUI.enabled;

            if (!this.Enabled)
            {
                GUI.enabled = false;
            }

            // Row Background
            if (_RowIndex % 2 == 0)
                GUILayout.BeginVertical(AGuiStyle.ActiveStyle.Row_Background_Light, GUILayout.MinHeight(this.MinHeight));
            else
                GUILayout.BeginVertical(AGuiStyle.ActiveStyle.Row_Background_Dark, GUILayout.MinHeight(this.MinHeight));

            // Same space after the row.
            GUILayout.Space(5);

            // Show Notifications
            if (!String.IsNullOrEmpty(this.Notification_Info))
            {
                EditorGUILayout.HelpBox(this.Notification_Info, MessageType.Info);
            }
            if (!String.IsNullOrEmpty(this.Notification_Warning))
            {
                EditorGUILayout.HelpBox(this.Notification_Warning, MessageType.Warning);
            }
            if (!String.IsNullOrEmpty(this.Notification_Error))
            {
                EditorGUILayout.HelpBox(this.Notification_Error, MessageType.Error);
            }

            // Row Content
            GUILayout.BeginHorizontal();

            // Row Content Text
            GUILayout.BeginVertical();

            GUILayout.FlexibleSpace();
            GUILayout.Label(this.Text, this.Bold ? AGuiStyle.ActiveStyle.Row_Label_Big : AGuiStyle.ActiveStyle.Row_Label); // TODO: Maybe bold style.
            GUILayout.FlexibleSpace();

            // End Row Content Text
            GUILayout.EndVertical();

            // Begin check row content modification.
            EditorGUI.BeginChangeCheck();

            // Render Content
            this.OnGui(_RowIndex);

            // End check row content modification.
            if (EditorGUI.EndChangeCheck())
            {
                // Has settings assigned.
                if (this.settings != null)
                {
                    // Store them.
                    this.Save(this.settings, this.RowContent);
                }
            }

            // End Row Content
            GUILayout.EndHorizontal();

            // Same space after the row.
            GUILayout.Space(5);

            // End Row Background
            GUILayout.EndVertical();

            GUI.enabled = var_Enabled;
        }

        /// <summary>
        /// Override to change how the row content will be rendered.
        /// </summary>
        /// <param name="_RowIndex"></param>
        protected virtual void OnGui(int _RowIndex)
        {
        }

        #endregion
    }
}
