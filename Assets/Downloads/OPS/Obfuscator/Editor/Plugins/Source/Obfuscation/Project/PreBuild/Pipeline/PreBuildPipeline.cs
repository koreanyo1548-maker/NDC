// System
using System;
using System.Collections.Generic;
using System.Linq;

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
using OPS.Obfuscator.Editor.Assets.Unity.Cache;

// OPS - Obfuscator - Settings
using OPS.Obfuscator.Editor.Settings;

// OPS - Obfuscator - Project - PreBuild
using OPS.Obfuscator.Editor.Project.PreBuild.DataContainer;
using OPS.Obfuscator.Editor.Project.PreBuild.Pipeline;
using OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component;

namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline
{
    public class PreBuildPipeline : APipeline
    {
        // Constructor
        #region Constructor

        public PreBuildPipeline(PreBuildStep _Step) 
            : base(_Step)
        {
        }

        #endregion

        // Project
        #region Project

        /// <summary>
        /// Project this Pipeline belongs too.
        /// </summary>
        public new PreBuildStep Step
        {
            get
            {
                return (PreBuildStep) base.Step;
            }
        }


        /// <summary>
        /// DataContainer of the Project.
        /// </summary>
        public PreBuildDataContainer DataContainer
        {
            get
            {
                return (PreBuildDataContainer)this.Step.DataContainer;
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
        /// Processes the whole pre build pipeline.
        /// </summary>
        /// <returns></returns>
        public override bool OnProcessPipeline()
        {
            try
            {
                // Display Progress Bar
                UnityEditor.EditorUtility.DisplayProgressBar("Obfuscator", "Pre processing...", 0.33f);

                // ###### 1. Process Pre Build ######

                // Report Header
                Obfuscator.Report.SetHeader("OnPreBuild");

                // Get all IPreBuildComponent in this Pipeline.
                List<IPreBuildComponent> var_PreBuildComponentList = this.GetPipelineComponents<IPreBuildComponent>();

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
                    if (!var_PreBuildComponentList[c].OnPreBuild())
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_PreBuildComponentList[c]);

                        return false;
                    }
                }

                // ###### 2. Analyse Assets ######

                // Report Header
                Obfuscator.Report.SetHeader("OnAnalyse_Assets");

                // Analyse Assets
                if (!this.Process_Assets_Analyse())
                {
                    return false;
                }

                // Report Header
                Obfuscator.Report.SetHeader("OnProcess_Assets");                

                // Process Assets
                if (!this.Process_Assets_Process())
                {
                    return false;
                }

                // ###### 3. Analyse Components ######

                // Report Header
                Obfuscator.Report.SetHeader("OnAnalyse_Component_Scenes");

                // Scene Components.
                if (!this.Process_Component_IterateBuildSceneComponents())
                {
                    return false;
                }

                // Report Header
                Obfuscator.Report.SetHeader("OnAnalyse_Component_Prefabs");

                // Prefab Components.
                if (!this.Process_Component_IteratePrefabComponents())
                {
                    return false;
                }

                // Unload not used assets!
                Resources.UnloadUnusedAssets();
            }
            catch(Exception e)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "While processing pre build pipeline: " + e.ToString());

                return false;
            }
            finally
            {
                // Clear Progress Bar
                UnityEditor.EditorUtility.ClearProgressBar();
            }

            return true;
        }

        // Process - Asset
        #region Process - Asset

        /// <summary>
        /// The second step of the pipeline is to analyse all assets from the asset cache.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        private bool Process_Assets_Analyse()
        {
            // Get all IAssetProcessingComponent in this Pipeline.
            List<IAssetProcessingComponent> var_AssetProcessingComponentList = this.GetPipelineComponents<IAssetProcessingComponent>();

            // Get all required Asset References.
            Dictionary<String, List<UnityAssetReference>> var_ExtensionToUnityAssetReferenceDictionary = this.DataContainer.UnityAssetCache.GetUnityAssetReferences();

            // Process all Asset Components.
            for (int c = 0; c < var_AssetProcessingComponentList.Count; c++)
            {
                // Deactivated components will be skipped.
                if (!var_AssetProcessingComponentList[c].IsActive)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssetProcessingComponentList[c]);

                    continue;
                }

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssetProcessingComponentList[c]);

                // Process all Extensions to Reference Arrays.
                foreach (var var_Pair in var_ExtensionToUnityAssetReferenceDictionary)
                {
                    // Process UnityAssetReferences only when the component wants to process those. 
                    if (!var_AssetProcessingComponentList[c].AssetExtensionsToProcess.Contains(var_Pair.Key))
                    {
                        continue;
                    }

                    // Process all UnityAssetReferences with current extension.
                    for (int a = 0; a < var_Pair.Value.Count; a++)
                    {
                        // Analyse UnityAssetReference.
                        if (!var_AssetProcessingComponentList[c].OnAnalyse_Assets(var_Pair.Value[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssetProcessingComponentList[c]);

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// The third step of the pipeline is to process all assets from the asset cache. Processing means to edit the assets if required for the obfuscation.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        private bool Process_Assets_Process()
        {
            // Get all IAssetProcessingComponent in this Pipeline.
            List<IAssetProcessingComponent> var_AssetProcessingComponentList = this.GetPipelineComponents<IAssetProcessingComponent>();

            // Get all required Asset References.
            Dictionary<String, List<UnityAssetReference>> var_ExtensionToUnityAssetReferenceDictionary = this.DataContainer.UnityAssetCache.GetUnityAssetReferences();

            // Process all Asset Components.
            for (int c = 0; c < var_AssetProcessingComponentList.Count; c++)
            {
                // Deactivated components will be skipped.
                if (!var_AssetProcessingComponentList[c].IsActive)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_AssetProcessingComponentList[c]);

                    continue;
                }

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_AssetProcessingComponentList[c]);

                // Process all Extensions to Reference Arrays.
                foreach (var var_Pair in var_ExtensionToUnityAssetReferenceDictionary)
                {
                    // Process UnityAssetReferences only when the component wants to process those. 
                    if (!var_AssetProcessingComponentList[c].AssetExtensionsToProcess.Contains(var_Pair.Key))
                    {
                        continue;
                    }

                    // Process all UnityAssetReferences with current extension.
                    for (int a = 0; a < var_Pair.Value.Count; a++)
                    {
                        // Analyse UnityAssetReference.
                        if (!var_AssetProcessingComponentList[c].OnProcess_Assets(var_Pair.Value[a]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_AssetProcessingComponentList[c]);

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion

        // Process - Component
        #region Process - Component

        /// <summary>
        /// Analyse and manipulate all passed components.
        /// </summary>
        /// <param name="_ComponentList">The list of components to analyse and manipulate.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool Process_Component_Analyse_And_Manipulate(List<UnityEngine.Component> _ComponentList)
        {
            // Get all IComponentProcessingComponent in this Pipeline.
            List<IComponentProcessingComponent> var_ComponentProcessingComponentList = this.GetPipelineComponents<IComponentProcessingComponent>();

            // Process all Pipeline Components.
            for (int c = 0; c < var_ComponentProcessingComponentList.Count; c++)
            {
                // Deactivated Pipeline Components will be skipped.
                if (!var_ComponentProcessingComponentList[c].IsActive)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip component " + var_ComponentProcessingComponentList[c]);

                    continue;
                }

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Process component " + var_ComponentProcessingComponentList[c]);

                // Iterate all unity prefab components.
                for (int s = 0; s < _ComponentList.Count; s++)
                {
                    // Analyse each component!
                    try
                    {
                        // Analyse iterated component!
                        if (!var_ComponentProcessingComponentList[c].OnAnalyse_Component(_ComponentList[s]))
                        {
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, "Failed component " + var_ComponentProcessingComponentList[c]);

                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to process current iterated Component at index " + s + " with Exception: " + e.ToString());
                    }
                    
                }
            }

            AssetDatabase.StartAssetEditing();

            // TODO: OnProcess_Component

            AssetDatabase.StopAssetEditing();

            return true;
        }

        /// <summary>
        /// The fourth step of the pipeline is to iterate all components in the scenes included in the build settings and analyses and edits them, if the obfuscation requires it.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        private bool Process_Component_IterateBuildSceneComponents()
        {
            // Validate if the scene processing is enabled.
            bool var_SceneProcessingEnabled = false;

            // Get all IComponentProcessingComponent in this Pipeline.
            List<IComponentProcessingComponent> var_ComponentProcessingComponentList = this.GetPipelineComponents<IComponentProcessingComponent>();

            // Iterate all Pipeline Components and check if they require scene processing.
            for (int c = 0; c < var_ComponentProcessingComponentList.Count; c++)
            {
                // Check if the component should process scenes.
                if (var_ComponentProcessingComponentList[c].IsActive && var_ComponentProcessingComponentList[c].ProcessScenes)
                {
                    var_SceneProcessingEnabled = true;
                    break;
                }
            }

            // If no component requires scene processing, skip this step.
            if (!var_SceneProcessingEnabled)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip scene processing because no component requires it.");

                return true;
            }

            // Scene Path in Build.
            List<String> var_ScenesInBuild = Helper_Scene_GetScenesInBuild();

            // Scene Path of loaded Scenes.
            List<String> var_LoadedScenes = Helper_Scene_GetLoadedScenes();

            // Iterate loaded scenes included in the build.
            for (var i = 0; i < var_LoadedScenes.Count; i++)
            {
                // Get the loaded scene at index i.
                String var_ScenePath = var_LoadedScenes[i];

                // Scene is not included in the build settings.
                if (!var_ScenesInBuild.Contains(var_ScenePath))
                {
                    // Log - Skip loaded scene from analysis.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Skip loaded scene '" + var_ScenePath + "' from analysis because it is not included in the build settings.");
                    continue;
                }

                // Log - Iterate Components in Scene.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Iterate components in loaded build scene '" + var_ScenePath + "'.");

                try
                {
                    // Get the loaded scene.
                    Scene var_Scene = EditorSceneManager.GetSceneByPath(var_ScenePath);

                    // Get Components!
                    List<UnityEngine.Component> var_SceneComponentList = Helper_Scene_GetAllComponentsInScene(var_Scene);

                    // Process Components.
                    this.Process_Component_Analyse_And_Manipulate(var_SceneComponentList);

                    // Note: Scenes are not allowed to be closed here.
                }
                catch (Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to load and process components for loaded build scene " + var_ScenePath + ". Exception: " + e.ToString());
                }
                finally
                {
                    var_ScenesInBuild.Remove(var_ScenePath);
                }
            }

            // Iterate the other, not loaded, scenes included in the build.
            for (var i = 0; i < var_ScenesInBuild.Count; i++)
            {
                // Get the scene at index i but not loaded.
                String var_ScenePath = var_ScenesInBuild[i];

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Iterate components in not loaded build scene " + var_ScenePath + ".");

                try
                {
                    // Open it alone.
                    Scene var_Scene = EditorSceneManager.OpenScene(var_ScenePath, OpenSceneMode.Single);

                    // Get Components!
                    List<UnityEngine.Component> var_SceneComponentList = Helper_Scene_GetAllComponentsInScene(var_Scene);

                    // Process Components.
                    this.Process_Component_Analyse_And_Manipulate(var_SceneComponentList);

                    // Note: Scenes are not allowed to be closed here.
                }
                catch (Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to load and process components for not loaded build scene " + var_ScenePath + ". Exception: " + e.ToString());
                }
            }

            // Reload the scenes, active in the editor at build time.
            Helper_Scene_LoadScenes(var_LoadedScenes);

            // Make sure to unload all unused assets.
            EditorUtility.UnloadUnusedAssetsImmediate();

            return true;
        }

        /// <summary>
        /// The fifth step of the pipeline is to iterate all components in the prefabs in the project and analyses and edits them, if the obfuscation requires it.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        private bool Process_Component_IteratePrefabComponents()
        {
            // Validate if the prefab processing is enabled.
            bool var_PrefabProcessingEnabled = false;

            // Get all IComponentProcessingComponent in this Pipeline.
            List<IComponentProcessingComponent> var_ComponentProcessingComponentList = this.GetPipelineComponents<IComponentProcessingComponent>();

            // Iterate all Pipeline Components and check if they require prefab processing.
            for (int c = 0; c < var_ComponentProcessingComponentList.Count; c++)
            {
                // Check if the component should process prefabs.
                if (var_ComponentProcessingComponentList[c].IsActive && var_ComponentProcessingComponentList[c].ProcessPrefabs)
                {
                    var_PrefabProcessingEnabled = true;
                    break;
                }
            }

            // If no component requires prefab processing, skip this step.
            if (!var_PrefabProcessingEnabled)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip prefab processing because no component requires it.");

                return true;
            }

            // Get all prefabs references in the project.
            List<UnityAssetReference> var_UnityAssetReferenceList = this.DataContainer.UnityAssetCache.GetUnityAssetReferences(".prefab");

            // Log - Found Prefabs.
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Found '{0}' Prefabs.", var_UnityAssetReferenceList.Count));

            // Iterate all prefabs.
            for (int i = 0; i < var_UnityAssetReferenceList.Count; i++)
            {
                // Get the prefab path.
                String var_AssetPath = var_UnityAssetReferenceList[i].RelativeFilePath;

                // Try to load and process the prefab.
                try
                {
                    // Load Prefab as GameObject.
                    GameObject var_GameObject = AssetDatabase.LoadAssetAtPath<GameObject>(var_AssetPath);

                    if (var_GameObject == null)
                    {
                        // Log - Failed to load prefab.
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to load prefab at path: '{0}'.", var_AssetPath));

                        continue;
                    }

                    try
                    {
                        // Get all Components of the current GameObject.
                        UnityEngine.Component[] var_Components = var_GameObject.GetComponentsInChildren<UnityEngine.Component>(true);

                        // Log - Found Components.
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Found '{0}' Components in the prefab at path '{1}'.", var_Components.Length, var_AssetPath));

                        // Process the components in the prefab.
                        this.Process_Component_Analyse_And_Manipulate(var_Components.ToList());

                    }
                    catch (Exception var_Exception)
                    {
                        // Log - Failed to get components.
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to get components for gameobject '{0}' at path '{1}'. Exception: {2}", var_GameObject, var_AssetPath, var_Exception));
                    }

                    // Make sure to unload all unused assets.
                    EditorUtility.UnloadUnusedAssetsImmediate();
                }
                catch (Exception var_Exception)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to load prefab at path '{0}'. Exception: {1}", var_AssetPath, var_Exception));
                }
            }

            return true;
        }

        #endregion

        #endregion

        // Helper
        #region Helper

        // Scene - Helper
        #region Scene - Helper

        /// <summary>
        /// Get all scenes currently open in the editor.
        /// </summary>
        private static List<string> Helper_Scene_GetLoadedScenes()
        {
            List<string> var_OpenScenes = new List<string>();

#if UNITY_2023_1_OR_NEWER
            for (var i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                var_OpenScenes.Add(SceneManager.GetSceneAt(i).path);
            }
#else
            for (var i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                var_OpenScenes.Add(SceneManager.GetSceneAt(i).path);
            }
#endif

            return var_OpenScenes;
        }

        /// <summary>
        /// Get all enabled scenes in Build Settings.
        /// </summary>
        private static List<String> Helper_Scene_GetScenesInBuild()
        {
            return (from var_Scene in EditorBuildSettings.scenes where var_Scene.enabled select var_Scene.path).ToList();
        }

        /// <summary>
        /// Load multiple scenes in the editor.
        /// </summary>
        /// <param name="_Scenes">The scenes to load.</param>
        private static void Helper_Scene_LoadScenes(List<String> _Scenes)
        {
            // No scenes to load.
            if (_Scenes.Count == 0 || String.IsNullOrEmpty(_Scenes[0]))
            {
                return;
            }

            // Get the currently loaded scenes.
            List<String> var_LoadedScenes = Helper_Scene_GetLoadedScenes();

            // Check if the scenes are already loaded.
            if (var_LoadedScenes.SequenceEqual(_Scenes))
            {
                return;
            }

            // Open all scenes additively.
            EditorSceneManager.OpenScene(_Scenes[0], OpenSceneMode.Single);
            for (var i = 1; i < _Scenes.Count; i++)
            {
                EditorSceneManager.OpenScene(_Scenes[i], OpenSceneMode.Additive);
            }
        }

        /// <summary>
        /// Returns a list of all components in _Scene.
        /// </summary>
        /// <param name="_Scene">The scene to get the components from.</param>
        /// <returns>A list of all components in _Scene.</returns>
        private static List<UnityEngine.Component> Helper_Scene_GetAllComponentsInScene(Scene _Scene)
        {
            if(_Scene == null)
            {
                throw new ArgumentNullException("_Scene");
            }

            List<UnityEngine.Component> var_Result = new List<UnityEngine.Component>();

            // Iterate GameObjects.
            GameObject[] var_RootGameObjects = _Scene.GetRootGameObjects();

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found " + var_RootGameObjects.Length + " Root GameObjects in Scene " + _Scene.path);

            foreach (var var_GameObject in var_RootGameObjects)
            {
                if (var_GameObject == null)
                {
                    continue;
                }

                try
                {
                    // Get all Components of the current GameObject.
                    UnityEngine.Component[] var_Components = var_GameObject.GetComponentsInChildren<UnityEngine.Component>(true);

                    // Add to result.
                    var_Result.AddRange(var_Components);
                }
                catch (Exception e)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to get components for gameobject " + var_GameObject + " in scene " + _Scene.path + ". Exception: " + e.ToString());
                }
            }

            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found " + var_Result.Count + " Components in Scene " + _Scene.path);

            return var_Result;
        }

        #endregion

        #endregion
    }
}
