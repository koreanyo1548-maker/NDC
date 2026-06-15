using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Unity
using UnityEditor.Compilation;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;
using OPS.Obfuscator.Editor.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.Step;

// OPS - Settings
using OPS.Obfuscator.Editor.Settings;

// OPS - Obfuscator - Settings
using OPS.Obfuscator.Editor.Settings.Unity.Build;
using OPS.Obfuscator.Editor.Settings.Unity.Editor;

// OPS - Obfuscator - PreBuild
using OPS.Obfuscator.Editor.Project.PreBuild;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild
{
    public class PostAssemblyBuildStep : AStep<PreBuildOutput, PostAssemblyBuildOutput>
    {
        // Constructor
        #region Constructor

        public PostAssemblyBuildStep(EditorSettings _EditorSettings, BuildSettings _BuildSettings)
        {
            // Assign EditorSettings
            this.EditorSettings = _EditorSettings;

            // Assign BuildSettings
            this.BuildSettings = _BuildSettings;
        }

        #endregion

        // Name
        #region Name

        /// <summary>
        /// Name of the step.
        /// </summary>
        public override String Name
        {
            get
            {
                return "PostAssemblyBuild";
            }
        }

        #endregion

        // Editor Settings
        #region Editor Settings

        /// <summary>
        /// Settings used for the editor.
        /// </summary>
        public EditorSettings EditorSettings { get; private set; }

        #endregion

        // Build Settings
        #region Build Settings

        /// <summary>
        /// Settings used for the current build.
        /// </summary>
        public BuildSettings BuildSettings { get; private set; }

        #endregion

        // DataContainer
        #region DataContainer

        /// <summary>
        /// Shared data container for the project.
        /// </summary>
        public new PostAssemblyBuildDataContainer DataContainer
        {
            get
            {
                return (PostAssemblyBuildDataContainer) base.DataContainer;
            }
            private set
            {
                base.DataContainer = value;
            }
        }

        /// <summary>
        /// Load the DataContainer.
        /// </summary>
        /// <returns></returns>
        private bool LoadDataContainer()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load DataContainer");

            // Create
            this.DataContainer = new PostAssemblyBuildDataContainer();

            return true;
        }

        /// <summary>
        /// Unload the DataContainer.
        /// </summary>
        /// <returns></returns>
        private bool UnloadDataContainer()
        {
            if(this.DataContainer == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload DataContainer");

            // Clear
            this.DataContainer = null;

            return true;
        }

        #endregion

        // Obfuscator Settings
        #region Obfuscator Settings

        /// <summary>
        /// Shared settings for the project.
        /// </summary>
        public new ObfuscatorSettings Settings
        {
            get
            {
                return (ObfuscatorSettings) base.Settings;
            }
            private set
            {
                base.Settings = value;
            }
        }

        /// <summary>
        /// Load the Obfuscator Settings.
        /// </summary>
        /// <returns></returns>
        private bool LoadObfuscatorSettings()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load ObfuscatorSettings");

            // Load Settings
            this.Settings = ObfuscatorSettings.Load();

            // Modify Settings based on build platform!
            this.BuildSettings.BuildPlatform.ModifiedSettings(this.Settings);

            return true;
        }

        /// <summary>
        /// Unload the Obfuscator Settings.
        /// </summary>
        /// <returns></returns>
        private bool UnloadObfuscatorSettings()
        {
            if (this.Settings == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload ObfuscatorSettings");

            // Clear
            this.Settings = null;

            return true;
        }

        #endregion

        // Components
        #region Components

        /// <summary>
        /// Loads the components if they are not already loaded.
        /// </summary>
        private bool LoadComponents()
        {
            if(this.DataContainer == null)
            {
                return false;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Components");

            // Load all components!

            this.DataContainer.ComponentList = OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Helper.ComponentHelper.GetObfuscatorPipelineComponentList();

            this.DataContainer.CompatibilityComponentList = OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Helper.ComponentHelper.GetObfuscationCompatibilityComponentList();

            // Log
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found Components", this.DataContainer.ComponentList.Select(c => c.Name + " (" + c.ShortDescription + ")").ToList());
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found Compatibility Components", this.DataContainer.CompatibilityComponentList.Select(c => c.Name + " (" + c.ShortDescription + ")").ToList());

            return true;
        }

        /// <summary>
        /// Get the assembly load infos from the components.
        /// </summary>
        /// <returns></returns>
        private List<AssemblyLoadInfo> GetComponentsAssemblyLoadInfoList()
        {
            List<AssemblyLoadInfo> var_Result = new List<AssemblyLoadInfo>();

            for (int i = 0; i < this.DataContainer.ComponentList.Count; i++)
            {
                if(this.DataContainer.ComponentList[i] is IAssemblyProvidingComponent)
                {
                    IAssemblyProvidingComponent var_AssemblyProvidingComponent = this.DataContainer.ComponentList[i] as IAssemblyProvidingComponent;

                    try
                    {
                        var_Result.AddRange(var_AssemblyProvidingComponent.GetAssemblyLoadInfoList());
                    }
                    catch (Exception e)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Error for component: " + this.DataContainer.ComponentList[i].Name + " while receiving AssemblyLoadInfos. Exception: " + e.ToString());

                        continue;
                    }
                }
            }

            return var_Result;
        }

        /// <summary>
        /// Get all component dependencies.
        /// </summary>
        /// <param name="_AssemblyDependencyDirectoryList"></param>
        /// <returns></returns>
        private bool LoadComponentsAssemblyDependencyDirectories(out List<String> _AssemblyDependencyDirectoryList)
        {
            List<String> var_Result = new List<String>();

            for (int i = 0; i < this.DataContainer.ComponentList.Count; i++)
            {
                if (this.DataContainer.ComponentList[i] is IAssemblyProvidingComponent)
                {
                    IAssemblyProvidingComponent var_AssemblyProvidingComponent = this.DataContainer.ComponentList[i] as IAssemblyProvidingComponent;

                    try
                    {
                        var_Result.AddRange(var_AssemblyProvidingComponent.GetAssemblyReferenceDirectoryList());
                    }
                    catch (Exception e)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Error for component: " + this.DataContainer.ComponentList[i].Name + " while receiving assembly reference directories. Exception: " + e.ToString());

                        _AssemblyDependencyDirectoryList = null;
                        return false;
                    }
                }
            }

            _AssemblyDependencyDirectoryList = var_Result;
            return true;
        }

        /// <summary>
        /// Run the prepare process on _AssemblyLoadInfoList for all active IAssemblyProcessingComponent.
        /// </summary>
        /// <param name="_AssemblyLoadInfoList"></param>
        /// <returns></returns>
        private bool PrepareAssemblyLoadInfoList(List<AssemblyLoadInfo> _AssemblyLoadInfoList)
        {
            for (int i = 0; i < this.DataContainer.ComponentList.Count; i++)
            {
                if (this.DataContainer.ComponentList[i] is IAssemblyProcessingComponent)
                {
                    IAssemblyProcessingComponent var_AssemblyProcessingComponent = this.DataContainer.ComponentList[i] as IAssemblyProcessingComponent;

                    // Deactivated components will be skipped.
                    if (!var_AssemblyProcessingComponent.IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssemblyProcessingComponent);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssemblyProcessingComponent);

                    // Process all to assembly load infos.
                    for (int a = 0; a < _AssemblyLoadInfoList.Count; a++)
                    {
                        // Check if wont be processed by this component.
                        if (!var_AssemblyProcessingComponent.CanBeProcessedByComponent(_AssemblyLoadInfoList[a]))
                        {
                            continue;
                        }

                        // Prepare assemblies.
                        if (!var_AssemblyProcessingComponent.Prepare_Assemblies(_AssemblyLoadInfoList[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssemblyProcessingComponent);

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a List of all CompatibilityComponents inherit or are TObfuscationCompatibilityComponent.
        /// </summary>
        /// <typeparam name="TObfuscationCompatibilityComponent"></typeparam>
        /// <returns></returns>
        public List<TObfuscationCompatibilityComponent> GetCompatibilityComponents<TObfuscationCompatibilityComponent>()
            where TObfuscationCompatibilityComponent : IObfuscationCompatibilityComponent
        {
            List<TObfuscationCompatibilityComponent> var_ComponentList = new List<TObfuscationCompatibilityComponent>();

            for (int i = 0; i < this.DataContainer.CompatibilityComponentList.Count; i++)
            {
                if (this.DataContainer.CompatibilityComponentList[i] is TObfuscationCompatibilityComponent)
                {
                    var_ComponentList.Add((TObfuscationCompatibilityComponent)(IObfuscationCompatibilityComponent)this.DataContainer.CompatibilityComponentList[i]);
                }
            }

            return var_ComponentList;
        }


        /// <summary>
        /// Run the post process on _AssemblyLoadInfoList for all active IAssemblyProcessingComponent.
        /// </summary>
        /// <param name="_AssemblyLoadInfoList"></param>
        /// <returns></returns>
        private bool PostprocessAssemblyLoadInfoList(List<AssemblyLoadInfo> _AssemblyLoadInfoList)
        {
            for (int i = 0; i < this.DataContainer.ComponentList.Count; i++)
            {
                if (this.DataContainer.ComponentList[i] is IAssemblyProcessingComponent)
                {
                    IAssemblyProcessingComponent var_AssemblyProcessingComponent = this.DataContainer.ComponentList[i] as IAssemblyProcessingComponent;

                    // Deactivated components will be skipped.
                    if (!var_AssemblyProcessingComponent.IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssemblyProcessingComponent);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssemblyProcessingComponent);

                    // Process all to assembly load infos.
                    for (int a = 0; a < _AssemblyLoadInfoList.Count; a++)
                    {
                        // Check if wont be processed by this component.
                        if (!var_AssemblyProcessingComponent.CanBeProcessedByComponent(_AssemblyLoadInfoList[a]))
                        {
                            continue;
                        }

                        // Post process Assemblies.
                        if (!var_AssemblyProcessingComponent.PostProcess_Assemblies(_AssemblyLoadInfoList[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssemblyProcessingComponent);

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Unload all components.
        /// </summary>
        /// <returns></returns>
        private bool UnloadComponents()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Components");

            // Clear
            this.DataContainer.CompatibilityComponentList = null;
            this.DataContainer.ComponentList = null;

            return true;
        }

        #endregion

        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline for the project.
        /// </summary>
        public new PostAssemblyBuildPipeline Pipeline
        {
            get
            {
                return (PostAssemblyBuildPipeline)base.Pipeline;
            }
            private set
            {
                base.Pipeline = value;
            }
        }

        /// <summary>
        /// Load the Obfuscator Pipeline.
        /// </summary>
        /// <returns></returns>
        private bool LoadPipeline()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Pipeline");

            // Create
            this.Pipeline = new PostAssemblyBuildPipeline(this);

            // Attach Components to Pipeline
            for (int i = 0; i < this.DataContainer.ComponentList.Count; i++)
            {
                this.Pipeline.AddPipelineComponent(this.DataContainer.ComponentList[i]);
            }

            return true;
        }

        /// <summary>
        /// Unload the Obfuscator Pipeline.
        /// </summary>
        /// <returns></returns>
        private bool UnloadPipeline()
        {
            if (this.Pipeline == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Pipeline");

            // Clear
            this.Pipeline = null;

            return true;
        }

        #endregion

        // Assembly
        #region Assembly

        /// <summary>
        /// Load all AssemblyLoadInfos.
        /// </summary>
        /// <returns></returns>
        private bool LoadAssemblyLoadInfos()
        {
            if (this.DataContainer == null)
            {
                return false;
            }

            // Check if custom assemblies!
            if (this.BuildSettings.AssemblyLoadInfoList != null && this.BuildSettings.AssemblyLoadInfoList.Count != 0)
            {
                // Custom assemblies.

                this.DataContainer.AssemblyLoadInfoList = this.BuildSettings.AssemblyLoadInfoList;
            }
            else
            {
                // Component assemblies.

                this.DataContainer.AssemblyLoadInfoList = this.GetComponentsAssemblyLoadInfoList();
            }

            // Log
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found to obfuscate assemblies", this.DataContainer.AssemblyLoadInfoList.Where(a => !a.IsHelperAssembly).Select(a => a.FilePath).ToList());
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found helper assemblies", this.DataContainer.AssemblyLoadInfoList.Where(a => a.IsHelperAssembly).Select(a => a.FilePath).ToList());

            return true;
        }

        /// <summary>
        /// Load all the dependency directories.
        /// </summary>
        /// <returns></returns>
        private bool LoadAssemblyDependencyDirectories()
        {
            if (this.DataContainer == null)
            {
                return false;
            }

            HashSet<String> var_DirectoryHashSet = new HashSet<String>();

            // Iterate through all Assemblies in Current Domain (MyGame/Plugins for example)
            var AssemblyLocationArray = AppDomain.CurrentDomain.GetAssemblies().Select(a =>
            {
                try { return a.Location; }
                catch { return null; }
            }).Where(s => s != null).ToArray();

            for (int i = 0; i < AssemblyLocationArray.Length; i++)
            {
                if (AssemblyLocationArray[i] == null || String.IsNullOrEmpty(AssemblyLocationArray[i]))
                {
                    continue;
                }

                String var_Directory = System.IO.Path.GetDirectoryName(AssemblyLocationArray[i]);

                if (!var_DirectoryHashSet.Contains(var_Directory))
                {
                    var_DirectoryHashSet.Add(var_Directory);
                }
            }

            // Unity CompilationPipeline Assemblies
            foreach (UnityEditor.Compilation.Assembly var_Assembly in CompilationPipeline.GetAssemblies(AssembliesType.Player))
            {
                if (var_Assembly.compiledAssemblyReferences == null)
                {
                    continue;
                }

                foreach (String var_CompiledReference in var_Assembly.compiledAssemblyReferences)
                {
                    String var_Path = Path.GetDirectoryName(var_CompiledReference);

                    if (!var_DirectoryHashSet.Contains(var_Path))
                    {
                        var_DirectoryHashSet.Add(var_Path);
                    }
                }
            }

            // Add Editor Platform Directories
            List<String> var_EditorPlatformReferences = this.EditorSettings.EditorPlatform.AdditionalAssemblyResolvingDirectories();
            for (int i = 0; i < var_EditorPlatformReferences.Count; i++)
            {
                if (!var_DirectoryHashSet.Contains(var_EditorPlatformReferences[i]))
                {
                    var_DirectoryHashSet.Add(var_EditorPlatformReferences[i]);
                }
            }

            // Add Build Platform Directories
            List<String> var_BuildPlatformReferences = this.BuildSettings.BuildPlatform.AdditionalAssemblyResolvingDirectories();
            for (int i = 0; i < var_BuildPlatformReferences.Count; i++)
            {
                if (!var_DirectoryHashSet.Contains(var_BuildPlatformReferences[i]))
                {
                    var_DirectoryHashSet.Add(var_BuildPlatformReferences[i]);
                }
            }

            // Add Component Directories
            List<String> var_ComponentAssemblyDependencyList;
            if (!this.LoadComponentsAssemblyDependencyDirectories(out var_ComponentAssemblyDependencyList))
            {
                throw new Exception("[OPS.OBF] Some Component failed to load the assembly dependency!");
            }
            for (int i = 0; i < var_ComponentAssemblyDependencyList.Count; i++)
            {
                if (!var_DirectoryHashSet.Contains(var_ComponentAssemblyDependencyList[i]))
                {
                    var_DirectoryHashSet.Add(var_ComponentAssemblyDependencyList[i]);
                }
            }

            // Add Settings Directories from obfuscation settings.
            List<String> var_SettingAssemblyDependencyList = this.Settings.Get_Or_Create_ComponentSettings(PostAssemblyBuild.Pipeline.Component.Assembly.AssemblyObfuscationComponent.CSettingsKey).Get_Setting_AsArray(PostAssemblyBuild.Pipeline.Component.Assembly.AssemblyObfuscationComponent.CObfuscate_Assembly_Additional_Assembly_Dependencies_Array).ToList();
            for (int i = 0; i < var_SettingAssemblyDependencyList.Count; i++)
            {
                if (!var_DirectoryHashSet.Contains(var_SettingAssemblyDependencyList[i]))
                {
                    var_DirectoryHashSet.Add(var_SettingAssemblyDependencyList[i]);
                }
            }

            // Add additional (user custom) Directories from build settings.
            for (int i = 0; i < this.BuildSettings.AssemblyDependencyDirectoryPathList.Count; i++)
            {
                if (!var_DirectoryHashSet.Contains(this.BuildSettings.AssemblyDependencyDirectoryPathList[i]))
                {
                    var_DirectoryHashSet.Add(this.BuildSettings.AssemblyDependencyDirectoryPathList[i]);
                }
            }

            this.DataContainer.AssemblyDependencyDirectoryPathList = var_DirectoryHashSet.ToList();

            // Log
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found dependency directories", this.DataContainer.AssemblyDependencyDirectoryPathList);

            return true;
        }

        /// <summary>
        /// Prepare and loads the assemblies contained in AssemblyLoadInfoList. Important: AssemblyCache has to be loaded!
        /// </summary>
        private bool LoadAssemblies()
        {
            // Requires the datacontainer.
            if(this.DataContainer == null)
            {
                return false;
            }

            // Requires assemblyloadinfos to load.
            if(this.DataContainer.AssemblyLoadInfoList == null)
            {
                return false;
            }

            // Prepare assemblies before loading.
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Prepare Assemblies");

            if(!this.PrepareAssemblyLoadInfoList(this.DataContainer.AssemblyLoadInfoList))
            {
                return false;
            }
            
            // Load assemblies.
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Load Assemblies");

            // Init
            this.DataContainer.HelperAssemblyList = new List<AssemblyInfo>();
            this.DataContainer.ObfuscateAssemblyList = new List<AssemblyInfo>();

            // Load to obfuscate and helper assemblies
            for (int i = 0; i < this.DataContainer.AssemblyLoadInfoList.Count; i++)
            {
                // Skip if null entry!
                if (this.DataContainer.AssemblyLoadInfoList[i] == null)
                {
                    continue;
                }

                try
                {
                    // Load AssemblyDefinition
                    AssemblyDefinition var_AssemblyDefinition = Editor.Assembly.Mono.Helper.AssemblyHelper.LoadAssembly(this.DataContainer.AssemblyLoadInfoList[i].FilePath, this.DataContainer.AssemblyResolver, this.BuildSettings.BuildTarget);

                    // Create AssemblyInfo
                    AssemblyInfo var_AssemblyInfo = new AssemblyInfo();
                    var_AssemblyInfo.AssemblyDefinition = var_AssemblyDefinition;
                    var_AssemblyInfo.AssemblyLoadInfo = this.DataContainer.AssemblyLoadInfoList[i];

                    // Assign to 
                    if (this.DataContainer.AssemblyLoadInfoList[i].IsHelperAssembly)
                    {
                        this.DataContainer.HelperAssemblyList.Add(var_AssemblyInfo);
                    }
                    else
                    {
                        if (this.DataContainer.AssemblyLoadInfoList[i].Obfuscate)
                        {
                            this.DataContainer.ObfuscateAssemblyList.Add(var_AssemblyInfo);
                        }
                        else
                        {
                            var_AssemblyDefinition.Dispose();
                        }
                    }
                }
                catch (Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not load: " + this.DataContainer.AssemblyLoadInfoList[i].FilePath + "! Exception: " + e.ToString(), true);
                }
            }

            return true;
        }

        /// <summary>
        /// Saves the assemblies from AssemblyList.
        /// </summary>
        public bool SaveAssemblies()
        {
            // No DataContainer so non assembly got loaded.
            if (this.DataContainer == null)
            {
                return true;
            }

            // Check if assemblies already got loaded. If not, nothing can be disposed.
            if (this.DataContainer.ObfuscateAssemblyList == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Save Assemblies");

            // Save
            bool var_FailedToSave = false;

            for (int i = 0; i < this.DataContainer.ObfuscateAssemblyList.Count; i++)
            {
                if (this.DataContainer.ObfuscateAssemblyList[i] == null
                    || this.DataContainer.ObfuscateAssemblyList[i].AssemblyDefinition == null)
                {
                    continue;
                }

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Saving assembly: " + this.DataContainer.ObfuscateAssemblyList[i].AssemblyLoadInfo.FilePath);

                try
                {
                    if (Editor.Assembly.Mono.Helper.AssemblyHelper.SaveAssembly(this.DataContainer.ObfuscateAssemblyList[i].AssemblyDefinition, this.DataContainer.ObfuscateAssemblyList[i].AssemblyLoadInfo.FilePath))
                    {

                    }
                    else
                    {
                        var_FailedToSave = true;

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not save assembly: " + this.DataContainer.ObfuscateAssemblyList[i].AssemblyLoadInfo.FilePath + "!", true);
                    }
                }
                catch (Exception e)
                {
                    var_FailedToSave = true;

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not save assembly: " + this.DataContainer.ObfuscateAssemblyList[i].AssemblyLoadInfo.FilePath + "! Exception: " + e.ToString(), true);
                }
            }

            // Only if save did not fail.
            if(!var_FailedToSave)
            {
                // Post process assemblies after saving and obfuscation.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Post process Assemblies");

                if (!this.PostprocessAssemblyLoadInfoList(this.DataContainer.AssemblyLoadInfoList))
                {
                    return false;
                }
            }

            return !var_FailedToSave;
        }

        /// <summary>
        /// Disposes all to obfuscate and helper assemblies.
        /// </summary>
        private bool UnloadAssemblies()
        {
            // No DataContainer so non assembly got loaded.
            if (this.DataContainer == null)
            {
                return true;
            }

            // Check if assemblies already got loaded. If not, nothing can be disposed.
            if (this.DataContainer.ObfuscateAssemblyList == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Assemblies");

            // Dispose to Obfuscate Assemblies!            
            for (int i = 0; i < this.DataContainer.ObfuscateAssemblyList.Count; i++)
            {
                if (this.DataContainer.ObfuscateAssemblyList[i] == null 
                    || this.DataContainer.ObfuscateAssemblyList[i].AssemblyDefinition == null)
                {
                    continue;
                }

                try
                {
                    this.DataContainer.ObfuscateAssemblyList[i].AssemblyDefinition.Dispose();

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Unloaded assembly successfully: " + this.DataContainer.ObfuscateAssemblyList[i].AssemblyLoadInfo.FilePath);
                }
                catch (Exception e)
                {
                    // Log as debug.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not unload assembly: " + this.DataContainer.ObfuscateAssemblyList[i].AssemblyLoadInfo.FilePath);
                }
            }

            // Dispose to Helper Assemblies!
            for (int i = 0; i < this.DataContainer.HelperAssemblyList.Count; i++)
            {
                if (this.DataContainer.HelperAssemblyList[i] == null 
                    || this.DataContainer.HelperAssemblyList[i].AssemblyDefinition == null)
                {
                    continue;
                }

                try
                {
                    this.DataContainer.HelperAssemblyList[i].AssemblyDefinition.Dispose();

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Unloaded assembly successfully: " + this.DataContainer.HelperAssemblyList[i].AssemblyLoadInfo.FilePath);
                }
                catch (Exception e)
                {
                    // Log as debug.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not unload assembly: " + this.DataContainer.HelperAssemblyList[i].AssemblyLoadInfo.FilePath);
                }
            }

            return true;
        }

        /// <summary>
        /// True: _AssemblyName is in the to obfuscate assemblies.
        /// </summary>
        /// <param name="_AssemblyName"></param>
        /// <returns></returns>
        public bool IsAssemblyInObfuscateAssemblies(String _AssemblyName)
        {
            for (int i = 0; i < this.DataContainer.ObfuscateAssemblyList.Count; i++)
            {
                if (this.DataContainer.ObfuscateAssemblyList[i].Name == _AssemblyName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the AssemblyInfo, from the to obfuscate assemblies, by _AssemblyName.
        /// </summary>
        /// <param name="_AssemblyName"></param>
        /// <returns></returns>
        public AssemblyInfo GetAssemblyFromObfuscateAssemblies(String _AssemblyName)
        {
            for (int i = 0; i < this.DataContainer.ObfuscateAssemblyList.Count; i++)
            {
                if (this.DataContainer.ObfuscateAssemblyList[i].Name == _AssemblyName)
                {
                    return this.DataContainer.ObfuscateAssemblyList[i];
                }
            }

            return null;
        }

        #endregion

        // Assembly Resolver
        #region Assembly Resolver

        /// <summary>
        /// Load the AssemblyResolver. Important: AssemblyDependencyDirectoryPathList and AssemblyLoadInfoList must be set!
        /// </summary>
        private bool LoadAssemblyResolver()
        {
            if (this.DataContainer == null)
            {
                return false;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Assembly Resolver");

            // Set AssemblyResolver
            this.DataContainer.AssemblyResolver = new AssemblyResolver();

            // Add Dependencies
            foreach (var var_DirectoryPath in this.DataContainer.AssemblyDependencyDirectoryPathList)
                this.DataContainer.AssemblyResolver.AddSearchDirectory(var_DirectoryPath);

            foreach (var var_AssemblyLoadInfo in this.DataContainer.AssemblyLoadInfoList)
                this.DataContainer.AssemblyResolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(var_AssemblyLoadInfo.FilePath));

            return true;
        }

        /// <summary>
        /// Disposes the AssemblyResolver.
        /// </summary>
        private bool UnloadAssemblyResolver()
        {
            if(this.DataContainer == null)
            {
                return true;
            }

            if (this.DataContainer.AssemblyResolver == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Assembly Resolver");

            try
            {
                this.DataContainer.AssemblyResolver.Dispose();

                this.DataContainer.AssemblyResolver = null;

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Unloaded successfully the AssemblyResolver.");
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not unload the AssemblyResolver.");
            }

            return true;
        }

        #endregion

        // Cache
        #region Cache

        /// <summary>
        /// Load the type/member caches.
        /// </summary>
        /// <returns></returns>
        private bool LoadCache()
        {
            if(this.DataContainer == null)
            {
                return false;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Cache");

            // Load cache
            this.DataContainer.CacheDictionary = new Dictionary<EMemberType, IMemberCache>();

            this.DataContainer.CacheDictionary.Add(EMemberType.Type, new TypeCache(this));
            this.DataContainer.CacheDictionary.Add(EMemberType.Method, new MethodCache(this));
            this.DataContainer.CacheDictionary.Add(EMemberType.Field, new FieldCache(this));
            this.DataContainer.CacheDictionary.Add(EMemberType.Property, new PropertyCache(this));
            this.DataContainer.CacheDictionary.Add(EMemberType.Event, new EventCache(this));

            return true;
        }

        /// <summary>
        /// Returns the IMemberCache for TCache.
        /// </summary>
        /// <typeparam name="TCache"></typeparam>
        /// <returns></returns>
        public TCache GetCache<TCache>() 
            where TCache : IMemberCache
        {
            if(typeof(TCache) == typeof(TypeCache))
            {
                return (TCache)this.DataContainer.CacheDictionary[EMemberType.Type];
            }
            if (typeof(TCache) == typeof(MethodCache))
            {
                return (TCache)this.DataContainer.CacheDictionary[EMemberType.Method];
            }
            if (typeof(TCache) == typeof(FieldCache))
            {
                return (TCache)this.DataContainer.CacheDictionary[EMemberType.Field];
            }
            if (typeof(TCache) == typeof(PropertyCache))
            {
                return (TCache)this.DataContainer.CacheDictionary[EMemberType.Property];
            }
            if (typeof(TCache) == typeof(EventCache))
            {
                return (TCache)this.DataContainer.CacheDictionary[EMemberType.Event];
            }

            return default(TCache);
        }

        /// <summary>
        /// Unload the type and member caches.
        /// </summary>
        /// <returns></returns>
        private bool UnloadCache()
        {
            if (this.DataContainer == null)
            {
                return true;
            }

            if (this.DataContainer.CacheDictionary == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Cache");

            // Clear cache.
            this.DataContainer.CacheDictionary = null;

            return true;
        }

        #endregion
        
        // RenameManager
        #region RenameManager

        /// <summary>
        /// Load up and setup the RenameManager.
        /// </summary>
        private bool LoadRenameManager()
        {
            if (this.DataContainer == null)
            {
                return false;
            }
            
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load RenameManager");

            // Create
            this.DataContainer.RenameManager = new RenameManager(this);
            this.DataContainer.RenameManager.Load();

            return true;
        }

        /// <summary>
        /// Unload the RenameManager.
        /// </summary>
        /// <returns></returns>
        private bool UnloadRenameManager()
        {
            if (this.DataContainer == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload RenameManager");

            // Clear
            this.DataContainer.RenameManager.Unload();
            this.DataContainer.RenameManager = null;

            return true;
        }

        #endregion

        // Type
        #region Type

        // Cache

        // Methods

        /// <summary>
        /// Get the AssemblyInfo, from the to obfuscate assemblies, for _TypeDefinition.
        /// If _TypeDefinition is not part of the to obfuscate assemblies, null will be returned. 
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public AssemblyInfo GetAssembly(TypeDefinition _TypeDefinition)
        {
            if (_TypeDefinition == null)
            {
                throw new ArgumentNullException("_TypeDefinition");
            }

            String var_ScopeName = _TypeDefinition.GetScopeName();

            // Has no scope! Somehow?
            if (String.IsNullOrEmpty(var_ScopeName))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, _TypeDefinition.ToString() + " has no scope!?");
                return null;
            }

            return GetAssemblyFromObfuscateAssemblies(var_ScopeName);
        }

        /// <summary>
        /// True: _TypeReferences Scope/Assembly is in the to obfuscate assemblies.
        /// </summary>
        /// <param name="_TypeReference"></param>
        /// <returns></returns>
        public bool IsTypeInObfuscateAssemblies(TypeReference _TypeReference)
        {
            if (_TypeReference == null)
            {
                throw new ArgumentNullException("_TypeReference");
            }

            String var_ScopeName = _TypeReference.GetScopeName();

            // Has no scope! Somehow?
            if(String.IsNullOrEmpty(var_ScopeName))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, _TypeReference.ToString() + " has no scope!?");
                return false;
            }

            return this.IsAssemblyInObfuscateAssemblies(var_ScopeName);
        }

        /// <summary>
        /// Tries to resolve the a type in all assemblies and their dependencies.
        /// If this cannot be resolved, returns null!
        /// </summary>
        /// <param name="_TypeReference"></param>
        /// <returns></returns>
        public TypeDefinition Resolve_TypeDefinition(TypeReference _TypeReference)
        {
            if(_TypeReference == null)
            {
                throw new ArgumentNullException("_TypeReference");
            }

            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.MemberReferenceHelper.TryToResolve(_TypeReference, out TypeDefinition var_TypeDefinition))
            {
                return var_TypeDefinition;
            }
            else
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Cannot resolve " + _TypeReference.GetExtendedOriginalFullName(this.GetCache<TypeCache>()) + "!");

                return null;
            }
        }

        /// <summary>
        /// Add _TypeDefinition to the main module of _AssemblyInfo.
        /// Add _TypeDefinition to the TypeCache. 
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        public bool Add_TypeDefinition(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition)
        {
            if (_AssemblyInfo == null)
            {
                throw new ArgumentNullException("_AssemblyInfo");
            }
            if (_TypeDefinition == null)
            {
                throw new ArgumentNullException("_TypeDefinition");
            }

            // Add _TypeDefinition to the main module of _AssemblyInfo.
            return this.Add_TypeDefinition(_AssemblyInfo, _AssemblyInfo.AssemblyDefinition.MainModule, _TypeDefinition);
        }

        /// <summary>
        /// Add _TypeDefinition to the _ModuleDefinition of _AssemblyInfo.
        /// Add _TypeDefinition to the TypeCache. 
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_ModuleDefinition"></param>
        /// <param name="_TypeDefinition"></param>
        public bool Add_TypeDefinition(AssemblyInfo _AssemblyInfo, ModuleDefinition _ModuleDefinition, TypeDefinition _TypeDefinition)
        {
            if (_AssemblyInfo == null)
            {
                throw new ArgumentNullException("_AssemblyInfo");
            }
            if (_ModuleDefinition == null)
            {
                throw new ArgumentNullException("_ModuleDefinition");
            }
            if (_TypeDefinition == null)
            {
                throw new ArgumentNullException("_TypeDefinition");
            }

            // Check if there is already a Type with the same Namespace and Name.
            if(_AssemblyInfo.HasTypeDefinition(_TypeDefinition.Namespace, _TypeDefinition.Name))
            {
                return false;
            }

            // Notice: First add to assembly, else _TypeDefinition has no Scope set!
            // Add type to assembly.
            _AssemblyInfo.AddTypeDefinition(_ModuleDefinition, _TypeDefinition);

            // Add type to type cache.
            this.DataContainer.CacheDictionary[EMemberType.Type].Add_MemberDefinition(_TypeDefinition);

            return true;
        }


        #endregion

        // Load
        #region Load

        /// <summary>
        /// Custom Load and Initialize of the Project.
        /// </summary>
        /// <returns></returns>
        protected override bool OnLoad()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Load");

            // Load DataContainer
            if (!this.LoadDataContainer())
            {
                return false;
            }

            // Load Settings
            if (!this.LoadObfuscatorSettings())
            {
                return false;
            }

            // Load Components
            if (!this.LoadComponents())
            {
                return false;
            }

            // Load Pipeline
            if (!this.LoadPipeline())
            {
                return false;
            }

            // Load Assembly Load Infos
            if (!this.LoadAssemblyLoadInfos())
            {
                return false;
            }

            // Load Assembly Dependencies
            if(!this.LoadAssemblyDependencyDirectories())
            {
                return false;
            }

            // Load Assembly Resolver
            if (!this.LoadAssemblyResolver())
            {
                return false;
            }

            // Load Assemblies
            if(!this.LoadAssemblies())
            {
                return false;
            }

            // Load Cache
            if (!this.LoadCache())
            {
                return false;
            }

            // Load RenameManager
            if (!this.LoadRenameManager())
            {
                return false;
            }

            return true;
        }

        #endregion

        // Process
        #region Process

        /// <summary>
        /// Saves the assemblies if processing did not fail.
        /// </summary>
        /// <param name="_StepOutput"></param>
        protected override void PostProcess(PostAssemblyBuildOutput _StepOutput)
        {
            base.PostProcess(_StepOutput);

            if (_StepOutput.Failed)
            {
                UnityEngine.Debug.LogError("[OPS.OBF] Obfuscation failed!");
            }
            else
            {
                if (this.SaveAssemblies())
                {
                    UnityEngine.Debug.Log("[OPS.OBF] Finished obfuscation!");
                }
                else
                {
                    UnityEngine.Debug.LogError("[OPS.OBF] Obfuscation failed!");
                }
            }
        }

        #endregion

        // Unload
        #region Unload

        /// <summary>
        /// Custom Unload and Deinitialize of the Project.
        /// </summary>
        /// <returns></returns>
        protected override bool OnUnload()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Unload");

            // Unload RenameManager
            if (!this.UnloadRenameManager())
            {

            }

            // Unload Cache
            if (!this.UnloadCache())
            {

            }

            // Unload Assemblies
            if (!this.UnloadAssemblies())
            {

            }

            // Unload Assembly Resolver
            if (!this.UnloadAssemblyResolver())
            {

            }

            // Unload Pipeline
            if (!this.UnloadPipeline())
            {

            }

            // Unload Components
            if (!this.UnloadComponents())
            {

            }

            // Unload Settings
            if (!this.UnloadObfuscatorSettings())
            {

            }

            // Unload DataContainer
            if(!this.UnloadDataContainer())
            {

            }

            return true;
        }

        #endregion
    }
}
