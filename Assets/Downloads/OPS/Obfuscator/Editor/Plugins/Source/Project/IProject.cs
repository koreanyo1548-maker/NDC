using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Report
using OPS.Editor.Report;

// OPS - Project - Step
using OPS.Editor.Project.Step;

namespace OPS.Editor.Project
{
    /// <summary>
    /// A project executes steps and passed their output as input to the next one.
    /// </summary>
    public interface IProject
    {
        // Report
        #region Report

        /// <summary>
        /// Active Report for the project. Used for logging.
        /// </summary>
        ProjectReport ActiveReport { get; }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// Stores if some of the runned steps failed.
        /// </summary>
        bool SomeStepFailed { get; }

        /// <summary>
        /// Loads, proceses and unlaods a step. Returns if it failed.
        /// </summary>
        /// <param name="_Step"></param>
        /// <returns></returns>
        bool RunStep(IStep _Step);

        #endregion
    }
}
