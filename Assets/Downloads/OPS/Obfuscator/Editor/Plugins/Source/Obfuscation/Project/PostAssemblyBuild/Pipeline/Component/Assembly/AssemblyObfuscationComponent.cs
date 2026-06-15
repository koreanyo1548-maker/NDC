using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Pipeling
using OPS.Editor.Project.Pipeline;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator - Assembly - Mono
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Assembly
{
    public class AssemblyObfuscationComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProvidingComponent
    {
        // Name
        #region Name

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Assembly - Settings";
            }
        }

        #endregion

        // Description
        #region Description

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override string Description
        {
            get
            {
                return "Manage obfuscateable assemblies and their dependencies.";
            }
        }

        #endregion

        // Short Description
        #region Short Description

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Manage obfuscateable assemblies and their dependencies.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Assembly";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CObfuscate_Assembly_AssemblyCSharp = "Obfuscate_Assembly_AssemblyCSharp";
        public const String CObfuscate_Assembly_AssemblyCSharpFirstPass = "Obfuscate_Assembly_AssemblyCSharpFirstPass";

        public const String CObfuscate_Assembly_In_Assets_AssemblyDefinitionFiles = "Obfuscate_Assembly_In_Assets_AssemblyDefinitionFiles";

        public const String CObfuscate_Assembly_In_Packages_AssemblyDefinitionFiles = "Obfuscate_Assembly_In_Packages_AssemblyDefinitionFiles";

        public const String CObfuscate_Assembly_Additional_Assembly_Array = "Obfuscate_Assembly_Additional_Assembly_Array";
        public const String CObfuscate_Assembly_Additional_Assembly_Dependencies_Array = "Obfuscate_Assembly_Additional_Assembly_Dependencies_Array";

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Category this Component belongs too.
        /// </summary>
        public EObfuscatorCategory ObfuscatorCategory
        {
            get
            {
                return EObfuscatorCategory.Obfuscation;
            }
        }

        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // Core Assembly
            Row_Boolean var_AssemblyCSharpRow = new Row_Boolean("Obfuscate Assembly-CSharp:", _ComponentSettings, CObfuscate_Assembly_AssemblyCSharp);
            var_AssemblyCSharpRow.Notification_Info = "This is the core unity assembly where all the code is stored if not stored in some assembly definition file. Most at the time you want to active this option!";
            var_Content.AddRow(var_AssemblyCSharpRow);
            Row_Boolean var_AssemblyCSharpFirstPassRow = new Row_Boolean("Obfuscate Assembly-CSharp-firstpass:", _ComponentSettings, CObfuscate_Assembly_AssemblyCSharpFirstPass);
            var_AssemblyCSharpFirstPassRow.Notification_Info = "This is the second core unity assembly where all the code from files inside 'Plugins' directories can be found! Mostly you want to obfuscate this assembly too.";
            var_Content.AddRow(var_AssemblyCSharpFirstPassRow);

            // Asmdef - Assets
            Row_Boolean var_AsmdefRow = new Row_Boolean("Obfuscate Assembly Definition Files in 'Assets':", _ComponentSettings, CObfuscate_Assembly_In_Assets_AssemblyDefinitionFiles);
            var_AsmdefRow.Notification_Info = "Activating this option will search for all assembly definition files in the 'Assets' directory and obfuscate the belonging assemblies.";
            var_Content.AddRow(var_AsmdefRow);

            // Asmdef - Packages
            Row_Boolean var_Asmdef_Package_Row = new Row_Boolean("Obfuscate Assembly Definition Files in 'Packages':", _ComponentSettings, CObfuscate_Assembly_In_Packages_AssemblyDefinitionFiles);
            var_Asmdef_Package_Row.Notification_Info = "Activating this option will search for assembly definition files in the 'Packages' directory and obfuscate the belonging assemblies. Here you find assemblies which are downloaded through the Unity Package Manager.";
            var_Asmdef_Package_Row.Notification_Warning = "Beta: This is currently a beta feature, because Unity Core Assemblies can be found here too and would be obfuscated too, so be aware.";
            var_Content.AddRow(var_Asmdef_Package_Row);

            // Other Assemblies
            Row_Array var_AdditionalAssemblyRow = new Row_Array("Obfuscate external assemblies:", _ComponentSettings, CObfuscate_Assembly_Additional_Assembly_Array);
            var_AdditionalAssemblyRow.Notification_Info = "Enter here the name of external/precompiled assemblies you want to obfuscate too, like: 'MyAssembly.dll', or enter the full path like: 'C:/[MyGame]/MyAssembly.dll'.";
            var_Content.AddRow(var_AdditionalAssemblyRow);

            // Dependency Assemblies
            Row_Array var_ReferenceAssemblyRow = new Row_Array("Dependencies:", _ComponentSettings, CObfuscate_Assembly_Additional_Assembly_Dependencies_Array);
            var_ReferenceAssemblyRow.Notification_Info = "If you receive some errors, like 'Assembly XYZ could not be resolved!', 'Assembly XYZ could not be found!' or similar, add their directory path here. Those assemblies can be mostly found in your projects 'Assets' or 'Packages' directory. For example if an assembly called 'GameAnalytics' was not found, and it is locate here 'Assets/GameAnalytics/Plugins/GameAnalytics.dll'. Add the relative directory path to the below list 'Assets/GameAnalytics/Plugins'. If you still receive the error add the full path like '[D:]/[Your Project]/Assets/GameAnalytics/Plugins'.";
            var_ReferenceAssemblyRow.Notification_Warning = "Do not enter editor only or test assemblies.";
            var_Content.AddRow(var_ReferenceAssemblyRow);

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Assemblies to load
        #region Assemblies to load

        public List<AssemblyLoadInfo> GetAssemblyLoadInfoList()
        {
            List<AssemblyLoadInfo> var_AssemblyLoadInfos = new List<AssemblyLoadInfo>();

            // Assembly-CSharp.dll
            if (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Assembly_AssemblyCSharp))
            {
                String var_FilePath = Editor.Assembly.Mono.Helper.AssemblyHelper.FindAssemblyLocation("Assembly-CSharp.dll");

                if (String.IsNullOrEmpty(var_FilePath))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "There is no assembly: " + "Assembly-CSharp.dll");
                }
                else
                {
                    AssemblyLoadInfo var_AssemblyLoadInfo = new AssemblyLoadInfo();
                    var_AssemblyLoadInfo.FilePath = var_FilePath;
                    var_AssemblyLoadInfo.IsUnityAssembly = true;
                    var_AssemblyLoadInfo.Obfuscate = true;
                    var_AssemblyLoadInfos.Add(var_AssemblyLoadInfo);
                }
            }

            // Assembly-CSharp-firstpass.dll
            if (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Assembly_AssemblyCSharpFirstPass))
            {
                String var_FilePath = Editor.Assembly.Mono.Helper.AssemblyHelper.FindAssemblyLocation("Assembly-CSharp-firstpass.dll");

                if (String.IsNullOrEmpty(var_FilePath))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "There is no assembly: " + "Assembly-CSharp-firstpass.dll");
                }
                else
                {
                    AssemblyLoadInfo var_AssemblyLoadInfo = new AssemblyLoadInfo();
                    var_AssemblyLoadInfo.FilePath = var_FilePath;
                    var_AssemblyLoadInfo.IsUnityAssembly = true;
                    var_AssemblyLoadInfo.Obfuscate = true;
                    var_AssemblyLoadInfos.Add(var_AssemblyLoadInfo);
                }
            }

            // Assembly Definition Files
            List<UnityEditor.Compilation.Assembly> var_AssemblyDefinitionFileList = Editor.Assembly.Unity.Helper.AssemblyHelper.FindAllAssemblyDefinitionFiles();

            for (int i = 0; i < var_AssemblyDefinitionFileList.Count; i++)
            {
                // Skip Assembly-CSharp and Assembly-CSharp-firstpass
                if (var_AssemblyDefinitionFileList[i].name == "Assembly-CSharp"
                    || var_AssemblyDefinitionFileList[i].name == "Assembly-CSharp-firstpass")
                {
                    continue;
                }

                // Continue only assemblies with source files
                if (var_AssemblyDefinitionFileList[i].sourceFiles == null
                    || var_AssemblyDefinitionFileList[i].sourceFiles.Length == 0)
                {
                    continue;
                }

                // Check if obfuscation of assemblies in Assets is allowed.
                if (var_AssemblyDefinitionFileList[i].sourceFiles[0].StartsWith("Assets"))
                {
                    if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Assembly_In_Assets_AssemblyDefinitionFiles))
                    {
                        continue;
                    }
                }

                // Check if obfuscation of assemblies in Packages is allowed.
                if (var_AssemblyDefinitionFileList[i].sourceFiles[0].StartsWith("Packages"))
                {
                    if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Assembly_In_Packages_AssemblyDefinitionFiles))
                    {
                        continue;
                    }
                }

                // Continue only assemblies that are not test assemblies.
                if(Editor.Assembly.Unity.Helper.AssemblyHelper.IsTestAssembly(var_AssemblyDefinitionFileList[i]))
                {
                    continue;
                }

                String var_FilePath = Editor.Assembly.Mono.Helper.AssemblyHelper.FindAssemblyLocation(var_AssemblyDefinitionFileList[i].name);

                if (String.IsNullOrEmpty(var_FilePath))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "There is no assembly: " + var_AssemblyDefinitionFileList[i].name);
                }
                else
                {
                    AssemblyLoadInfo var_AssemblyLoadInfo = new AssemblyLoadInfo();
                    var_AssemblyLoadInfo.FilePath = var_FilePath;
                    var_AssemblyLoadInfo.IsUnityAssembly = true;
                    var_AssemblyLoadInfo.Obfuscate = true;

                    // If OPS.Obfuscator, then it is a helper assembly! Do not obfuscate it.
                    if (var_AssemblyDefinitionFileList[i].name == "OPS.Obfuscator")
                    {
                        var_AssemblyLoadInfo.IsHelperAssembly = true;
                        var_AssemblyLoadInfo.Obfuscate = false;
                    }

                    var_AssemblyLoadInfos.Add(var_AssemblyLoadInfo);
                }
            }

            // Other Assemblies added through settings.
            List<String> var_AdditionalAssemblyList = this.Step.Settings.Get_ComponentSettings_As_Array(this.SettingsKey, CObfuscate_Assembly_Additional_Assembly_Array).ToList();
            for (int i = 0; i < var_AdditionalAssemblyList.Count; i++)
            {
                String var_FilePath = Editor.Assembly.Mono.Helper.AssemblyHelper.FindAssemblyLocation(var_AdditionalAssemblyList[i]);

                if (String.IsNullOrEmpty(var_FilePath))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "There is no assembly: " + var_AdditionalAssemblyList[i]);
                }
                else
                {
                    AssemblyLoadInfo var_AssemblyLoadInfo = new AssemblyLoadInfo();
                    var_AssemblyLoadInfo.FilePath = var_FilePath;
                    var_AssemblyLoadInfo.IsUnityAssembly = Editor.Assembly.Unity.Helper.AssemblyHelper.IsAssemblyDefinitionFileAssembly(var_AdditionalAssemblyList[i]);
                    var_AssemblyLoadInfo.Obfuscate = true;
                    var_AssemblyLoadInfos.Add(var_AssemblyLoadInfo);
                }
            }

            return var_AssemblyLoadInfos;
        }

        #endregion

        // Dependencies
        #region Dependencies

        public List<string> GetAssemblyReferenceDirectoryList()
        {
            return new List<string>();
        }

        #endregion
    }
}
