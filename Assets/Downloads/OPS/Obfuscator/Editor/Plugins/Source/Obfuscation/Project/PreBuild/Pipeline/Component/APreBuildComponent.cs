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

// OPS - Obfuscator - PreBuild
using OPS.Obfuscator.Editor.Project.PreBuild.DataContainer;

namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component
{
    /// <summary>
    /// Component of the pre build.
    /// </summary>
    public abstract class APreBuildComponent : APipelineComponent, IPreBuildComponent
    {
        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline this Component belongs too.
        /// </summary>
        public new PreBuildPipeline Pipeline
        {
            get
            {
                return (PreBuildPipeline)base.Pipeline;
            }
        }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// Returns the Step this Pipeline belongs to.
        /// </summary>
        public PreBuildStep Step
        {
            get
            {
                return this.Pipeline.Step;
            }
        }

        #endregion

        // OnPreBuild
        #region OnPreBuild

        /// <summary>
        /// Callen before the project getting build.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnPreBuild();

        #endregion
    }
}
