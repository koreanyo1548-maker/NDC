// System
using System;
using System.Collections.Generic;
using System.IO;

// Unity
using UnityEngine;

// GUPS - Assets
using GUPS.Assets.Editor.Files.Bundle;
using GUPS.Assets.Editor.Files.Serialized;

// OPS - Report
using OPS.Editor.Report;

// OPS - Project
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Assets
using OPS.Obfuscator.Editor.Assets.Unity;

namespace OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline.Component
{
    internal class MonoBehaviourAssetsComponent : APostAssetsBuildComponent
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
                return "Post Process Unity MonoBehaviour Assets";
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
                return "Post Process Unity MonoBehaviour Assets On Post Assets Build";
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
                return "Post Process Unity MonoBehaviour Assets";
            }
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
                // Check if monobehaviour obfuscation is activated. If not, return false.
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_MonoBehaviour))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        // MonoBehaviour
        #region MonoBehaviour

        /// <summary>
        /// A mapping of the original monobehaviour type to the obfuscated monobehaviour type.
        /// </summary>
        private Dictionary<TypeKey, TypeKey> MonobehaviourObfuscationMapping = new Dictionary<TypeKey, TypeKey>();

#if Obfuscator_Free
#else

        /// <summary>
        /// If the game is compressed, the assets are bundled.
        /// </summary>
        private bool AssetsAreBundled
        {
            get
            {
                return this.Step.BuildSettings.Compression != OPS.Editor.Settings.Unity.Build.CompressionType.None;
            }
        }

        /// <summary>
        /// Returns the directory to the build assets in the tmp directory. This is the directory where the globalgamemanagers.assets or data.unity3d file is located.
        /// </summary>
        /// <returns>The directory to the build assets in the tmp directory.</returns>
        private String GetBuildAssetsDirectory_Tmp()
        {
            // For example: [Path] -> [Path]/Temp/StagingArea/Data
            String var_Path = System.IO.Path.GetDirectoryName(Application.dataPath);
            var_Path = System.IO.Path.Combine(var_Path, "Temp");
            var_Path = System.IO.Path.Combine(var_Path, "StagingArea");
            var_Path = System.IO.Path.Combine(var_Path, "Data");
            return var_Path;
        }

        /// <summary>
        /// Returns the directory to the build assets in the library directory. This is the directory where the globalgamemanagers.assets or data.unity3d file is located.
        /// </summary>
        /// <returns>The directory to the build assets in the library directory.</returns>
        private String GetBuildAssetsDirectory_Cache()
        {
            // iOS later 2022.1 - The files 'might' be located in the iOS2/Data directory.
            if(this.Step.BuildSettings.IsIOSBuildTarget)
            {
                // For example: [Path] -> [Path]/Library/PlayerDataCache/iOS2/Data
                String var_PathIOS = System.IO.Path.GetDirectoryName(Application.dataPath);
                var_PathIOS = System.IO.Path.Combine(var_PathIOS, "Library");
                var_PathIOS = System.IO.Path.Combine(var_PathIOS, "PlayerDataCache");
                var_PathIOS = System.IO.Path.Combine(var_PathIOS, this.Step.BuildSettings.BuildTarget.ToString() + "2");
                var_PathIOS = System.IO.Path.Combine(var_PathIOS, "Data");

                // Check if the directory exists.
                if (Directory.Exists(var_PathIOS))
                {
                    return var_PathIOS;
                }
            }

            // For example: [Path] -> [Path]/Library/PlayerDataCache/[Target]/Data
            String var_Path = System.IO.Path.GetDirectoryName(Application.dataPath);
            var_Path = System.IO.Path.Combine(var_Path, "Library");
            var_Path = System.IO.Path.Combine(var_Path, "PlayerDataCache");
            var_Path = System.IO.Path.Combine(var_Path, this.Step.BuildSettings.BuildTarget.ToString());
            var_Path = System.IO.Path.Combine(var_Path, "Data");
            return var_Path;
        }

        /// <summary>
        /// Tries to override the globalgamemanagers.assets file with the obfuscated MonoBehaviours.
        /// </summary>
        /// <param name="_Path">The path to the globalgamemanagers.assets file.</param>
        /// <returns>True if successful; false otherwise.</returns>
        private bool TryOverrideGlobalGameManagersAssets(String _Path)
        {
            // If file does not exist, return false.
            if (!File.Exists(_Path))
            {
                return false;
            }

            // Log - Obfuscating assets file.
            Obfuscator.Report.Append(EReportLevel.Info, String.Format("Obfuscating globalgamemanagers.assets file '{0}'.", _Path));

            try
            {
                // Read assets file.
                AssetsFile var_AssetsFile = UnityAssetHelper.LoadAssetsFile(_Path);

                // Override MonoScripts
                UnityAssetHelper.UpdateMonoScriptsInAssetsFile(var_AssetsFile, this.MonobehaviourObfuscationMapping);

                // Write assets file.
                UnityAssetHelper.SaveAssetsFile(var_AssetsFile, _Path);

                return true;
            }
            catch (Exception var_Exception)
            {
                // Log - Error while obfuscating globalgamemanagers.assets file.
                Obfuscator.Report.Append(EReportLevel.Error, String.Format("Error while obfuscating globalgamemanagers.assets file '{0}': {1}", _Path, var_Exception.Message));

                return false;
            }
        }

        /// <summary>
        /// Tries to override the data.unity3d file with the obfuscated MonoBehaviours.
        /// </summary>
        /// <param name="_Path">The path to the data.unity3d file.</param>
        /// <returns>True if successful; false otherwise.</returns>
        private bool TryOverrideDataUnity3d(String _Path)
        {
            // If file does not exist, return false.
            if (!File.Exists(_Path))
            {
                return false;
            }

            // Log - Obfuscating assets file.
            Obfuscator.Report.Append(EReportLevel.Info, String.Format("Obfuscating data.unity3d file '{0}'.", _Path));

            try
            {
                // Read bundle file.
                BundleFile var_BundleFile = UnityAssetHelper.LoadBundleFile(_Path);

                // Log - Loaded bundle file.
                Obfuscator.Report.Append(EReportLevel.Debug, String.Format("Loaded bundle file successfully."));

                // Override MonoScripts
                UnityAssetHelper.UpdateMonoScriptsInBundleFile(var_BundleFile, this.MonobehaviourObfuscationMapping);

                // Log - Updated bundle file.
                Obfuscator.Report.Append(EReportLevel.Debug, String.Format("Updated bundle file successfully."));

                // Write bundle file.
                UnityAssetHelper.SaveBundleFile(var_BundleFile, _Path);

                // Log - Saved bundle file.
                Obfuscator.Report.Append(EReportLevel.Debug, String.Format("Saved bundle file successfully."));

                return true;
            }
            catch (Exception var_Exception)
            {
                // Log - Error while obfuscating data.unity3d file.
                Obfuscator.Report.Append(EReportLevel.Error, String.Format("Error while obfuscating data.unity3d file '{0}': {1}", _Path, var_Exception.Message));

                return false;
            }
        }
