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
    /// Interface over a custom post assembly build plugin component.
    /// </summary>
    public interface IPostAssemblyBuildPluginComponent : IAssemblyProcessingComponent
    {
    }
}
