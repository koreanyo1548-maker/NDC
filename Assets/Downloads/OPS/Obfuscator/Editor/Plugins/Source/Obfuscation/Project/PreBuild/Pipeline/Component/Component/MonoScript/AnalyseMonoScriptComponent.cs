// System
using System;
using System.Collections.Generic;
using System.Linq;

// Unity
using UnityEditor;
using UnityEngine;

// OPS - Project - Pipeline
using OPS.Editor.Project.Pipeline;

// OPS - Project - Step
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component
{
    internal class AnalyseMonoScriptComponent : APreBuildComponent, IComponentProcessingComponent
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
                return "Analyse MonoScript";
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
                return "";
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
                return "";
            }
        }

        #endregion

        // OnPreBuild
        #region OnPreBuild

        public override bool OnPreBuild()
        {
            return true;
        }

        #endregion

        // Process Scenes or Prefabs
        #region Process Scenes or Prefabs

        /// <summary>
        /// The component is not required at the moment, so it does not need to process scenes.
        /// </summary>
        public bool ProcessScenes { get; } = false;

        /// <summary>
        /// The component is not required at the moment, so it does not need to process prefabs.
        /// </summary>
        public bool ProcessPrefabs { get; } = false;

        #endregion

        // MonoScript
        #region MonoScript

        /// <summary>
        /// The step output data key for the ReferencedMonoScriptTypeList.
        /// </summary>
        public const String CReferencedMonoScriptTypeListKey = "ReferencedMonoScriptTypeList";

        /// <summary>
        /// A list of all referenced types of unity mono scripts in the scene and prefabs.
        /// </summary>
        private List<TypeKey> ReferencedMonoScriptTypeList = new List<TypeKey>();

        #endregion

        // On Analyse Component
        #region On Analyse Component

        public bool OnAnalyse_Component(UnityEngine.Component _Component)
        {
            // Check if MonoBehaviour.
            if (_Component is MonoBehaviour)
            {
                MonoScript var_MonoScript = MonoScript.FromMonoBehaviour(_Component as MonoBehaviour);

                Type var_Type = var_MonoScript.GetClass();

                TypeKey var_TypeKey = new TypeKey(var_Type);

                if (!this.ReferencedMonoScriptTypeList.Contains(var_TypeKey))
                {
                    this.ReferencedMonoScriptTypeList.Add(var_TypeKey);
                }

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Found mono script reference '{0}' for component '{1}'.", var_Type, _Component.name));
            }

            // Check SerializedProperties.
            List<MonoScript> var_FoundMonoScripts = OPS.Obfuscator.Editor.Serialization.Unity.Helper.SerializedPropertyHelper.GetAllMonoScriptReferences(_Component);

            if(var_FoundMonoScripts.Count == 0)
            {
                return true;
            }

            for(int i = 0; i < var_FoundMonoScripts.Count; i++)
            {
                Type var_Type = var_FoundMonoScripts[i].GetClass();

                TypeKey var_TypeKey = new TypeKey(var_Type);

                if (!this.ReferencedMonoScriptTypeList.Contains(var_TypeKey))
                {
                    this.ReferencedMonoScriptTypeList.Add(var_TypeKey);
                }
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Found mono script reference in serialized properties for component '{0}'", _Component.name), var_FoundMonoScripts.Select(m => m.GetClass().ToString()).ToList());

            return true;
        }

        #endregion

        // On Process Component
        #region On Process Component

        public bool OnProcess_Component(UnityEngine.Component _Component)
        {
            return true;
        }

        #endregion

        // On Post Process Pipeline
        #region On Post Process Pipeline

        public override bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            bool var_Result = base.OnPostPipelineProcess(_StepOutput);

            // Add types to step output.
            var_Result = var_Result && _StepOutput.Add(AnalyseMonoScriptComponent.CReferencedMonoScriptTypeListKey, this.ReferencedMonoScriptTypeList, true);

            return var_Result;
        }

        #endregion
    }
}
