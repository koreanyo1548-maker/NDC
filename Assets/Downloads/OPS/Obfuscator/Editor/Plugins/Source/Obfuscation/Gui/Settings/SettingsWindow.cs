using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Settings
using OPS.Obfuscator.Editor.Settings;
using OPS.Editor.Settings.File;

namespace OPS.Obfuscator.Editor.Gui
{
    public class SettingsWindow : EditorWindow
    {
        [MenuItem("OPS/Obfuscator/Settings")]

        public static void ShowWindow()
        {
            var var_Window = EditorWindow.GetWindow(typeof(SettingsWindow));
            var_Window.titleContent = new GUIContent("Obfuscator");
        }

        private void OnEnable()
        {
            // Load container settings.
            this.Load_Settings();

            // Setup the container gui.
            this.Setup_Gui();
        }

        private void OnDisable()
        {
            //Save container settings.
            this.Save_Settings();
        }

        private ObfuscatorSettings settings;

        private void Load_Settings()
        {
            this.settings = ObfuscatorSettings.Load();
        }

        private void Save_Settings()
        {
            if (this.settings != null)
            {
                this.settings.Save();
            }
        }

        //Container
        #region Container

        private Dictionary<EObfuscatorCategory, List<ObfuscatorContainer>> categoryContainerListDictionary;

        private void LoadAllContainer()
        {
            this.categoryContainerListDictionary = new Dictionary<EObfuscatorCategory, List<ObfuscatorContainer>>();

            // Get all gui components.
            var var_ComponentList = OPS.Obfuscator.Editor.Component.Gui.Helper.GuiComponentHelper.GetGuiComponents();
            for (int i = 0; i < var_ComponentList.Count; i++)
            {
                // Get settings for the component.
                ComponentSettings var_ComponentSettings = this.settings.Get_Or_Create_ComponentSettings(var_ComponentList[i].SettingsKey);

                // Get the gui container.
                ObfuscatorContainer var_Container = var_ComponentList[i].GetGuiContainer(var_ComponentSettings);

                // Assign tho the category to container list.
                if (this.categoryContainerListDictionary.TryGetValue(var_ComponentList[i].ObfuscatorCategory, out List<ObfuscatorContainer> var_ObfuscatorContainerList))
                {
                    var_ObfuscatorContainerList.Add(var_Container);
                }
                else
                {
                    this.categoryContainerListDictionary.Add(var_ComponentList[i].ObfuscatorCategory, new List<ObfuscatorContainer>() { var_Container });
                }
            }

            //Custom
        }

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Scrollview position 2d.
        /// </summary>
        private Vector2 scrollPosition;

        /// <summary>
        /// Obfuscator Text
        /// </summary>
        private Row_Text row_Obfuscator;

        /// <summary>
        /// Activate or Deactive Obfuscation.
        /// </summary>
        private Row_Boolean row_GlobalObfuscation;

        /// <summary>
        /// The tab index.
        /// </summary>
        private int tabIndex = 0;

        private void Setup_Gui()
        {
            try
            {
                // Obfuscator Text
                this.row_Obfuscator = new Row_Text("Obfuscator v. " + ObfuscatorSettings.Global_Obfuscator_Version_Value);
                this.row_Obfuscator.Bold = true;

                // Obfuscator Activation
                this.row_GlobalObfuscation = new Row_Boolean("Enable Obfuscation:", this.settings, ObfuscatorSettings.Global_Enable_Obfuscation);
                this.row_GlobalObfuscation.Bold = true;

                // Create Container Gui Objects.
                this.LoadAllContainer();
            }
            catch (Exception e)
            {
                Debug.LogError("[OPS.OBF] " + e.ToString());
                this.Close();
            }
        }

