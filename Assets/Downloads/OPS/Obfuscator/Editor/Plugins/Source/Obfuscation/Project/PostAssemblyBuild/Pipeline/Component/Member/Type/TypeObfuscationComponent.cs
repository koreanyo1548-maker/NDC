using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Pipeling
using OPS.Editor.Project.Pipeline;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Type
{
    public class TypeObfuscationComponent : AMemberObfuscationComponent<TypeReference, TypeKey>
    {
        // Name
        #region Name

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Class - Obfuscation";
            }
        }

        #endregion

        // Description
        #region Description

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override string Description
        {
            get
            {
                return "Activate and manage the obfuscation of classes and their content.";
            }
        }

        #endregion

        // Short Description
        #region Short Description

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Activate and manage the obfuscation of classes and their content.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Class";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CObfuscate_Classes = "Obfuscate_Classes";
        public const String CObfuscate_Class_Public = "Obfuscate_Class_Public";
        public const String CObfuscate_Class_Internal = "Obfuscate_Class_Internal";
        public const String CObfuscate_Class_Protected = "Obfuscate_Class_Protected";
        public const String CObfuscate_Class_Private = "Obfuscate_Class_Private";

        public const String CObfuscate_Class_Abstract = "Obfuscate_Class_Abstract";
        public const String CObfuscate_Class_Generic = "Obfuscate_Class_Generic";
        public const String CObfuscate_Class_Serializable = "Obfuscate_Class_Serializable";
        
        public const String CObfuscate_Class_MonoBehaviour = "Obfuscate_Class_MonoBehaviour";

        public const String CObfuscate_Class_MonoBehaviour_Extern = "Obfuscate_Class_MonoBehaviour_Extern";

        public const String CObfuscate_Class_ScriptableObject = "Obfuscate_Class_ScriptableObject";

        public const String CObfuscate_Class_Playable = "Obfuscate_Class_Playable";

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
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CObfuscate_Classes);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // MultiRow
            Row_Multi_Boolean var_CustomObfuscateRow = new Row_Multi_Boolean("Obfuscate:", new string[] { "Internal", "Private", "Protected", "Public" }, _ComponentSettings, new string[] { CObfuscate_Class_Internal, CObfuscate_Class_Private, CObfuscate_Class_Protected, CObfuscate_Class_Public });
            var_CustomObfuscateRow.Notification_Info = "Obfuscation controlled through the accessibility.";
            var_Content.AddRow(var_CustomObfuscateRow);

            // Subclasses
            Row_Section var_SubclassesRow = new Row_Section("Subclasses:");
            var_Content.AddRow(var_SubclassesRow);

            // Obfuscate Abstract Classes
            Row_Boolean var_ObfuscateAbstractClassRow = new Row_Boolean("Obfuscate Abstract Classes:", _ComponentSettings, CObfuscate_Class_Abstract);
            var_ObfuscateAbstractClassRow.Notification_Info = "Activate this setting to obfuscate abstract classes.";
            var_Content.AddRow(var_ObfuscateAbstractClassRow);

            // Obfuscate Generic Classes
            Row_Boolean var_ObfuscateGenericClassRow = new Row_Boolean("Obfuscate Generic Classes:", _ComponentSettings, CObfuscate_Class_Generic);
            var_ObfuscateGenericClassRow.Notification_Info = "Activate this setting to obfuscate generic classes.";
            var_Content.AddRow(var_ObfuscateGenericClassRow);

            // Obfuscate Serializable Classes
            Row_Boolean var_ObfuscateSerializableClassRow = new Row_Boolean("Obfuscate Serializable Classes:", _ComponentSettings, CObfuscate_Class_Serializable);
            var_ObfuscateSerializableClassRow.Notification_Info = "Activate this setting to obfuscate serializeable classes or classes that are marked with the [Serializeable] attribute.";
            var_ObfuscateSerializableClassRow.Notification_Warning = "If you want to obfuscate serializeable classes, there are two prerequisites: \r\n- First, these classes must not be saved in the editor, but only at runtime. Otherwise, the data saved in the editor will have different names than the build runtime classes, which means that the saved data cannot be resolved, as Unity saves serializable classes by name.\r\n- Secondly, you need to enable the 'Rename Mapping' setting in the optional tab. This option saves and loads a mapping of the original classes/methods/fields/... and the matching obfuscated name. This way the name will match between the different builds.";
            var_Content.AddRow(var_ObfuscateSerializableClassRow, true);

            // Unity Subclasses
            Row_Section var_UnitySublcassesRow = new Row_Section("Unity Subclasses:");
            var_Content.AddRow(var_UnitySublcassesRow);

            // Obfuscate MonoBehaviour Classes
            Row_Boolean var_ObfuscateMonoBehaviourClassRow = new Row_Boolean("Obfuscate MonoBehaviour SubClasses:", _ComponentSettings, CObfuscate_Class_MonoBehaviour);
            var_ObfuscateMonoBehaviourClassRow.Notification_Info = "Activate this setting to obfuscate MonoBehaviour subclasses. This requires the activation of the namespace obfuscation setting.";
            var_Content.AddRow(var_ObfuscateMonoBehaviourClassRow, true);

            // Obfuscate External MonoBehaviour Classes
            Row_Boolean var_ObfuscateUnityClassExternalRow = new Row_Boolean("Obfuscate MonoBehaviour SubClasses in external Assemblies:", _ComponentSettings, CObfuscate_Class_MonoBehaviour_Extern);
            var_ObfuscateUnityClassExternalRow.Notification_Info = "Activate this setting to obfuscate MonoBehaviour subclasses in external assemblies you set in the assembly settings. This setting will obfuscate the classes in the 'Past 2018.2' way.";
            var_Content.AddRow(var_ObfuscateUnityClassExternalRow, true);

            // Obfuscate ScriptableObject Classes
            Row_Boolean var_ObfuscateScriptableObjectClassRow = new Row_Boolean("Obfuscate ScriptableObject SubClasses:", _ComponentSettings, CObfuscate_Class_ScriptableObject);
            var_ObfuscateScriptableObjectClassRow.Notification_Info = "Activate this setting to obfuscate ScriptableObject subclasses. This requires the activation of the namespace obfuscation setting.";
            var_Content.AddRow(var_ObfuscateScriptableObjectClassRow, true);

            // Obfuscate PlayAble Classes
            Row_Boolean var_ObfuscatePlayAbleClassRow = new Row_Boolean("Obfuscate Playable SubClasses:", _ComponentSettings, CObfuscate_Class_Playable);
            var_ObfuscatePlayAbleClassRow.Notification_Info = "Activate this setting to obfuscate Playable/PlayableAsset/PlayableBehaviour subclasses. This requires the activation of the namespace obfuscation setting.";
            var_Content.AddRow(var_ObfuscatePlayAbleClassRow, true);

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Analyse A
        #region Analyse A

        // Find TypeReferences
        #region Find TypeReferences

        protected override List<TypeReference> OnAnalyse_A_FindMemberReferences(AssemblyInfo _AssemblyInfo)
        {
            // Result
            List<TypeReference> var_TypeReferenceList = new List<TypeReference>();

            // Module References
            var_TypeReferenceList.AddRange(this.OnAnalyse_A_FindAllModuleTypeReferences(_AssemblyInfo));

            // Custom Attribute References
            var_TypeReferenceList.AddRange(this.OnAnalyse_A_FindAllCustomAttributeTypeReferences(_AssemblyInfo));

            return var_TypeReferenceList;
        }

        /// <summary>
        /// Add the 'GetTypeReferences' in all the Modules of _AssemblyInfo.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private List<TypeReference> OnAnalyse_A_FindAllModuleTypeReferences(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Analyse Type References in Module...");

            // Result
            List<TypeReference> var_TypeReferenceList = new List<TypeReference>();

            //Analyse the modules GetTypeReferences
            for (int m = 0; m < _AssemblyInfo.AssemblyDefinition.Modules.Count; m++)
            {
                foreach (TypeReference var_TypeReference in _AssemblyInfo.AssemblyDefinition.Modules[m].GetTypeReferences())
                {
                    var_TypeReferenceList.Add(var_TypeReference);
                }
            }

            return var_TypeReferenceList;
        }

        /// <summary>
        /// Add the 'Custom Attributes' TypeReferences of _AssemblyInfo.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private List<TypeReference> OnAnalyse_A_FindAllCustomAttributeTypeReferences(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Analyse Type References in Custom Attribute References...");

            // Result
            List<TypeReference> var_TypeReferenceList = new List<TypeReference>();

            // Process Custom Attributes in Assembly
            List<TypeReference> var_CustomAttribute_Reference_Assembly = Find_TypeReferences_In_CustomAttributes_In_Assembly(_AssemblyInfo.AssemblyDefinition);
            foreach (TypeReference var_TypeReference in var_CustomAttribute_Reference_Assembly)
            {
                var_TypeReferenceList.Add(var_TypeReference);
            }

            // Process Custom Attributes in Assembly Types
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                List<TypeReference> var_CustomAttribute_Reference_Type = Find_TypeReferences_In_CustomAttributes_In_TypeDefinition(var_TypeDefinition);

                foreach (TypeReference var_TypeReference in var_CustomAttribute_Reference_Type)
                {
                    var_TypeReferenceList.Add(var_TypeReference);
                }
            }

            return var_TypeReferenceList;
        }

        // Find Type References - Custom Attributes
        #region Find Type References - Custom Attributes

        /// <summary>
        /// Returns a list of all TypeReference in _AssemblyDefinition CustomAttributes.
        /// </summary>
        /// <param name="_AssemblyDefinition"></param>
        /// <returns></returns>
        private List<TypeReference> Find_TypeReferences_In_CustomAttributes_In_Assembly(AssemblyDefinition _AssemblyDefinition)
        {
            return Find_TypeReferences_In_CustomAttribute_Collection(_AssemblyDefinition);
        }

        /// <summary>
        /// Returns a list of all TypeReference of the CustomAttributes in _TypeDefinition, and its members.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private List<TypeReference> Find_TypeReferences_In_CustomAttributes_In_TypeDefinition(TypeDefinition _TypeDefinition)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "]  Analyse Type References in Custom Attribute References...");

            List<TypeReference> var_Result = new List<TypeReference>();

            var_Result.AddRange(Find_TypeReferences_In_CustomAttribute_Collection(_TypeDefinition));

            foreach (MethodDefinition var_Method in _TypeDefinition.Methods)
            {
                foreach (ParameterDefinition var_Parameter in var_Method.Parameters)
                {
                    var_Result.AddRange(Find_TypeReferences_In_CustomAttribute_Collection(var_Parameter));
                }
                var_Result.AddRange(Find_TypeReferences_In_CustomAttribute_Collection(var_Method));
            }
            foreach (FieldDefinition var_Field in _TypeDefinition.Fields)
            {
                var_Result.AddRange(Find_TypeReferences_In_CustomAttribute_Collection(var_Field));
            }
            foreach (PropertyDefinition var_Property in _TypeDefinition.Properties)
            {
                var_Result.AddRange(Find_TypeReferences_In_CustomAttribute_Collection(var_Property));
            }
            foreach (EventDefinition var_Event in _TypeDefinition.Events)
            {
                var_Result.AddRange(Find_TypeReferences_In_CustomAttribute_Collection(var_Event));
            }

            return var_Result;
        }

        /// <summary>
        /// CustomAttributes have ConstructorArguments and Properties and Fields.
        /// CustomAttributes having a Value. If it is a TypeReference, it is a reference.
        /// CustomAttributeNamedArgument have a Name and an Argument. The Name has to be overriden if the belonging Property or Field got obfuscated.
        /// The Argument does maybe recursive reference a TypeReference. 
        /// </summary>
        /// <param name="_CustomAttributeProvider"></param>
        /// <returns></returns>
        private List<TypeReference> Find_TypeReferences_In_CustomAttribute_Collection(ICustomAttributeProvider _CustomAttributeProvider)
        {
            List<TypeReference> var_Result = new List<TypeReference>();

            foreach(CustomAttributeArgument var_CustomAttributeArgument in AttributeHelper.Iterate_CustomAttributeArguments(_CustomAttributeProvider))
            {
                List<TypeReference> var_TypeReferenceList = Find_TypeReferences_In_CustomAttributeArgument(var_CustomAttributeArgument);

                foreach (TypeReference var_TypeReference in var_TypeReferenceList)
                {
                    if (var_TypeReference != null)
                    {
                        var_Result.Add(var_TypeReference);
                    }
                }
            }

            return var_Result;
        }

        /// <summary>
        /// If the Value of _CustomAttributeArgument is a TypeReference return it else null.
        /// </summary>
        /// <param name="_CustomAttributeArgument"></param>
        /// <returns></returns>
        private List<TypeReference> Find_TypeReferences_In_CustomAttributeArgument(CustomAttributeArgument _CustomAttributeArgument)
        {
            List<TypeReference> var_Result = new List<TypeReference>();

            // Add _CustomAttributeArgument.Type
            if (_CustomAttributeArgument.Type is TypeReference)
            {
                var_Result.Add(_CustomAttributeArgument.Type);
            }

            // Add _CustomAttributeArgument.Value
            MetadataType metadataType = _CustomAttributeArgument.Type.MetadataType;

            switch (metadataType)
            {
                case MetadataType.Object:
                    {
                        if (_CustomAttributeArgument.Value is CustomAttributeArgument)
                        {
                            CustomAttributeArgument var_CustomAttributeArgument = (CustomAttributeArgument)_CustomAttributeArgument.Value;

                            var_Result.AddRange(Find_TypeReferences_In_CustomAttributeArgument(var_CustomAttributeArgument));
                        }

                        break;
                    }
                default:
                    {
                        if (_CustomAttributeArgument.Value is TypeReference)
                        {
                            var_Result.Add((TypeReference)_CustomAttributeArgument.Value);
                        }

                        break;
                    }
            }

            return var_Result;
        }

        #endregion

        #endregion

        // Find do not rename TypeDefinition
        #region Find do not rename TypeDefinition

        protected override bool OnAnalyse_A_FindMemberToNotRename(AssemblyInfo _AssemblyInfo)
        {
            // Skip Types
            this.OnAnalyse_A_FindTypeDefinitionsShouldBeSkipped(_AssemblyInfo);

            // Dot not rename Type
            this.OnAnalyse_A_FindTypeDefinitionsShouldBeNotRenamed(_AssemblyInfo);

            return true;
        }

        // Skip Type
        #region Skip Type

        /// <summary>
        /// Checks if the whole Type and members should be skipped!
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindTypeDefinitionsShouldBeSkipped(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                bool var_SkipType = false;

                //Check Assembly
                if (!_AssemblyInfo.AssemblyLoadInfo.Obfuscate)
                {
                    //Cause
                    String var_DoNotObuscateCause = "Obfuscation for this assembly got deactivated. This assembly might be a helper assembly.";

                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type and Members [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);

                    var_SkipType = true;
                }

                //Check Component for Type
                if (!this.Analyse_A_Helper_Compatibility_SkipWholeType(_AssemblyInfo, var_TypeDefinition, out List<String> var_CauseList))
                {
                    //Cause
                    for (int c = 0; c < var_CauseList.Count; c++)
                    {
                        this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_CauseList[c]);
                        this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_CauseList[c]);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type and Members [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_CauseList[c]);
                    }

                    var_SkipType = true;
                }
                
                //Generic
                if (TypeDefinitionHelper.IsTypeGeneric(var_TypeDefinition) && !this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Generic))
                {
                    //Cause
                    String var_DoNotObuscateCause = "Because of your generic class obfuscation settings.";
                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Full Type [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);

                    var_SkipType = true;
                }

                //Abstract
                if (TypeDefinitionHelper.IsTypeAbstract(var_TypeDefinition) && !this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Abstract))
                {
                    //Cause
                    String var_DoNotObuscateCause = "Because of your abstract class obfuscation settings.";
                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Full Type [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);

                    var_SkipType = true;
                }

                //Gets skipped!
                if (var_SkipType)
                {
                    this.Analyse_A_Helper_Recursive_SkipFullType(var_TypeDefinition);
                }
            }
        }

        /// <summary>
        /// Skip the whole type?
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_SkipWholeType(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out List<String> _CauseList)
        {
            bool var_Allowed = true;

            List<ITypeCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<ITypeCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (var_CompatibilityComponentList[a].SkipWholeType(this.Step, _AssemblyInfo, _TypeDefinition, out String var_Cause))
                {
                    _CauseList.Add("Because of compatibility component: " + var_CompatibilityComponentList[a].Name + " : " + var_Cause);
                    var_Allowed = false;
                }
            }

            return var_Allowed;
        }

        private void Analyse_A_Helper_Recursive_SkipFullType(TypeDefinition _TypeDefinition)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Full Type [" + _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + "Because of the type skipping settings.");

            //Do not obfuscate.
            String var_DoNotObuscateCause = "Because of some type skipping settings.";

            this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, _TypeDefinition, var_DoNotObuscateCause);
            this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition, var_DoNotObuscateCause);

            foreach (var var_Member in _TypeDefinition.Fields)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, var_Member, var_DoNotObuscateCause);
            }

            foreach (var var_Member in _TypeDefinition.Properties)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Property, var_Member, var_DoNotObuscateCause);
            }

            foreach (var var_Member in _TypeDefinition.Events)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Event, var_Member, var_DoNotObuscateCause);
            }

            foreach (var var_Member in _TypeDefinition.Methods)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_Member, var_DoNotObuscateCause);
            }

            foreach (TypeDefinition var_NestedType in _TypeDefinition.NestedTypes)
            {
                this.Analyse_A_Helper_Recursive_SkipFullType(var_NestedType);
            }
        }

        #endregion

        // Do not rename Type
        #region Do not rename Type

        /// <summary>
        /// Analyse the Types to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindTypeDefinitionsShouldBeNotRenamed(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                //Check Component for Type
                if (!this.Analyse_A_Helper_Compatibility_IsTypeRenamingAllowed(_AssemblyInfo, var_TypeDefinition, out List<String> var_CauseList))
                {
                    //Cause
                    for (int c = 0; c < var_CauseList.Count; c++)
                    {
                        this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_CauseList[c]);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_CauseList[c]);
                    }
                }

                //General Obfuscation
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Classes))
                {
                    //Cause
                    String var_DoNotObuscateCause = "Because of the global class obfuscation settings.";

                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                }

                //Check if Obfuscation allowed by Settings.
                if ((this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Public) && (var_TypeDefinition.IsPublic || var_TypeDefinition.IsNestedPublic)) // || var_TypeDefinition.IsNotPublic
                || (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Protected) && var_TypeDefinition.IsNestedFamily)
                || (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Private) && (var_TypeDefinition.IsNestedPrivate || var_TypeDefinition.IsNotPublic))
                || (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Internal) && (var_TypeDefinition.IsNestedAssembly))
                || ((this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Protected) || this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Internal)) && (var_TypeDefinition.IsNestedFamilyOrAssembly))
                || ((this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Protected) && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Internal)) && (var_TypeDefinition.IsNestedFamilyAndAssembly)))
                {
                }
                else
                {
                    //Cause
                    String var_DoNotObuscateCause = "Because of some class obfuscation settings.";

                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + var_TypeDefinition.ToString() + "] " + var_DoNotObuscateCause);
                }
            }
        }

        /// <summary>
        /// Check Compatibility Components.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_IsTypeRenamingAllowed(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out List<String> _CauseList)
        {
            bool var_Allowed = true;

            List<ITypeCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<ITypeCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (!var_CompatibilityComponentList[a].IsTypeRenamingAllowed(this.Step, _AssemblyInfo, _TypeDefinition, out String var_Cause))
                {
                    _CauseList.Add("Because of compatibility component: " + var_CompatibilityComponentList[a].Name + " : " + var_Cause);
                    var_Allowed = false;
                }
            }

            return var_Allowed;
        }

        #endregion

        #endregion

        #endregion

        // Analyse B
        #region Analyse B

        public override bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Find Names
        #region Find Names

        /// <summary>
        /// Find names for the member to obfuscate.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public override bool OnFindMemberNames_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                {
                    continue;
                }

                String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition);

                if (var_ObfuscatedName == null)
                {
                    var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetUniqueObfuscatedName(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, int.MaxValue);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found a new Type Name [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] : " + var_ObfuscatedName);
                }
                else
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found a looked up Type Name [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] : " + var_ObfuscatedName);
                }

                this.Step.DataContainer.RenameManager.AddObfuscated(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_ObfuscatedName);
            }

            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        public override bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Check if the type is null.
                if (var_TypeDefinition == null)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("The assembly '{0}' contains a null type.", _AssemblyInfo.Name));
                    continue;
                }

                // Check if the type is skipped.
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                {
                    continue;
                }

                // Get the obfuscated name.
                String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition);

                // If the obfuscated name is null, continue.
                if (String.IsNullOrEmpty(var_ObfuscatedName))
                {
                    continue;
                }

                // Get the original key.
                TypeKey var_OriginalKey = this.Step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(var_TypeDefinition);

                // Check if the original key is null.
                if(var_OriginalKey == default(TypeKey))
                {                     
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Could not find the original type key for the type '{0}' while obfuscating the type name.", new TypeKey(var_TypeDefinition).ToString()));
                    continue;
                }

                // Go through all references.
                for (int r = 0; r < this.MemberReferenceMapping.ReferenceList.Count;)
                {
                    var var_ReferencePair = this.MemberReferenceMapping.ReferenceList[r];

                    // Check if they match.
                    if (var_ReferencePair.Value == var_OriginalKey)
                    {
                        // Match!

                        // Set name
                        var_ReferencePair.Key.GetElementType().Name = var_ObfuscatedName;

                        // Remove the reference.
                        this.MemberReferenceMapping.ReferenceList.RemoveAt(r);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Update Reference Type [" + var_ReferencePair.Value + "]");

                        continue;
                    }

                    r++;
                }

                // Rename MethodDefinition.
                var_TypeDefinition.Name = var_ObfuscatedName;

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Rename Type [" + var_OriginalKey.ToString() + "] to " + var_TypeDefinition.Name);
            }

            return true;
        }

        #endregion
    }
}
