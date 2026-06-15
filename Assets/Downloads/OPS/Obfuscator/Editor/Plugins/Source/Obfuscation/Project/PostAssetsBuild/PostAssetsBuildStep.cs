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

// OPS - Obfuscator - Project - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild;

// OPS - Obfuscator - Project - PostAssetsBuild
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.DataContainer;
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline.Component;

namespace OPS.Obfuscator.Editor.Project.PostAssetsBuild
{
    public class PostAssetsBuildStep : AStep<PostAssemblyBuildOutput, PostAssetsBuildOutput>
    {
        // Constructor
        #region Constructor

        public PostAssetsBuildStep(EditorSettings _EditorSettings, BuildSettings _BuildSettings)
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
                return "PostAssetsBuild";
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
        public new PostAssetsBuildDataContainer DataContainer
        {
            get
            {
                return (PostAssetsBuildDataContainer) base.DataContainer;
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
            this.DataContainer = new PostAssetsBuildDataContainer();

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
            this.DataContainer.ComponentList = OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline.Component.Helper.ComponentHelper.GetComponents();

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
        public new PostAssetsBuildPipeline Pipeline
        {
            get
            {
                return (PostAssetsBuildPipeline) base.Pipeline;
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
            this.Pipeline = new PostAssetsBuildPipeline(this);

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
