using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace OPS.Editor.Settings.Unity.Build
{
    /// <summary>
    /// Abstract unity build settings.
    /// </summary>
    public abstract class ABuildSettings
    {
        // Unity BuildTarget
        #region Unity BuildTarget

        /// <summary>
        /// Target building to.
        /// </summary>
        public BuildTarget BuildTarget { get; set; } = BuildTarget.NoTarget;

        /// <summary>
        /// Target group building to.
        /// </summary>
        public BuildTargetGroup BuildTargetGroup { get; set; } = BuildTargetGroup.Unknown;

        #endregion

        // Unity BuildReport
        #region Unity BuildReport

        /// <summary>
        /// Active unity build report;
        /// </summary>
        public BuildReport UnityBuildReport = null;

        #endregion

        // IL2CPP
        #region IL2CPP

        /// <summary>
        /// Building to il2cpp.
        /// </summary>
        public bool IsIL2CPPBuild { get; set; } = false;

        #endregion

        // Mono
        #region Mono

        /// <summary>
        /// Building to mono.
        /// </summary>
        public bool IsMonoBuild { get => !IsIL2CPPBuild; set => IsIL2CPPBuild = !value; }

        #endregion

        // Compressed
        #region Compressed

        /// <summary>
        /// The compression type.
        /// </summary>
        public CompressionType Compression { get; set; } = CompressionType.None;

        #endregion

        // Project
        #region Project

        /// <summary>
        /// Build into a project. True if target is MacOS and build to XCode or Windows/Linux and build to Visual Studio solution.
        /// </summary>
        public bool BuildIntoProject { get; set; } = false;

        #endregion
    }
}
