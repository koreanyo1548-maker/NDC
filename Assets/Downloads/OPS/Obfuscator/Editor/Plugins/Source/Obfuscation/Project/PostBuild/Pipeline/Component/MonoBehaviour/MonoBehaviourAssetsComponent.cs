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

namespace OPS.Obfuscator.Editor.Project.PostBuild.Pipeline.Component
{
    internal class MonoBehaviourAssetsComponent : APostBuildComponent
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
                return "Post Process Unity MonoBehaviour Assets On Post Build";
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
        /// Returns the directory to the build assets. This is the directory where the globalgamemanagers.assets or data.unity3d file is located.
        /// </summary>
        /// <returns>The directory to the build assets.</returns>
        private String GetBuildAssetsDirectory()
        {
            switch(this.Step.BuildSettings.BuildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return this.GetBuildAssetsDirectory_Windows();
                case UnityEditor.BuildTarget.StandaloneLinux64:
                    return this.GetBuildAssetsDirectory_Linux();
                case UnityEditor.BuildTarget.StandaloneOSX:
                    return this.GetBuildAssetsDirectory_MacOS();
            }

            return null;
        }

        /// <summary>
        /// Returns the directory to the build assets for windows.
        /// </summary>
        /// <returns>The directory to the build assets for windows.</returns>
        private String GetBuildAssetsDirectory_Windows()
        {
            // Base output path.
            String var_OutputPath = this.Step.BuildSettings.UnityBuildReport.summary.outputPath;

            // Check if the build is built into the project.
            if (this.Step.BuildSettings.BuildIntoProject)
            {
                // For example: [Path]/Builds/Windows/Game.exe -> [Path]/Builds/Windows/build/bin/Game_Data
                String var_Path = Path.Combine(Path.GetDirectoryName(var_OutputPath), "build");
                var_Path = Path.Combine(var_Path, "bin");
                var_Path = Path.Combine(var_Path, Path.GetFileNameWithoutExtension(var_OutputPath) + "_Data");
                return var_Path;
            }
            else
            {
                // For example: [Path]/Builds/Windows/Game.exe -> [Path]/Builds/Windows/Game_Data
                String var_Path = Path.Combine(Path.GetDirectoryName(var_OutputPath), Path.GetFileNameWithoutExtension(var_OutputPath) + "_Data");
                return var_Path;
            }
        }

        /// <summary>
        /// Returns the directory to the build assets for linux.
        /// </summary>
        /// <returns>The directory to the build assets for linux.</returns>
        private String GetBuildAssetsDirectory_Linux()
        {
            // Base output path.
            String var_OutputPath = this.Step.BuildSettings.UnityBuildReport.summary.outputPath;

            // Check if the build is built into the project.
            if (this.Step.BuildSettings.BuildIntoProject)
            {
                // For example: [Path]/Builds/Linux/Game.exe -> [Path]/Builds/Linux/build/bin/Game_Data
                String var_Path = Path.Combine(Path.GetDirectoryName(var_OutputPath), "build");
                var_Path = Path.Combine(var_Path, "bin");
                var_Path = Path.Combine(var_Path, Path.GetFileNameWithoutExtension(var_OutputPath) + "_Data");
                return var_Path;
            }
            else
            {
                // For example: [Path]/Builds/Linux/Game.exe -> [Path]/Builds/Linux/Game_Data
                String var_Path = Path.Combine(Path.GetDirectoryName(var_OutputPath), Path.GetFileNameWithoutExtension(var_OutputPath) + "_Data");
                return var_Path;
            }
        }

        /// <summary>
        /// Returns the directory to the build assets for mac.
        /// </summary>
        /// <returns>The directory to the build assets for mac.</returns>
        private String GetBuildAssetsDirectory_MacOS()
        {
            // Base output path.
            String var_OutputPath = this.Step.BuildSettings.UnityBuildReport.summary.outputPath;

            // Check if the build is built into the project.
            if (this.Step.BuildSettings.BuildIntoProject)
            {
                // For example: [Path]/Builds/MacOS -> [Path]/Builds/MacOS/Game/Resources/Data
                String var_Path = Path.Combine(var_OutputPath, Application.productName);
                var_Path = Path.Combine(var_Path, "Resources");
                var_Path = Path.Combine(var_Path, "Data");
                return var_Path;
            }
            else
            {
                // For example: [Path]/Builds/MacOS/Game.app -> [Path]/Builds/MacOS/Game.app/Contents/Resources/Data
                String var_Path = Path.Combine(var_OutputPath, "Contents");
                var_Path = Path.Combine(var_Path, "Resources");
                var_Path = Path.Combine(var_Path, "Data");
                return var_Path;
            }
        }

        /// <summary>
        /// Returns the path to the globalgamemanagers.assets file.
        /// </summary>
        /// <returns>The path to the globalgamemanagers.assets file.</returns>
        private String GetGlobalGameManagersAssetsPath()
        {
            String var_Path = this.GetBuildAssetsDirectory();

            var_Path = Path.Combine(var_Path, "globalgamemanagers.assets");

            return var_Path;
        }

        /// <summary>
        /// Returns the path to the data.unity3d file.
        /// </summary>
        /// <returns>The path to the data.unity3d file.</returns>
        private String GetDataUnity3dPath()
        {
            String var_Path = this.GetBuildAssetsDirectory();

            var_Path = Path.Combine(var_Path, "data.unity3d");

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

            if(this.MonobehaviourObfuscationMapping == null || this.MonobehaviourObfuscationMapping.Count == 0)
            {
                // Log - No obfuscation mapping found.
                Obfuscator.Report.Append(EReportLevel.Warning, String.Format("No MonoBehaviour obfuscation mapping found. The globalgamemanagers.assets file will not be obfuscated."));
            }

            return var_Result;
        }

        #endregion

        // On Post Build
        #region On Post Build

#if Obfuscator_Free

        /// <summary>
        /// Does nothing in the free version.
        /// </summary>
        /// <returns>Always true.</returns>
        public override bool OnPostBuild()
        {
            return true;
        }

#else
        /// <summary>
        /// Obfuscated the MonoBehaviours in the globalgamemanagers.assets or data.unity3d file.
        /// </summary>
        /// <returns>True if successful; false otherwise.</returns>
        public override bool OnPostBuild()
        {
            // Only process if the build target is a standalone build. Other build targets get obfuscated at some other build step.
            if (!this.Step.BuildSettings.IsStandaloneBuildTarget)
            {
                return true;
            }

            // Check if the assets are bundled.
            if (this.AssetsAreBundled)
            {
                // Log - Assets are bundled.
                Obfuscator.Report.Append(EReportLevel.Info, "Assets are bundled. Obfuscating data.unity3d file.");

                // Try to override the data.unity3d file.
                if (!this.TryOverrideDataUnity3d(this.GetDataUnity3dPath()))
                {
                    return false;
                }
            }
            else
            {
                // Log - Assets are not bundled.
                Obfuscator.Report.Append(EReportLevel.Info, "Assets are not bundled. Obfuscating globalgamemanagers.assets file.");

                // Try to override the globalgamemanagers.assets file.
                if (!this.TryOverrideGlobalGameManagersAssets(this.GetGlobalGameManagersAssetsPath()))
                {
                    return false;
                }
            }

            return true;
        }


#endif

        #endregion
    }
}
