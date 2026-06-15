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

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Event
{
    public class EventObfuscationComponent : AMemberObfuscationComponent<EventReference, EventKey>
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
                return "Event - Obfuscation";
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
                return "Activate and manage the obfuscation of events.";
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
                return "Activate and manage the obfuscation of events.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Event";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        private const String CObfuscate_Events = "Obfuscate_Events";
        private const String CObfuscate_Event_Public = "Obfuscate_Event_Public";
        private const String CObfuscate_Event_Internal = "Obfuscate_Event_Internal";
        private const String CObfuscate_Event_Protected = "Obfuscate_Event_Protected";
        private const String CObfuscate_Event_Private = "Obfuscate_Event_Private";

        #endregion

        //Gui
        #region Gui

        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CObfuscate_Events);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //MultiRow
            Row_Multi_Boolean var_CustomObfuscateRow = new Row_Multi_Boolean("Obfuscate:", new string[] { "Internal", "Private", "Protected", "Public" }, _ComponentSettings,new string[] { CObfuscate_Event_Internal, CObfuscate_Event_Private, CObfuscate_Event_Protected, CObfuscate_Event_Public });
            var_CustomObfuscateRow.Notification_Info = "Accessibility based obfuscation.";
            var_Content.AddRow(var_CustomObfuscateRow);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Analyse A
        #region Analyse A

        // Find EventReference
        #region Find EventReference

        protected override List<EventReference> OnAnalyse_A_FindMemberReferences(AssemblyInfo _AssemblyInfo)
        {
            // Result
            List<EventReference> var_ReferenceList = new List<EventReference>();

            return var_ReferenceList;
        }

        #endregion

        // Find do not rename EventDefinition
        #region Find do not rename EventDefinition

        protected override bool OnAnalyse_A_FindMemberToNotRename(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Events...");

            // Dot not rename Properties
            this.OnAnalyse_A_FindEventDefinitionShouldBeNotRenamed(_AssemblyInfo);

            // Analyse Method Properties
            this.Analyse_AnalyseEventGroups(_AssemblyInfo);

            return true;
        }

        // Do not rename Event
        #region Do not rename Event

        /// <summary>
        /// Skip the _EventDefinition with _Cause.
        /// If Event belongs to a Field, skip Field too!
        /// </summary>
        /// <param name="_EventDefinition"></param>
        /// <param name="_Cause"></param>
        private void OnAnalyse_Helper_SkipEvent(EventDefinition _EventDefinition, String _Cause)
        {
            // Skip Property.
            this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Event, _EventDefinition, _Cause);

            // Skip Property methods.
            this.OnAnalyse_Helper_SkipEventMethods(_EventDefinition, _Cause);

            // Log
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Event [" + _EventDefinition.GetExtendedOriginalFullName(this.Step.GetCache<EventCache>()) + "] " + _Cause);

            // Skip Field if some belongs too!
            FieldDefinition var_BelongingFieldDefinition = TypeDefinitionHelper.GetFieldBelongingToEvent(this.Step.GetCache<FieldCache>(), _EventDefinition);
            if (var_BelongingFieldDefinition != null)
            {
                this.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, var_BelongingFieldDefinition, "Belonging event gets skipped: " + _Cause);

                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Field [" + var_BelongingFieldDefinition.GetExtendedOriginalFullName(this.Step.GetCache<FieldCache>()) + "] " + "Belonging event gets skipped: " + _Cause);
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
        /// Analyse the Properties to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindEventDefinitionShouldBeNotRenamed(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (EventDefinition var_EventDefinition in var_TypeDefinition.Events)
                {
                    //Check Component for Event
                    if (!this.Analyse_A_Helper_Compatibility_IsEventRenamingAllowed(_AssemblyInfo, var_EventDefinition, out List<String> var_CauseList))
                    {
                        //Cause
                        for (int c = 0; c < var_CauseList.Count; c++)
                        {
                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipEvent(var_EventDefinition, var_CauseList[c]);
                        }
                    }

                    if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Events))
                    {
                        // Cause
                        String var_DoNotObuscateCause = "Because of the global Event obfuscation settings.";

                        // Add do not rename.
                        this.OnAnalyse_Helper_SkipEvent(var_EventDefinition, var_DoNotObuscateCause);
                    }

                    //Check if Obfuscation allowed by Settings.
                    if (AccessibilityHelper.AccessibilityLevel_Event(var_EventDefinition) == EAccessibilityLevel.Public && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Public)
                        || AccessibilityHelper.AccessibilityLevel_Event(var_EventDefinition) == EAccessibilityLevel.Protected && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Protected)
                        || AccessibilityHelper.AccessibilityLevel_Event(var_EventDefinition) == EAccessibilityLevel.Protected_And_Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Protected) && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Event(var_EventDefinition) == EAccessibilityLevel.Protected_Or_Private && (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Private) || this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Protected))
                        || AccessibilityHelper.AccessibilityLevel_Event(var_EventDefinition) == EAccessibilityLevel.Internal && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Internal)
                        || AccessibilityHelper.AccessibilityLevel_Event(var_EventDefinition) == EAccessibilityLevel.Private && this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Event_Private))
                    {
                    }
                    else
                    {
                        // Cause
                        String var_DoNotObuscateCause = "Because of your defined Event obfuscation settings.";

                        // Add do not rename.
                        this.OnAnalyse_Helper_SkipEvent(var_EventDefinition, var_DoNotObuscateCause);
                    }
                }
            }
        }

        /// <summary>
        /// Check Compatibility Components.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_EventDefinition"></param>
        /// <param name="_CauseList"></param>
        /// <returns></returns>
        private bool Analyse_A_Helper_Compatibility_IsEventRenamingAllowed(AssemblyInfo _AssemblyInfo, EventDefinition _EventDefinition, out List<String> _CauseList)
        {
            bool var_Allowed = true;

            List<IEventCompatibility> var_CompatibilityComponentList = this.Step.GetCompatibilityComponents<IEventCompatibility>();

            _CauseList = new List<string>();
            for (int a = 0; a < var_CompatibilityComponentList.Count; a++)
            {
                if (!var_CompatibilityComponentList[a].IsEventRenamingAllowed(this.Step, _AssemblyInfo, _EventDefinition, out String var_Cause))
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
        private void Analyse_AnalyseEventGroups(AssemblyInfo _AssemblyInfo)
        {
            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (EventDefinition var_EventDefinition in var_TypeDefinition.Events)
                {
                    //Only Properties that will be obfuscated are of interest here.
                    if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition))
                    {
                        continue;
                    }

                    //Check the group!
                    MemberGroup var_Group = this.Step.GetCache<EventCache>().GetMemberGroup(var_EventDefinition);

                    if (var_Group != null)
                    {
                        //Check if some method is defined extern!         
                        if (var_Group.SomeGroupMemberDefinitionIsInExternalAssembly)
                        {
                            // Cause
                            String var_DoNotObuscateCause = "Event is in group that has external definition (is in some assembly that gets not obfuscated).";

                            // Add do not rename.
                            this.OnAnalyse_Helper_SkipEvent(var_EventDefinition, var_DoNotObuscateCause);
                        }

                        // Check if some method wont get obfsucated!
                        foreach (EventDefinition var_EventDefinitionInGroup in var_Group.GroupMemberDefinitionList)
                        {
                            if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinitionInGroup))
                            {
                                // Cause
                                String var_DoNotObuscateCause = "Some other Event in the Event group getting skipped.";

                                // Add do not rename.
                                this.OnAnalyse_Helper_SkipEvent(var_EventDefinition, var_DoNotObuscateCause);

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
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Events...");

            // Analyse Events again!
            this.Analyse_AnalyseEventGroups(_AssemblyInfo);

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
                foreach (EventDefinition var_EventDefinition in var_TypeDefinition.Events)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = null;

                    // Get Obfuscated Name from group!
                    MemberGroup var_Group = this.Step.GetCache<EventCache>().GetMemberGroup(var_EventDefinition);

                    if (var_Group != null)
                    {
                        var_ObfuscatedName = var_Group.GroupName;

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found group Event Name [" + var_EventDefinition.GetExtendedOriginalFullName(this.Step.GetCache<EventCache>()) + "] : " + var_ObfuscatedName);
                    }

                    // Is in no group or has no group name yet!
                    if (var_ObfuscatedName == null)
                    {
                        // Check if is stored in a loaded mapping or already set somewhere else.
                        var_ObfuscatedName = this.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition);
                        
                        if (var_ObfuscatedName != null)
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found looked up Event Name [" + var_EventDefinition.GetExtendedOriginalFullName(this.Step.GetCache<EventCache>()) + "] : " + var_ObfuscatedName);
                        }

                        // Check if having some belonging field.
                        if (var_ObfuscatedName == null)
                        {
                            FieldDefinition var_BelongingFieldDefinition = TypeDefinitionHelper.GetFieldBelongingToEvent(this.Step.GetCache<FieldCache>(), var_EventDefinition);
                            if (var_BelongingFieldDefinition != null)
                            {
                                // If belonging field gets obfuscated, use fields obfuscated name.
                                if (!this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Field, var_BelongingFieldDefinition))
                                {
                                    var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Field, var_BelongingFieldDefinition);

                                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found field belonging Event Name [" + var_EventDefinition.GetExtendedOriginalFullName(this.Step.GetCache<EventCache>()) + "] : " + var_ObfuscatedName);
                                }
                            }
                        }

                        // Generate new name.
                        if (var_ObfuscatedName == null)
                        {
                            var_ObfuscatedName = this.DataContainer.RenameManager.GetUniqueObfuscatedName(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition, int.MaxValue);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Found new Event Name [" + var_EventDefinition.GetExtendedOriginalFullName(this.Step.GetCache<EventCache>()) + "] : " + var_ObfuscatedName);
                        }
                    }

                    // Set Name for group!
                    if (var_Group != null)
                    {
                        var_Group.GroupName = var_ObfuscatedName;
                    }

                    this.DataContainer.RenameManager.AddObfuscated(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition, var_ObfuscatedName);
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
                foreach (EventDefinition var_EventDefinition in var_TypeDefinition.Events)
                {
                    if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition))
                    {
                        continue;
                    }

                    String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition);

                    if (String.IsNullOrEmpty(var_ObfuscatedName))
                    {
                        continue;
                    }

                    // Get Original Key
                    EventKey var_OriginalKey = this.Step.GetCache<EventCache>().GetOriginalMemberKeyByMemberDefinition(var_EventDefinition);

                    // Rename EventDefinition.
                    var_EventDefinition.Name = var_ObfuscatedName;

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Rename Event [" + var_OriginalKey.ToString() + "] to " + var_EventDefinition.Name);
                }
            }

            return true;
        }

        #endregion
    }
}
