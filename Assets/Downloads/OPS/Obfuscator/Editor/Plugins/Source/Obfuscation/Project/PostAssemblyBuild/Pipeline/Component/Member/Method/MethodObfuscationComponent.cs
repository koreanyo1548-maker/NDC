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

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Method
{
    public class MethodObfuscationComponent : AMemberObfuscationComponent<MethodReference, MethodKey>
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
                return "Method - Obfuscation";
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
                return "Activate and manage the obfuscation of methods.";
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
                return "Activate and manage the obfuscation of methods.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Method";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        private const String CObfuscate_Methods = "Obfuscate_Methods";
        private const String CObfuscate_Method_Public = "Obfuscate_Method_Public";
        private const String CObfuscate_Method_Internal = "Obfuscate_Method_Internal";
        private const String CObfuscate_Method_Protected = "Obfuscate_Method_Protected";
        private const String CObfuscate_Method_Private = "Obfuscate_Method_Private";

        public const String CObfuscate_Method_Unity = "Obfuscate_Method_Unity";

        #endregion

        //Gui
        #region Gui

        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CObfuscate_Methods);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //MultiRow
            Row_Multi_Boolean var_CustomObfuscateRow = new Row_Multi_Boolean("Obfuscate:", new string[] { "Internal", "Private", "Protected", "Public" }, _ComponentSettings, new string[] { CObfuscate_Method_Internal, CObfuscate_Method_Private, CObfuscate_Method_Protected, CObfuscate_Method_Public });
            var_CustomObfuscateRow.Notification_Info = "Accessibility based obfuscation.";
            var_Content.AddRow(var_CustomObfuscateRow);

            //Advanced
            Row_Section var_AdvancedRow = new Row_Section("Advanced");
            var_Content.AddRow(var_AdvancedRow);

            //Obfuscate Unity Methods
            Row_Boolean var_ObfuscateUnityRow = new Row_Boolean("Obfuscate Unity Methods:", _ComponentSettings, CObfuscate_Method_Unity);
            var_ObfuscateUnityRow.Notification_Info = "Adds obfuscation on simple unity methods like: Awake/Start/Update/...";
            var_ObfuscateUnityRow.Notification_Warning = "Currently disabled, getting a revision.";
            var_ObfuscateUnityRow.Enabled = false;
            var_Content.AddRow(var_ObfuscateUnityRow, true);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Analyse A
        #region Analyse A

        // Find MethodReference
        #region Find MethodReference

        protected override List<MethodReference> OnAnalyse_A_FindMemberReferences(AssemblyInfo _AssemblyInfo)
        {
            // Result
            List<MethodReference> var_ReferenceList = new List<MethodReference>();

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
        private List<MethodReference> OnAnalyse_A_FindAllModuleMethodReferences(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Find Method References in Modules...");

            // Result
            List<MethodReference> var_ReferenceList = new List<MethodReference>();

            // Analyse the modules GetMemberReferences
            for (int m = 0; m < _AssemblyInfo.AssemblyDefinition.Modules.Count; m++)
            {
                foreach (MemberReference var_Reference in _AssemblyInfo.AssemblyDefinition.Modules[m].GetMemberReferences())
                {
                    if (var_Reference == null)
                    {
                        continue;
                    }

                    if (var_Reference is MethodReference)
                    {
                        var_ReferenceList.Add((MethodReference) var_Reference);
                    }
                }
            }

            return var_ReferenceList;
        }

        /// <summary>
        /// Find the MethodReferences inside the Overrides and Instructions of _AssemblyInfo.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private List<MethodReference> OnAnalyse_A_FindAllInstructionMethodReferences(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Find Field References in Instructions...");

            // Result
            List<MethodReference> var_MethodReferenceList = new List<MethodReference>();

            //Analyse the methods references in the instructions.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (MethodDefinition method in var_TypeDefinition.Methods)
                {
                    foreach (MethodReference var_MethodReference in method.Overrides)
                    {
                        if (var_MethodReference == null)
                        {
                            continue;
                        }

                        if (!this.OnAnalyse_A_MethodIsOnlyReference(var_MethodReference))
                        {
                            continue;
                        }

                        var_MethodReferenceList.Add(var_MethodReference);
                    }

                    if (method.Body != null)
                    {
                        foreach (Instruction inst in method.Body.Instructions)
                        {
                            MemberReference var_MemberReference = inst.Operand as MemberReference;
                            if (var_MemberReference == null)
                            {
                                continue;
                            }

                            if (var_MemberReference is MethodReference && !(var_MemberReference is MethodDefinition))
                            {
                                MethodReference var_MethodReference = (MethodReference)var_MemberReference;

                                if (!this.OnAnalyse_A_MethodIsOnlyReference(var_MethodReference))
                                {
                                    continue;
                                }

                                var_MethodReferenceList.Add(var_MethodReference);
                            }
                        }
                    }
                }
            }

            return var_MethodReferenceList;
        }

        private bool OnAnalyse_A_MethodIsOnlyReference(MethodReference _MethodReference)
        {
            if (_MethodReference is MethodReference)
            {
                if (_MethodReference is MethodDefinition)
                {
                    return false;
                }

                if (_MethodReference is MethodSpecification)
                {
                    if (_MethodReference is GenericInstanceMethod)
                    {
                        return true;
                    }

                    return false;
                }

#pragma warning disable
                return !(_MethodReference is CallSite); // Note: Only valid for IMethodSignature
#pragma warning restore
            }

            return false;
        }

        #endregion

        // Find do not rename MethodDefinition
        #region Find do not rename MethodDefinition

        protected override bool OnAnalyse_A_FindMemberToNotRename(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Methods...");

            // Dot not rename Method
            this.OnAnalyse_A_FindMethodDefinitionShouldBeNotRenamed(_AssemblyInfo);

            // Analyse Method Groups
            this.Analyse_AnalyseMethodGroups(_AssemblyInfo);

            return true;
        }

        // Do not rename Method
        #region Do not rename Method

        /// <summary>
        /// Analyse the Methods to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindMethodDefinitionShouldBeNotRenamed(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    //Check Component for Method
                    if (!this.Analyse_A_Helper_Compatibility_IsMethodRenamingAllowed(_AssemblyInfo, var_MethodDefinition, out List<String> var_CauseList))
                    {
                        //Cause
                        for (int c = 0; c < var_CauseList.Count; c++)
                        {
                            // Add do not rename.
                            this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_CauseList[c]);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_CauseList[c]);
                        }
                    }

                    if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Methods))
                    {
                        //Cause
                        String var_DoNotObuscateCause = "Because of the global Method obfuscation settings.";
                        this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_DoNotObuscateCause);
                    }

                    //Check if Obfuscation allowed by Settings.
                    if (AccessibilityHelper.AccessibilityLevel_Method(var_MethodDefinition) == EAccessibilityLevel.Public && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Public)
                        || AccessibilityHelper.AccessibilityLevel_Method(var_MethodDefinition) == EAccessibilityLevel.Protected && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Protected)
                        || AccessibilityHelper.AccessibilityLevel_Method(var_MethodDefinition) == EAccessibilityLevel.Protected_And_Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Protected) && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Method(var_MethodDefinition) == EAccessibilityLevel.Protected_Or_Private && (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Private) || this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Protected))
                        || AccessibilityHelper.AccessibilityLevel_Method(var_MethodDefinition) == EAccessibilityLevel.Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Method(var_MethodDefinition) == EAccessibilityLevel.Private && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Private))
                    {
                    }
                    else
                    {
                        //Cause
                        String var_DoNotObuscateCause = "Because of your defined Method obfuscation settings.";
                        this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_DoNotObuscateCause);
                    }
                }
            }
        }

        /// <summary>
        /// Check Compatibility Components.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_MethodDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_IsMethodRenamingAllowed(AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition, out List<String> _CauseList)
        {
            bool var_Allowed = true;

            List<IMethodCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<IMethodCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (!var_CompatibilityComponentList[a].IsMethodRenamingAllowed(this.Step, _AssemblyInfo, _MethodDefinition, out String var_Cause))
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
        private void Analyse_AnalyseMethodGroups(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    //Only Methods that will be obfuscated are of interest here.
                    if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition))
                    {
                        continue;
                    }

                    //Check the group!
                    MemberGroup var_Group = this.Step.GetCache<MethodCache>().GetMemberGroup(var_MethodDefinition);

                    if (var_Group != null)
                    {
                        //Check if some method is defined extern!         
                        if (var_Group.SomeGroupMemberDefinitionIsInExternalAssembly)
                        {
                            //Cause
                            String var_DoNotObuscateCause = "Method is in group that has external definition (is in some assembly that gets not obfuscated).";
                            this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_DoNotObuscateCause);
                        }

                        // Check if some method wont get obfsucated!
                        foreach (MethodDefinition var_MethodDefinitionInGroup in var_Group.GroupMemberDefinitionList)
                        {
                            if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinitionInGroup))
                            {
                                // Cause
                                String var_DoNotObuscateCause = "Some other Method in the Method group getting skipped.";
                                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_DoNotObuscateCause);

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
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Methods...");

            // Analyse Method Groups again!
            this.Analyse_AnalyseMethodGroups(_AssemblyInfo);

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
                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = null;

                    //Get Obfuscated Name from group!
                    MemberGroup var_Group = this.Step.GetCache<MethodCache>().GetMemberGroup(var_MethodDefinition);

                    if (var_Group != null)
                    {
                        var_ObfuscatedName = var_Group.GroupName;
                    }

                    //Is in no group or has no group name yet!
                    if (var_ObfuscatedName == null)
                    {
                        //Check if is stored in a loaded mapping or already set somewhere else.
                        var_ObfuscatedName = this.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition);

                        //Generate new name or use stored.
                        if (var_ObfuscatedName == null)
                        {
                            var_ObfuscatedName = this.DataContainer.RenameManager.GetUniqueObfuscatedName(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, int.MaxValue);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found new Method Name [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] : " + var_ObfuscatedName);
                        }
                        else
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found looked up Method Name [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] : " + var_ObfuscatedName);
                        }
                    }
                    else
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found group Method Name [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] : " + var_ObfuscatedName);
                    }

                    //Set Name for group!
                    if (var_Group != null)
                    {
                        var_Group.GroupName = var_ObfuscatedName;
                    }

                    this.DataContainer.RenameManager.AddObfuscated(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_ObfuscatedName);
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
                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition);

                    if (String.IsNullOrEmpty(var_ObfuscatedName))
                    {
                        continue;
                    }

                    // Get Original Key
                    MethodKey var_OriginalKey = this.Step.GetCache<MethodCache>().GetOriginalMemberKeyByMemberDefinition(var_MethodDefinition);

                    //Go through all references.
                    for (int r = 0; r < this.MemberReferenceMapping.ReferenceList.Count;)
                    {
                        var var_ReferencePair = this.MemberReferenceMapping.ReferenceList[r];

                        //Check if they match.
                        if (var_ReferencePair.Value == var_OriginalKey)
                        {
                            //Match!

                            GenericInstanceMethod var_GenericMethod = var_ReferencePair.Key as GenericInstanceMethod;
                            if (var_GenericMethod == null)
                            {
                                //Set name
                                var_ReferencePair.Key.Name = var_ObfuscatedName;
                            }
                            else
                            {
                                //Set name
                                var_GenericMethod.ElementMethod.Name = var_ObfuscatedName;
                            }

                            //Remove the reference.
                            this.MemberReferenceMapping.ReferenceList.RemoveAt(r);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Update Reference Method [" + var_ReferencePair.Value + "]");

                            continue;
                        }

                        r++;
                    }

                    //Rename MethodDefinition.
                    var_MethodDefinition.Name = var_ObfuscatedName;

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Rename Method [" + var_OriginalKey.ToString() + "] to " + var_MethodDefinition.Name); 
                }
            }

            return true;
        }

        #endregion
    }
}
