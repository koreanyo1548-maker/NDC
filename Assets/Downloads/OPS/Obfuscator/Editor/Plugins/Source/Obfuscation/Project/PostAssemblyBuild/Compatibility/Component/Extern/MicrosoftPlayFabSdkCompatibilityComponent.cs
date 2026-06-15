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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component
{
    /// <summary>
    /// The compatibility component for the PlayFab Sdk.
    /// </summary>
    public class MicrosoftPlayFabSdkCompatibilityComponent : AObfuscationCompatibilityComponent, INamespaceCompatibility, ITypeCompatibility
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
                return "PlayFab - Compatibility";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Controls the Obfuscator compatibility to PlayFab Sdks.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Controls the Obfuscator compatibility to PlayFab Sdks.";
            }
        }

        #endregion

        // Namespace
        #region Namespace

        /// <summary>
        /// The list of namespaces to skip.
        /// </summary>
        private List<String> namespaceToSkip = new List<string>()
        {
            "PlayFab",
        };

        /// <summary>
        /// Skip the whole 'PlayFab' namespaces.
        /// </summary>
        /// <returns>True if the whole namespace should be skipped; otherwise false.</returns>
        public bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipPlayFabActivated(_Step))
            {
                for (int i = 0; i < this.namespaceToSkip.Count; i++)
                {
                    if (_TypeDefinition.Namespace.StartsWith(this.namespaceToSkip[i]))
                    {
                        _Cause = "Inside PlayFab Namespace";
                        return true;
                    }
                }
            }

            _Cause = "";
            return false;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public bool IsNamespaceRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = "";
            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string ApplyNamespaceRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Type
        #region Type

        /// <summary>
        /// The list of types, if inheriting from, should be skipped.
        /// </summary>
        private List<String> inheritanceTypeToSkip = new List<string>()
        {
            "PlayFab.SharedModels.PlayFabRequestCommon",
            "PlayFab.SharedModels.PlayFabResultCommon",
        };

        /// <summary>
        /// Skip the whole types that inherite from the 'PlayFab' request or response classes.
        /// </summary>
        /// <returns>True if the whole type should be skipped; otherwise false.</returns>
        public bool SkipWholeType(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipPlayFabActivated(_Step))
            {
                for (int i = 0; i < this.inheritanceTypeToSkip.Count; i++)
                {
                    if (TypeDefinitionHelper.InheritsFrom(_TypeDefinition, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), this.inheritanceTypeToSkip[i]))
                    {
                        _Cause = "Inherits from PlayFab Request or Response";
                        return true;
                    }
                }
            }

            _Cause = "";
            return false;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public bool IsTypeRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = "";
            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string ApplyTypeRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public const String CSettingsKey = "Default_Compatibility_Component_PlayFab_Sdk";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        /// <summary>
        /// The settings key for the skip PlayFab setting.
        /// </summary>
        public const String CSkip_PlayFab = "Skip_PlayFab";

        /// <summary>
        /// Helper method to check if the user has activated the skip PlayFab setting.
        /// </summary>
        private bool Helper_Has_SkipPlayFabActivated(PostAssemblyBuildStep _Step)
        {
            return _Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CSkip_PlayFab, true);
        }

        #endregion

        // Gui
        #region Gui

        /// <summary>
        /// Get the GUI container for this component.
        /// </summary>
        /// <param name="_ComponentSettings">The settings for this component.</param>
        /// <returns>The GUI container for this component.</returns>
        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CSkip_PlayFab);

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
