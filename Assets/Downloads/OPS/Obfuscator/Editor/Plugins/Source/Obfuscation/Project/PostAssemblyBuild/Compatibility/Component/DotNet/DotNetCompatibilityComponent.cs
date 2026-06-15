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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component
{
    public class DotNetCompatibilityComponent : AObfuscationCompatibilityComponent, INamespaceCompatibility, ITypeCompatibility, IMethodCompatibility, IFieldCompatibility, IPropertyCompatibility, IEventCompatibility
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
                return ".Net Framework - Compatibility";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Controls the Obfuscator compatibility to the .Net / Mono Framework. Without this, the obfuscation would not be possible.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Controls the Obfuscator compatibility to the .Net / Mono Framework.";
            }
        }

        #endregion

        //Namespace
        #region Namespace

        public bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = "";
            return false;
        }

        public bool IsNamespaceRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            // Has the System.Reflection.ObfuscationAttribute
            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, "ObfuscationAttribute"))
            {
                _Cause = "Has ObfuscationAttribute.";
                return false;
            }

            // Serialization - Type or base is serializeable.
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.IsTypeOrBaseSerializable(_TypeDefinition))
            {
                // Is Serializeable or some base is serializeable.

                // Only for non enums, because enums are understood as serializeable but they have a own setting!
                if (!_TypeDefinition.IsEnum)
                {
                    if (_Step.Settings.Get_ComponentSettings_As_Bool(PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey, PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_Serializable))
                    {
                        //Is serializable and the obfuscation of serializable classes is activated.
                    }
                    else
                    {
                        //Is serializable and the obfuscation of serializeable types is deactivated.

                        _Cause = "Because of the 'Do not obfuscate serializable classes' settings.";
                        return false;
                    }
                }
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
            String var_ReflectionObfuscationAttributeFullName = typeof(System.Reflection.ObfuscationAttribute).FullName;

            foreach (CustomAttribute var_CustomAttribute in _TypeDefinition.CustomAttributes)
            {
                var var_AttributeFullName = var_CustomAttribute.Constructor.DeclaringType.FullName;

                if (var_AttributeFullName == var_ReflectionObfuscationAttributeFullName)
                {
                    if(!AttributeHelper.TryGetAttributeValueByName<bool>(var_CustomAttribute, "Exclude", out bool var_Exclude))
                    {
                        var_Exclude = true;
                    }
                    if (!AttributeHelper.TryGetAttributeValueByName<bool>(var_CustomAttribute, "ApplyToMembers", out bool var_ApplyToMembers))
                    {
                        var_ApplyToMembers = true;
                    }

                    if (var_Exclude && var_ApplyToMembers)
                    {
                        _Cause = "This type should be excluded because of the 'ObfuscationAttribute' and its members too.";
                        return true;
                    }
                }
            }

            _Cause = "";
            return false;
        }

        public bool IsTypeRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            //Special Type
            if (_TypeDefinition.Name == "<Module>")
            {
                _Cause = "Is Module.";
                return false;
            }
            if (_TypeDefinition.Name == "Resources")
            {
                _Cause = "Is Resources.";
                return false;
            }
            if (_TypeDefinition.Name == "MonoPInvokeCallbackAttribute")
            {
                _Cause = "Is MonoPInvokeCallbackAttribute.";
                return false;
            }
            
            // Has the System.Reflection.ObfuscationAttribute
            if (AttributeHelper.HasCustomAttribute(_TypeDefinition, "ObfuscationAttribute"))
            {
                _Cause = "Has ObfuscationAttribute.";
                return false;
            }

            // Serialization - Type or base is serializeable.
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.IsTypeOrBaseSerializable(_TypeDefinition))
            {
                // Is Serializeable or some base is serializeable.

                // Only for non enums, because enums are understood as serializeable but they have a own setting!
                if (!_TypeDefinition.IsEnum)
                {
                    if (_Step.Settings.Get_ComponentSettings_As_Bool(PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey, PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_Serializable))
                    {
                        //Is serializable and the obfuscation of serializable classes is activated.
                    }
                    else
                    {
                        //Is serializable and the obfuscation of serializeable types is deactivated.

                        _Cause = "Because of the 'Do not obfuscate serializable classes' settings.";
                        return false;
                    }
                }
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
            _Cause = "Is Special.";

            if (_MethodDefinition.Name == "Invoke")
                return false;
            if (_MethodDefinition.Name == "BeginInvoke")
                return false;
            if (_MethodDefinition.Name == "EndInvoke")
                return false;
            if (_MethodDefinition.Name == "GetEnumerator")
                return false;
            if (_MethodDefinition.Name == "MoveNext")
                return false;
            if (_MethodDefinition.IsConstructor)
                return false;
            if (_MethodDefinition.IsNative)
                return false;
            if (_MethodDefinition.IsPInvokeImpl)
                return false;
            if (_MethodDefinition.IsUnmanaged)
                return false;

            // Extern DLL
            if (AttributeHelper.HasCustomAttribute(_MethodDefinition, "DllImportAttribute"))
            {
                _Cause = "Has DllImportAttribute.";
                return false;
            }

            // Has the System.Reflection.ObfuscationAttribute
            if (AttributeHelper.HasCustomAttribute(_MethodDefinition, "ObfuscationAttribute"))
            {
                _Cause = "Has ObfuscationAttribute.";
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
            _Cause = "Is a special name.";

            if (_FieldDefinition.Name == "version")
                return false;
            if (_FieldDefinition.Name == "collection")
                return false;
            if (_FieldDefinition.Name == "next")
                return false;
            if (_FieldDefinition.Name == "current")
                return false;
            if (_FieldDefinition.Name == "container")
                return false;
            if (_FieldDefinition.Name == "Array")
                return false;

            // ENUM
            // Note: value__ is a reserved name, so fields can not be named like this :)!
            if (_FieldDefinition.Name == "value__")
            {
                _Cause = "Is the enums value__.";
                return false;
            }

            // Has the System.Reflection.ObfuscationAttribute
            if (AttributeHelper.HasCustomAttribute(_FieldDefinition, "ObfuscationAttribute"))
            {
                _Cause = "Has ObfuscationAttribute.";
                return false;
            }

            _Cause = null;
            return true;
        }

        public string ApplyFieldRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Property
        #region Property

        public bool IsPropertyRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, out string _Cause)
        {
            // Property represents the 'this' on a class!
            if (_PropertyDefinition.HasParameters)
            {
                _Cause = "Is 'Item'";
                return false;
            }

            // Has the System.Reflection.ObfuscationAttribute
            if (AttributeHelper.HasCustomAttribute(_PropertyDefinition, "ObfuscationAttribute"))
            {
                _Cause = "Has ObfuscationAttribute.";
                return false;
            }

            _Cause = "";
            return true;
        }

        public string ApplyPropertyRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Event
        #region Event

        public bool IsEventRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, EventDefinition _EventDefinition, out string _Cause)
        {
            // Has the System.Reflection.ObfuscationAttribute
            if (AttributeHelper.HasCustomAttribute(_EventDefinition, "ObfuscationAttribute"))
            {
                _Cause = "Has ObfuscationAttribute.";
                return false;
            }

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
        public const String CSettingsKey = "Default_Compatibility_Component_DotNet";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        #endregion

        //Gui
        #region Gui

        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name);

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
