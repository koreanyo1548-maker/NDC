using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// Unity
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// OPS - Project
using OPS.Editor.Project;
using OPS.Editor.Project.DataContainer;
using OPS.Editor.Project.Pipeline;
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Assets
using OPS.Obfuscator.Editor.Assets.Unity;

// OPS - Obfuscator - Settings
using OPS.Obfuscator.Editor.Settings;

// OPS - Obfuscator - Project - PostAssetsBuild
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.DataContainer;
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline.Component;

namespace OPS.Obfuscator.Editor.Project.PostAssetsBuild.Pipeline
{
    public class PostAssetsBuildPipeline : APipeline
    {
        // Constructor
        #region Constructor

        public PostAssetsBuildPipeline(PostAssetsBuildStep _Step) 
            : base(_Step)
        {
        }

        #endregion

        // Project
        #region Project

        /// <summary>
        /// Project this Pipeline belongs too.
        /// </summary>
        public new PostAssetsBuildStep Step
        {
            get
            {
                return (PostAssetsBuildStep) base.Step;
            }
        }


        /// <summary>
        /// DataContainer of the Project.
        /// </summary>
        public PostAssetsBuildDataContainer DataContainer
        {
            get
            {
                return (PostAssetsBuildDataContainer)this.Step.DataContainer;
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
        /// Processes the whole post build pipeline.
        /// </summary>
        /// <returns></returns>
        public override bool OnProcessPipeline()
        {
            try
            {
                // Display Progress Bar
                UnityEditor.EditorUtility.DisplayProgressBar("Obfuscator", "Post processing assets...", 0.66f);

                // ###### 1. Process Pre Build ######

                // Report Header
                Obfuscator.Report.SetHeader("OnPostAssetsBuild");

                // Get all IPreBuildComponent in this Pipeline.
                List<IPostAssetsBuildComponent> var_PreBuildComponentList = this.GetPipelineComponents<IPostAssetsBuildComponent>();

                // Process all PreBuild Components.
                for (int c = 0; c < var_PreBuildComponentList.Count; c++)
                {
                    // Deactivated components will be skipped.
                    if(!var_PreBuildComponentList[c].IsActive)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_PreBuildComponentList[c]);

                        continue;
                    }

                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_PreBuildComponentList[c]);

                    // Do pre build job.
                    if (!var_PreBuildComponentList[c].OnPostAssetsBuild())
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_PreBuildComponentList[c]);

                        return false;
                    }
                }
            }
            catch(Exception e)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "While processing post build pipeline: " + e.ToString());

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
