using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPS.Obfuscator.Editor.Gui;

// OPS - Mono
using OPS.Mono.Cecil;

// OPS - Pipeline
using OPS.Editor.Project.Pipeline;
using OPS.Editor.Project.Pipeline.Component;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Project;

// OPS - Obfuscator - PostBuild
using OPS.Obfuscator.Editor.Project.PostBuild.DataContainer;

namespace OPS.Obfuscator.Editor.Project.PostBuild.Pipeline.Component
{
    /// <summary>
    /// Component of the pre build.
    /// </summary>
    public abstract class APostBuildComponent : APipelineComponent, IPostBuildComponent
    {
        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline this Component belongs too.
        /// </summary>
        public new PostBuildPipeline Pipeline
        {
            get
            {
                return (PostBuildPipeline)base.Pipeline;
            }
        }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// Returns the Step this Pipeline belongs to.
        /// </summary>
        public PostBuildStep Step
        {
            get
            {
                return this.Pipeline.Step;
            }
        }

        #endregion

        // On Post Build
        #region On Post Build

        /// <summary>
        /// Callen after the project getting build.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnPostBuild();

        #endregion
    }
}
