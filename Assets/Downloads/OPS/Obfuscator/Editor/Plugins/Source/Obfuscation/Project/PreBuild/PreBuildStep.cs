using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEditor.SceneManagement;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.DataContainer;
using OPS.Editor.Project.Pipeline;
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Settings
using OPS.Obfuscator.Editor.Settings;
using OPS.Obfuscator.Editor.Settings.Unity.Build;
using OPS.Obfuscator.Editor.Settings.Unity.Editor;

// OPS - Obfuscator - Project - PreBuild
using OPS.Obfuscator.Editor.Project.PreBuild.DataContainer;
using OPS.Obfuscator.Editor.Project.PreBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component;

namespace OPS.Obfuscator.Editor.Project.PreBuild
{
    public class PreBuildStep : AStep<PreBuildInput, PreBuildOutput>
    {
        // Constructor
        #region Constructor

        public PreBuildStep(EditorSettings _EditorSettings, BuildSettings _BuildSettings)
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
                return "PreBuild";
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
        public new PreBuildDataContainer DataContainer
        {
            get
            {
                return (PreBuildDataContainer) base.DataContainer;
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
            this.DataContainer = new PreBuildDataContainer();

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
                return (ObfuscatorSettings)base.Settings;
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
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Obfuscator Settings");

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

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Obfuscator Settings");

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

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Pipeline Components");

            // Load Components
            this.DataContainer.ComponentList = OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component.Helper.ComponentHelper.GetComponents();

            // Log Components
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found Components", this.DataContainer.ComponentList.Select(c => c.Name + " (" + c.ShortDescription + ")").ToList());

            return true;
        }
        
        /// <summary>
        /// Unload all components.
        /// </summary>
        /// <returns></returns>
        private bool UnloadComponents()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Pipeline Components");

            this.DataContainer.ComponentList = null;

            return true;
        }

        #endregion

        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline for the project.
        /// </summary>
        public new PreBuildPipeline Pipeline
        {
            get
            {
                return (PreBuildPipeline) base.Pipeline;
            }
            private set
            {
                base.Pipeline = value;
            }
        }

        /// <summary>
        /// Load the PreBuild Pipeline.
        /// </summary>
        /// <returns></returns>
        private bool LoadPipeline()
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Pipeline");

            // Create
            this.Pipeline = new PreBuildPipeline(this);

            // Attach Components to Pipeline
            for (int i = 0; i < this.DataContainer.ComponentList.Count; i++)
            {
                this.Pipeline.AddPipelineComponent(this.DataContainer.ComponentList[i]);
            }

            return true;
        }

        /// <summary>
        /// Unload the PreBuild Pipeline.
        /// </summary>
        /// <returns></returns>
        private bool UnloadPipeline()
        {
            if(this.Pipeline == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Pipeline");

            // Clear
            this.Pipeline = null;

            return true;
        }

        #endregion

        // Asset Cache
        #region Asset Cache

        /// <summary>
        /// Get the extensions should be processed by the IAssetProcessingComponent.
        /// </summary>
        /// <returns></returns>
        private List<String> GetComponentAssetProcessingExtensionList()
        {
            List<String> var_Result = new List<String>();

            // Iterate all components in the datacontainer and get the extensions to process.
            for (int i = 0; i < this.DataContainer.ComponentList.Count; i++)
            {
                if (this.DataContainer.ComponentList[i] is IAssetProcessingComponent)
                {
                    IAssetProcessingComponent var_AssetProcessingComponent = this.DataContainer.ComponentList[i] as IAssetProcessingComponent;

                    try
                    {
                        var_Result = var_Result.Union(var_AssetProcessingComponent.AssetExtensionsToProcess).ToList();
                    }
                    catch (Exception e)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Error for component: " + this.DataContainer.ComponentList[i].Name + " while receiving a list of Asset extensions to process. " + e.ToString());

                        continue;
                    }
                }
            }

            // Add prefab extension if not already added. For the Component Processing.
            if (!var_Result.Contains(".prefab"))
            {
                var_Result.Add(".prefab");
            }

            return var_Result;
        }

        private bool LoadAssetCache()
        {
            if (this.DataContainer == null)
            {
                return false;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Load Asset Cache");

            // Get all the extensions.
            List<String> var_ExtensionList = this.GetComponentAssetProcessingExtensionList();

            // Load Extensions
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found Extensions", var_ExtensionList);

            // Load Unity Asset Cache.
            this.DataContainer.UnityAssetCache = new Assets.Unity.Cache.UnityAssetCache(var_ExtensionList.ToArray());
            this.DataContainer.UnityAssetCache.Load();

            return true;
        }

        private bool UnloadAssetCache()
        {
            if (this.DataContainer == null)
            {
                return true;
            }

            if (this.DataContainer.UnityAssetCache == null)
            {
                return true;
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Unload Asset Cache");

            // Clear
            this.DataContainer.UnityAssetCache.Unload();
            this.DataContainer.UnityAssetCache = null;

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
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Load Project");

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
            if(!this.LoadComponents())
            {
                return false;
            }

            // Load Pipeline
            if (!this.LoadPipeline())
            {
                return false;
            }

            // Load Asset Cache
            if (!this.LoadAssetCache())
            {
                return false;
            }

            return true;
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
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Unload Project");

            // Unload Asset Cache
            if (!this.UnloadAssetCache())
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
            if(!this.UnloadObfuscatorSettings())
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
