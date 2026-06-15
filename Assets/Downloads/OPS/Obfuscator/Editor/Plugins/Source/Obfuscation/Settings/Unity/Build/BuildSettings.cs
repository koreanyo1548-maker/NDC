// System
using System;
using System.Collections.Generic;

// OPS - Settings
using OPS.Editor.Settings.Unity.Build;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Build
using OPS.Obfuscator.Editor.Platform.Build;

namespace OPS.Obfuscator.Editor.Settings.Unity.Build
{
    /// <summary>
    /// Obfuscator unity build settings.
    /// </summary>
    public class BuildSettings : ABuildSettings
    {
        // Development
        #region Development

        /// <summary>
        /// The build is a development build.
        /// </summary>
        public bool IsDevelopmentBuild { get; set; } = false;

        #endregion

        // Platform
        #region Platform

        /// <summary>
        /// Returns if is a standalone build target (Windows, Linux, OSX).
        /// </summary>
        public bool IsStandaloneBuildTarget
        {
            get
            {
                return this.BuildTarget == UnityEditor.BuildTarget.StandaloneWindows
                        || this.BuildTarget == UnityEditor.BuildTarget.StandaloneWindows64
                        || this.BuildTarget == UnityEditor.BuildTarget.StandaloneLinux64
                        || this.BuildTarget == UnityEditor.BuildTarget.StandaloneOSX;
            }
        }

        /// <summary>
        /// Returns if is a mobile build target (Android, iOS).
        /// </summary>
        public bool IsMobileBuildTarget
        {
            get
            {
                return this.BuildTarget == UnityEditor.BuildTarget.Android
                        || this.BuildTarget == UnityEditor.BuildTarget.iOS;
            }
        }

        /// <summary>
        /// Returns if is an iOS build target.
        /// </summary>
        public bool IsIOSBuildTarget
        {
            get
            {
                return this.BuildTarget == UnityEditor.BuildTarget.iOS;
            }
        }

        /// <summary>
        /// The platform built to.
        /// </summary>
        internal DefaultBuildPlatform BuildPlatform { get; set; } = new DefaultBuildPlatform();

        #endregion

        // Assembly
        #region Assembly

        /// <summary>
        /// Additional dependencies. Enter the full path of the directory of the dependency.
        /// </summary>
        public List<String> AssemblyDependencyDirectoryPathList { get; set; } = new List<String>();

        /// <summary>
        /// Assemblies used in the obfuscation process.
        /// Either you can add all the assemblies you want to obfuscate here,
        /// or the obfuscator adds them based on the obfuscation settings.
        /// </summary>
        public List<AssemblyLoadInfo> AssemblyLoadInfoList { get; set; } = new List<AssemblyLoadInfo>();

        #endregion
    }
}
