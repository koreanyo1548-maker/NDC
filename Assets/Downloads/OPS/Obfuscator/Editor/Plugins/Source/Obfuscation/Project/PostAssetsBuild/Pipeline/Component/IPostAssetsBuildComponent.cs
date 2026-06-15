using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Pipeline
using OPS.Editor.Project.Pipeline.Component;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline.Component
{
    /// <summary>
    /// Interface over the post assets build component hooks.
    /// </summary>
    public interface IPostAssetsBuildComponent : IPipelineComponent
    {
        /// <summary>
        /// Callen after the project assets getting build.
        /// </summary>
        /// <returns></returns>
        bool OnPostAssetsBuild();
    }
}
