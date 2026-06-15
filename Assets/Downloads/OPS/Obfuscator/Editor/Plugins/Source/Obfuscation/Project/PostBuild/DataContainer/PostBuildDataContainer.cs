using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.DataContainer;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Project - PostBuild
using OPS.Obfuscator.Editor.Project.PostBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PostBuild.Pipeline.Component;

// OPS - Obfuscator - Assets - Unity
using OPS.Obfuscator.Editor.Assets.Unity.Cache;

namespace OPS.Obfuscator.Editor.Project.PostBuild.DataContainer
{
    /// <summary>
    /// Contains all the data for the post build needed.
    /// </summary>
    public class PostBuildDataContainer : ADataContainer
    {
        // Pipeline Component
        #region Pipeline Component

        /// <summary>
        /// All pipeline pre build components components.
        /// </summary>
        public List<IPostBuildComponent> ComponentList { get; set; } = new List<IPostBuildComponent>();

        #endregion
    }
}
