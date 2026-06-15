using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.DataContainer;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Project - PreBuild
using OPS.Obfuscator.Editor.Project.PreBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component;

// OPS - Obfuscator - Assets - Unity
using OPS.Obfuscator.Editor.Assets.Unity.Cache;

namespace OPS.Obfuscator.Editor.Project.PreBuild.DataContainer
{
    /// <summary>
    /// Contains all the data for the pre build needed.
    /// </summary>
    public class PreBuildDataContainer : ADataContainer
    {
        // Pipeline Component
        #region Pipeline Component

        /// <summary>
        /// All pipeline pre build components components.
        /// </summary>
        public List<IPreBuildComponent> ComponentList { get; set; } = new List<IPreBuildComponent>();

        #endregion

        // Asset Cache
        #region Asset Cache

        /// <summary>
        /// Cache of all Unity Assets needed by IAssetProcessingComponent.
        /// </summary>
        public UnityAssetCache UnityAssetCache { get; set; } = new UnityAssetCache(new String[0]);

        #endregion
    }
}
