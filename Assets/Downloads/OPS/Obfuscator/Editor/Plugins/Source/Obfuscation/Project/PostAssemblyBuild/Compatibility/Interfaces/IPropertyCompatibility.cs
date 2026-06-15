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
    public interface IPropertyCompatibility : IObfuscationCompatibilityComponent
    {
        /// <summary>
        /// Return false, if the renaming of this property is not allowed!
        /// </summary>
        /// <param name="_Step"></param>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_PropertyDefinition"></param>
        /// <param name="_Cause"></param>
        /// <returns></returns>
        bool IsPropertyRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, out String _Cause);

        /// <summary>
        /// Modify the obfuscated property name if has to.
        /// </summary>
        /// <param name="_Step"></param>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_PropertyDefinition"></param>
        /// <param name="_OriginalName"></param>
        /// <param name="_CurrentName"></param>
        /// <returns></returns>
        String ApplyPropertyRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, String _OriginalName, String _CurrentName);
    }
}
