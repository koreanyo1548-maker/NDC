// System
using System;
using System.Collections.Generic;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component
{
    /// <summary>
    /// Component for MongoDB Realms compatibility.
    /// </summary>
    public class RealmsCompatibilityComponent : AObfuscationCompatibilityComponent, INamespaceCompatibility, ITypeCompatibility
    {
        // Info
        #region Info

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Realms - Compatibility";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Controls the Obfuscator compatibility to MongoDb Realms Sdk.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Controls the Obfuscator compatibility to MongoDb Realms Sdk.";
            }
        }

        #endregion

        // Helper
        #region Helper

        /// <summary>
        /// Returns true if the type inherits from 'Realms.IRealmObjectBase'.
        /// </summary>
        /// <param name="_Step">The current step.</param>
        /// <param name="_Type">The type to check.</param>
        /// <returns>True if the type inherits from 'Realms.IRealmObjectBase', otherwise false.</returns>
        private static bool Helper_InheritsFrom_IRealmObjectBase(PostAssemblyBuildStep _Step, TypeDefinition _Type)
        {
            return OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "Realms.IRealmObjectBase");
        }

        /// <summary>
        /// Returns true if the type inherits from 'Realms.IAsymmetricObject'.
        /// </summary>
        /// <param name="_Step">The current step.</param>
        /// <param name="_Type">The type to check.</param>
        /// <returns>True if the type inherits from 'Realms.IAsymmetricObject', otherwise false.</returns>
        private static bool Helper_InheritsFrom_IAsymmetricObject(PostAssemblyBuildStep _Step, TypeDefinition _Type)
        {
            return OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "Realms.IAsymmetricObject");
        }

        /// <summary>
        /// Returns true if the type inherits from 'Realms.IEmbeddedObject'.
        /// </summary>
        /// <param name="_Step">The current step.</param>
        /// <param name="_Type">The type to check.</param>
        /// <returns>True if the type inherits from 'Realms.IEmbeddedObject', otherwise false.</returns>
        private static bool Helper_InheritsFrom_IEmbeddedObject(PostAssemblyBuildStep _Step, TypeDefinition _Type)
        {
            return OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "Realms.IEmbeddedObject");
        }

        /// <summary>
        /// Returns true if the type inherits from 'Realms.IRealmObject'.
        /// </summary>
        /// <param name="_Step">The current step.</param>
        /// <param name="_Type">The type to check.</param>
        /// <returns>True if the type inherits from 'Realms.IRealmObject', otherwise false.</returns>
        private static bool Helper_InheritsFrom_IRealmObject(PostAssemblyBuildStep _Step, TypeDefinition _Type)
        {
            return OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "Realms.IRealmObject");
        }

        #endregion

        // Namespace
        #region Namespace

        /// <summary>
        /// The realms namespace to skip.
        /// </summary>
        private List<String> namespaceToSkip = new List<string>()
        {
            "Realms",
        };

        /// <summary>
        /// Skip the whole 'Realms' namespace from obfuscation.
        /// </summary>
        /// <param name="_Step">The current step.</param>
        /// <param name="_AssemblyInfo">The current assembly info.</param>
        /// <param name="_TypeDefinition">The current type definition.</param>
        /// <param name="_Cause">The cause of skipping.</param>
        /// <returns>True if the namespace should be skipped, otherwise false.</returns>
        public bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipRealmsActivated(_Step))
            {
                // Skip the namespace if it is in the list.
                for (int i = 0; i < this.namespaceToSkip.Count; i++)
                {
                    if (_TypeDefinition.Namespace.StartsWith(this.namespaceToSkip[i]))
                    {
                        _Cause = "Is inside the MongoDB Realms Namespace";
                        return true;
                    }
                }
            }

            _Cause = "";
            return false;
        }

        /// <summary>
        /// Check if the namespace renaming is allowed. Does nothing for this component.
        /// </summary>
        public bool IsNamespaceRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = "";
            return true;
        }

        /// <summary>
        /// Apply a custom namespace renaming filter. Return a custom name for the namespace if needed. Does nothing for this component.
        /// </summary>
        public string ApplyNamespaceRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Type
        #region Type

        /// <summary>
        /// Skip the whole type (including the namespace name) if it is a Realm object.
        /// </summary>
        /// <param name="_Step">The current step.</param>
        /// <param name="_AssemblyInfo">The current assembly info.</param>
        /// <param name="_TypeDefinition">The current type definition.</param>
        /// <param name="_Cause">The cause of skipping.</param>
        /// <returns>True if the type (including the namespace name) should be skipped, otherwise false.</returns>
        public bool SkipWholeType(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipRealmsActivated(_Step))
            {
                // Check if the type inherites from the 'IAsymmetricObject' interface.
                if (Helper_InheritsFrom_IAsymmetricObject(_Step, _TypeDefinition))
                {
                    _Cause = "Is a Realm Asymmetric Object";
                    return true;
                }

                // Check if the type inherites from the 'IEmbeddedObject' interface.
                if (Helper_InheritsFrom_IEmbeddedObject(_Step, _TypeDefinition))
                {
                    _Cause = "Is a Realm Embedded Object";
                    return true;
                }

                // Check if the type inherites from the 'IRealmObject' interface.
                if (Helper_InheritsFrom_IRealmObject(_Step, _TypeDefinition))
                {
                    _Cause = "Is a Realm Object";
                    return true;
                }

                // Check if the type inherites from the 'IRealmObjectBase' interface. Do the base at the end, because it is the most general one. And all other interfaces are derived from it.
                if (Helper_InheritsFrom_IRealmObjectBase(_Step, _TypeDefinition))
                {
                    _Cause = "Is a Realm Object";
                    return true;
                }
            }

            _Cause = "";
            return false;
        }

        /// <summary>
        /// Check if the type renaming is allowed. Does nothing for this component.
        /// </summary>
        public bool IsTypeRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = null;
            return true;
        }

        /// <summary>
        /// Apply a custom type renaming filter. Return a custom name for the type if needed. Does nothing for this component.
        /// </summary>
        public string ApplyTypeRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component.
        /// </summary>
        public const String CSettingsKey = "Default_Compatibility_Component_Realms_Sdk";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        /// <summary>
        /// The settings key for the skip Realms setting.
        /// </summary>
        public const String CSkip_Realms = "Skip_Realms";

        /// <summary>
        /// Helper method to check if the user has activated the skip Realms setting.
        /// </summary>
        private bool Helper_Has_SkipRealmsActivated(PostAssemblyBuildStep _Step)
        {
            return _Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CSkip_Realms, true);
        }

        #endregion

        // Gui
        #region Gui

        /// <summary>
        /// Returns the gui container for this component.
        /// </summary>
        /// <param name="_ComponentSettings">The component settings.</param>
        /// <returns>The gui container for this component.</returns>
        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CSkip_Realms);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion
    }
}
