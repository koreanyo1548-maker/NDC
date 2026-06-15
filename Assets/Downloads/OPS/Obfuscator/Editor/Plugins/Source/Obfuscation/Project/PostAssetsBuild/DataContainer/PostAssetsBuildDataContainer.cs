using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.DataContainer;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Project - PostAssetsBuild
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline.Component;

// OPS - Obfuscator - Assets - Unity
using OPS.Obfuscator.Editor.Assets.Unity.Cache;

namespace OPS.Obfuscator.Editor.Project.PostAssetsBuild.DataContainer
{
    /// <summary>
    /// Contains all the data for the post build needed.
    /// </summary>
    public class PostAssetsBuildDataContainer : ADataContainer
    {
        // Pipeline Component
        #region Pipeline Component

        /// <summary>
        /// All pipeline pre build components components.
        /// </summary>
        public List<IPostAssetsBuildComponent> ComponentList { get; set; } = new List<IPostAssetsBuildComponent>();

        #endregion
    }
}
