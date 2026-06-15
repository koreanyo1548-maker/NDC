using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility
{
    /// <summary>
    /// A component independent from the pipeline process. Allows custom compatibility to plugins / assets.
    /// All compatibility component are called in the belonging member obfuscation component.
    /// </summary>
    public interface IEventCompatibility : IObfuscationCompatibilityComponent
    {
        /// <summary>
        /// Return false, if the renaming of this event is not allowed!
        /// </summary>
        /// <param name="_Step"></param>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_EventDefinition"></param>
        /// <param name="_Cause"></param>
        /// <returns></returns>
        bool IsEventRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, EventDefinition _EventDefinition, out String _Cause);

        /// <summary>
        /// Modify the obfuscated event name if has to.
        /// </summary>
        /// <param name="_Step"></param>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_EventDefinition"></param>
        /// <param name="_OriginalName"></param>
        /// <param name="_CurrentName"></param>
        /// <returns></returns>
        String ApplyEventRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, EventDefinition _EventDefinition, String _OriginalName, String _CurrentName);
    }
}
