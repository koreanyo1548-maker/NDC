using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Project
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member.Type
{
    /// <summary>
    /// Obfuscation of MonoBehaviours by renaming. 
    /// </summary>
    public class MonoBehaviourRenameObfuscationComponent : APostAssemblyBuildComponent, IAssemblyProcessingComponent
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
                return "MonoBehaviour - Class - Obfuscation";
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
                return "Manages the MonoBehaviour Subclass obfuscation.";
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
                return "Manages the MonoBehaviour Subclass obfuscation.";
            }
        }

        #endregion

        // Check Assembly
        #region Check Assembly

        /// <summary>
        /// True: The _AssemblyLoadInfo should be processed by this component.
        /// </summary>
        /// <param name="_AssemblyLoadInfo"></param>
        /// <returns></returns>
        public override bool CanBeProcessedByComponent(AssemblyLoadInfo _AssemblyLoadInfo)
        {
            if(!base.CanBeProcessedByComponent(_AssemblyLoadInfo))
            {
                return false;
            }

            return true;
        }

        #endregion

        // MonoBehaviour
        #region MonoBehaviour

        /// <summary>
        /// The step data key for the MonoBehaviour rename mapping.
        /// </summary>
        public const String CMonobehaviourObfuscationMapping = "PostAssemblyBuid_Monobehaviour_ObfuscationMapping";

        /// <summary>
        /// A mapping of the original monobehaviour type to the obfuscated monobehaviour type.
        /// </summary>
        private Dictionary<TypeKey, TypeKey> MonobehaviourObfuscationMapping = new Dictionary<TypeKey, TypeKey>();

        #endregion

        // OnAnalyse
        #region OnAnalyse

        /// <summary>
        /// Validate if MonoBehaviours are obfuscatable in general.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Check if obfuscate MonoBehaviours in general!

            // Iterate all types in the _AssemblyInfo.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                this.OnAnalyse_A_CheckMonoBehaviour(_AssemblyInfo, var_TypeDefinition);
            }

            return true;
        }

        /// <summary>
        /// Check if MonoBehaviours in general should be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private bool OnAnalyse_A_CheckMonoBehaviour(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition)
        {
            // Continue only MonoBehaviour
            if (!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsMonoBehaviour(_TypeDefinition, this.Step.GetCache<TypeCache>()))
            {
                // Is not a MonoBehaviour.
                return true;
            }

#if Obfuscator_Free

            // Is free, skip every MonoBehaviour.

            // Cause
            String var_DoNotObuscateCause = "MonoBehaviour obfuscation is deactivated - Obfuscator Free is used.";

            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, _TypeDefinition, var_DoNotObuscateCause);
            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition, var_DoNotObuscateCause);

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);

#else

            // Check if obfuscation in unity assemblies got deactivated.
            if (_AssemblyInfo.AssemblyLoadInfo.IsUnityAssembly)
            {
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(TypeObfuscationComponent.CSettingsKey, TypeObfuscationComponent.CObfuscate_Class_MonoBehaviour))
                {
                    // Skip type name and namespace.

                    // Cause
                    String var_DoNotObuscateCause = "Obfuscation of MonoBehaviours in Unity Assemblies got deactivated by settings.";

                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, _TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                }
            }

            // Check if obfuscation in precompiled assemblies got deactivated.
            if (_AssemblyInfo.AssemblyLoadInfo.IsThirdPartyAssembly)
            {
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(TypeObfuscationComponent.CSettingsKey, TypeObfuscationComponent.CObfuscate_Class_MonoBehaviour_Extern))
                {
                    // Skip type name and namespace.

                    // Cause
                    String var_DoNotObuscateCause = "Obfuscation of MonoBehaviours in Third Party Assemblies got deactivated by settings.";

                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, _TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                }
            }

            // Check if the assets files are compatible with the obfuscation.
            if(!this.Step.BuildSettings.IsStandaloneBuildTarget)
            {
                if(!this.Step.BuildSettings.IsIL2CPPBuild)
                {
                    // Skip type name and namespace.

                    // Cause
                    String var_DoNotObuscateCause = "Not supported build target.";

                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, _TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition, var_DoNotObuscateCause);

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + _TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);
                }
            }
