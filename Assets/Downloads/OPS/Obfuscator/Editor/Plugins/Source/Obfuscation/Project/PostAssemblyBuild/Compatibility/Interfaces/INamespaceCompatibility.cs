using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

//OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility
{
    /// <summary>
    /// A component independent from the pipeline process. Allows custom compatibility to plugins / assets.
    /// All compatibility component are called in the belonging member obfuscation component.
    /// </summary>
    public interface INamespaceCompatibility : IObfuscationCompatibilityComponent
    {
        /// <summary>
        /// Return true, to skip the whole namespace and all its members!
        /// </summary>
        /// <param name="_Step"></param>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_Cause"></param>
        /// <returns></returns>
        bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out String _Cause);

        /// <summary>
        /// Return false, if the renaming of this namespace is not allowed!
        /// </summary>
        /// <param name="_Step"></param>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_Cause"></param>
        /// <returns></returns>
        bool IsNamespaceRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out String _Cause);

        /// <summary>
        /// Modify the obfuscated namespace name if has to.
        /// </summary>
        /// <param name="_Step"></param>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_OriginalName"></param>
        /// <param name="_CurrentName"></param>
        /// <returns></returns>
        String ApplyNamespaceRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, String _OriginalName, String _CurrentName);
    }
}
