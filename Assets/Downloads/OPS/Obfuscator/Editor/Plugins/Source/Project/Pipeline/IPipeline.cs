using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project - Step
using OPS.Editor.Project.Step;

// OPS - Project - Pipeline
using OPS.Editor.Project.Pipeline.Component;

namespace OPS.Editor.Project.Pipeline
{
    /// <summary>
    /// A pipeline does a processing for a input and returns an output.
    /// </summary>
    public interface IPipeline
    {
        // Step
        #region Step

        /// <summary>
        /// Step this Pipeline belongs too.
        /// </summary>
        IStep Step { get; }

        #endregion

        // Components
        #region Components

        /// <summary>
        /// Components in this Pipeline.
        /// </summary>
        List<IPipelineComponent> ComponentList { get; }

        #endregion

        // Process
        #region Process

        /// <summary>
        /// Process the pipeline.
        /// Also you can add data to the _StepOutput being passed to the next step.
        /// </summary>
        /// <returns></returns>
        bool ProcessPipeline(IStepInput _StepInput, IStepOutput _StepOutput);

        #endregion
    }
}
