using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//OPS Mono
using OPS.Mono.Cecil;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Project;

// OPS - Pipeline
using OPS.Editor.Project.Pipeline;
using OPS.Editor.Project.Pipeline.Component;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component
{
    /// <summary>
    /// Component of the obfuscator.
    /// </summary>
    public abstract class APostAssemblyBuildComponent : APipelineComponent, IPostAssemblyBuildComponent
    {
        // Pipeline
        #region Pipeline

        /// <summary>
        /// Pipeline this Component belongs too.
        /// </summary>
        public new PostAssemblyBuildPipeline Pipeline
        {
            get
            {
                return (PostAssemblyBuildPipeline)base.Pipeline;
            }
        }

        #endregion

        // DataContainer
        #region DataContainer

        /// <summary>
        /// Returns the DataContainer of the belonging Pipeline.
        /// </summary>
        public PostAssemblyBuildDataContainer DataContainer
        {
            get
            {
                return this.Pipeline.DataContainer;
            }
        }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// Returns the Step this Pipeline belongs to.
        /// </summary>
        public PostAssemblyBuildStep Step
        {
            get
            {
                return this.Pipeline.Step;
            }
        }

        #endregion

        // Check Assembly
        #region Check Assembly

        /// <summary>
        /// True: The _AssemblyLoadInfo should be processed by this component.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        public virtual bool CanBeProcessedByComponent(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            return _AssemblyLoadInfo.Obfuscate;
        }

        #endregion

        // Prepare Assembly
        #region Prepare Assembly

        /// <summary>
        /// Prepare / preprocess the assembly before it will be loaded.
        /// Is a special hook called independently from the pipeline process itself.
        /// Called before the assemblies itself are loaded.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        public virtual bool Prepare_Assemblies(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            return true;
        }

        #endregion

        // Post Process Assembly
        #region Post Process Assembly

        /// <summary>
        /// Postprocess the assembly after it was obfuscated and saved.
        /// Is a special hook called independently from the pipeline process itself.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        public virtual bool PostProcess_Assemblies(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            return true;
        }

        #endregion
    }
}