#endif

        #endregion

        // On Pre Process Pipeline
        #region On Pre Process Pipeline

        /// <summary>
        /// Read the monobehaviour obfuscation mapping from the step input.
        /// </summary>
        /// <param name="_StepInput"></param>
        /// <returns></returns>
        public override bool OnPrePipelineProcess(IStepInput _StepInput)
        {
            // Call base.
            bool var_Result = base.OnPrePipelineProcess(_StepInput);

            // Get the mapping from the step input.
            this.MonobehaviourObfuscationMapping = _StepInput.Get<Dictionary<TypeKey, TypeKey>>(PostAssemblyBuild.Pipeline.Component.Member.Type.MonoBehaviourRenameObfuscationComponent.CMonobehaviourObfuscationMapping);

            if (this.MonobehaviourObfuscationMapping == null || this.MonobehaviourObfuscationMapping.Count == 0)
            {
                // Log - No obfuscation mapping found.
                Obfuscator.Report.Append(EReportLevel.Warning, String.Format("No MonoBehaviour obfuscation mapping found. The globalgamemanagers.assets file will not be obfuscated."));
            }

            return var_Result;
        }

        #endregion

        // On Post Assets Build
        #region On Post Assets Build

#if Obfuscator_Free

        /// <summary>
        /// Does nothing in the free version.
        /// </summary>
        /// <returns>Always true.</returns>
        public override bool OnPostAssetsBuild()
        {
            return true;
        }

