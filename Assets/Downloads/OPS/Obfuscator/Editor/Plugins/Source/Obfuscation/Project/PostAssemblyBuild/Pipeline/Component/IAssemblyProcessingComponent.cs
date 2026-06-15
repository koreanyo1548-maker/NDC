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
    /// Used to process assemblies.
    /// </summary>
    public interface IAssemblyProcessingComponent : IPostAssemblyBuildComponent
    {
        /// <summary>
        /// Analyse an _AssemblyInfo the first time.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo);

        /// <summary>
        /// Analyse an _AssemblyInfo the second time, after all other components have already analysed it the first time.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo);

        /// <summary>
        /// Apply obfuscation to the _AssemblyInfo here.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo);

        /// <summary>
        /// Callen after the assemblies got obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo);
    }
}
