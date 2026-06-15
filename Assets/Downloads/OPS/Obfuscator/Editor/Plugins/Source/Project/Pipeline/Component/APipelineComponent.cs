using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project.Step;

namespace OPS.Editor.Project.Pipeline.Component
{
    /// <summary>
    /// Abstract base class for a component in a pipeline process.
    /// </summary>
    public abstract class APipelineComponent : IPipelineComponent
    {
        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline this Component belongs too.
        /// </summary>
        public IPipeline Pipeline { get; private set; }

        /// <summary>
        /// Assigns the Pipeline Component to _Pipeline.
        /// </summary>
        /// <param name="_Pipeline"></param>
        public void AssignToPipeline(IPipeline _Pipeline)
        {
            this.Pipeline = _Pipeline;
        }

        #endregion

        // Name
        #region Name

        /// <summary>
        /// Components name.
        /// </summary>
        public abstract String Name { get; }

        #endregion
        
        // Description
        #region Description

        /// <summary>
        /// Components description.
        /// </summary>
        public abstract String Description { get; }

        #endregion

        // Short Description
        #region Short Description

        /// <summary>
        /// Components short description.
        /// </summary>
        public abstract String ShortDescription { get; }

        #endregion

        // Component
        #region Component

        /// <summary>
        /// Return whether the component is activated or deactivated for the pipeline processing.
        /// </summary>
        public virtual bool IsActive
        {
            get
            {
                return true;
            }
        }

        #endregion

        // Hooks
        #region Hooks

        /// <summary>
        /// Is the first hook / event in the pipeline process. Getting called before each other hook / event was called.
        /// Read here data from the step input from the previous next step. The step was already loaded at this point.
        /// </summary>
        /// <param name="_StepInput"></param>
        public virtual bool OnPrePipelineProcess(IStepInput _StepInput)
        {
            return true;
        }

        /// <summary>
        /// Is the last hook / event in the pipeline process. Getting called after each other hook / event was called.
        /// Add here data to the step output if you wish to pass it to the next step.
        /// </summary>
        /// <param name="_StepOutput"></param>
        public virtual bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            return true;
        }

        #endregion

        // ToString
        #region ToString

        /// <summary>
        /// Returns the name of the pipeline component.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}
