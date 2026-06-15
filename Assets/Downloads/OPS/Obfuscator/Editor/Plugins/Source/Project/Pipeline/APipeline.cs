using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.Component;
using OPS.Editor.Project.DataContainer;
using OPS.Editor.Project.Pipeline.Component;
using OPS.Editor.Project.Step;

// OPS - Settings
using OPS.Editor.Settings.File;

namespace OPS.Editor.Project.Pipeline
{
    /// <summary>
    /// Abstract implementation of a pipeline.
    /// </summary>
    public abstract class APipeline : IPipeline
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// A step needs the step belonging too.
        /// </summary>
        /// <param name="_Step"></param>
        public APipeline(IStep _Step)
        {
            // Assign step.
            this.Step = _Step;
        }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// Step this Pipeline belongs too.
        /// </summary>
        public IStep Step { get; private set; }

        #endregion

        // Components
        #region Components

        /// <summary>
        /// All Pipeline Components.
        /// </summary>
        public List<IPipelineComponent> ComponentList { get; private set; } = new List<IPipelineComponent>();

        /// <summary>
        /// Returns a List of all PipelineComponents inherit or are TPipelineComponent.
        /// </summary>
        /// <typeparam name="TPipelineComponent"></typeparam>
        /// <returns></returns>
        public List<TPipelineComponent> GetPipelineComponents<TPipelineComponent>()
            where TPipelineComponent : IPipelineComponent
        {
            List<TPipelineComponent> var_PipelineComponentList = new List<TPipelineComponent>();

            for (int i = 0; i < this.ComponentList.Count; i++)
            {
                if (this.ComponentList[i] is TPipelineComponent)
                {
                    var_PipelineComponentList.Add((TPipelineComponent)this.ComponentList[i]);
                }
            }

            return var_PipelineComponentList;
        }

        /// <summary>
        /// Add an _PipelineComponent.
        /// </summary>
        /// <param name="_PipelineComponent"></param>
        /// <returns></returns>
        public bool AddPipelineComponent(IPipelineComponent _PipelineComponent)
        {
            if(this.ComponentList == null)
            {
                return false;
            }

            // Set Pipeline for component.
            _PipelineComponent.AssignToPipeline(this);

            // Add to component list in pipeline.
            this.ComponentList.Add(_PipelineComponent);

            return true;
        }

        #endregion

        // Process
        #region Process

        /// <summary>
        /// Process the pipeline.
        /// Returns true, if the processing succeeded.
        /// </summary>
        /// <returns></returns>
        public bool ProcessPipeline(IStepInput _StepInput, IStepOutput _StepOutput)
        {
            // Get all IPreObfuscationComponent in this Pipeline.
            List<IPipelineComponent> var_PipelineComponentList = this.GetPipelineComponents<IPipelineComponent>();

            // ###### 1. Pre pipeline process ######

            // Process all Assembly Components.
            for (int c = 0; c < var_PipelineComponentList.Count; c++)
            {
                // Deactivated components will be skipped.
                if (!var_PipelineComponentList[c].IsActive)
                {
                    continue;
                }

                // Pre pipeline process. Data will be read here.
                if (!var_PipelineComponentList[c].OnPrePipelineProcess(_StepInput))
                {
                    return false;
                }
            }

            // ###### 2. Custom pipeline process ######

            // If the custom process failed, directly return false!
            if (!this.OnProcessPipeline())
            {
                return false;
            }

            // ###### 3. Post pipeline process ######

            // Process all Assembly Components.
            for (int c = 0; c < var_PipelineComponentList.Count; c++)
            {
                // Deactivated components will be skipped.
                if (!var_PipelineComponentList[c].IsActive)
                {
                    continue;
                }

                // Post pipeline process. Data will be written here.
                if (!var_PipelineComponentList[c].OnPostPipelineProcess(_StepOutput))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Custom pipeline process.
        /// Return true, if the processing succeeded.
        /// Also data can be passed to _StepOutput.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnProcessPipeline();

        #endregion
    }
}
