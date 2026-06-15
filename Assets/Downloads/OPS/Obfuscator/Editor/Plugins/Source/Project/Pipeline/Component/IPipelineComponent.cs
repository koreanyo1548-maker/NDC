using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project.Component;
using OPS.Editor.Project.Step;

namespace OPS.Editor.Project.Pipeline.Component
{
    /// <summary>
    /// Interface describing a component in a pipeline process.
    /// </summary>
    public interface IPipelineComponent : IComponent
    {
        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline this component belongs too.
        /// </summary>
        IPipeline Pipeline { get; }

        /// <summary>
        /// Assigns the Pipeline Component to _Pipeline.
        /// </summary>
        /// <param name="_Pipeline"></param>
        void AssignToPipeline(IPipeline _Pipeline);

        #endregion

        // Component
        #region Component

        /// <summary>
        /// Return whether the component is activated or deactivated for the pipeline processing.
        /// </summary>
        bool IsActive { get; }

        #endregion

        // Hooks
        #region Hooks

        /// <summary>
        /// Is the first hook / event in the pipeline process. Getting called before each other hook / event was called.
        /// Read here data from the step input from the previous next step. The step was already loaded at this point.
        /// </summary>
        /// <param name="_StepInput"></param>
        bool OnPrePipelineProcess(IStepInput _StepInput);

        /// <summary>
        /// Is the last hook / event in the pipeline process. Getting called after each other hook / event was called.
        /// Add here data to the step output if you wish to pass it to the next step.
        /// </summary>
        /// <param name="_StepOutput"></param>
        bool OnPostPipelineProcess(IStepOutput _StepOutput);

        #endregion
    }
}
