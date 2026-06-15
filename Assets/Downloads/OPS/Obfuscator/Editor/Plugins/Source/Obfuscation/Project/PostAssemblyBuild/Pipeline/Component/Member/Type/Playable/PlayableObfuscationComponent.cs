using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Pipeling
using OPS.Editor.Project.Pipeline;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

// OPS - Obfuscator - Extension
using OPS.Obfuscator.Editor.Extension;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Type
{
    public class PlayableObfuscationComponent : APostAssemblyBuildComponent, IAssemblyProcessingComponent
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
                return "Playable - Class - Obfuscation";
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
                return "Manages the Playable Subclass obfuscation.";
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
                return "Manages the Playable Subclass obfuscation.";
            }
        }

        #endregion

        // Analyse A
        #region Analyse A

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Evaluate the Playable.
            this.OnAnalyse_A_EvaluatePlayable(_AssemblyInfo);

            return true;
        }

        // Do not rename Playable
        #region Do not rename Playable

        private void OnAnalyse_Skip_Playable(TypeDefinition _TypeDefinition, String _Cause)
        {
            // Skip Type and Namespace
            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, _TypeDefinition, _Cause);
            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition, _Cause);

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + _Cause);
        }

        private void OnAnalyse_A_EvaluatePlayable(AssemblyInfo _AssemblyInfo)
        {
            // Check if obfuscate Playables in general!

            // Iterate all types in the _AssemblyInfo.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                this.OnAnalyse_A_CheckPlayable(_AssemblyInfo, var_TypeDefinition);
            }
        }

        /// <summary>
        /// Check if Playables in general should be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private bool OnAnalyse_A_CheckPlayable(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition)
        {
            // Continue only Playable.
            if (!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsPlayable(_TypeDefinition, this.Step.GetCache<TypeCache>())
                && !OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsPlayableAsset(_TypeDefinition, this.Step.GetCache<TypeCache>())
                && !OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsPlayableBehaviour(_TypeDefinition, this.Step.GetCache<TypeCache>()))
            {
                // Is not a Playable.
                return true;
            }

            // TODO: Skip Content?

#if Obfuscator_Free

            // Is free, skip every Playable.

            //Cause
            String var_DoNotObuscateCause = "Obfuscator Free is used.";

            this.OnAnalyse_Skip_Playable(_TypeDefinition, var_DoNotObuscateCause);

#else

            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(TypeObfuscationComponent.CSettingsKey, TypeObfuscationComponent.CObfuscate_Class_Playable))
            {
                // Skip type name and namespace.

                // Cause
                String var_DoNotObuscateCause = "Obfuscation of Playables got deactivated by settings.";

                // Skip
                this.OnAnalyse_Skip_Playable(_TypeDefinition, var_DoNotObuscateCause);
            }

#endif

            return true;
        }

        #endregion

        #endregion

        // Analyse B
        #region Analyse B

        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Iterate all types in the _AssemblyInfo.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Continue only Playable.
                if (!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsPlayable(var_TypeDefinition, this.Step.GetCache<TypeCache>())
                    && !OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsPlayableAsset(var_TypeDefinition, this.Step.GetCache<TypeCache>())
                    && !OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsPlayableBehaviour(var_TypeDefinition, this.Step.GetCache<TypeCache>()))
                {
                    // Is not a Playable.
                    return true;
                }

                // If namespace getting not obfuscated of ScriptableObject skip their name too!
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition)
                    && !this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                {
                    this.Step.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, "Because namespace getting skipped!");
                }
                // If name getting not obfuscated of ScriptableObject skip their namespace too!
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition)
                    && !this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition))
                {
                    this.Step.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, "Because namespace getting skipped!");
                }
            }

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
