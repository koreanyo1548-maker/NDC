using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono
using OPS.Mono;
using OPS.Mono.Cecil;

// OPS - Pipeline
using OPS.Editor.Project.Pipeline.Component;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component
{
    /// <summary>
    /// Interface over a post assembly build component.
    /// </summary>
    public interface IPostAssemblyBuildComponent : IPipelineComponent
    {
        /// <summary>
        /// Return true if the _AssemblyLoadInfo can be processed by this component.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        bool CanBeProcessedByComponent(AssemblyLoadInfo _AssemblyLoadInfo);

        /// <summary>
        /// Prepare / preprocess the assembly before it will be loaded.
        /// Is a special hook called independently from the pipeline process itself.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        bool Prepare_Assemblies(AssemblyLoadInfo _AssemblyLoadInfo);

        /// <summary>
        /// Postprocess the assembly after it was obfuscated and saved.
        /// Is a special hook called independently from the pipeline process itself.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        bool PostProcess_Assemblies(AssemblyLoadInfo _AssemblyLoadInfo);
    }
}
