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
    public class AProject : IProject
    {
        // Report
        #region Report

        /// <summary>
        /// Active Report for the project. Used for logging.
        /// </summary>
        public ProjectReport ActiveReport { get; set; }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// Stores if some of the runned steps failed.
        /// </summary>
        public bool SomeStepFailed { get; private set; }

        /// <summary>
        /// Output of the last runned step.
        /// </summary>
        private IStepOutput lastStepOutput;

        /// <summary>
        /// Loads, proceses and unlaods a step. Returns if it failed.
        /// </summary>
        /// <param name="_Step"></param>
        /// <returns></returns>
        public bool RunStep(IStep _Step)
        {
            // Check parameter.
            if(_Step == null)
            {
                throw new ArgumentNullException("_Step");
            }

            // Check if already some step failed!
            if(this.SomeStepFailed)
            {
                return false;
            }

            // Load Step
            bool var_FailedToLoad = false;
            if (!_Step.Load())
            {
                var_FailedToLoad = true;

                this.ActiveReport.Append(EReportLevel.Error, "Failed to load " + _Step.Name + "!", true);
            }

            // Process Step
            bool var_FailedToProcess = false;
            if (!var_FailedToLoad)
            {
                // Process
                this.lastStepOutput = _Step.Process(this.lastStepOutput);

                if (this.lastStepOutput.Failed)
                {
                    var_FailedToProcess = true;

                    this.ActiveReport.Append(EReportLevel.Error, "Failed to process " + _Step.Name + "!", true);
                }
            }

            // Unload Step
            if (!_Step.Unload())
            {

            }

            this.ActiveReport.Append(EReportLevel.Info, "Finished " + _Step.Name + "!");

            // Store if some step failed!
            this.SomeStepFailed = var_FailedToLoad || var_FailedToProcess;

            // Return if success.
            return !this.SomeStepFailed;
        }

        #endregion
    }
}
