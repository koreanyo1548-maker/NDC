using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

//OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component
{
    public class ObfuscatorCompatibilityComponent : AObfuscationCompatibilityComponent, INamespaceCompatibility, ITypeCompatibility, IMethodCompatibility, IFieldCompatibility, IPropertyCompatibility, IEventCompatibility
    {
        //Info
        #region Info

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Obfuscator - Compatibility";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Controls the Obfuscator own components.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Controls the Obfuscator own components";
            }
        }

        #endregion

        //Namespace
        #region Namespace

        private List<String> namespaceToSkip = new List<string>()
        {
            "OPS"
        };

        public bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            for (int i = 0; i < this.namespaceToSkip.Count; i++)
            {
                if (_TypeDefinition.Namespace.StartsWith(this.namespaceToSkip[i]))
                {
                    _Cause = "Inside OPS Namespace";
                    return true;
                }
            }

            _Cause = "";
            return false;
        }

        public bool IsNamespaceRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            /*if (TypeDefinitionHelper.IsTypeAnAttribute(_TypeDefinition))
            {
                _Cause = "Attribute obfuscation is currently not supported.";
                return false;
            }

            if (_TypeDefinition.DeclaringType != null)
            {
                if (TypeDefinitionHelper.IsTypeAnAttribute(_TypeDefinition.DeclaringType))
                {
                    _Cause = "Attribute obfuscation is currently not supported.";
                    return false;
                }
            }*/

            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotRenameAttribute).Name))
            {
                _Cause = "Has DoNotRenameAttribute.";
                return false;
            }

            _Cause = null;
            return true;
        }

        public string ApplyNamespaceRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Type
        #region Type

        public bool SkipWholeType(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, "DoNotObfuscateClassAttribute"))
            {
                _Cause = "Has the DoNotObfuscateClassAttribute.";
                return true;
            }

            _Cause = null;
            return false;
        }

        public bool IsTypeRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            /*//Check if type equals DoNotRenameAttribute then skip!
            if (_TypeInfo.OriginalName == "DoNotRenameAttribute")
            {
                _Cause = "Is DoNotRenameAttribute.";
                return false;
            }

            //Check if type equals DoNotObfuscateClassAttribute then skip!
            if (_TypeInfo.OriginalName == "DoNotObfuscateClassAttribute")
            {
                _Cause = "Is DoNotObfuscateClassAttribute.";
                return false;
            }

            //Check if type equals DoNotMakeClassUnDecompileAbleAttribute then skip!
            if (_TypeInfo.OriginalName == "DoNotMakeClassUnDecompileAbleAttribute")
            {
                _Cause = "Is DoNotMakeClassUnDecompileAbleAttribute.";
                return false;
            }

            //Check if type equals DoNotUseClassForFakeCodeAttribute then skip!
            if (_TypeInfo.OriginalName == "DoNotUseClassForFakeCodeAttribute")
            {
                _Cause = "Is DoNotUseClassForFakeCodeAttribute.";
                return false;
            }

            //Check if type equals DoNotObfuscateMethodBodyAttribute then skip!
            if (_TypeInfo.OriginalName == "DoNotObfuscateMethodBodyAttribute")
            {
                _Cause = "Is DoNotObfuscateMethodBodyAttribute.";
                return false;
            }

            //Check if type equals SharedNameAttribute then skip!
            if (_TypeInfo.OriginalName == "SharedNameAttribute")
            {
                _Cause = "Is SharedNameAttribute.";
                return false;
            }*/

            /*if (TypeDefinitionHelper.IsTypeAnAttribute(_TypeDefinition))
            {
                _Cause = "Attribute obfuscation is currently not supported.";
                return false;
            }

            if (_TypeDefinition.DeclaringType != null)
            {
                if (TypeDefinitionHelper.IsTypeAnAttribute(_TypeDefinition.DeclaringType))
                {
                    _Cause = "Attribute obfuscation is currently not supported.";
                    return false;
                }
            }*/

            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, typeof(OPS.Obfuscator.Attribute.DoNotRenameAttribute).Name))
            {
                _Cause = "Has DoNotRenameAttribute.";
                return false;
            }

            _Cause = null;
            return true;
        }

        public string ApplyTypeRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Method
        #region Method

        public bool IsMethodRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition, out string _Cause)
        {
            if (AttributeHelper.HasCustomAttribute(_MethodDefinition, typeof(OPS.Obfuscator.Attribute.DoNotRenameAttribute).Name))
            {
                _Cause = "Has the DoNotRenameAttribute.";
                return false;
            }

            _Cause = null;
            return true;
        }

        public string ApplyMethodRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Field
        #region Field

        public bool IsFieldRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, out string _Cause)
        {
            if (AttributeHelper.HasCustomAttribute(_FieldDefinition, typeof(OPS.Obfuscator.Attribute.DoNotRenameAttribute).Name))
            {
                _Cause = "Has the DoNotRenameAttribute.";
                return false;
            }

            //Public Fields of Attributes should get skipped! Until the bug is fixed! Attributes with public field cannot be obfuscated currently. Result in errors.
            /*if (_FieldInfo.FieldDefinition.IsPublic)
            {
                if (IsAttribute(_FieldInfo.FieldDefinition.DeclaringType))
                {
                    _Cause = "Is Attribute.";
                    return false;
                }
            }*/

            /*if (TypeDefinitionHelper.IsTypeAnAttribute(_FieldDefinition.DeclaringType))
            {
                _Cause = "Attribute obfuscation is currently not supported.";
                return false;
            }*/

            _Cause = "";
            return true;
        }

        public string ApplyFieldRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Property
        #region Property

        public bool IsPropertyRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, out string _Cause)
        {
            if (AttributeHelper.HasCustomAttribute(_PropertyDefinition, typeof(OPS.Obfuscator.Attribute.DoNotRenameAttribute).Name))
            {
                _Cause = "Has the DoNotRenameAttribute.";
                return false;
            }

            //Public Fields of Attributes should get skipped! Until the bug is fixed! Attributes with public field cannot be obfuscated currently. Result in errors.
            /*if (_FieldInfo.FieldDefinition.IsPublic)
            {
                if (IsAttribute(_FieldInfo.FieldDefinition.DeclaringType))
                {
                    _Cause = "Is Attribute.";
                    return false;
                }
            }*/

            /*if (TypeDefinitionHelper.IsTypeAnAttribute(_PropertyDefinition.DeclaringType))
            {
                _Cause = "Attribute obfuscation is currently not supported.";
                return false;
            }*/

            _Cause = "";
            return true;
        }

        public string ApplyPropertyRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Event
        #region Event

        public bool IsEventRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, EventDefinition _EventDefinition, out string _Cause)
        {
            if (AttributeHelper.HasCustomAttribute(_EventDefinition, typeof(OPS.Obfuscator.Attribute.DoNotRenameAttribute).Name))
            {
                _Cause = "Has the DoNotRenameAttribute.";
                return false;
            }

            //Public Fields of Attributes should get skipped! Until the bug is fixed! Attributes with public field cannot be obfuscated currently. Result in errors.
            /*if (_FieldInfo.FieldDefinition.IsPublic)
            {
                if (IsAttribute(_FieldInfo.FieldDefinition.DeclaringType))
                {
                    _Cause = "Is Attribute.";
                    return false;
                }
            }*/

            /*if (TypeDefinitionHelper.IsTypeAnAttribute(_EventDefinition.DeclaringType))
            {
                _Cause = "Attribute obfuscation is currently not supported.";
                return false;
            }*/

            _Cause = "";
            return true;
        }

        public string ApplyEventRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, EventDefinition _EventDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Compatibility_Component_Obfuscator";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        #endregion

        //Gui
        #region Gui

        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion
    }
}