#else
        /// <summary>
        /// Obfuscated the MonoBehaviours in the globalgamemanagers.assets or data.unity3d file.
        /// </summary>
        /// <returns>True if successful; false otherwise.</returns>
        public override bool OnPostAssetsBuild()
        {
            // Only process if the build target is not a standalone build. Other build targets get obfuscated at some other build step.
            if (this.Step.BuildSettings.IsStandaloneBuildTarget)
            {
                return true;
            }

            // Only process if the build target is not a mono build. Other build targets get obfuscated at some other build step.
            if (this.Step.BuildSettings.IsMonoBuild)
            {
                return true;
            }

            // Check if the assets are bundled.
            if (this.AssetsAreBundled)
            {
                // Log - Assets are bundled.
                Obfuscator.Report.Append(EReportLevel.Info, "Assets are bundled. Obfuscating data.unity3d file.");

                // Try to override the data.unity3d file in tmp directory.
                if (!this.TryOverrideDataUnity3d(Path.Combine(this.GetBuildAssetsDirectory_Tmp(), "data.unity3d")))
                {
                    // Log - Could not obfuscate data.unity3d file.
                    Obfuscator.Report.Append(EReportLevel.Warning, "Could not obfuscate data.unity3d file in tmp directory.");
                }

                // Try to override the data.unity3d file in library directory.
                if (!this.TryOverrideDataUnity3d(Path.Combine(this.GetBuildAssetsDirectory_Cache(), "data.unity3d")))
                {
                    // Log - Could not obfuscate data.unity3d file.
                    Obfuscator.Report.Append(EReportLevel.Warning, "Could not obfuscate data.unity3d file in cache directory.");
                }
            }
            else
            {
                // Log - Assets are not bundled.
                Obfuscator.Report.Append(EReportLevel.Info, "Assets are not bundled. Obfuscating globalgamemanagers.assets file.");

                // Try to override the globalgamemanagers.assets file in tmp directory.
                if (!this.TryOverrideGlobalGameManagersAssets(Path.Combine(this.GetBuildAssetsDirectory_Tmp(), "globalgamemanagers.assets")))
                {
                    // Log - Could not obfuscate globalgamemanagers.assets file.
                    Obfuscator.Report.Append(EReportLevel.Warning, "Could not obfuscate globalgamemanagers.assets file in tmp directory.");
                }

                // Try to override the globalgamemanagers.assets file in library directory.
                if (!this.TryOverrideGlobalGameManagersAssets(Path.Combine(this.GetBuildAssetsDirectory_Cache(), "globalgamemanagers.assets")))
                {
                    // Log - Could not obfuscate globalgamemanagers.assets file.
                    Obfuscator.Report.Append(EReportLevel.Warning, "Could not obfuscate globalgamemanagers.assets file in cache directory.");
                }
            }

            return true;
        }

#endif

        #endregion

        // On Post Process Pipeline
        #region On Post Process Pipeline

        public override bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            bool var_Result = base.OnPostPipelineProcess(_StepOutput);

            // Add mapping to output.
            var_Result = var_Result && _StepOutput.Add(PostAssemblyBuild.Pipeline.Component.Member.Type.MonoBehaviourRenameObfuscationComponent.CMonobehaviourObfuscationMapping, this.MonobehaviourObfuscationMapping, true);

            return var_Result;
        }

        #endregion
    }
}
