// System
using System;
using System.IO;

// Unity
using UnityEditor;
using UnityEditor.SceneManagement;

// OPS - Report
using OPS.Editor.Report;

// OPS - Project
using OPS.Editor.Project;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Platform.Build;
using OPS.Obfuscator.Editor.Platform.Editor;
using OPS.Obfuscator.Editor.Settings;

namespace OPS.Obfuscator.Editor
{
    /// <summary>
    /// The start point for the whole obfuscation.
    /// </summary>
    public class Obfuscator : AProject
    {
        // Singleton
        #region Singleton
        
        /// <summary>
        /// Obfuscator singleton instance. Has to be new initialized each build.
        /// </summary>
        public static Obfuscator Singleton { get; set; }

        #endregion

        // Constructor
        #region Constructor

        /// <summary>
        /// Static constructor, creating a new Obfuscator.Singleton instance.
        /// </summary>
        static Obfuscator()
        {
            Obfuscator.Init();
        }

        #endregion

        // Init
        #region Init

        /// <summary>
        /// Creating a new Obfuscator.Singleton instance.
        /// </summary>
        public static void Init()
        {
            Obfuscator.Singleton = new Obfuscator();
        }

        #endregion

        // Editor Platform
        #region Editor Platform

        /// <summary>
        /// Loads the EditorPlatform based on the local platform.
        /// </summary>
        private static DefaultEditorPlatform LoadEditorPlatform()
        {
            // Result.
            DefaultEditorPlatform var_EditorPlatform;

            // Switch platform.
            EEditorPlatform var_RunningEditorPlatform = Platform.Helper.EditorPlatformHelper.RunningPlatform();
            switch(var_RunningEditorPlatform)
            {
                case EEditorPlatform.Windows:
                    {
                        var_EditorPlatform = new WindowsEditorPlatform();
                        break;
                    }
                case EEditorPlatform.Mac:
                    {
                        var_EditorPlatform = new MacEditorPlatform();
                        break;
                    }
                case EEditorPlatform.Linux:
                    {
                        var_EditorPlatform = new LinuxEditorPlatform();
                        break;
                    }
                default:
                    {
                        var_EditorPlatform = new DefaultEditorPlatform();
                        break;
                    }
            }

            Report.Append(EReportLevel.Info, "Active Editor Platform: " + var_EditorPlatform.ToString());

            return var_EditorPlatform;
        }

        #endregion

        // Build Platform
        #region Build Platform

        /// <summary>
        /// Loads the BuildPlatform based on the build target.
        /// </summary>
        private static DefaultBuildPlatform LoadBuildPlatform(BuildTarget _BuildTarget)
        {
            // Result.
            DefaultBuildPlatform var_DefaultBuildPlatform;

            switch (_BuildTarget)
            {
                case BuildTarget.WSAPlayer:
                    {
                        var_DefaultBuildPlatform = new WindowsStoreAppBuildPlatform();
                        break;
                    }
                default:
                    {
                        var_DefaultBuildPlatform = new DefaultBuildPlatform();
                        break;
                    }
            }

            Report.Append(EReportLevel.Info, "Active Build Platform: " + var_DefaultBuildPlatform.ToString());

            return var_DefaultBuildPlatform;
        }

        #endregion

        // Report
        #region Report

        /// <summary>
        /// The active report.
        /// </summary>
        public static ProjectReport Report
        {
            get
            {
                return Singleton.ActiveReport;
            }
            private set
            {
                Singleton.ActiveReport = value;
            }
        }

        /// <summary>
        /// The default report directory path.
        /// </summary>
        /// <returns></returns>
        private static String GetReportDirectoryPath()
        {
            return OPS.Obfuscator.Editor.IO.PathHelper.Get_Obfuscator_Log_Directory();
        }

