using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Project
using OPS.Editor.Project.Pipeline;
using OPS.Editor.Project.Step;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility
{
    /// <summary>
    /// This component skips methods which are refered by Unity Events.
    /// </summary>
    public class UnityEventComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Unity Event Methods - Compatibility";
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
                return "Manages the obfuscation of unity event methods set through the inspector.";
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
                return "Manages the obfuscation of inspector set unity event methods.";
            }
        }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public const String CSettingsKey = "Default_Obfuscation_Component_Find_Unity_Event_MethodReferences";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        /// <summary>
        /// The settings key to enable the try find inspector unity event methods.
        /// </summary>
        public const String CEnable_Try_Find_Inspector_Unity_Event_Methods = "Enable_Try_Find_Inspector_Unity_Event_Methods";

        /// <summary>
        /// The settings key to enable the try find inspector unity event methods in scenes.
        /// </summary>
        public const String CEnable_Try_Find_Inspector_Unity_Event_Methods_In_Scenes = "Enable_Try_Find_Inspector_Unity_Event_Methods_In_Scenes";

        /// <summary>
        /// The settings key to enable the try find inspector unity event methods in prefabs.
        /// </summary>
        public const String CEnable_Try_Find_Inspector_Unity_Event_Methods_In_Prefabs = "Enable_Try_Find_Inspector_Unity_Event_Methods_In_Prefabs";

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

        /// <summary>
        /// The gui container for this component.
        /// </summary>
        /// <param name="_ComponentSettings">The component settings.</param>
        /// <returns>The gui container for this component.</returns>
        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_Try_Find_Inspector_Unity_Event_Methods);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // Explanation
            Row_Text var_ExplanationRow = new Row_Text("");
            var_ExplanationRow.Notification_Info = "Attempts to find all unity event methods (which are set in the Unity Editor inspector) in the scenes or prefabs and skips them. This is necessary because Unity calls them by name via reflection. To trigger the skipping manually, add an 'OPS.Obfuscator.DoNotRename' attribute to these methods in your code.";
            var_Content.AddRow(var_ExplanationRow);

            // Search in scenes.
            Row_Boolean var_EnableSceneSearch = new Row_Boolean("Search in scenes:", _ComponentSettings, CEnable_Try_Find_Inspector_Unity_Event_Methods_In_Scenes);
            var_EnableSceneSearch.Notification_Info = "Searches for unity event methods in scenes included in the build.";
            var_Content.AddRow(var_EnableSceneSearch);

            // Search in prefabs.
            Row_Boolean var_EnablePrefabSearch = new Row_Boolean("Search in prefabs:", _ComponentSettings, CEnable_Try_Find_Inspector_Unity_Event_Methods_In_Prefabs);
            var_EnablePrefabSearch.Notification_Info = "Searches for unity event methods in prefabs.";
            var_Content.AddRow(var_EnablePrefabSearch);

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion

        // Component
        #region Component

        /// <summary>
        /// Return whether the component is activated or deactivated for the pipeline processing.
        /// </summary>
        public override bool IsActive
        {
            get
            {
                // Check if setting is activated.
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Try_Find_Inspector_Unity_Event_Methods))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        // Gui Methods
        #region Gui Methods

        /// <summary>
        /// A list of all references unity event methods.
        /// </summary>
        private HashSet<String> ReferencedEventMethodHashSet = new HashSet<string>();

        #endregion

        // On Pre Process Pipeline
        #region On Pre Process Pipeline

        /// <summary>
        /// Reads the analysed Unity Events assigned in the Pre Build Step.
        /// </summary>
        /// <param name="_StepInput"></param>
        /// <returns></returns>
        public override bool OnPrePipelineProcess(IStepInput _StepInput)
        {
            bool var_Result = base.OnPrePipelineProcess(_StepInput);

            // Read ReferencedGuiMethodHashSet from step input.
            this.ReferencedEventMethodHashSet = _StepInput.Get<HashSet<String>>(PreBuild.Pipeline.Component.AnalyseUnityEventComponent.CReferencedEventMethodHashSet);

            return var_Result;
        }

        #endregion

        // Analyse A
        #region Analyse A

        /// <summary>
        /// Search methods to skip.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Skip the methods based on the found gui methods.
            this.OnAnalyse_A_Skip_Methods(_AssemblyInfo);

            return true;
        }

        /// <summary>
        /// Analyse the Methods to check if they can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        private void OnAnalyse_A_Skip_Methods(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Gui Methods...");

            //Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Continue only MonoBehaviours!
                if(!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsMonoBehaviour(var_TypeDefinition, this.Step.GetCache<TypeCache>()))
                {
                    continue;
                }

                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    if (this.ReferencedEventMethodHashSet.Contains(var_MethodDefinition.Name))
                    {
                        //Cause
                        String var_DoNotObuscateCause = "Because the method might be a gui method.";
                        this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Method [" + var_MethodDefinition.GetExtendedOriginalFullName(this.Step.GetCache<MethodCache>()) + "] " + var_DoNotObuscateCause);
                    }
                }
            }
        }

        #endregion

        // Analyse B
        #region Analyse B

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion

        // Post Obfuscate
        #region Post Obfuscate

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            return true;
        }

        #endregion
    }
}
