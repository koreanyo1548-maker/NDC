using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Pipeline
using OPS.Editor.Project.Pipeline.Component;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostBuild.Pipeline.Component
{
    /// <summary>
    /// Interface over the pre build component hooks.
    /// </summary>
    public interface IPostBuildComponent : IPipelineComponent
    {
        /// <summary>
        /// Callen after the project getting build.
        /// </summary>
        /// <returns></returns>
        bool OnPostBuild();
    }
}
