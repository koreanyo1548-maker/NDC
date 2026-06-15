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
    /// Interface over a custom post assets build plugin component.
    /// </summary>
    public interface IPostAssetsBuildPluginComponent : IPostAssetsBuildComponent
    {
    }
}
