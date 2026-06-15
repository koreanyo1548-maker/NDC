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

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility
{
    public class StringReflectionComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "String Reflection and Coroutine - Compatibility";
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
                return "Manage the obfuscation of classes/methods/... called through strings.";
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
                return "Manage the obfuscation of members called through strings.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_String_Reflection_And_Coroutine";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements
        private const String CEnable_Try_Find_String_Reflection_And_Coroutine = "Enable_Try_Find_String_Reflection_And_Coroutine";

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
                return EObfuscatorCategory.Compatibility;
            }
        }

        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_Try_Find_String_Reflection_And_Coroutine);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Explanation
            Row_Text var_ExplanationRow = new Row_Text("");
            var_ExplanationRow.Notification_Info = "Activate this setting if you use string based reflection or calling coroutines through a string. Calling a coroutine through a string is not recommended when using obfuscation. Call a coroutine better like this: StartCoroutine(this.MyCoroutine);. If you cannot avoid string based calling, better add an OPS.Obfuscator.DoNotRename Attribute to those members and deactivate this setting here.";
            var_Content.AddRow(var_ExplanationRow);

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Component
        #region Component

        /// <summary>
        /// True: StringReflectionComponent is active.
        /// </summary>
        public override bool IsActive
        {
            get
            {
                // Check if string analysing got activated.
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Try_Find_String_Reflection_And_Coroutine))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        // Analyse A
        #region Analyse A

        /// <summary>
        /// HashSet of all strings inside the project.
        /// </summary>
        private HashSet<String> stringHashSet = new HashSet<string>();

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Find Strings
            this.OnAnalyse_A_FindStringsInMethods(_AssemblyInfo);

            // Skip Strings
            this.OnAnalyse_A_Skip_TypesAndMethods(_AssemblyInfo);

            return true;
        }

        /// <summary>
        /// Analyse the Methods strings.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_FindStringsInMethods(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Find Strings in Methods...");

            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                foreach (MethodDefinition var_Method in var_TypeDefinition.Methods)
                {
                    if (!var_Method.HasBody)
                    {
                        continue;
                    }

                    for (int index = 0; index < var_Method.Body.Instructions.Count; index++)
                    {
                        Instruction var_CurrentInstruction = var_Method.Body.Instructions[index];

                        if (var_CurrentInstruction.OpCode == OpCodes.Ldstr)
                        {
                            string var_String = (string)var_CurrentInstruction.Operand;

                            if (!this.stringHashSet.Contains(var_String))
                            {
                                this.stringHashSet.Add(var_String);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Analyse the Types and Methods to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_Skip_TypesAndMethods(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Member names based on found Strings...");

            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                if (this.stringHashSet.Contains(OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.TypeNameWithoutGenericSuffix(var_TypeDefinition.Name)))
                {
                    //Cause
                    String var_DoNotObuscateCause = "Because the type might be a type called through reflection.";
                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type and Namespace [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                }

                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    if (this.stringHashSet.Contains(var_MethodDefinition.Name))
                    {
                        //Cause
                        String var_DoNotObuscateCause = "Because the method might be a method called through reflection.";
                        this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_DoNotObuscateCause);
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
