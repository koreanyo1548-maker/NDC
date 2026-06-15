using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Unity
using UnityEditor;

// OPS - Settings
using OPS.Obfuscator.Editor.Settings;

namespace OPS.Obfuscator.Editor.Platform.Build
{
    public class DefaultBuildPlatform
    {
        public virtual List<String> AdditionalAssemblyResolvingDirectories()
        {
            return new List<string>();
        }

        public virtual void ModifiedSettings(ObfuscatorSettings _Settings)
        {
#if Obfuscator_Free     

            // Namespace
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.NamespaceObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.NamespaceObfuscationComponent.CObfuscate_Namespaces, false);

            // Type
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_Serializable, false);
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_MonoBehaviour, false);
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_MonoBehaviour_Extern, false);
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_ScriptableObject, false);
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_Playable, false);

            // Method
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Method.MethodObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Method.MethodObfuscationComponent.CObfuscate_Method_Unity, false);

            // Field
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Field.FieldObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Field.FieldObfuscationComponent.CObfuscate_Field_Serializable, false);
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Member.Field.FieldObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Member.Field.FieldObfuscationComponent.CObfuscate_Field_Public_Unity, false);

            // Security
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Security.CloneMethodComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Security.CloneMethodComponent.CEnable_Add_Random_Code, false);
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Security.StringObfuscationComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Security.StringObfuscationComponent.CEnable_String_Obfuscation, false);
            _Settings.Get_Or_Create_ComponentSettings(Project.PostAssemblyBuild.Pipeline.Component.Security.SuppressIldasmAttributeComponent.CSettingsKey).Add_Or_UpdateSettingElement(Project.PostAssemblyBuild.Pipeline.Component.Security.SuppressIldasmAttributeComponent.CEnable_Assembly_Suppress_ILDasm, false);

#endif
        }

        public override string ToString()
        {
            return "DefaultBuildPlatform";
        }
    }
}
