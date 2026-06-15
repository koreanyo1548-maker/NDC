// System
using System;
using System.Collections.Generic;
using System.IO;

// Unity
using UnityEditor;

// GUPS - Assets
using GUPS.Assets.Editor.Files.Bundle;
using GUPS.Assets.Editor.Files.Serialized;

// OPS - Logging
using OPS.Editor.Report;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Project
using OPS.Editor.Project.Step;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Assets
using OPS.Obfuscator.Editor.Assets.Unity;
using GUPS.Assets.Editor.Files.Addressable.Catalog;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility
{
    /// <summary>
    /// Obfuscates the addressable bundles after the assembly build process.
    /// </summary>
    public class AddressableComponent : APostAssemblyBuildComponent, IGuiComponent
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
                return "Addressable - Obfuscation";
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
                return "Manages the obfuscation of Addressables.";
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
                return "Manages the obfuscation of Addressables.";
            }
        }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public const String CSettingsKey = "Default_Addressable_Component_Class";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        /// <summary>
        /// The settings key to obfuscate addressables.
        /// </summary>
        public const String CEnable_Try_Obfuscate_Addressables = "Enable_Try_Obfuscate_Addressables";

        /// <summary>
        /// The settings key for custom addressable paths.
        /// </summary>
        public const String CCustom_Addressable_Paths = "Custom_Addressable_Paths";

        #endregion

        // Gui
        #region Gui

        /// <summary>
        /// Category this Component belongs too.
        /// </summary>
        public EObfuscatorCategory ObfuscatorCategory
        {
            get
            {
                return EObfuscatorCategory.Compatibility;
            }
        }

        /// <summary>
        /// Get the gui container for this component.
        /// </summary>
        /// <param name="_ComponentSettings">The settings for this component.</param>
        /// <returns>The gui container.</returns>
        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_Try_Obfuscate_Addressables);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // Explanation
            Row_Text var_ExplanationRow = new Row_Text("");
            var_ExplanationRow.Notification_Info = "Addressables are a popular way to support loading assets from any location with any collection of dependencies. But the built addressable bundles mostly include assets that reference to scripts. After obfuscation, these scripts will have different names. So the addressable bundles have to be obfuscated too, so these references get updated.";
            var_ExplanationRow.Notification_Warning = "At the moment only the JSON catalog version is supported. Binary catalog support will be added in the future.";
            var_Content.AddRow(var_ExplanationRow);

            // Default Path
            Row_TextBox var_DefaultPathRow = new Row_TextBox("Default Path:", DefaultBuildPath);
            var_DefaultPathRow.Enabled = false;
            var_DefaultPathRow.Notification_Info = "This is the default build location for local addressable bundles. For the current build target.";
            var_Content.AddRow(var_DefaultPathRow);

            // Custom Path
            Row_Array var_CustomPathsRow = new Row_Array("Custom Paths:", _ComponentSettings, CCustom_Addressable_Paths);
            var_CustomPathsRow.Notification_Info = "If you do not use the default addressable build location, or build addressables which will be shipped to a remote location, you mostly use custom build locations. Enter the directory path of these locations here. For example '[MyPath]/RemoteData/StandaloneWindows64'. You can also use '" + BUILD_TARGET_PLACEHOLDER + "' as placeholder for the current build target. So you could enter '[MyPath]/RemoteData/" + BUILD_TARGET_PLACEHOLDER + "' which will be resolved as '[MyPath]/RemoteData/StandaloneWindows64'.";
            var_Content.AddRow(var_CustomPathsRow);

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Component
        #region Component

        /// <summary>
        /// Return whether the component is activated or deactivated for the pipeline processing.
        /// </summary>
        public override bool IsActive
        {
            get
            {
                // Check if setting is activated.
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Try_Obfuscate_Addressables))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        // Addressable
        #region Addressable

        /// <summary>
        /// The placeholder for the build target in the custom addressable paths.
        /// </summary>
        private const String BUILD_TARGET_PLACEHOLDER = "{BuildTarget}";

        /// <summary>
        /// Maps the build target to the Addressables platform subfolder.
        /// </summary>
        private static readonly Dictionary<BuildTarget, String> buildTargetMapping = new Dictionary<BuildTarget, String>() {
                {BuildTarget.XboxOne, "XboxOne"},
                {BuildTarget.Switch, "Switch"},
                {BuildTarget.PS4, "PS4"},
                {BuildTarget.iOS, "iOS"},
                {BuildTarget.Android, "Android"},
                {BuildTarget.WebGL, "WebGL"},
                {BuildTarget.StandaloneWindows, "Windows"},
                {BuildTarget.StandaloneWindows64, "Windows"},
                {BuildTarget.StandaloneOSX, "OSX"},
                {BuildTarget.StandaloneLinux64, "Linux"},
                {BuildTarget.WSAPlayer, "WindowsUniversal"},
            };

        /// <summary>
        /// Maps the build target to the Addressables platform subfolder. If the build target is not found in the mapping, it will return the build target as a string.
        /// </summary>
        /// <param name="_Target">The build target to get the Addressables platform subfolder for.</param>
        /// <returns>The Addressables platform subfolder for the build target.</returns>
        private static String GetAddressablesPlatformPathInternal(BuildTarget _Target)
        {
            if (buildTargetMapping.ContainsKey(_Target))
                return buildTargetMapping[_Target].ToString();
            return _Target.ToString();
        }

        /// <summary>
        /// Retrieves the Addressables platform subfolder of the build platform that is targeted.
        /// </summary>
        /// <remarks>
        /// For example: 
        /// - StandaloneWindows, StandaloneWindows64 > Windows <br />
        /// - StandaloneOSX > OSX <br />
        /// - StandaloneLinux64 > Linux <br />
        /// ...
        /// </remarks>
        /// <returns>Returns the Addressables platform subfolder of the build platform that is targeted.</returns>
        private static String GetPlatformPathSubFolder()
        {
            return GetAddressablesPlatformPathInternal(EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// The catalog while always be build inside the default build path.
        /// </summary>
        /// <remarks>
        /// If older 2021.2: Assets/StreamingAssets/aa/[SubPlatform].<br />
        /// If newer 2021.2: Library/com.unity.addressables/aa/[SubPlatform].<br />
        /// ...
        /// </remarks>
        /// <returns>Returns the default build path for the catalog file build.</returns>
        private static String CatalogBuildPath
        {
            get
            {
#if UNITY_2021_2_OR_NEWER
                return Path.Combine("Library", "com.unity.addressables", "aa", GetPlatformPathSubFolder());
#else
                return Path.Combine("Assets", "StreamingAssets", "aa", GetPlatformPathSubFolder());
#endif
            }
        }

        /// <summary>
        /// Retrieves the default build path for the Addressables build.
        /// </summary>
        /// <remarks>
        /// If older 2021.2: Assets/StreamingAssets/aa/[SubPlatform]/[Platform].<br />
        /// If newer 2021.2: Library/com.unity.addressables/aa/[SubPlatform]/[Platform].<br />
        /// ...
        /// </remarks>
        /// <returns>Returns the default build path for the Addressables build.</returns>
        private static String DefaultBuildPath
        {
            get
            {
#if UNITY_2021_2_OR_NEWER
                return Path.Combine("Library", "com.unity.addressables", "aa", GetPlatformPathSubFolder(), EditorUserBuildSettings.activeBuildTarget.ToString());
#else
                return Path.Combine("Assets", "StreamingAssets", "aa", GetPlatformPathSubFolder(), EditorUserBuildSettings.activeBuildTarget.ToString());
#endif
            }
        }

#if Obfuscator_Free
#else

        /// <summary>
        /// Returns a list of all bundle file infos in the given directory and its subdirectories.
        /// </summary>
        /// <param name="_BundleDirectory">The directory to search for bundle files.</param>
        /// <returns>A list of all bundle file infos in the given directory.</returns>
        private List<FileInfo> GetAllBundleFiles(String _BundleDirectory)
        {
            List<FileInfo> var_BundleFiles = new List<FileInfo>();

            // Get all bundle files.
            DirectoryInfo var_DirectoryInfo = new DirectoryInfo(_BundleDirectory);
            if (var_DirectoryInfo.Exists)
            {
                FileInfo[] var_Files = var_DirectoryInfo.GetFiles("*.bundle", SearchOption.AllDirectories);
                var_BundleFiles.AddRange(var_Files);
            }

            return var_BundleFiles;
        }

        /// <summary>
        /// Obfuscates all bundle files in the default and custom paths.
        /// </summary>
        /// <param name="_ObfuscationMapping">The mapping of the original monobehaviour type to the obfuscated monobehaviour type.</param>
        private void ObfuscateBundleFiles(Dictionary<TypeKey, TypeKey> _ObfuscationMapping)
        {
            // Get all bundle files.
            List<FileInfo> var_BundleFiles = new List<FileInfo>();

            // Get first form the default path.
            var_BundleFiles.AddRange(this.GetAllBundleFiles(DefaultBuildPath));

            // Get all custom paths.
            String[] var_CustomPaths = this.Step.Settings.Get_ComponentSettings_As_Array(this.SettingsKey, CCustom_Addressable_Paths);

            // Add all custom paths.
            foreach (String var_CustomPath in var_CustomPaths)
            {
                // Replace placeholder with the current build target.
                String var_ReplacedCustomPath = var_CustomPath.Replace(BUILD_TARGET_PLACEHOLDER, EditorUserBuildSettings.activeBuildTarget.ToString());

                // Find all bundle files in the custom path.
                var_BundleFiles.AddRange(this.GetAllBundleFiles(var_ReplacedCustomPath));
            }

            // Log - Found bundle files.
            Obfuscator.Report.Append(EReportLevel.Info, String.Format("Found {0} bundle files to obfuscate.", var_BundleFiles.Count));

            // Obfuscate all bundle files.
            foreach (FileInfo var_BundleFile in var_BundleFiles)
            {
                this.ObfuscateBundleFile(var_BundleFile.FullName, _ObfuscationMapping);
            }
        }

        /// <summary>
        /// Obfuscates the bundle file with the given obfuscation mapping and overrides the existing bundle file.
        /// </summary>
        /// <param name="_BundleFilePath">The path to the bundle file to obfuscate.</param>
        /// <param name="_ObfuscationMapping">The mapping of the original monobehaviour type to the obfuscated monobehaviour type.</param>
        private void ObfuscateBundleFile(String _BundleFilePath, Dictionary<TypeKey, TypeKey> _ObfuscationMapping)
        {
            // Log - Obfuscating bundle file.
            Obfuscator.Report.Append(EReportLevel.Info, String.Format("Obfuscating addressable bundle file '{0}'.", _BundleFilePath));

            // The catalog.json file is always in the default build path.
            String var_CatalogFilePath = Path.Combine(CatalogBuildPath, "catalog.json");

            // Check if the catalog file exists.
            if (!File.Exists(var_CatalogFilePath))
            {
                // Log - No catalog file found.
                Obfuscator.Report.Append(EReportLevel.Warning, String.Format("No catalog.json file found for addressable bundle file '{0}'. The bundle file will not be obfuscated. Do you use the binary catalog.bin version? Only catalog.json is supported at the moment.", _BundleFilePath), true);
                return;
            }

            // Read bundle file.
            BundleFile var_BundleFile = UnityAssetHelper.LoadBundleFile(_BundleFilePath);

            // Calculate the crc of the bundle file.
            UInt32 var_CurrentCrc = var_BundleFile.CalculateCrc();

            // Override MonoScripts
            int var_UpdatedMonoScriptCount = UnityAssetHelper.UpdateMonoScriptsInBundleFile(var_BundleFile, _ObfuscationMapping);

            // If there are no updated monoscripts, return here.
            if (var_UpdatedMonoScriptCount == 0)
            {
                // Log - No monoscripts found.
                Obfuscator.Report.Append(EReportLevel.Debug, String.Format("No monoscripts updated in addressable bundle file '{0}'. The bundle file has not to be saved.", _BundleFilePath));

                return;
            }

            // Write bundle file.
            UnityAssetHelper.SaveBundleFile(var_BundleFile, _BundleFilePath);

            // Calculate the new crc of the bundle file.
            UInt32 var_NewCrc = var_BundleFile.CalculateCrc();

            // Load the catalog file.
            CatalogFile var_CatalogFile = CatalogFile.Load(File.ReadAllText(var_CatalogFilePath));

            // Update the crc of the bundle file in the catalog file.
            if (!var_CatalogFile.ReplaceCrcForFile(Path.GetFileName(_BundleFilePath), var_CurrentCrc, var_NewCrc))
            {
                // Log - Failed to update crc in catalog file.
                Obfuscator.Report.Append(EReportLevel.Warning, String.Format("Failed to update crc in catalog file '{0}' for addressable bundle file '{1}'.", var_CatalogFilePath, _BundleFilePath));
            }

            // Save the catalog file.
            File.WriteAllText(var_CatalogFilePath, var_CatalogFile.Save());

            // Log - Obfuscated bundle file.
            Obfuscator.Report.Append(EReportLevel.Info, String.Format("Obfuscated addressable bundle file '{0}'. Crc changed from '{1}' to '{2}'.", _BundleFilePath, var_CurrentCrc, var_NewCrc));
        }
#endif

        #endregion

        // On Post Process Pipeline
        #region On Post Process Pipeline

#if Obfuscator_Free
#else

        /// <summary>
        /// On post process get the monobehaviour obfuscation mapping and obfuscate the addressable bundles.
        /// </summary>
        /// <param name="_StepOutput">The step output of the post assembly build pipeline.</param>
        /// <returns>True if the post process was successful, otherwise false.</returns>
        public override bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            // Call base.
            if (!base.OnPostPipelineProcess(_StepOutput))
            {
                return false;
            }

            // Get the mapping from the monobehaviour obfuscation component.
            Dictionary<TypeKey, TypeKey> var_ObfuscationMapping = _StepOutput.Get<Dictionary<TypeKey, TypeKey>>(PostAssemblyBuild.Pipeline.Component.Member.Type.MonoBehaviourRenameObfuscationComponent.CMonobehaviourObfuscationMapping);

            // Only obfuscate if the mapping is not null or empty.
            if (var_ObfuscationMapping == null || var_ObfuscationMapping.Count == 0)
            {
                // Log - No obfuscation mapping found.
                Obfuscator.Report.Append(EReportLevel.Warning, String.Format("No MonoBehaviour obfuscation mapping found. The addressable bundles will not be obfuscated."));
            }
            else
            {
                // Obuscate the addressable bundles.
                this.ObfuscateBundleFiles(var_ObfuscationMapping);
            }

            // Return true.
            return true;
        }

#endif

        #endregion
    }
}
