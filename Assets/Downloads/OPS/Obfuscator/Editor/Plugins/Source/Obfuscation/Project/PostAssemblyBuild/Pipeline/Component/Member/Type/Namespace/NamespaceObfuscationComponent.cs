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
    public class NamespaceObfuscationComponent : AMemberObfuscationComponent<TypeReference, TypeKey>
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
                return "Namespace - Settings";
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
                return "Manage the obfuscation of namespaces and their class content.";
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
                return "Manage the obfuscation of namespaces and their class content.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Namespace";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        public const String CObfuscate_Namespaces = "Obfuscate_Namespaces";
        public const String CSkip_Namespace_Array = "Skip_Namespace_Array";
        public const String CSkip_Namespace_ViceVersa = "Skip_Namespace_ViceVersa";

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

            //Obfuscate Namespace
            Row_Boolean var_ObfuscateNamespaceRow = new Row_Boolean("Obfuscate Namespaces:", _ComponentSettings, CObfuscate_Namespaces);
            var_ObfuscateNamespaceRow.Notification_Info = "Activate this setting to obfuscate namespaces. If possible the obfuscator will remove the namespace, so the level of obfuscation will be increased.";
            var_Content.AddRow(var_ObfuscateNamespaceRow, true);

            //To Skip namespaces
            Row_Array var_SkipRow = new Row_Array("Skip following Namespaces:", _ComponentSettings, CSkip_Namespace_Array);
            var_SkipRow.Notification_Info = "Add here namespaces you want to skip. The obfuscator checks the prefix to decide whether a namespace gets skipped or not. For example, if you enter 'Unity.StandardAssets', all classes inside this entered namespace getting skipped and all classes with a namespace starting with 'Unity.StandardAssets' too.";
            var_SkipRow.Notification_Warning = "Do not enter an emtpy row! This will skip all namespaces and so nothing will be obfuscated!";
            var_Content.AddRow(var_SkipRow);

            //Vice versa
            Row_Boolean var_ViceVersaRow = new Row_Boolean("Vice Versa Namespace Skipping:", _ComponentSettings, CSkip_Namespace_ViceVersa);
            var_ViceVersaRow.Notification_Info = "Enable this setting to vice versa the namespace skipping. So only namespaces and classes entered in the above list getting obfuscated. The rest will not.";
            var_Content.AddRow(var_ViceVersaRow);

            //Container
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

            foreach (CustomAttributeArgument var_CustomAttributeArgument in AttributeHelper.Iterate_CustomAttributeArguments(_CustomAttributeProvider))
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
            if(_CustomAttributeArgument.Type is TypeReference)
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

        // Find do not rename TypeDefinition Namespace
        #region Find do not rename TypeDefinition Namespace

        protected override bool OnAnalyse_A_FindMemberToNotRename(AssemblyInfo _AssemblyInfo)
        {
            // Skip Types
            this.OnAnalyse_A_FindTypeDefinitionNamespacesShouldBeSkipped(_AssemblyInfo);

            // Dot not rename Type
            this.OnAnalyse_A_FindTypeDefinitionNamespacesShouldBeNotRenamed(_AssemblyInfo);

            return true;
        }

        // Skip Type Namespace
        #region Skip Type Namespace

        /// <summary>
        /// Checks if the whole Type and members should be skipped!
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindTypeDefinitionNamespacesShouldBeSkipped(AssemblyInfo _AssemblyInfo)
        {
            // Iterate all types in the passed assembly.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                bool var_SkipNamespace = false;

                // 1. Check if one of the compatibility components wants to skip the whole namespace.
                if (this.Analyse_A_Helper_Compatibility_SkipWholeNamespace(_AssemblyInfo, var_TypeDefinition, out List<String> var_CauseList))
                {
                    // Store the cause.
                    for (int c = 0; c < var_CauseList.Count; c++)
                    {
                        this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_CauseList[c]);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Namespace, Type and Members [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_CauseList[c]);
                    }

                    // Activate the skip flag.
                    var_SkipNamespace = true;
                }

                // 2. Check the user settings, if the namespace should be skipped.
                String[] var_Settings_NamespaceToSkipList = this.Step.Settings.Get_ComponentSettings_As_Array(this.SettingsKey, CSkip_Namespace_Array);
                if (var_Settings_NamespaceToSkipList != null && !String.IsNullOrEmpty(var_TypeDefinition.Namespace))
                {
                    bool var_NamespaceIsInSkipList = false;

                    // Check if the namespace is in the namespace skip list.
                    for (int i = 0; i < var_Settings_NamespaceToSkipList.Length; i++)
                    {
                        if (var_TypeDefinition.Namespace.StartsWith(var_Settings_NamespaceToSkipList[i]))
                        {
                            var_NamespaceIsInSkipList = true;
                            break;
                        }
                    }

                    // If is in, skip if Namespace viceversa is deactivated.
                    if (var_NamespaceIsInSkipList)
                    {
                        //It is in NamespacesToIgnoreList.
                        if (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CSkip_Namespace_ViceVersa))
                        {
                            //ViceVersa is active! So it gets not skipped!
                        }
                        else
                        {
                            //ViceVersa is not active! So it gets skipped!
                            var_SkipNamespace = true;
                        }
                    }
                    // If is not in, skip if namespace vice versa is active.
                    else
                    {
                        //It is not in NamespacesToIgnoreList.
                        if (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CSkip_Namespace_ViceVersa))
                        {
                            //ViceVersa is active! So Skip it!
                            var_SkipNamespace = true;
                        }
                        else
                        {
                            //ViceVersa is not active! So it gets not skipped!
                        }
                    }
                }

                // Namespace is empty, so no check from above.
                if (String.IsNullOrEmpty(var_TypeDefinition.Namespace))
                {
                    //Type has no Namespace
                    if (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CSkip_Namespace_ViceVersa))
                    {
                        //ViceVersa is active! So it gets skipped!
                        var_SkipNamespace = true;
                    }
                }

                // If the namespace should be skipped, skip the whole type.
                if (var_SkipNamespace)
                {
                    this.Analyse_A_Helper_Recursive_SkipFullType(var_TypeDefinition);
                }
            }
        }

        /// <summary>
        /// Skip the whole namespace?
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_SkipWholeNamespace(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out List<String> _CauseList)
        {
            bool var_Skip = false;

            List<INamespaceCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<INamespaceCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (var_CompatibilityComponentList[a].SkipWholeNamespace(this.Step, _AssemblyInfo, _TypeDefinition, out String var_Cause))
                {
                    _CauseList.Add("Because of compatibility component: " + var_CompatibilityComponentList[a].Name + " : " + var_Cause);
                    var_Skip = true;
                }
            }

            return var_Skip;
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

        // Do not rename Type Namespace
        #region Do not rename Type Namespace

        /// <summary>
        /// Analyse the Types to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindTypeDefinitionNamespacesShouldBeNotRenamed(AssemblyInfo _AssemblyInfo)
        {
#if Obfuscator_Free
            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                //Cause
                String var_DoNotObuscateCause = "Obfuscator Free is used.";
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Types Namespace [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
            }
#else
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                //Check Component for Namespace
                if (!this.Analyse_A_Helper_Compatibility_IsNamespaceRenamingAllowed(_AssemblyInfo, var_TypeDefinition, out List<String> var_CauseList))
                {
                    //Cause
                    for (int c = 0; c < var_CauseList.Count; c++)
                    {
                        this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_CauseList[c]);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Types Namespace [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_CauseList[c]);
                    }
                }

                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Namespaces))
                {
                    //Cause
                    String var_DoNotObuscateCause = "Because of the global Namespace obfuscation settings.";
                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Types Namespace [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                }

                if (var_TypeDefinition.IsNested || var_TypeDefinition.DeclaringType != null)
                {
                    //Cause
                    String var_DoNotObuscateCause = "Is a nested class and has no namespace.";
                    this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Types Namespace [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                }
            }
#endif
        }

        /// <summary>
        /// Check Compatibility Components.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_IsNamespaceRenamingAllowed(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out List<String> _CauseList)
        {
            bool var_Allowed = true;

            List<INamespaceCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<INamespaceCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (!var_CompatibilityComponentList[a].IsNamespaceRenamingAllowed(this.Step, _AssemblyInfo, _TypeDefinition, out String var_Cause))
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
#if Obfuscator_Free
#else
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition))
                {
                    continue;
                }

                String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition);

                if (var_ObfuscatedName == null)
                {
                    if (!this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                    {
                        var_ObfuscatedName = "";
                    }
                    else
                    {
                        var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetUniqueObfuscatedName(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, int.MaxValue);
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found a new Type Namespace [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] : " + var_ObfuscatedName);
                }
                else
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found a looked up Type Namespace [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] : " + var_ObfuscatedName);
                }

                this.Step.DataContainer.RenameManager.AddObfuscated(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_ObfuscatedName);
            }
