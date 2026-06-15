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
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.DataContainer;

namespace OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline.Component
{
    /// <summary>
    /// Component of the post assets build.
    /// </summary>
    public abstract class APostAssetsBuildComponent : APipelineComponent, IPostAssetsBuildComponent
    {
        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline this Component belongs too.
        /// </summary>
        public new PostAssetsBuildPipeline Pipeline
        {
            get
            {
                return (PostAssetsBuildPipeline)base.Pipeline;
            }
        }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// Returns the Step this Pipeline belongs to.
        /// </summary>
        public PostAssetsBuildStep Step
        {
            get
            {
                return this.Pipeline.Step;
            }
        }

        #endregion

        // On Post Assets Build
        #region On Post Assets Build

        /// <summary>
        /// Callen after the project getting build.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnPostAssetsBuild();

        #endregion
    }
}
