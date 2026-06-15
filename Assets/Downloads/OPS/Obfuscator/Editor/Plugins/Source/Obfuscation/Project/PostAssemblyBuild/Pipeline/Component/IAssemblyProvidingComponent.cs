using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Pipeline
using OPS.Editor.Project.Pipeline.Component;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component
{
    /// <summary>
    /// Returns assemblies and assembly dependencies to process.
    /// </summary>
    interface IAssemblyProvidingComponent : IPostAssemblyBuildComponent
    {
        /// <summary>
        /// Return a list of all assemblies should be used for obfuscation.
        /// Either ones that should be obfuscated or only loaded as helper.
        /// </summary>
        /// <returns></returns>
        List<AssemblyLoadInfo> GetAssemblyLoadInfoList();

        /// <summary>
        /// Return a list of all directories containing assemblies referenced in the to obfuscate assemblies.
        /// </summary>
        /// <returns></returns>
        List<String> GetAssemblyReferenceDirectoryList();
    }
}