#endif

            return true;
        }

#endregion

        // Obfuscate
#region Obfuscate

        public override bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
#if Obfuscator_Free
#else
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
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition))
                {
                    continue;
                }

                // Get the obfuscated name.
                String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition);

                // Notice: Namespace is mostly null!!
                /*if (String.IsNullOrEmpty(var_ObfuscatedName))
                {
                    continue;
                }*/

                // Get the original key.
                TypeKey var_OriginalKey = this.Step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(var_TypeDefinition);

                // Check if the original key is null.
                if (var_OriginalKey == default(TypeKey))
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Could not find the original type key for the type '{0}' while obfuscating the namespace name.", new TypeKey(var_TypeDefinition).ToString()));
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
                        var_ReferencePair.Key.GetElementType().Namespace = var_ObfuscatedName;

                        // Remove the reference.
                        this.MemberReferenceMapping.ReferenceList.RemoveAt(r);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Update Reference Type [" + var_ReferencePair.Value + "]");

                        continue;
                    }

                    r++;
                }

                // Rename MethodDefinition.
                var_TypeDefinition.Namespace = var_ObfuscatedName;

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Rename Type Namespace [" + var_OriginalKey.ToString() + "] to " + var_TypeDefinition.Namespace);
            }
#endif

            return true;
        }

#endregion
    }
}
