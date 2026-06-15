using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project.DataContainer;
using OPS.Editor.Project.Pipeline;

// OPS - Settings
using OPS.Editor.Settings.File;

namespace OPS.Editor.Project.Step
{
    /// <summary>
    /// A step belongs to a project and processes a pipeline.
    /// </summary>
    public interface IStep
    {
        // Name
        #region Name

        /// <summary>
        /// Name of the step.
        /// </summary>
        String Name { get; }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// Shared Settings for this project.
        /// </summary>
        ASettings Settings { get; }

        #endregion

        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline for this project.
        /// </summary>
        IPipeline Pipeline { get; }

        #endregion

        // DataContainer
        #region DataContainer

        /// <summary>
        /// Is the DataContainer for this class and shared for the pipeline.
        /// </summary>
        IDataContainer DataContainer { get; }

        #endregion

        // Load
        #region Load

        /// <summary>
        /// Load the project up.
        /// </summary>
        /// <returns></returns>
        bool Load();

        /// <summary>
        /// Project got loaded.
        /// </summary>
        bool GotLoaded { get; }

        #endregion

        // Process
        #region Process

        /// <summary>
        /// Process the step and return an output.
        /// </summary>
        /// <returns></returns>
        IStepOutput Process(IStepInput _IStepInput);

        #endregion

        // Unload
        #region Unload

        /// <summary>
        /// Unloads the project.
        /// </summary>
        /// <returns></returns>
        bool Unload();

        #endregion
    }
}
