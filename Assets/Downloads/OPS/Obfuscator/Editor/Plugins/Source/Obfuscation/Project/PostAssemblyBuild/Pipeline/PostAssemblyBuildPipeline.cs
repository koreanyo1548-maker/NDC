using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project
using OPS.Editor.Project.Pipeline;
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Assets
using OPS.Obfuscator.Editor.Assets.Unity;

// OPS - Obfuscator - Settings
using OPS.Obfuscator.Editor.Settings;
using OPS.Obfuscator.Editor.Settings.Unity.Build;
using OPS.Obfuscator.Editor.Settings.Unity.Editor;

// OPS - Obfuscator - PostAssemblyBuild - Pipeline
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component;
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline
{
    /// <summary>
    /// The post assembly build pipeline.
    /// </summary>
    public class PostAssemblyBuildPipeline : APipeline
    {
        // Constructor
        #region Constructor

        public PostAssemblyBuildPipeline(PostAssemblyBuildStep _Step)
            : base(_Step)
        {
        }

        #endregion

        // Project
        #region Project

        /// <summary>
        /// Project this Pipeline belongs too.
        /// </summary>
        public new PostAssemblyBuildStep Step
        {
            get
            {
                return (PostAssemblyBuildStep)base.Step;
            }
        }


        /// <summary>
        /// DataContainer of the Project.
        /// </summary>
        public PostAssemblyBuildDataContainer DataContainer
        {
            get
            {
                return (PostAssemblyBuildDataContainer)this.Step.DataContainer;
            }
        }

        /// <summary>
        /// Settings of the Project.
        /// </summary>
        public ObfuscatorSettings Settings
        {
            get
            {
                return (ObfuscatorSettings)this.Step.Settings;
            }
        }

        #endregion

        // Process
        #region Process

        /// <summary>
        /// Processes the whole obfuscation pipeline.
        /// </summary>
        /// <returns></returns>
        public override bool OnProcessPipeline()
        {
            try
            {
                // Display Progress Bar
                UnityEditor.EditorUtility.DisplayProgressBar("Obfuscator", "Obfuscating...", 0.66f);

                // ###### 1. Process Assembly Analysation A ######

                // Report Header
                Obfuscator.Report.SetHeader("OnAnalyse_Assemblies");

                // Get all IAssemblyProcessingComponent in this Pipeline.
                List<IAssemblyProcessingComponent> var_AssemblyProcessingComponentList = this.GetPipelineComponents<IAssemblyProcessingComponent>();

                // Get all the to obfuscate assemblies.
                List<AssemblyInfo> var_ToObfuscateAssemblyInfoList = this.DataContainer.ObfuscateAssemblyList;

                // Process all Assembly Components.
                for (int c = 0; c < var_AssemblyProcessingComponentList.Count; c++)
                {
                    // Deactivated components will be skipped.
                    if (!var_AssemblyProcessingComponentList[c].IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssemblyProcessingComponentList[c]);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssemblyProcessingComponentList[c]);

                    // Process all to obfuscate assemblies first time.
                    for (int a = 0; a < var_ToObfuscateAssemblyInfoList.Count; a++)
                    {
                        // Check if wont be processed by this component.
                        if (!var_AssemblyProcessingComponentList[c].CanBeProcessedByComponent(var_ToObfuscateAssemblyInfoList[a].AssemblyLoadInfo))
                        {
                            continue;
                        }

                        // Analyse Assemblies first time.
                        if (!var_AssemblyProcessingComponentList[c].OnAnalyse_Assemblies(var_ToObfuscateAssemblyInfoList[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssemblyProcessingComponentList[c]);

                            return false;
                        }
                    }
                }

                // ###### 2. Process Assembly Analysation B ######

                // Report Header
                Obfuscator.Report.SetHeader("OnPostAnalyse_Assemblies");

                // Process all Assembly Components.
                for (int c = 0; c < var_AssemblyProcessingComponentList.Count; c++)
                {
                    // Deactivated components will be skipped.
                    if (!var_AssemblyProcessingComponentList[c].IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssemblyProcessingComponentList[c]);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssemblyProcessingComponentList[c]);

                    // Process all to obfuscate assemblies second time.
                    for (int a = 0; a < var_ToObfuscateAssemblyInfoList.Count; a++)
                    {
                        // Check if wont be processed by this component.
                        if (!var_AssemblyProcessingComponentList[c].CanBeProcessedByComponent(var_ToObfuscateAssemblyInfoList[a].AssemblyLoadInfo))
                        {
                            continue;
                        }

                        // Analyse Assemblies second time.
                        if (!var_AssemblyProcessingComponentList[c].OnPostAnalyse_Assemblies(var_ToObfuscateAssemblyInfoList[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssemblyProcessingComponentList[c]);

                            return false;
                        }
                    }
                }

                // ###### Calculate all member count before obfuscation ######

                int var_AllTypeCount = AssemblyHelper.GetCountOfAllTypes(var_ToObfuscateAssemblyInfoList, true);
                int var_FullTypeCount = AssemblyHelper.GetCountOfAllTypes(var_ToObfuscateAssemblyInfoList, false);
                int var_NestedTypeCount = var_AllTypeCount - var_FullTypeCount;

                int var_AllMethodCount = AssemblyHelper.GetCountOfAllMethods(var_ToObfuscateAssemblyInfoList);
                int var_AllFieldCount = AssemblyHelper.GetCountOfAllFields(var_ToObfuscateAssemblyInfoList);
                int var_AllPropertyCount = AssemblyHelper.GetCountOfAllProperties(var_ToObfuscateAssemblyInfoList);
                int var_AllEventCount = AssemblyHelper.GetCountOfAllEvents(var_ToObfuscateAssemblyInfoList);

                // ###### 3. Process On Pre Obfuscation ######

                // Report Header
                Obfuscator.Report.SetHeader("OnFindMemberNames_Assemblies");

                // Get all IAssemblyProcessingComponent in this Pipeline.
                List<IMemberObfuscationComponent> var_MemberObfuscationComponentList = this.GetPipelineComponents<IMemberObfuscationComponent>();

                // Process all Assembly Components.
                for (int c = 0; c < var_MemberObfuscationComponentList.Count; c++)
                {
                    // Deactivated components will be skipped.
                    if (!var_MemberObfuscationComponentList[c].IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_MemberObfuscationComponentList[c]);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_MemberObfuscationComponentList[c]);

                    // Process all to obfuscate assemblies third time.
                    for (int a = 0; a < var_ToObfuscateAssemblyInfoList.Count; a++)
                    {
                        // Check if wont be processed by this component.
                        if (!var_MemberObfuscationComponentList[c].CanBeProcessedByComponent(var_ToObfuscateAssemblyInfoList[a].AssemblyLoadInfo))
                        {
                            continue;
                        }

                        // Analyse Assemblies second time.
                        if (!var_MemberObfuscationComponentList[c].OnFindMemberNames_Assemblies(var_ToObfuscateAssemblyInfoList[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_MemberObfuscationComponentList[c]);

                            return false;
                        }
                    }
                }

                // ###### 4. Process Assembly Obfuscation ######

                // Report Header
                Obfuscator.Report.SetHeader("OnObfuscate_Assemblies");

                // Process all Assembly Components.
                for (int c = 0; c < var_AssemblyProcessingComponentList.Count; c++)
                {
                    // Deactivated components will be skipped.
                    if (!var_AssemblyProcessingComponentList[c].IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssemblyProcessingComponentList[c]);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssemblyProcessingComponentList[c]);

                    // Process all to obfuscate assemblies third time.
                    for (int a = 0; a < var_ToObfuscateAssemblyInfoList.Count; a++)
                    {
                        // Check if wont be processed by this component.
                        if (!var_AssemblyProcessingComponentList[c].CanBeProcessedByComponent(var_ToObfuscateAssemblyInfoList[a].AssemblyLoadInfo))
                        {
                            continue;
                        }

                        // Analyse Assemblies second time.
                        if (!var_AssemblyProcessingComponentList[c].OnObfuscate_Assemblies(var_ToObfuscateAssemblyInfoList[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssemblyProcessingComponentList[c]);

                            return false;
                        }
                    }
                }

                // ###### 5. Process Assembly Post Obfuscation ######

                // Report Header
                Obfuscator.Report.SetHeader("OnPostObfuscate_Assemblies");

                // Process all Assembly Components.
                for (int c = 0; c < var_AssemblyProcessingComponentList.Count; c++)
                {
                    // Deactivated components will be skipped.
                    if (!var_AssemblyProcessingComponentList[c].IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssemblyProcessingComponentList[c]);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssemblyProcessingComponentList[c]);

                    // Process all to obfuscate assemblies fourth time.
                    for (int a = 0; a < var_ToObfuscateAssemblyInfoList.Count; a++)
                    {
                        // Check if wont be processed by this component.
                        if (!var_AssemblyProcessingComponentList[c].CanBeProcessedByComponent(var_ToObfuscateAssemblyInfoList[a].AssemblyLoadInfo))
                        {
                            continue;
                        }

                        // Analyse Assemblies second time.
                        if (!var_AssemblyProcessingComponentList[c].OnPostObfuscate_Assemblies(var_ToObfuscateAssemblyInfoList[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssemblyProcessingComponentList[c]);

                            return false;
                        }
                    }
                }

                String var_Namespaces = "Of " + var_FullTypeCount + " Namespaces " + (var_FullTypeCount == 0 ? 100 : Math.Ceiling(100f - (float)Math.Max(0, this.DataContainer.RenameManager.GetCountOfDoNotObfuscate(EMemberType.Namespace) - var_NestedTypeCount) / (float)var_FullTypeCount * 100f)) + "%, ";
                String var_Types = "of " + var_AllTypeCount + " Types " + (var_AllTypeCount == 0 ? 100 : Math.Ceiling(100f - (float)Math.Max(0, this.DataContainer.RenameManager.GetCountOfDoNotObfuscate(EMemberType.Type)) / (float)var_AllTypeCount * 100f)) + "%, ";

                String var_Methods = "of " + var_AllMethodCount + " Methods " + (var_AllMethodCount == 0 ? 100 : Math.Ceiling(100f - (float)this.DataContainer.RenameManager.GetCountOfDoNotObfuscate(EMemberType.Method) / (float)var_AllMethodCount * 100f)) + "%, ";
                String var_Fields = "of " + var_AllFieldCount + " Fields " + (var_AllFieldCount == 0 ? 100 : Math.Ceiling(100f - (float)this.DataContainer.RenameManager.GetCountOfDoNotObfuscate(EMemberType.Field) / (float)var_AllFieldCount * 100f)) + "%, ";
                String var_Properties = "of " + var_AllPropertyCount + " Properties " + (var_AllPropertyCount == 0 ? 100 : Math.Ceiling(100f - (float)this.DataContainer.RenameManager.GetCountOfDoNotObfuscate(EMemberType.Property) / (float)var_AllPropertyCount * 100f)) + "%, ";
                String var_Events = "of " + var_AllEventCount + " Events " + (var_AllEventCount == 0 ? 100 : Math.Ceiling(100f - (float)this.DataContainer.RenameManager.GetCountOfDoNotObfuscate(EMemberType.Event) / (float)var_AllEventCount * 100f)) + "%";

                UnityEngine.Debug.Log("[OPS.OBF] Obfuscated names: " + var_Namespaces + var_Types + var_Methods + var_Fields + var_Properties + var_Events + ".");
            }
            catch (Exception e)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "While processing post assembly build pipeline: " + e.ToString());

                return false;
            }
            finally
            {
                // Clear Progress Bar
                UnityEditor.EditorUtility.ClearProgressBar();
            }

            return true;
        }

        #endregion
    }
}