        /// <summary>
        /// Get the report file path.
        /// </summary>
        /// <returns></returns>
        private static String GetReportFilePath()
        {
            // Get a valid report file path.
            try
            {
                // Try load settings and use the set path there!
                ObfuscatorSettings var_Settings = ObfuscatorSettings.Load();

                // Check if a custom log file path is set.
                bool var_UseCustomLogFile = true;

                // Check if custom log path had set. 
                if (!var_Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Debug.LoggingComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Debug.LoggingComponent.CEnable_Custom_Log))
                {
                    var_UseCustomLogFile = false;
                }
                else
                {
                    // Check if custom log path is empty.
                    String var_ReplacedWhitespace = System.Text.RegularExpressions.Regex.Replace(var_Settings.Get_ComponentSettings_As_String(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Debug.LoggingComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Debug.LoggingComponent.CCustom_Log_FilePath), @"\s+", " ");

                    if (String.IsNullOrEmpty(var_ReplacedWhitespace.Trim()))
                    {
                        var_UseCustomLogFile = false;
                    }
                }

                // Check if either custom or default log path should be used.
                if (var_UseCustomLogFile)
                {
                    // Get custom path.
                    String var_CustomFilePath = var_Settings.Get_ComponentSettings_As_String(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Debug.LoggingComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Debug.LoggingComponent.CCustom_Log_FilePath);

                    // Check if has extension. Then it is a file.
                    if (System.IO.Path.HasExtension(var_CustomFilePath))
                    {
                        // Do nothing is already a file.
                    }
                    else
                    {
                        // Add build target.
                        String var_CustomFileName = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString() + ".txt";

                        var_CustomFilePath = Path.Combine(var_CustomFilePath, var_CustomFileName);
                    }

                    // Validate path, if the path is not valid use default path.
                    if (!ValidateReportPath(var_CustomFilePath))
                    {
                        // Log warning and use default path.
                        UnityEngine.Debug.LogWarning(String.Format("[OPS.OBF] Custom log file path '{0}' is not valid! Using default path.", var_CustomFilePath));
                    }
                    else
                    {
                        return var_CustomFilePath;
                    }
                }

                // No custom path or invalid custom path. Use default path.
                String var_DefaultFileName = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString() + ".txt";

                String var_DefaultDirectoryPath = GetReportDirectoryPath();
                String var_DefaultFullFilePath = Path.Combine(var_DefaultDirectoryPath, var_DefaultFileName);

                // Validate the default path, if the path is not valid use temporary path.
                if (!ValidateReportPath(var_DefaultFullFilePath))
                {
                    // Log warning and use temporary path.
                    UnityEngine.Debug.LogWarning(String.Format("[OPS.OBF] Default log file path '{0}' is not valid! Using temporary path.", var_DefaultFullFilePath));

                    // Use temporary path.
                    var_DefaultFullFilePath = Path.GetTempFileName();
                }

                return var_DefaultFullFilePath;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(String.Format("[OPS.OBF] Could not get report file path properly, now using a temporary path! Exception: {0}", e.ToString()));

                return Path.GetTempFileName();
            }
        }

