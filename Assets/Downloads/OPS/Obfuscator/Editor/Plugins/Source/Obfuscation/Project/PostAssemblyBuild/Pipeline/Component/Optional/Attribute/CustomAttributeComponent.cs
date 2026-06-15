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
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key.Helper;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Optional
{
    public class CustomAttributeComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Attribute - Settings";
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
                return "Manage here obfuscator attributes or custom attributes.";
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
                return "Manage here obfuscator attributes or custom attributes.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Custom_Attributes";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        private const String CAttributes_Behave_Like_DoNotRename_Array = "Attributes_Behave_Like_DoNotRename_Array";

        #endregion

        //Gui
        #region Gui

        /// <summary>
        /// Category this Component belongs too.
        /// </summary>
        public EObfuscatorCategory ObfuscatorCategory
        {
            get
            {
                return EObfuscatorCategory.Optional;
            }
        }

        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Custom DoNotRename Attributes.
            Row_Array var_CustomDoNotRenameRow = new Row_Array("Custom 'DoNotRename' Attributes: ", _ComponentSettings, CAttributes_Behave_Like_DoNotRename_Array);
            var_CustomDoNotRenameRow.Notification_Info = "Add here custom attributes that should behave like the obfuscator 'DoNotRename' attribute.";
            var_Content.AddRow(var_CustomDoNotRenameRow);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Analyse A
        #region Analyse A

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Skip Member
            this.OnAnalyse_A_Skip_Member(_AssemblyInfo);

            return true;
        }

        private void OnAnalyse_A_Skip_Member(AssemblyInfo _AssemblyInfo)
        {
            //Get Attribute Names.
            List<String> var_DoNotRenameLikeAttributeList = this.Step.Settings.Get_ComponentSettings_As_Array(this.SettingsKey, CAttributes_Behave_Like_DoNotRename_Array).ToList();

            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                //Check Type
                for (int a = 0; a < var_DoNotRenameLikeAttributeList.Count; a++)
                {
                    if (AttributeHelper.HasCustomAttribute(var_TypeDefinition, var_DoNotRenameLikeAttributeList[a]))
                    {
                        //Cause
                        String var_DoNotObuscateCause = "Because the type has the '" + var_DoNotRenameLikeAttributeList[a] + "' Attribute, which causes skipping by settings.";
                        this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);
                        this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                    }
                }

                //Check Method
                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    for (int a = 0; a < var_DoNotRenameLikeAttributeList.Count; a++)
                    {
                        if (AttributeHelper.HasCustomAttribute(var_MethodDefinition, var_DoNotRenameLikeAttributeList[a]))
                        {
                            //Cause
                            String var_DoNotObuscateCause = "Because the method has the '" + var_DoNotRenameLikeAttributeList[a] + "' Attribute, which causes skipping by settings.";
                            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_DoNotObuscateCause);
                        }
                    }
                }

                //Check Field
                foreach (FieldDefinition var_FieldDefinition in var_TypeDefinition.Fields)
                {
                    for (int a = 0; a < var_DoNotRenameLikeAttributeList.Count; a++)
                    {
                        if (AttributeHelper.HasCustomAttribute(var_FieldDefinition, var_DoNotRenameLikeAttributeList[a]))
                        {
                            //Cause
                            String var_DoNotObuscateCause = "Because the field has the '" + var_DoNotRenameLikeAttributeList[a] + "' Attribute, which causes skipping by settings.";
                            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition, var_DoNotObuscateCause);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Field [" + var_FieldDefinition.GetExtendedOriginalFullName(this.Step.GetCache<FieldCache>()) + "] " + var_DoNotObuscateCause);
                        }
                    }
                }

                //Check Property
                foreach (PropertyDefinition var_PropertyDefinition in var_TypeDefinition.Properties)
                {
                    for (int a = 0; a < var_DoNotRenameLikeAttributeList.Count; a++)
                    {
                        if (AttributeHelper.HasCustomAttribute(var_PropertyDefinition, var_DoNotRenameLikeAttributeList[a]))
                        {
                            //Cause
                            String var_DoNotObuscateCause = "Because the property has the '" + var_DoNotRenameLikeAttributeList[a] + "' Attribute, which causes skipping by settings.";
                            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Property, var_PropertyDefinition, var_DoNotObuscateCause);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Property [" + var_PropertyDefinition.GetExtendedOriginalFullName(this.Step.GetCache<PropertyCache>()) + "] " + var_DoNotObuscateCause);
                        }
                    }
                }

                //Check Event
                foreach (EventDefinition var_EventDefinition in var_TypeDefinition.Events)
                {
                    for (int a = 0; a < var_DoNotRenameLikeAttributeList.Count; a++)
                    {
                        if (AttributeHelper.HasCustomAttribute(var_EventDefinition, var_DoNotRenameLikeAttributeList[a]))
                        {
                            //Cause
                            String var_DoNotObuscateCause = "Because the event has the '" + var_DoNotRenameLikeAttributeList[a] + "' Attribute, which causes skipping by settings.";
                            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Event, var_EventDefinition, var_DoNotObuscateCause);

                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Event [" + var_EventDefinition.GetExtendedOriginalFullName(this.Step.GetCache<EventCache>()) + "] " + var_DoNotObuscateCause);
                        }
                    }
                }
            }
        }

        #endregion

        // Analyse B
        #region Analyse B

        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        public bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion
    }
}
