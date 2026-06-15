using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member
{
    public interface IMemberObfuscationComponent : IAssemblyProcessingComponent
    {
        /// <summary>
        /// Find member names here.
        /// Called after Analyse and before Obfuscate.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        bool OnFindMemberNames_Assemblies(AssemblyInfo _AssemblyInfo);
    }
}