#endif

            return true;
        }

        #endregion

        // On Post Analyse
        #region On Post Analyse

        /// <summary>
        /// Validates if the namespace and class name getting obfuscated.
        /// Only both together can be obfuscated.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnPostAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Iterate all types in the _AssemblyInfo.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Continue only MonoBehaviour
                if (!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsMonoBehaviour(var_TypeDefinition, this.Step.GetCache<TypeCache>()))
                {
                    // Is not a MonoBehaviour.
                    continue;
                }

                // If namespace getting not obfuscated of MonoBehaviour skip their name too!
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition)
                    && !this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition))
                {
                    this.Step.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, "Because namespace getting skipped! MonoBehaviour obfuscation requires class name and namespace obfuscation.");
                }
                // If name getting not obfuscated of MonoBehaviour skip their namespace too!
                if (this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition)
                    && !this.Step.DataContainer.RenameManager.GetDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition))
                {
                    this.Step.DataContainer.RenameManager.AddDoNotObfuscate(Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, "Because class getting skipped! MonoBehaviour obfuscation requires class name and namespace obfuscation.");
                }
            }

            return true;
        }

        #endregion

        // Obfuscate
        #region Obfuscate

        /// <summary>
        /// Returns just true.
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
        /// Map the unobfuscated MonoBehaviours to the obfuscated MonoBehaviours.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public bool OnPostObfuscate_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Iterate all types in the _AssemblyInfo and map type to obfuscated type.
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                this.OnPostObfuscate_Assemblies_FindMonoBehaviourMapping(_AssemblyInfo, var_TypeDefinition);
            }

            // Return true.
            return true;
        }

        /// <summary>
        /// Process only if MonoBehaviour, not generic and not abstract!
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private void OnPostObfuscate_Assemblies_FindMonoBehaviourMapping(AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition)
        {
            // Continue only MonoBehaviour
            if (!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsMonoBehaviour(_TypeDefinition, this.Step.GetCache<TypeCache>()))
            {
                // Is not a MonoBehaviour.
                return;
            }

            // Continue only none generic or abstract types.
            if (TypeDefinitionHelper.IsTypeGeneric(_TypeDefinition) || TypeDefinitionHelper.IsTypeAbstract(_TypeDefinition))
            {
                // Is generic or abstract.
                return;
            }

            // Create key element.
            TypeKey var_TypeKey = this.Step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_TypeDefinition);

            if(var_TypeKey == null)
            {
                var_TypeKey = new TypeKey(_TypeDefinition);
            }

            // Add only if not already added.
            if (this.MonobehaviourObfuscationMapping.ContainsKey(var_TypeKey))
            {
                // Is already added.
                return;
            }

            // Obfuscated
            String var_ObfuscatedNamespace = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Namespace, _TypeDefinition) ?? var_TypeKey.Namespace;
            String var_ObfuscatedName = this.Step.DataContainer.RenameManager.GetObfuscated(Editor.Assembly.Mono.Member.EMemberType.Type, _TypeDefinition) ?? var_TypeKey.Name;

            // Create value element
            TypeKey var_ObfuscatedTypeKey = new TypeKey(var_TypeKey.Assembly, var_ObfuscatedNamespace, var_ObfuscatedName);

            // Add to mapping
            this.MonobehaviourObfuscationMapping.Add(var_TypeKey, var_ObfuscatedTypeKey);
        }

        #endregion

        // On Post Process Pipeline
        #region On Post Process Pipeline

        /// <summary>
        /// Adds the unobfuscated MonoBehaviours to the obfuscated MonoBehaviours map to the _StepOutput.
        /// </summary>
        /// <param name="_StepOutput"></param>
        /// <returns></returns>
        public override bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            bool var_Result = base.OnPostPipelineProcess(_StepOutput);

            // Add mapping to output.
            var_Result = var_Result && _StepOutput.Add(MonoBehaviourRenameObfuscationComponent.CMonobehaviourObfuscationMapping, this.MonobehaviourObfuscationMapping, true);

            return var_Result;
        }

        #endregion
    }
}