        /// <summary>
        /// Remove the last report file, if there already exist one on the same path.
        /// </summary>
        private static void RemoveReportFile()
        {
            if(Report == null)
            {
                return;
            }

            try
            {
                // Remove Report Path
                if (System.IO.File.Exists(Report.FilePath))
                {
                    System.IO.File.Delete(Report.FilePath);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogWarning(String.Format("[OPS.OBF] Could not remove '{0}' properly! Exception: {1}", Report.FilePath, e.ToString()));
            }

            try
            {
                // Remove Report .meta Path
                if (System.IO.File.Exists(Report.FilePath + ".meta"))
                {
                    System.IO.File.Delete(Report.FilePath + ".meta");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(String.Format("[OPS.OBF] Could not remove '{0}.meta' properly! Exception: {1}", Report.FilePath, e.ToString()));
            }
        }

        /// <summary>
        /// Validate if a file path is a valid path on the system.
        /// </summary>
        /// <param name="_FilePath">The path to validate.</param>
        /// <returns></returns>
        private static bool ValidateReportPath(String _FilePath)
        {
            // For creating a FileInfo instance the file does not need to exist.
            FileInfo var_FileInfo = null;

            // Try get the file info.
            try
            {
                var_FileInfo = new FileInfo(_FilePath);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }

            // If the file info is null, the file name is not valid.
            if (ReferenceEquals(var_FileInfo, null))
            {
                // The file name is not valid.
                return false;
            }

            // The file name is valid.
            return true;
        }

        #endregion

        // PreBuild
        #region PreBuild

        /// <summary>
        /// Pre build running process.
        /// </summary>
        /// <param name="_EditorSettings"></param>
        /// <param name="_BuildSettings"></param>
        /// <returns></returns>
        public bool PreBuild(OPS.Obfuscator.Editor.Settings.Unity.Editor.EditorSettings _EditorSettings, OPS.Obfuscator.Editor.Settings.Unity.Build.BuildSettings _BuildSettings)
        {
            // Check parameter.
            if (_EditorSettings == null)
            {
                throw new ArgumentNullException("_EditorSettings");
            }
            if (_BuildSettings == null)
            {
                throw new ArgumentNullException("_BuildSettings");
            }

            // Load Obfuscator Settings
            ObfuscatorSettings var_ObfuscatorSettings = ObfuscatorSettings.Load();

            if (!var_ObfuscatorSettings.Get_Setting_AsBool(ObfuscatorSettings.Global_Enable_Obfuscation))
            {
                UnityEngine.Debug.LogWarning("[OPS.OBF] Obfuscation is deactivated by settings!");

                return true;
            }

            // User has to save scenes or want to abort them!
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                UnityEngine.Debug.LogWarning("[OPS.OBF] Obfuscation got deactivated because current scenes have not been saved!");

                return true;
            }

            // Create new Report
            Report = new ProjectReport("OPS", "OBF", "Obfuscation Report", "Report for the obfuscation.", GetReportFilePath());

            // Remove old Report.
            RemoveReportFile();

            // Set Header for report
            Report.SetHeader("Setup");

            // Load Editor Settings
            _EditorSettings.EditorPlatform = LoadEditorPlatform();

            // Load Build Settings
            _BuildSettings.BuildPlatform = LoadBuildPlatform(_BuildSettings.BuildTarget);

            try
            {
                UnityEngine.Debug.Log("[OPS.OBF] Prepare obfuscation...");

                // Init Step.
                Project.PreBuild.PreBuildStep var_PreBuildStep = new Project.PreBuild.PreBuildStep(_EditorSettings, _BuildSettings);

                // Run Step.
                return this.RunStep(var_PreBuildStep);
            }
            catch (Exception e)
            {
                Report.Append(EReportLevel.Error, "Processing 'PreBuild' failed. Error: " + e.ToString(), true);

                return false;
            }
            finally
            {
                //Save Log
                Report.SaveToFile(true);
            }
        }

        #endregion

        // PostAssemblyBuild
        #region PostAssemblyBuild

        /// <summary>
        /// Running the post assembly build process.
        /// </summary>
        /// <param name="_EditorSettings"></param>
        /// <param name="_BuildSettings"></param>
        /// <returns></returns>
        public bool PostAssemblyBuild(OPS.Obfuscator.Editor.Settings.Unity.Editor.EditorSettings _EditorSettings, OPS.Obfuscator.Editor.Settings.Unity.Build.BuildSettings _BuildSettings)
        {
            if(_EditorSettings == null)
            {
                throw new ArgumentNullException("_EditorSettings");
            }
            if (_BuildSettings == null)
            {
                throw new ArgumentNullException("_BuildSettings");
            }

            // Load Obfuscator Settings
            ObfuscatorSettings var_ObfuscatorSettings = ObfuscatorSettings.Load();

            // Check if obfuscation is deactivated.
            if (!var_ObfuscatorSettings.Get_Setting_AsBool(ObfuscatorSettings.Global_Enable_Obfuscation))
            {
                return true;
            }

            // Create new Report
            Report = new ProjectReport("OPS", "OBF", "Obfuscation Report", "Report for the obfuscation.", GetReportFilePath());

            // Set Header for report
            Report.SetHeader("Setup");

            // Load Editor Settings
            _EditorSettings.EditorPlatform = LoadEditorPlatform();

            // Load Build Settings
            _BuildSettings.BuildPlatform = LoadBuildPlatform(_BuildSettings.BuildTarget);

            try
            {
                UnityEngine.Debug.Log("[OPS.OBF] Obfuscate...");

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Start 'PostAssemblyBuild' process.");

                // Init Step.
                Project.PostAssemblyBuild.PostAssemblyBuildStep var_PostAssemblyBuildStep = new Project.PostAssemblyBuild.PostAssemblyBuildStep(_EditorSettings, _BuildSettings);

                // Run Step.
                return this.RunStep(var_PostAssemblyBuildStep);
            }
            catch (Exception e)
            {
                Report.Append(EReportLevel.Error, "Processing 'PostAssemblyBuild' failed. Error: " + e.ToString(), true);

                return false;
            }
            finally
            {
                //Save Report
                Report.SaveToFile(true);
            }
        }

        #endregion

        // PostBuild
        #region PostBuild

        /// <summary>
        /// Running the post build process.
        /// </summary>
        /// <param name="_EditorSettings"></param>
        /// <param name="_BuildSettings"></param>
        /// <returns></returns>
        public bool PostBuild(OPS.Obfuscator.Editor.Settings.Unity.Editor.EditorSettings _EditorSettings, OPS.Obfuscator.Editor.Settings.Unity.Build.BuildSettings _BuildSettings)
        {
            if (_EditorSettings == null)
            {
                throw new ArgumentNullException("_EditorSettings");
            }
            if (_BuildSettings == null)
            {
                throw new ArgumentNullException("_BuildSettings");
            }

            // Load Obfuscator Settings
            ObfuscatorSettings var_ObfuscatorSettings = ObfuscatorSettings.Load();

            // Check if obfuscation is deactivated.
            if (!var_ObfuscatorSettings.Get_Setting_AsBool(ObfuscatorSettings.Global_Enable_Obfuscation))
            {
                return true;
            }

            // Create new Report
            Report = new ProjectReport("OPS", "OBF", "Obfuscation Report", "Report for the obfuscation.", GetReportFilePath());

            // Set Header for report
            Report.SetHeader("Setup");

            // Load Editor Settings
            _EditorSettings.EditorPlatform = LoadEditorPlatform();

            // Load Build Settings
            _BuildSettings.BuildPlatform = LoadBuildPlatform(_BuildSettings.BuildTarget);

            try
            {
                UnityEngine.Debug.Log("[OPS.OBF] Post process...");

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Start 'PostBuild' process.");

                // Init Step.
                Project.PostBuild.PostBuildStep var_PostBuildStep = new Project.PostBuild.PostBuildStep(_EditorSettings, _BuildSettings);

                // Run Step.
                return this.RunStep(var_PostBuildStep);
            }
            catch (Exception e)
            {
                Report.Append(EReportLevel.Error, "Processing 'PostBuild' failed. Error: " + e.ToString(), true);

                return false;
            }
            finally
            {
                //Save Report
                Report.SaveToFile(true);
            }
        }

        #endregion

        // PostAssetsBuild
        #region PostAssetsBuild

        /// <summary>
        /// Running the post assets build process.
        /// </summary>
        /// <param name="_EditorSettings"></param>
        /// <param name="_BuildSettings"></param>
        /// <returns></returns>
        public bool PostAssetsBuild(OPS.Obfuscator.Editor.Settings.Unity.Editor.EditorSettings _EditorSettings, OPS.Obfuscator.Editor.Settings.Unity.Build.BuildSettings _BuildSettings)
        {
            if (_EditorSettings == null)
            {
                throw new ArgumentNullException("_EditorSettings");
            }
            if (_BuildSettings == null)
            {
                throw new ArgumentNullException("_BuildSettings");
            }

            // Load Obfuscator Settings
            ObfuscatorSettings var_ObfuscatorSettings = ObfuscatorSettings.Load();

            // Check if obfuscation is deactivated.
            if (!var_ObfuscatorSettings.Get_Setting_AsBool(ObfuscatorSettings.Global_Enable_Obfuscation))
            {
                return true;
            }

            // Create new Report
            Report = new ProjectReport("OPS", "OBF", "Obfuscation Report", "Report for the obfuscation.", GetReportFilePath());

            // Set Header for report
            Report.SetHeader("Setup");

            // Load Editor Settings
            _EditorSettings.EditorPlatform = LoadEditorPlatform();

            // Load Build Settings
            _BuildSettings.BuildPlatform = LoadBuildPlatform(_BuildSettings.BuildTarget);

            try
            {
                UnityEngine.Debug.Log("[OPS.OBF] Post process assets...");

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Start 'PostAssetsBuild' process.");

                // Init Step.
                Project.PostAssetsBuild.PostAssetsBuildStep var_PostAssetsBuildStep = new Project.PostAssetsBuild.PostAssetsBuildStep(_EditorSettings, _BuildSettings);

                // Run Step.
                return this.RunStep(var_PostAssetsBuildStep);
            }
            catch (Exception e)
            {
                Report.Append(EReportLevel.Error, "Processing 'PostAssetsBuild' failed. Error: " + e.ToString(), true);

                return false;
            }
            finally
            {
                //Save Report
                Report.SaveToFile(true);
            }
        }

        #endregion
    }
}