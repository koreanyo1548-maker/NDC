using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.Project.Step
{
    /// <summary>
    /// Interface for a step output.
    /// Each step output is also a input for the next step.
    /// </summary>
    public interface IStepOutput : IStepInput
    {
        // Failed
        #region Failed

        /// <summary>
        /// Returns if the step processing failed!
        /// </summary>
        bool Failed { get; set; }

        #endregion
    }
}
