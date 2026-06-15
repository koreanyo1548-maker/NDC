using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Obfuscator - Settings
using OPS.Obfuscator.Editor.Settings;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Optional;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Gui
{
    public class ErrorStackWindow : EditorWindow
    {
        // Mapping
        #region Mapping

        private ObfuscatorV4Mapping renameMapping;

        private bool LoadMapping()
        {
            if (this.renameMapping == null)
            {
                String var_FilePath = this.gui_Mapping_File_Row.RowContent;

                if (!System.IO.File.Exists(var_FilePath))
                {
                    UnityEngine.Debug.LogWarning("There is no file at: " + var_FilePath);

                    return false;
                }

                try
                {
                    // Read mapping.
                    using (System.IO.StreamReader var_Reader = new System.IO.StreamReader(new System.IO.FileStream(var_FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read)))
                    {
                        this.renameMapping = OPS.Serialization.Json.JsonSerializer.ParseStringToObject<ObfuscatorV4Mapping>(var_Reader.ReadToEnd());
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning("File at: " + var_FilePath + " is no valid mapping! Exception: " + e.ToString());

                    return false;
                }
            }

            return true;
        }

        #endregion

        //Container
        #region Container

        private DefaultContainer gui_Container;

        private Row_TextBox gui_Mapping_File_Row;

        private Row_TextArea gui_StackTrace_Row;

        private Row_TextArea gui_StackTrace_Deobfuscated_Row;

        private void LoadContainer()
        {
            if (this.gui_Container == null)
            {
                // Header
                AHeader var_Header = new EmptyHeader();

                // Description
                ADescription var_Description = new DefaultDescription("Stacktrace - Deobfuscation");

                // Content
                AContent var_Content = new DefaultContent();

                // Rows
                this.gui_Mapping_File_Row = new Row_TextBox("Mapping File Path", mappingFilePath);
                this.gui_Mapping_File_Row.Notification_Info = "Enter here the mapping file path you want to use for deobfuscation.";
                var_Content.AddRow(this.gui_Mapping_File_Row);

                this.gui_StackTrace_Row = new Row_TextArea("StackTrace", stackTrace);
                this.gui_StackTrace_Row.Notification_Info = "Enter here the obfuscated stack trace you want to deobfuscate. Has to be in a format like:\n" + @"at ObjectModel.Character.bju()[0x00003] in D:\...\bju.cs:55 
  at GameItems.bsr(Game.ObjectModel.g a, ObjectModel.ud b, cy c)[0x000e6] in D:\...\bsr.cs:523
  at ho`2[a, b].bwx(System.Int32 a, System.Int32 b, System.Int32 c, System.Boolean d)[0x00075] in D:\...\bwx.cs:209
  at fg.sa()[0x0069c] in D:\...\sa.cs:756
  at jp`1[a].z()[0x0005f] in D:\...\jp.cs:79";
                var_Content.AddRow(this.gui_StackTrace_Row);

                Row_Button var_DeObfuscateButton = new Row_Button("Deobfuscate", this.DeObfuscateStackTrace);
                var_Content.AddRow(var_DeObfuscateButton);

                this.gui_StackTrace_Deobfuscated_Row = new Row_TextArea("Deobfuscated StackTrace", "");
                var_Content.AddRow(this.gui_StackTrace_Deobfuscated_Row);

                this.gui_Container = new DefaultContainer(var_Header, var_Description, var_Content);
            }
        }

        private void DeObfuscateStackTrace()
        {
            // Load the mapping if there is one.
            if(!this.LoadMapping())
            {
                return;
            }

            // Iterate stack trace lines and deobfuscate.
            this.IterateStackTraceLines();

            // Reset focus!
            GUI.FocusControl(null);

            // NOTE: Try: EditorGUI.FocusTextInControl(null);
        }

        #endregion

        [MenuItem("OPS/Obfuscator/ErrorStack")]

        public static void ShowWindow()
        {
            var var_Window = EditorWindow.GetWindow(typeof(ErrorStackWindow));
            var_Window.titleContent = new GUIContent("Obfuscator Error Stack");
        }

        private void OnEnable()
        {
            // Load settings and container settings.
            this.Load();
        }

        private void OnDisable()
        {
        }

        private void Load()
        {
            try
            {
                // Load Container
                this.LoadContainer();
            }
            catch (Exception e)
            {
                Debug.LogError("[OPS.OBF] " + e.ToString());
                this.Close();
            }
        }

        //Gui
        #region Gui

        /// <summary>
        /// Value / Full file path.
        /// </summary>
        private static String mappingFilePath;

        /// <summary>
        /// Value / The stack trace..
        /// </summary>
        private static String stackTrace;

        /// <summary>
        /// Scrollview position 2d.
        /// </summary>
        private Vector2 scrollPosition;

        private void OnGUI()
        {
            try
            {
                // Start Scroll
                this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);

                // Render Gui Container
                this.gui_Container.Gui();

                // Reassign static values.
                mappingFilePath = this.gui_Mapping_File_Row.RowContent;
                stackTrace = this.gui_StackTrace_Row.RowContent;

                // End Scroll
                GUILayout.EndScrollView();
            }
            catch(Exception e)
            {
                Debug.LogError("[OPS.OBF] " + e.ToString());
                this.Close();
            }
        }

        #endregion

        //Process
        #region Process

        // Some Tests:

        // dotnet

        // Type
        // Namespace.TypeName.NestedTypeName`Generic

        // Method
        // Type Namespace.TypeName.MethodName[Generic]()

        // Mono

        // Type
        // Namespace.TypeName/NestedTypeName`Generic

            /*

        NullReferenceException: Object reference not set to an instance of an object
at GameFramework.GameStructure.Characters.ObjectModel.Character.bju()[0x00003] in D:\Tim\Programmieren\Unity\Obfuscator 4.0\Obfuscator_Test_2019.4\Assets\FlipWebApps\GameFramework\Scripts\GameStructure\Characters\ObjectModel\Character.cs:55 
  at GameFramework.GameStructure.GameItems.ObjectModel.GameItem.bsr(GameFramework.GameStructure.Game.ObjectModel.GameConfiguration a, GameFramework.GameStructure.Players.ObjectModel.Player b, cy c) [0x000e6] in D:\Tim\Programmieren\Unity\Obfuscator 4.0\Obfuscator_Test_2019.4\Assets\FlipWebApps\GameFramework\Scripts\GameStructure\GameItems\ObjectModel\GameItem.cs:523 
  at ho`2[a, b].bwx(System.Int32 a, System.Int32 b, System.Int32 c, System.Boolean d) [0x00075] in D:\Tim\Programmieren\Unity\Obfuscator 4.0\Obfuscator_Test_2019.4\Assets\FlipWebApps\GameFramework\Scripts\GameStructure\GameItems\ObjectModel\GameItemManager.cs:209 
  at fg.sa() [0x0069c] in D:\Tim\Programmieren\Unity\Obfuscator 4.0\Obfuscator_Test_2019.4\Assets\FlipWebApps\GameFramework\Scripts\GameStructure\GameManager.cs:756 
  at jp`1[a].Awake() [0x0005f] in D:\Tim\Programmieren\Unity\Obfuscator 4.0\Obfuscator_Test_2019.4\Assets\FlipWebApps\GameFramework\Scripts\GameObjects\Components\Singleton.cs:79 

            */

        private void IterateStackTraceLines()
        {
            String var_Obfuscated_StackTrace = this.gui_StackTrace_Row.RowContent;

            StringBuilder var_Deobfuscated_StringBuilder = new StringBuilder();

            using(StringReader var_Reader = new StringReader(var_Obfuscated_StackTrace))
            {
                String var_Line;
                while((var_Line = var_Reader.ReadLine()) != null)
                {
                    String var_PreparedLine = this.PrepareStackTraceLine(var_Line);

                    MethodInfo var_MethodInfo = this.CreateMethodInfo(var_PreparedLine);

                    var_Deobfuscated_StringBuilder.AppendLine("at " + FindOriginalFullName(var_MethodInfo));
                }                
            }

            this.gui_StackTrace_Deobfuscated_Row.RowContent = var_Deobfuscated_StringBuilder.ToString();
        }

        private String PrepareStackTraceLine(String _Line)
        {
            String var_Line = _Line.TrimStart();

            // Remove the 'at' on beginning.
            if(var_Line.StartsWith("at "))
            {
                var_Line = var_Line.Substring(2);
                var_Line = var_Line.TrimStart();
            }

            // Remove the 'in' on the end.
            if (var_Line.Contains(" in "))
            {
                var_Line = var_Line.Remove(var_Line.IndexOf(" in "));
            }

            // Seperate Type and Method.
            String var_Method = "";
            if(OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper.TypeHelper.TryGetMemberNameStartIndex(var_Line, out int _MemberIndex))
            {
                var_Method = var_Line.Substring(_MemberIndex);

                var_Line = var_Line.Substring(0, _MemberIndex - 1);
            }

            // Seperate namespaces/classes and remove generic parameter '[' to ']'.
            String[] var_Split_Dot_Array = var_Line.Split('.');

            // Emtpy line
            var_Line = "";
            for (int t = 0; t < var_Split_Dot_Array.Length; t++)
            {
                // Remove '[...]' in each type.
                var_Line += OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper.MemberInfoHelper.GetMemberName(var_Split_Dot_Array[t]);

                var_Line += ".";
            }

            var_Line += var_Method;

            return var_Line;
        }

        private class MethodInfo
        {
            public String FullName;
            public String TypeNamespace;
            public String TypeName;
            public String Name;
            public int GenericParameter;
        }

        private MethodInfo CreateMethodInfo(String _Line)
        {
            // Remove the methods parameter.
            String var_Line = OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper.MethodInfoHelper.RemoveMethodParameter(_Line);

            // Extract generic parameter.
            int var_GenericParameter = 0;

            // Only methods has '[...]'. So is ok.
            if (var_Line.Contains("["))
            {
                var_GenericParameter = var_Line.Substring(var_Line.IndexOf("[")).Split(',').Length;
            }

            // Create MethodInfo.
            MethodInfo var_MethodInfo = new MethodInfo();

            var_MethodInfo.FullName = var_Line;
            var_MethodInfo.TypeNamespace = OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper.MemberInfoHelper.GetMemberName(var_Line.Substring(0, var_Line.LastIndexOf(".")).Contains(".") ? var_Line.Substring(0, var_Line.LastIndexOf(".")).Substring(0, var_Line.Substring(0, var_Line.LastIndexOf(".")).LastIndexOf(".")) : "");
            var_MethodInfo.TypeName = OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper.MemberInfoHelper.GetMemberName(var_Line.Substring(0, var_Line.LastIndexOf(".")));
            var_MethodInfo.Name = OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper.MemberInfoHelper.GetMemberName(var_Line);
            var_MethodInfo.GenericParameter = var_GenericParameter;

            return var_MethodInfo;
        }

        private String FindOriginalFullName(MethodInfo _MethodInfo)
        {
            // Find all type keys sharing the same type namespace as _MethodInfo.
            List<TypeKey> var_TypeKeys_Matching_MethodInfo_TypeNamespace = this.renameMapping.GetMapping(Assembly.Mono.Member.EMemberType.Namespace).GetAllMemberKeys(_MethodInfo.TypeNamespace).Cast<TypeKey>().ToList();

            // Find all type keys sharing the same type name as _MethodInfo.
            List<TypeKey> var_TypeKeys_Matching_MethodInfo_TypeName = this.renameMapping.GetMapping(Assembly.Mono.Member.EMemberType.Type).GetAllMemberKeys(_MethodInfo.TypeName).Cast<TypeKey>().ToList();

            // Find all method keys sharing the same name as _MethodInfo.
            List<MethodKey> var_MethodKeys_Matching_MethodInfo_Name = this.renameMapping.GetMapping(Assembly.Mono.Member.EMemberType.Method).GetAllMemberKeys(_MethodInfo.Name).Cast<MethodKey>().ToList();

            String var_Namespace = "";

            // If list empty, _MethodInfo.TypeNamespace is not obfuscated.
            if (var_TypeKeys_Matching_MethodInfo_TypeNamespace.Count == 0)
            {
                var_Namespace = _MethodInfo.TypeNamespace;
            }
            else
            {

            }

            String var_TypeName = "";

            // If list empty, _MethodInfo.TypeName is not obfuscated.
            if (var_TypeKeys_Matching_MethodInfo_TypeName.Count == 0)
            {
                var_TypeName = _MethodInfo.TypeName;
            }
            else
            {

            }

            String var_MethodName = "";

            // If list empty, _MethodInfo.Name is not obfuscated.
            if(var_MethodKeys_Matching_MethodInfo_Name.Count == 0)
            {
                var_MethodName = _MethodInfo.Name;
            }
            else
            {
                
            }

            if (String.IsNullOrEmpty(var_MethodName))
            {
                // Method is obfuscated.

                // Only 1 found, so return methods fullname.
                if (var_MethodKeys_Matching_MethodInfo_Name.Count == 1)
                {
                    return var_MethodKeys_Matching_MethodInfo_Name[0].FullName;
                }

                List<MethodKey> var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter = var_MethodKeys_Matching_MethodInfo_Name.Where(m => m.GenericParameterCount == _MethodInfo.GenericParameter).ToList();

                // Only 1 found, so return methods fullname.
                if (var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter.Count == 1)
                {
                    return var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter[0].FullName;
                }

                if (String.IsNullOrEmpty(var_TypeName))
                {
                    // Type is obfuscated

                    // Iterate methods
                    for (int m = 0; m < var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter.Count; m++)
                    {
                        // Iterate types
                        for (int t = 0; t < var_TypeKeys_Matching_MethodInfo_TypeName.Count; t++)
                        {
                            // Check if methods type name is equals a matching type name.
                            if (var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter[m].Type.Name == var_TypeKeys_Matching_MethodInfo_TypeName[t].Name)
                            {
                                return var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter[m].FullName;
                            }
                        }
                    }

                    return _MethodInfo.FullName;
                }
                else
                {
                    // Type is not obfuscated.

                    // Iterate methods
                    for (int m = 0; m < var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter.Count; m++)
                    {
                        // Check if methods type name is equals the not obfuscated type name.
                        if (var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter[m].Type.Name == _MethodInfo.TypeName)
                        {
                            return var_MethodKeys_Matching_MethodInfo_Name_And_GenericParameter[m].FullName;
                        }
                    }

                    return _MethodInfo.FullName;
                }
            }
            else
            {
                // Method is not obfuscated.

                if (String.IsNullOrEmpty(var_TypeName))
                {
                    // Type is obfuscated

                    // Iterate namespaces
                    for (int n = 0; n < var_TypeKeys_Matching_MethodInfo_TypeNamespace.Count; n++)
                    {
                        // Iterate types
                        for (int t = 0; t < var_TypeKeys_Matching_MethodInfo_TypeName.Count; t++)
                        {
                            if (var_TypeKeys_Matching_MethodInfo_TypeNamespace[n].FullName == var_TypeKeys_Matching_MethodInfo_TypeName[t].FullName)
                            {
                                return var_TypeKeys_Matching_MethodInfo_TypeNamespace[n].FullName + "::" + _MethodInfo.Name;
                            }
                        }
                    }

                    return _MethodInfo.FullName;
                }
                else
                {
                    // Type is not obfuscated.

                    return _MethodInfo.FullName;
                }
            }
        }

        #endregion
    }
}