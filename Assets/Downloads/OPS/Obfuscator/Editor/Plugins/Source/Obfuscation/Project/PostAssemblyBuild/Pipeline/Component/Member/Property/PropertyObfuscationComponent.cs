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

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Property
{
    public class PropertyObfuscationComponent : AMemberObfuscationComponent<PropertyReference, PropertyKey>
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
                return "Property - Obfuscation";
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
                return "Activate and manage the obfuscation of properties.";
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
                return "Activate and manage the obfuscation of properties.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Property";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        private const String CObfuscate_Properties = "Obfuscate_Properties";
        private const String CObfuscate_Property_Public = "Obfuscate_Property_Public";
        private const String CObfuscate_Property_Internal = "Obfuscate_Property_Internal";
        private const String CObfuscate_Property_Protected = "Obfuscate_Property_Protected";
        private const String CObfuscate_Property_Private = "Obfuscate_Property_Private";

        #endregion

        //Gui
        #region Gui

        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CObfuscate_Properties);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //MultiRow
            Row_Multi_Boolean var_CustomObfuscateRow = new Row_Multi_Boolean("Obfuscate:", new string[] { "Internal", "Private", "Protected", "Public" }, _ComponentSettings, new string[] { CObfuscate_Property_Internal, CObfuscate_Property_Private, CObfuscate_Property_Protected, CObfuscate_Property_Public });
            var_CustomObfuscateRow.Notification_Info = "Accessibility based obfuscation.";
            var_Content.AddRow(var_CustomObfuscateRow);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Analyse A
        #region Analyse A

        // Find PropertyReference
        #region Find PropertyReference

        protected override List<PropertyReference> OnAnalyse_A_FindMemberReferences(AssemblyInfo _AssemblyInfo)
        {
            // Result
            List<PropertyReference> var_ReferenceList = new List<PropertyReference>();

            return var_ReferenceList;
        }

        #endregion

        // Find do not rename PropertyDefinition
        #region Find do not rename PropertyDefinition

        protected override bool OnAnalyse_A_FindMemberToNotRename(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Properties...");

            // Dot not rename Properties
            this.OnAnalyse_A_FindPropertyDefinitionShouldBeNotRenamed(_AssemblyInfo);

            // Analyse Method Properties
            this.Analyse_AnalysePropertyGroups(_AssemblyInfo);

            return true;
        }

        // Do not rename Property
        #region Do not rename Property

        /// <summary>
        /// Skip the _PropertyDefinition with _Cause.
        /// If Property belongs to a Field, skip Field too!
        /// </summary>
        /// <param name="_PropertyDefinition"></param>
        /// <param name="_Cause"></param>
        private void OnAnalyse_Helper_SkipProperty(PropertyDefinition _PropertyDefinition, String _Cause)
        {
            // Skip Property.
            this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Property, _PropertyDefinition, _Cause);

            // Skip Property methods.
            this.OnAnalyse_Helper_SkipPropertyMethods(_PropertyDefinition, _Cause);

            // Log
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Property [" + _PropertyDefinition.GetExtendedOriginalFullName(this.Step.GetCache<PropertyCache>()) + "] " + _Cause);

            // Skip Field if some belongs too!
            FieldDefinition var_BelongingFieldDefinition = TypeDefinitionHelper.GetFieldBelongingToProperty(this.Step.GetCache<FieldCache>(), _PropertyDefinition);
            if (var_BelongingFieldDefinition != null)
            { 
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, var_BelongingFieldDefinition, "Belonging property gets skipped: " + _Cause);

                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Field [" + var_BelongingFieldDefinition.GetExtendedOriginalFullName(this.Step.GetCache<FieldCache>()) + "] " + "Belonging property gets skipped: " + _Cause);
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
        /// Analyse the Properties to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindPropertyDefinitionShouldBeNotRenamed(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (PropertyDefinition var_PropertyDefinition in var_TypeDefinition.Properties)
                {
                    //Check Component for Method
                    if (!this.Analyse_A_Helper_Compatibility_IsPropertyRenamingAllowed(_AssemblyInfo, var_PropertyDefinition, out List<String> var_CauseList))
                    {
                        //Cause
                        for (int c = 0; c < var_CauseList.Count; c++)
                        {
                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipProperty(var_PropertyDefinition, var_CauseList[c]);
                        }
                    }

                    if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Properties))
                    {
                        // Cause
                        String var_DoNotObuscateCause = "Because of the global Method obfuscation settings.";

                        // Add do not rename.
                        this.OnAnalyse_Helper_SkipProperty(var_PropertyDefinition, var_DoNotObuscateCause);
                    }

                    //Check if Obfuscation allowed by Settings.
                    if (AccessibilityHelper.AccessibilityLevel_Property(var_PropertyDefinition) == EAccessibilityLevel.Public && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Public)
                        || AccessibilityHelper.AccessibilityLevel_Property(var_PropertyDefinition) == EAccessibilityLevel.Protected && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Protected)
                        || AccessibilityHelper.AccessibilityLevel_Property(var_PropertyDefinition) == EAccessibilityLevel.Protected_And_Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Protected) && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Property(var_PropertyDefinition) == EAccessibilityLevel.Protected_Or_Private && (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Private) || this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Protected))
                        || AccessibilityHelper.AccessibilityLevel_Property(var_PropertyDefinition) == EAccessibilityLevel.Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Property(var_PropertyDefinition) == EAccessibilityLevel.Private && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Property_Private))
                    {
                    }
                    else
                    {
                        // Cause
                        String var_DoNotObuscateCause = "Because of your defined Property obfuscation settings.";

                        // Add do not rename.
                        this.OnAnalyse_Helper_SkipProperty(var_PropertyDefinition, var_DoNotObuscateCause);
                    }
                }
            }
        }

        /// <summary>
        /// Check Compatibility Components.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_PropertyDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_IsPropertyRenamingAllowed(AssemblyInfo _AssemblyInfo, PropertyDefinition _PropertyDefinition, out List<String> _CauseList)
        {
            bool var_Allowed = true;

            List<IPropertyCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<IPropertyCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (!var_CompatibilityComponentList[a].IsPropertyRenamingAllowed(this.Step, _AssemblyInfo, _PropertyDefinition, out String var_Cause))
                {
                    _CauseList.Add("Because of compatibility component: " + var_CompatibilityComponentList[a].Name + " : " + var_Cause);
                    var_Allowed = false;
                }
            }

            return var_Allowed;
        }

        #endregion

        // Do not rename Group
        #region Do not rename Group

        /// <summary>
        /// Analyse the Method groups.
        /// And check if some other Method in group getting skipped.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void Analyse_AnalysePropertyGroups(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (PropertyDefinition var_PropertyDefinition in var_TypeDefinition.Properties)
                {
                    //Only Properties that will be obfuscated are of interest here.
                    if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition))
                    {
                        continue;
                    }

                    //Check the group!
                    MemberGroup var_Group = this.Step.GetCache<PropertyCache>().GetMemberGroup(var_PropertyDefinition);

                    if (var_Group != null)
                    {
                        //Check if some method is defined extern!         
                        if (var_Group.SomeGroupMemberDefinitionIsInExternalAssembly)
                        {
                            // Cause
                            String var_DoNotObuscateCause = "Property is in group that has external definition (is in some assembly that gets not obfuscated).";

                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipProperty(var_PropertyDefinition, var_DoNotObuscateCause);
                        }

                        // Check if some method wont get obfsucated!
                        foreach (PropertyDefinition var_PropertyDefinitionInGroup in var_Group.GroupMemberDefinitionList)
                        {
                            if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinitionInGroup))
                            {
                                // Cause
                                String var_DoNotObuscateCause = "Some other Property in the Property group getting skipped.";

                                // Add do not rename.
                                this.OnAnalyse_Helper_SkipProperty(var_PropertyDefinition, var_DoNotObuscateCause);

                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion

        // Analyse B
        #region Analyse B

        public override bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Properties...");

            // Analyse Groups again!
            this.Analyse_AnalysePropertyGroups(_AssemblyInfo);

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
            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (PropertyDefinition var_PropertyDefinition in var_TypeDefinition.Properties)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = null;

                    // Get Obfuscated Name from group!
                    MemberGroup var_Group = this.Step.GetCache<PropertyCache>().GetMemberGroup(var_PropertyDefinition);

                    if (var_Group != null)
                    {
                        var_ObfuscatedName = var_Group.GroupName;

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found group Property Name [" + var_PropertyDefinition.GetExtendedOriginalFullName(this.Step.GetCache<PropertyCache>()) + "] : " + var_ObfuscatedName);
                    }

                    // Is in no group or has no group name yet!
                    if (var_ObfuscatedName == null)
                    {
                        // Check if is stored in a loaded mapping or already set somewhere else.
                        var_ObfuscatedName = this.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition);

                        if(var_ObfuscatedName != null)
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found looked up Property Name [" + var_PropertyDefinition.GetExtendedOriginalFullName(this.Step.GetCache<PropertyCache>()) + "] : " + var_ObfuscatedName);
                        }

                        // Check if having some belonging field.
                        if (var_ObfuscatedName == null)
                        {
                            FieldDefinition var_BelongingFieldDefinition = TypeDefinitionHelper.GetFieldBelongingToProperty(this.Step.GetCache<FieldCache>(), var_PropertyDefinition);
                            if (var_BelongingFieldDefinition != null)
                            {
                                // If belonging field gets obfuscated, use fields obfuscated name.
                                if (!this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, var_BelongingFieldDefinition))
                                {
                                    var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Field, var_BelongingFieldDefinition);

                                    if (var_ObfuscatedName.Contains("k__BackingField"))
                                    {
                                        int var_Index = var_ObfuscatedName.IndexOf('>');
                                        if (var_Index != -1 && var_Index >= 1)
                                        {
                                            var_ObfuscatedName = var_ObfuscatedName.Substring(1, var_Index - 1);
                                        }
                                    }

                                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found field belonging Property Name [" + var_PropertyDefinition.GetExtendedOriginalFullName(this.Step.GetCache<PropertyCache>()) + "] : " + var_ObfuscatedName);
                                }
                            }
                        }

                        // Generate new name.
                        if (var_ObfuscatedName == null)
                        {
                            var_ObfuscatedName = this.DataContainer.RenameManager.GetUniqueObfuscatedName(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition, int.MaxValue);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found new Property Name [" + var_PropertyDefinition.GetExtendedOriginalFullName(this.Step.GetCache<PropertyCache>()) + "] : " + var_ObfuscatedName);
                        }
                    }

                    // Set Name for group!
                    if (var_Group != null)
                    {
                        var_Group.GroupName = var_ObfuscatedName;
                    }

                    this.DataContainer.RenameManager.AddObfuscated(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition, var_ObfuscatedName);
                }
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
                foreach (PropertyDefinition var_PropertyDefinition in var_TypeDefinition.Properties)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition);

                    if (String.IsNullOrEmpty(var_ObfuscatedName))
                    {
                        continue;
                    }

                    // Get Original Key
                    PropertyKey var_OriginalKey = this.Step.GetCache<PropertyCache>().GetOriginalMemberKeyByMemberDefinition(var_PropertyDefinition);

                    // Rename PropertyDefinition.
                    var_PropertyDefinition.Name = var_ObfuscatedName;

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Rename Property [" + var_OriginalKey.ToString() + "] to " + var_PropertyDefinition.Name);
                }
            }

            return true;
        }

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        public override bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Obfuscate custom attribute names here, because only here, all properties got renamed!
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
            // Property
            foreach (KeyValuePair<CustomAttribute, CustomAttributeNamedArgument> var_Pair in OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper.AttributeHelper.Iterate_CustomAttributeNamedArguments(_CustomAttributeProvider, false))
            {
                if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.MemberReferenceHelper.TryToResolve<TypeDefinition>(var_Pair.Key.AttributeType, out TypeDefinition var_TypeDefinition))
                {
                    PropertyDefinition var_PropertyDefinition = TypeDefinitionHelper.FindProperty(var_TypeDefinition, this.Step.GetCache<PropertyCache>(), var_Pair.Value.Name);
                    if (var_PropertyDefinition != null)
                    {
                        var_Pair.Value.Name = var_PropertyDefinition.Name;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
