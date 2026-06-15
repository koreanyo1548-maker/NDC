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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component
{
    /// <summary>
    /// The compatibility component for Json Sdks.
    /// </summary>
    public class JsonSdkCompatibilityComponent : AObfuscationCompatibilityComponent, INamespaceCompatibility, IFieldCompatibility, IPropertyCompatibility
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
                return "JSON - Compatibility";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Controls the Obfuscator compatibility to Net.Json an Newtonsoft.Json.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Controls the Obfuscator compatibility to Json.";
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
            "System.Text.Json",
            "Net.Json",
            "Newtonsoft.Json"
        };

        /// <summary>
        /// Returns true if the whole namespace should be skipped.
        /// </summary>
        public bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipJsonActivated(_Step))
            {
                for (int i = 0; i < this.namespaceToSkip.Count; i++)
                {
                    if (_TypeDefinition.Namespace.StartsWith(this.namespaceToSkip[i]))
                    {
                        _Cause = "Inside Json Namespace";
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

        // Attributes
        #region Attributes

        /// <summary>
        /// The list of attributes to skip.
        /// </summary>
        private List<String> attributeToSkip = new List<string>()
        {
            // System.Text.Json
            "JsonIncludeAttribute",
            "JsonPropertyNameAttribute",
            "JsonRequiredAttribute",
            "JsonSerializableAttribute",
            // Newtonsoft.Json
            "JsonPropertyAttribute",
        };

        #endregion

        // Field
        #region Field

        /// <summary>
        /// Returns true if the field renaming is allowed.
        /// </summary>
        public bool IsFieldRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipJsonActivated(_Step))
            {
                // Check for attributes to skip.
                for (int a = 0; a < attributeToSkip.Count; a++)
                {
                    if (AttributeHelper.HasCustomAttribute(_FieldDefinition, attributeToSkip[a]))
                    {
                        _Cause = "Has a " + attributeToSkip[a] + " Attribute.";
                        return false;
                    }
                }
            }

            _Cause = null;
            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string ApplyFieldRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Property
        #region Property

        /// <summary>
        /// Returns true if the property renaming is allowed.
        /// </summary>
        public bool IsPropertyRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipJsonActivated(_Step))
            {
                // Check for attributes to skip.
                for (int a = 0; a < attributeToSkip.Count; a++)
                {
                    if (AttributeHelper.HasCustomAttribute(_PropertyDefinition, attributeToSkip[a]))
                    {
                        _Cause = "Has a " + attributeToSkip[a] + " Attribute.";
                        return false;
                    }
                }
            }

            _Cause = "";
            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string ApplyPropertyRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public const String CSettingsKey = "Default_Compatibility_Component_Json_Sdk";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        /// <summary>
        /// The settings key for the skip Json setting.
        /// </summary>
        public const String CSkip_Json = "Skip_Json";

        /// <summary>
        /// Helper method to check if the user has activated the skip Json setting.
        /// </summary>
        private bool Helper_Has_SkipJsonActivated(PostAssemblyBuildStep _Step)
        {
            return _Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CSkip_Json, true);
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
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CSkip_Json);

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
