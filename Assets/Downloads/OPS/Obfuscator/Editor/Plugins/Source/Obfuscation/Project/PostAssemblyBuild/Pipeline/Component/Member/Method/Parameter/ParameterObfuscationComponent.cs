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
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Method
{
    public class ParameterObfuscationComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Parameter - Obfuscation";
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
                return "Manage the obfuscation of parameter.";
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
                return "Manage the obfuscation of parameter.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Parameter";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        private const String CObfuscate_Method_Parameter = "Obfuscate_Method_Parameter";
        private const String CObfuscate_Class_Parameter = "Obfuscate_Class_Parameter";

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
                return EObfuscatorCategory.Obfuscation;
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


            //Obfuscate Internal
            Row_Boolean var_ObfuscateMethodRow = new Row_Boolean("Obfuscate Method Parameter:", _ComponentSettings, CObfuscate_Method_Parameter);
            var_ObfuscateMethodRow.Notification_Info = "Activate this setting to obfuscate method parameter and methods generic parameter.";
            var_Content.AddRow(var_ObfuscateMethodRow);

            //Obfuscate Private
            Row_Boolean var_ObfuscateTypeRow = new Row_Boolean("Obfuscate Class Parameter:", _ComponentSettings, CObfuscate_Class_Parameter);
            var_ObfuscateTypeRow.Notification_Info = "Activate this setting to obfuscate class generic parameter.";
            var_Content.AddRow(var_ObfuscateTypeRow);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Analyse A
        #region Analyse A

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
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
            // Rename Parameter
            this.OnObfuscate_RenameParameter(_AssemblyInfo);

            return true;
        }

        private void OnObfuscate_RenameParameter(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Obfuscating Parameter...");

            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Use a type wide generic parameter counter.
                uint var_Type_GenericParameter_Index = 0;

                if (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Method_Parameter))
                {
                    foreach (var var_MethodDefinition in var_TypeDefinition.Methods)
                    {
                        // Special case constructor!
                        if(var_MethodDefinition.IsConstructor)
                        {
                            if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition))
                            {
                                continue;
                            }
                        }
                        

                        uint var_Method_Parameter_Index = 0;
                        foreach (ParameterDefinition var_ParameterDefinition in var_MethodDefinition.Parameters)
                        {
                            if (var_ParameterDefinition.CustomAttributes.Count == 0)
                                var_ParameterDefinition.Name = this.DataContainer.RenameManager.NameGenerator.GetNextName(var_Method_Parameter_Index++);
                        }

                        foreach (GenericParameter var_GenericParameter in var_MethodDefinition.GenericParameters)
                        {
                            if (var_GenericParameter.CustomAttributes.Count == 0)
                                var_GenericParameter.Name = this.DataContainer.RenameManager.NameGenerator.GetNextName(var_Type_GenericParameter_Index++);
                        }
                    }
                }

                if (this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CObfuscate_Class_Parameter))
                {
                    if (this.DataContainer.RenameManager.GetDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                    {
                        continue;
                    }

                    foreach (GenericParameter var_GenericParameter in var_TypeDefinition.GenericParameters)
                    {
                        var_GenericParameter.Name = this.DataContainer.RenameManager.NameGenerator.GetNextName(var_Type_GenericParameter_Index++);
                    }
                }
            }
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
