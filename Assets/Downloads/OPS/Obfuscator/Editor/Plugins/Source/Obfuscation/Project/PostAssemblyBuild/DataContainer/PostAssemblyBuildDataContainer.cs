using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.DataContainer;

// OPS - Obfuscator - Project - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Assembly - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping;

// OPS - Obfuscator - Assets - Cache
using OPS.Obfuscator.Editor.Assets.Unity.Cache;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild
{
    /// <summary>
    /// Contains all the data the obfuscation project needs.
    /// </summary>
    public class PostAssemblyBuildDataContainer : ADataContainer
    {
        // Pipeline Component
        #region Pipeline Component

        /// <summary>
        /// All pipeline obfuscator components.
        /// </summary>
        public List<IPostAssemblyBuildComponent> ComponentList { get; set; } = new List<IPostAssemblyBuildComponent>();

        #endregion

        // Compatibility Component
        #region Compatibility Component

        /// <summary>
        /// All obfuscator compatibility components.
        /// </summary>
        public List<IObfuscationCompatibilityComponent> CompatibilityComponentList { get; set; } = new List<IObfuscationCompatibilityComponent>();

        #endregion

        // Assembly
        #region Assembly

        /// <summary>
        /// Information about to load assemblies.
        /// </summary>
        public List<AssemblyLoadInfo> AssemblyLoadInfoList { get; set; } = new List<AssemblyLoadInfo>();

        /// <summary>
        /// Directory path to all assembly dependencies.
        /// </summary>
        public List<String> AssemblyDependencyDirectoryPathList { get; set; } = new List<string>();

        /// <summary>
        /// Contains the to obfuscate assemblies.
        /// </summary>
        public List<AssemblyInfo> ObfuscateAssemblyList { get; set; } = new List<AssemblyInfo>();

        /// <summary>
        /// Contains the helper assemblies.
        /// </summary>
        public List<AssemblyInfo> HelperAssemblyList { get; set; } = new List<AssemblyInfo>();

        #endregion

        // Assembly Resolver
        #region Assembly Resolver

        /// <summary>
        /// Used to resolve assemblies and their types.
        /// </summary>
        public AssemblyResolver AssemblyResolver { get; set; } = new AssemblyResolver();

        #endregion

        // Cache
        #region Cache

        /// <summary>
        /// MemberType, Cache
        /// Contains the cache of all members.
        /// </summary>
        public Dictionary<EMemberType, IMemberCache> CacheDictionary { get; set; } = new Dictionary<EMemberType, IMemberCache>();

        #endregion

        // RenameManager
        #region RenameManager

        /// <summary>
        /// Manages what types/members should or should not get obfuscated. And their renaming mapping.
        /// </summary>
        public RenameManager RenameManager { get; set; } = new RenameManager(null);

        #endregion
    }
}
