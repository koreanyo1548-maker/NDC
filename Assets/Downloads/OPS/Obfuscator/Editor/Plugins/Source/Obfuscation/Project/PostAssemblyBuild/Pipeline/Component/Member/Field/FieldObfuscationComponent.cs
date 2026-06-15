using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Pipeling
using OPS.Editor.Project.Pipeline;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Accessibility;
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

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Field
{
    public class FieldObfuscationComponent : AMemberObfuscationComponent<FieldReference, FieldKey>
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
                return "Field - Obfuscation";
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
                return "Activate and manage the obfuscation of fields.";
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
                return "Activate and manage the obfuscation of fields.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Field";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        private const String CObfuscate_Fields = "Obfuscate_Fields";
        private const String CObfuscate_Field_Public = "Obfuscate_Field_Public";
        private const String CObfuscate_Field_Internal = "Obfuscate_Field_Internal";
        private const String CObfuscate_Field_Protected = "Obfuscate_Field_Protected";
        private const String CObfuscate_Field_Private = "Obfuscate_Field_Private";

        public const String CObfuscate_Field_Serializable = "Obfuscate_Field_Serializable";
        public const String CObfuscate_Field_Public_Unity = "Obfuscate_Field_Public_Unity";

        public const String CObfuscate_Field_Enums = "Obfuscate_Field_Enums";

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
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CObfuscate_Fields);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // MultiRow
            Row_Multi_Boolean var_CustomObfuscateRow = new Row_Multi_Boolean("Obfuscate:", new string[] { "Internal", "Private", "Protected", "Public" }, _ComponentSettings, new string[] { CObfuscate_Field_Internal, CObfuscate_Field_Private, CObfuscate_Field_Protected, CObfuscate_Field_Public });
            var_CustomObfuscateRow.Notification_Info = "Accessibility based obfuscation.";
            var_Content.AddRow(var_CustomObfuscateRow);

            // Advanced
            Row_Section var_AdvancedRow = new Row_Section("Advanced");
            var_Content.AddRow(var_AdvancedRow);

            // Obfuscate Serializable
            Row_Boolean var_ObfuscateSerializeableRow = new Row_Boolean("Obfuscate Serializeable Fields:", _ComponentSettings, CObfuscate_Field_Serializable);
            var_ObfuscateSerializeableRow.Notification_Info = "Activate this setting to obfuscate fields that are marked with the [SerializeField] attribute.";
            var_ObfuscateSerializeableRow.Notification_Warning = "If you want to obfuscate serializable fields, there are two prerequisites: \r\n- First, these fields must not be saved in the editor, but only at runtime. Otherwise, the data saved in the editor will have different names than the build runtime fields, which means that the saved data cannot be resolved, as Unity saves serializable fields by name.\r\n- Secondly, you need to enable the 'Rename Mapping' setting in the optional tab. This option saves and loads a mapping of the original classes/methods/fields/... and the matching obfuscated name. This way the name will match between the different builds.";
            var_Content.AddRow(var_ObfuscateSerializeableRow, true);

            // Obfuscate Unity Public Fields
            Row_Boolean var_ObfuscateUnityPublicFieldRow = new Row_Boolean("Obfuscate 'Unity' Public Fields:", _ComponentSettings, CObfuscate_Field_Public_Unity);
            var_ObfuscateUnityPublicFieldRow.Notification_Info = "Activate this setting to obfuscate public fields in monobehaviour/serializeable/networkbehaviour subclasses.";
            var_ObfuscateUnityPublicFieldRow.Notification_Warning = "If you want to obfuscate those public fields, there are two prerequisites: \r\n- First, these fields must not be saved in the editor, but only at runtime. Otherwise, the data saved in the editor will have different names than the build runtime fields, which means that the saved data cannot be resolved, as Unity saves serializable fields by name.\r\n- Secondly, you need to enable the 'Rename Mapping' setting in the optional tab. This option saves and loads a mapping of the original classes/methods/fields/... and the matching obfuscated name. This way the name will match between the different builds.";
            var_Content.AddRow(var_ObfuscateUnityPublicFieldRow, true);

            // Obfuscate Enum
            Row_Boolean var_EnumFieldRow = new Row_Boolean("Obfuscate Enum Values:", _ComponentSettings, CObfuscate_Field_Enums);
            var_EnumFieldRow.Notification_Info = "Activate this setting to obfuscate enum values.";
            var_Content.AddRow(var_EnumFieldRow);

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Analyse A
        #region Analyse A

        // Find FieldReference
        #region Find FieldReference

        protected override List<FieldReference> OnAnalyse_A_FindMemberReferences(AssemblyInfo _AssemblyInfo)
        {
            // Result
            List<FieldReference> var_ReferenceList = new List<FieldReference>();

            // Module References
            var_ReferenceList.AddRange(this.OnAnalyse_A_FindAllModuleMethodReferences(_AssemblyInfo));

            // Instruction References
            var_ReferenceList.AddRange(this.OnAnalyse_A_FindAllInstructionMethodReferences(_AssemblyInfo));

            return var_ReferenceList;
        }

        /// <summary>
        /// Add the 'GetMemberReferences' in all the Modules of _AssemblyInfo.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private List<FieldReference> OnAnalyse_A_FindAllModuleMethodReferences(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Find Field References in Modules...");

            // Result
            List<FieldReference> var_ReferenceList = new List<FieldReference>();

            // Analyse the modules GetMemberReferences
            for (int m = 0; m < _AssemblyInfo.AssemblyDefinition.Modules.Count; m++)
            {
                foreach (MemberReference var_Reference in _AssemblyInfo.AssemblyDefinition.Modules[m].GetMemberReferences())
                {
                    if (var_Reference == null)
                    {
                        continue;
                    }

                    if (var_Reference is FieldReference)
                    {
                        var_ReferenceList.Add((FieldReference)var_Reference);
                    }
                }
            }

            return var_ReferenceList;
        }

        /// <summary>
        /// Find the FieldReference inside the Instructions of _AssemblyInfo.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private List<FieldReference> OnAnalyse_A_FindAllInstructionMethodReferences(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Find Field References in Instructions...");

            // Result
            List<FieldReference> var_ReferenceList = new List<FieldReference>();

            //Analyse the methods references in the instructions.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (MethodDefinition method in var_TypeDefinition.Methods)
                {
                    if (method.Body != null)
                    {
                        foreach (Instruction inst in method.Body.Instructions)
                        {
                            MemberReference var_MemberReference = inst.Operand as MemberReference;
                            if (var_MemberReference == null)
                            {
                                continue;
                            }

                            if (var_MemberReference is FieldReference && !(var_MemberReference is FieldDefinition))
                            {
                                FieldReference var_FieldReference = (FieldReference)var_MemberReference;

                                var_ReferenceList.Add(var_FieldReference);
                            }
                        }
                    }
                }
            }

            return var_ReferenceList;
        }

        #endregion

        // Find do not rename FieldDefinition
        #region Find do not rename FieldDefinition

        protected override bool OnAnalyse_A_FindMemberToNotRename(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Fields...");

            // Dot not rename Method
            this.OnAnalyse_A_FindFieldDefinitionShouldBeNotRenamed(_AssemblyInfo);

            return true;
        }

        // Do not rename Field
        #region Do not rename Field

        /// <summary>
        /// Skip _FieldDefinition with _Cause.
        /// If Field belongs to a Property, skip Property too!
        /// </summary>
        /// <param name="_FieldDefinition"></param>
        /// <param name="_Cause"></param>
        private void OnAnalyse_Helper_SkipField(FieldDefinition _FieldDefinition, String _Cause)
        {
            // Skip Field
            this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, _FieldDefinition, _Cause);

            // Log
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Field [" + _FieldDefinition.GetExtendedOriginalFullName(this.Step.GetCache<FieldCache>()) + "] " + _Cause);

            // Skip Property if some belongs too!
            PropertyDefinition var_BelongingPropertyDefinition = TypeDefinitionHelper.GetPropertyBelongingToField(this.Step.GetCache<PropertyCache>(), _FieldDefinition);
            if (var_BelongingPropertyDefinition != null)
            {
                // Skip Property
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Property, var_BelongingPropertyDefinition, "Belonging field gets skipped: " + _Cause);

                // Skip Property Methods.
                this.OnAnalyse_Helper_SkipPropertyMethods(var_BelongingPropertyDefinition, _Cause);

                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Property [" + var_BelongingPropertyDefinition.GetExtendedOriginalFullName(this.Step.GetCache<PropertyCache>()) + "] " + "Belonging field gets skipped: " + _Cause);
            }

            // Skip Event if some belongs too!
            EventDefinition var_BelongingEventDefinition = TypeDefinitionHelper.GetEventBelongingToField(this.Step.GetCache<EventCache>(), _FieldDefinition);
            if (var_BelongingEventDefinition != null)
            {
                // Skip Property
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Event, var_BelongingEventDefinition, "Belonging field gets skipped: " + _Cause);

                // Skip Property Methods.
                this.OnAnalyse_Helper_SkipEventMethods(var_BelongingEventDefinition, _Cause);

                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Event [" + var_BelongingEventDefinition.GetExtendedOriginalFullName(this.Step.GetCache<EventCache>()) + "] " + "Belonging field gets skipped: " + _Cause);
            }
        }

        /// <summary>
        /// Skip the _PropertyDefinitions get and set methods with _Cause.
        /// </summary>
        /// <param name="_PropertyDefinition"></param>
        /// <param name="_Cause"></param>
        private void OnAnalyse_Helper_SkipPropertyMethods(PropertyDefinition _PropertyDefinition, String _Cause)
        {
            // Skip Property Methods
            if (_PropertyDefinition.GetMethod != null)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, _PropertyDefinition.GetMethod, "Belonging property gets skipped: " + _Cause);
            }
            if (_PropertyDefinition.SetMethod != null)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, _PropertyDefinition.SetMethod, "Belonging property gets skipped: " + _Cause);
            }
        }

        /// <summary>
        /// Skip the _EventDefinition add and remove methods with _Cause.
        /// </summary>
        /// <param name="_EventDefinition"></param>
        /// <param name="_Cause"></param>
        private void OnAnalyse_Helper_SkipEventMethods(EventDefinition _EventDefinition, String _Cause)
        {
            // Skip Event Methods
            if (_EventDefinition.AddMethod != null)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, _EventDefinition.AddMethod, "Belonging property gets skipped: " + _Cause);
            }
            if (_EventDefinition.RemoveMethod != null)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, _EventDefinition.RemoveMethod, "Belonging property gets skipped: " + _Cause);
            }
        }

        /// <summary>
        /// Analyse the Fields to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindFieldDefinitionShouldBeNotRenamed(AssemblyInfo _AssemblyInfo)
        {
            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (FieldDefinition var_FieldDefinition in var_TypeDefinition.Fields)
                {
                    // Check Component for Field
                    if (!this.Analyse_A_Helper_Compatibility_IsFieldRenamingAllowed(_AssemblyInfo, var_FieldDefinition, out List<String> var_CauseList))
                    {
                        //Cause
                        for (int c = 0; c < var_CauseList.Count; c++)
                        {
                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipField(var_FieldDefinition, var_CauseList[c]);
                        }
                    }
                    // Seperate Enum and Fields
                    if (var_FieldDefinition.DeclaringType.IsEnum)
                    {
                        // Is enum field

                        // Check Enum Setting
                        if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Enums))
                        {
                            //Cause
                            String var_DoNotObuscateCause = "Because of the 'Obfuscate enum values' setting.";

                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipField(var_FieldDefinition, var_DoNotObuscateCause);
                        }
                    }
                    else
                    {
                        if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Fields))
                        {
                            // Cause
                            String var_DoNotObuscateCause = "Because of the global Field obfuscation settings.";

                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipField(var_FieldDefinition, var_DoNotObuscateCause);
                        }

                        //Check if Obfuscation allowed by Settings.
                        if (AccessibilityHelper.AccessibilityLevel_Field(var_FieldDefinition) == EAccessibilityLevel.Public && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Public)
                        || AccessibilityHelper.AccessibilityLevel_Field(var_FieldDefinition) == EAccessibilityLevel.Protected && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Protected)
                        || AccessibilityHelper.AccessibilityLevel_Field(var_FieldDefinition) == EAccessibilityLevel.Protected_And_Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Protected) && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Field(var_FieldDefinition) == EAccessibilityLevel.Protected_Or_Private && (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Private) || this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Protected))
                        || AccessibilityHelper.AccessibilityLevel_Field(var_FieldDefinition) == EAccessibilityLevel.Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Field(var_FieldDefinition) == EAccessibilityLevel.Private && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Field_Private))
                        {
                        }
                        else
                        {
                            // Cause
                            String var_DoNotObuscateCause = "Because of your defined Field obfuscation settings.";

                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipField(var_FieldDefinition, var_DoNotObuscateCause);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check Compatibility Components.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_FieldDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_IsFieldRenamingAllowed(AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, out List<String> _CauseList)
        {
            bool var_Allowed = true;

            List<IFieldCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<IFieldCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (!var_CompatibilityComponentList[a].IsFieldRenamingAllowed(this.Step, _AssemblyInfo, _FieldDefinition, out String var_Cause))
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
                foreach (FieldDefinition var_FieldDefinition in var_TypeDefinition.Fields)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = null;

                    //Check if is stored in a loaded mapping or already set somewhere else.
                    var_ObfuscatedName = this.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition);

                    //Generate new name or use stored.
                    if (var_ObfuscatedName == null)
                    {
                        var_ObfuscatedName = this.DataContainer.RenameManager.GetUniqueObfuscatedName(Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition, int.MaxValue);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found new Field Name [" + var_FieldDefinition.GetExtendedOriginalFullName(this.Step.GetCache<FieldCache>()) + "] : " + var_ObfuscatedName);
                    }
                    else
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found looked up Field Name [" + var_FieldDefinition.GetExtendedOriginalFullName(this.Step.GetCache<FieldCache>()) + "] : " + var_ObfuscatedName);
                    }

                    // Is a property belonging field.
                    if (var_FieldDefinition.Name.EndsWith("k__BackingField") && !var_ObfuscatedName.EndsWith("k__BackingField"))
                    {
                        var_ObfuscatedName = "<" + var_ObfuscatedName + ">" + "k__BackingField";
                    }

                    this.DataContainer.RenameManager.AddObfuscated(Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition, var_ObfuscatedName);
                }
            }

            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        public override bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (FieldDefinition var_FieldDefinition in var_TypeDefinition.Fields)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition);

                    if (String.IsNullOrEmpty(var_ObfuscatedName))
                    {
                        continue;
                    }

                    // Get Original Key
                    FieldKey var_OriginalKey = this.Step.GetCache<FieldCache>().GetOriginalMemberKeyByMemberDefinition(var_FieldDefinition);

                    //Go through all references.
                    for (int r = 0; r < this.MemberReferenceMapping.ReferenceList.Count;)
                    {
                        var var_ReferencePair = this.MemberReferenceMapping.ReferenceList[r];

                        //Check if they match.
                        if (var_ReferencePair.Value == var_OriginalKey)
                        {
                            //Match!

                            //Set name
                            var_ReferencePair.Key.Name = var_ObfuscatedName;

                            //Remove the reference.
                            this.MemberReferenceMapping.ReferenceList.RemoveAt(r);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Update Reference Field [" + var_ReferencePair.Value + "]");

                            continue;
                        }

                        r++;
                    }

                    //Rename FieldDefinition.
                    var_FieldDefinition.Name = var_ObfuscatedName;

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Rename Field [" + var_OriginalKey.ToString() + "] to " + var_FieldDefinition.Name);
                }
            }

            return true;
        }

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        public override bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Obfuscate custom attribute names here, because only here, all fields got renamed!
            this.OnPostObfuscate_Obfuscate_CustomAttributeNames(_AssemblyInfo);

            return true;
        }

        private bool OnPostObfuscate_Obfuscate_CustomAttributeNames(AssemblyInfo _AssemblyInfo)
        {
            // Assembly Attributes
            if (!this.OnPostObfuscate_Obfuscate_CustomAttributeNames(_AssemblyInfo.AssemblyDefinition))
            {
                return false;
            }

            // Type Attributes
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                if (!this.OnPostObfuscate_Obfuscate_CustomAttributeNames(var_TypeDefinition))
                {
                    return false;
                }

                // Method
                foreach (IMemberDefinition var_MemberDefinition in var_TypeDefinition.Methods)
                {
                    if (!this.OnPostObfuscate_Obfuscate_CustomAttributeNames(var_MemberDefinition))
                    {
                        return false;
                    }
                }

                // Field
                foreach (IMemberDefinition var_MemberDefinition in var_TypeDefinition.Fields)
                {
                    if (!this.OnPostObfuscate_Obfuscate_CustomAttributeNames(var_MemberDefinition))
                    {
                        return false;
                    }
                }

                // Property
                foreach (IMemberDefinition var_MemberDefinition in var_TypeDefinition.Properties)
                {
                    if (!this.OnPostObfuscate_Obfuscate_CustomAttributeNames(var_MemberDefinition))
                    {
                        return false;
                    }
                }

                // Event
                foreach (IMemberDefinition var_MemberDefinition in var_TypeDefinition.Events)
                {
                    if (!this.OnPostObfuscate_Obfuscate_CustomAttributeNames(var_MemberDefinition))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool OnPostObfuscate_Obfuscate_CustomAttributeNames(ICustomAttributeProvider _CustomAttributeProvider)
        {
            // Field
            foreach (KeyValuePair<CustomAttribute, CustomAttributeNamedArgument> var_Pair in OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper.AttributeHelper.Iterate_CustomAttributeNamedArguments(_CustomAttributeProvider, true))
            {
                if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.MemberReferenceHelper.TryToResolve<TypeDefinition>(var_Pair.Key.AttributeType, out TypeDefinition var_TypeDefinition))
                {
                    FieldDefinition var_FieldDefinition = TypeDefinitionHelper.FindField(var_TypeDefinition, this.Step.GetCache<FieldCache>(), var_Pair.Value.Name);
                    if (var_FieldDefinition != null)
                    {
                        var_Pair.Value.Name = var_FieldDefinition.Name;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