        private void OnGUI()
        {
            try
            {
                //Start Scroll
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                GUIStyle var_TopBarButtonStyle = new GUIStyle("button");
                var_TopBarButtonStyle.normal.background = null;
                var_TopBarButtonStyle.active.background = null;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button((Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Rate.png"), var_TopBarButtonStyle, GUILayout.MaxWidth(100), GUILayout.MaxHeight(26)))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/obfuscator-pro-89589");
                }
                if (GUILayout.Button((Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/BugQuestion.png"), var_TopBarButtonStyle, GUILayout.MaxWidth(200), GUILayout.MaxHeight(26)))
                {
                    Application.OpenURL("mailto:orangepearsoftware@gmail.com?subject=ObfuscatorPro_Bug");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label((Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Header_Icon.png"), GUILayout.MaxWidth(24), GUILayout.MaxHeight(24), GUILayout.MinWidth(24), GUILayout.MinHeight(24));
                this.row_Obfuscator.Gui(1);
                GUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("De-/Activate Obfuscator here.", MessageType.Info);

                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                this.row_GlobalObfuscation.Gui(1);
                GUILayout.EndHorizontal();

                bool var_Enabled = GUI.enabled;

                if (!this.settings.Get_Setting_AsBool(ObfuscatorSettings.Global_Enable_Obfuscation))
                {
                    GUI.enabled = false;
                }

                this.tabIndex = GUILayout.Toolbar(this.tabIndex, new GUIContent[] { new GUIContent("Obfuscation", (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Icon/Icon_Obfuscation.png")),
                    new GUIContent("Security", (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Icon/Icon_Security.png")),
                    new GUIContent("Compatibility", (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Icon/Icon_Compatibility.png")),
                    new GUIContent("Optional", (Texture)EditorGUIUtility.Load("Assets/Downloads/OPS/Obfuscator/Editor/Gui/Resources/Icon/Icon_Optional.png")) }, GUILayout.Height(30f));

                switch (this.tabIndex)
                {
                    case 0:
                        {
                            if (this.categoryContainerListDictionary.TryGetValue(EObfuscatorCategory.Obfuscation, out List<ObfuscatorContainer> var_ObfuscatorContainerList))
                            {
                                for (int i = 0; i < var_ObfuscatorContainerList.Count; i++)
                                {
                                    var_ObfuscatorContainerList[i].Gui();
                                }
                            }

                            break;
                        }
                    case 1:
                        {
                            if (this.categoryContainerListDictionary.TryGetValue(EObfuscatorCategory.Security, out List<ObfuscatorContainer> var_ObfuscatorContainerList))
                            {
                                for (int i = 0; i < var_ObfuscatorContainerList.Count; i++)
                                {
                                    var_ObfuscatorContainerList[i].Gui();
                                }
                            }

                            break;
                        }
                    case 2:
                        {
                            if (this.categoryContainerListDictionary.TryGetValue(EObfuscatorCategory.Compatibility, out List<ObfuscatorContainer> var_ObfuscatorContainerList))
                            {
                                for (int i = 0; i < var_ObfuscatorContainerList.Count; i++)
                                {
                                    var_ObfuscatorContainerList[i].Gui();
                                }
                            }

                            break;
                        }
                    case 3:
                        {
                            if (this.categoryContainerListDictionary.TryGetValue(EObfuscatorCategory.Optional, out List<ObfuscatorContainer> var_ObfuscatorContainerList))
                            {
                                for (int i = 0; i < var_ObfuscatorContainerList.Count; i++)
                                {
                                    var_ObfuscatorContainerList[i].Gui();
                                }
                            }

                            break;
                        }
                }

                GUI.enabled = var_Enabled;

                //End Scroll
                GUILayout.EndScrollView();

                //Save Gui.
                if (GUI.changed)
                {
                    this.Save_Settings();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[OPS.OBF] " + e.ToString());
                this.Close();
            }
        }

        #endregion

        //Update
        #region Update

        private void Update()
        {
            foreach (var var_Pair in this.categoryContainerListDictionary)
            {
                for (int i = 0; i < var_Pair.Value.Count; i++)
                {
                    var_Pair.Value[i].Update();
                }
            }
        }

        #endregion
    }
}