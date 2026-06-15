using System;
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
    /// Abstract implementation of a step.
    /// </summary>
    public abstract class AStep<TStepInput, TStepOutput> : IStep
        where TStepInput : IStepInput
        where TStepOutput : IStepOutput
    {
        // Name
        #region Name

        /// <summary>
        /// Name of the step.
        /// </summary>
        public abstract String Name { get; }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// Shared Settings for this project.
        /// </summary>
        public ASettings Settings { get; protected set; }

        #endregion

        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline for this project.
        /// </summary>
        public IPipeline Pipeline { get; protected set; }

        #endregion

        // DataContainer
        #region DataContainer

        /// <summary>
        /// Is the DataContainer for this class and shared for the pipeline.
        /// </summary>
        public IDataContainer DataContainer { get; protected set; }

        #endregion

        // Load
        #region Load

        /// <summary>
        /// Load and Initialize the Project.
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            if(this.GotLoaded)
            {
                return true;
            }

            this.GotLoaded = this.OnLoad();

            return this.GotLoaded;
        }

        /// <summary>
        /// Custom Load and Initialize of the Project.
        /// </summary>
        /// <returns></returns>
        protected abstract bool OnLoad();

        /// <summary>
        /// Returns if the project got already loaded.
        /// </summary>
        public bool GotLoaded { get; private set; }

        #endregion

        // Process
        #region Process

        /// <summary>
        /// Reference.
        /// </summary>
        /// <returns></returns>
        IStepOutput IStep.Process(IStepInput _IStepInput)
        {
            return this.Process(_IStepInput);
        }

        /// <summary>
        /// Pre process after pipeline run.
        /// </summary>
        /// <param name="_IStepInput"></param>
        protected virtual void PreProcess(IStepInput _IStepInput)
        {

        }

        /// <summary>
        /// Process the pipeline and return an output.
        /// </summary>
        /// <returns></returns>
        public TStepOutput Process(IStepInput _IStepInput)
        {
            if (this.Pipeline == null)
            {
                throw new Exception("Pipeline is null!");
            }

            // Create output object
            TStepOutput var_StepOuput = Activator.CreateInstance<TStepOutput>();

            // Pre process
            this.PreProcess(_IStepInput);

            // Run pipeline
            bool var_PipelineProcesingSuccess = this.Pipeline.ProcessPipeline(_IStepInput, var_StepOuput);
            
            // Set if failed
            var_StepOuput.Failed = !var_PipelineProcesingSuccess;

            // Post process pipeline and add data to the output.
            this.PostProcess(var_StepOuput);

            return var_StepOuput;
        }

        /// <summary>
        /// Post process after pipeline run.
        /// </summary>
        /// <param name="_StepOutput"></param>
        protected virtual void PostProcess(TStepOutput _StepOutput)
        {

        }

        #endregion

        // Unload
        #region Unload

        /// <summary>
        /// Unload and Deinitialize the Project.
        /// </summary>
        /// <returns></returns>
        public bool Unload()
        {
            bool var_GotUnloaded = this.OnUnload();

            if(var_GotUnloaded)
            {
                this.GotLoaded = false;
            }

            return var_GotUnloaded;
        }

        /// <summary>
        /// Custom Unload and Deinitialize of the Project.
        /// </summary>
        /// <returns></returns>
        protected abstract bool OnUnload();

        #endregion
    }
}
