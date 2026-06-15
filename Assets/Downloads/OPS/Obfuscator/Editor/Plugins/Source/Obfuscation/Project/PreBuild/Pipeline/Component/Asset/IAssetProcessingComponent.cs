// System
using System;

// OPS - Obfuscator - Assets
using OPS.Obfuscator.Editor.Assets.Unity;

namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component
{
    /// <summary>
    /// A interface for a component, which processes assets.
    /// </summary>
    public interface IAssetProcessingComponent : IPreBuildComponent
    {
        /// <summary>
        /// Return the extensions of assets, should be processed by this component.
        /// </summary>
        String[] AssetExtensionsToProcess { get; }

        /// <summary>
        /// Analyse Assets pre build.
        /// </summary>
        /// <returns></returns>
        bool OnAnalyse_Assets(UnityAssetReference _UnityAssetReference);

        /// <summary>
        /// Process Assets pre build but after 'OnAnalyse_Assets(...)'.
        /// </summary>
        /// <returns></returns>
        bool OnProcess_Assets(UnityAssetReference _UnityAssetReference);
    }
}
