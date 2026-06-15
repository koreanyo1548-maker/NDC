namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component
{
    /// <summary>
    /// Analyse or manipulate a Unity Component pre build.
    /// </summary>
    public interface IComponentProcessingComponent : IPreBuildComponent
    {
        /// <summary>
        /// Get if the component should process scenes.
        /// </summary>
        bool ProcessScenes { get; }

        /// <summary>
        /// Get if the component should process prefabs.
        /// </summary>
        bool ProcessPrefabs { get; }

        /// <summary>
        /// Analyse a Unity Component before the build starts.
        /// </summary>
        /// <returns></returns>
        bool OnAnalyse_Component(UnityEngine.Component _Component);

        /// <summary>
        /// Process a Unity Component before the build starts but directly after 'OnAnalyse_Component(...)'.
        /// </summary>
        /// <returns></returns>
        bool OnProcess_Component(UnityEngine.Component _Component);
    }
}
