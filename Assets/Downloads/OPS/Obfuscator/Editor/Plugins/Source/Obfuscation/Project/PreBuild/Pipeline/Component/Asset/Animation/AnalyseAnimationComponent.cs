// System
using System;
using System.Collections.Generic;
using System.Linq;

// Unity
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// OPS - Project
using OPS.Editor.Project.Pipeline;
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Assets
using OPS.Obfuscator.Editor.Assets.Unity;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Project.PreBuild.Pipeline.Component
{
    /// <summary>
    /// Analyse model files, animation clips and controllers for animation methods and classes.
    /// </summary>
    internal class AnalyseAnimationComponent : APreBuildComponent, IAssetProcessingComponent
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
                return "Analyse Unity Animation Methods";
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
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CEnable_Try_Find_Inspector_Animation_Methods))
                {
                    return false;
                }

                return true;
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

        // Animation Methods
        #region Animation Methods

        /// <summary>
        /// The step output data key for the ReferencedAnimationMethodHashSet.
        /// </summary>
        public const String CAnimation_ReferencedAnimationMethodHashSet = "Animation_ReferencedAnimationMethodHashSet";

        /// <summary>
        /// A list of all references animation methods.
        /// </summary>
        private HashSet<String> ReferencedAnimationMethodHashSet = new HashSet<string>();

        #endregion

        // StateMachineBehaviour Types
        #region StateMachineBehaviour Types

        /// <summary>
        /// The step output data key for the ReferencedStateMachineBehaviourList.
        /// </summary>
        public const String CAnimation_ReferencedStateMachineBehaviourList = "Animation_ReferencedStateMachineBehaviourList";

        /// <summary>
        /// A list of all references StateMachineBehaviour types.
        /// </summary>
        private List<TypeKey> ReferencedStateMachineBehaviourList = new List<TypeKey>();

        #endregion

        // Asset
        #region Asset

        /// <summary>
        /// Extensions of supported model files.
        /// </summary>
        private static HashSet<String> modelFileExtension = new HashSet<string>()
        {
            ".fbx",
            ".dae",
            ".3ds",
            ".dxf",
            ".obj",
            ".skp",
        };

        /// <summary>
        /// Extensions of supported animation files.
        /// </summary>
        private static HashSet<String> animationFileExtension = new HashSet<string>()
        {
            ".anim",
        };

        /// <summary>
        /// Extensions of supported controller files.
        /// </summary>
        private static HashSet<String> controllerFileExtension = new HashSet<string>()
        {
            ".controller",
        };

        /// <summary>
        /// Returns all asset extensions to process by this component.
        /// </summary>
        public string[] AssetExtensionsToProcess
        {
            get
            {
                List<String> var_AssetExtensionList = new List<string>();

                // Add Models
                var_AssetExtensionList.AddRange(modelFileExtension);

                // Add Animations
                var_AssetExtensionList.AddRange(animationFileExtension);

                // Add Controllers
                var_AssetExtensionList.AddRange(controllerFileExtension);

                // Models and Animations
                return var_AssetExtensionList.ToArray();
            }
        }

        /// <summary>
        /// Process the asset and search for animation methods.
        /// </summary>
        /// <param name="_UnityAssetReference">The asset reference to process.</param>
        /// <returns>True if the asset was processed successfully; otherwise false.</returns>
        public bool OnAnalyse_Assets(UnityAssetReference _UnityAssetReference)
        {
            // 1. Model File Metadata

            // Check if searching model files is activated. Default is true.
            if (this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CEnable_Try_Find_Inspector_Animation_Methods_In_Models, true))
            {
                // Check if file is a model file.
                if (modelFileExtension.Contains(_UnityAssetReference.FileExtension))
                {
                    this.OnAnalyse_Assets_ModelFile(_UnityAssetReference);
                }
            }

            // 2. Animation Clip Files

            // Check if searching animation files is activated. Default is true.
            if (this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CEnable_Try_Find_Inspector_Animation_Methods_In_AnimationClips, true))
            {
                // Check if file is a animation clip.
                if (animationFileExtension.Contains(_UnityAssetReference.FileExtension))
                {
                    this.OnAnalyse_Assets_AnimationClip(_UnityAssetReference);
                }
            }

            // 3. Controller Files

            // Check if searching controller files is activated. Default is true.
            if (this.Step.Settings.Get_ComponentSettings_As_Bool(OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CSettingsKey, OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility.AnimationComponent.CEnable_Try_Find_Inspector_Animation_Methods_In_AnimatorControllers, true))
            {
                // Check if file is a controller file.
                if (controllerFileExtension.Contains(_UnityAssetReference.FileExtension))
                {
                    this.OnAnalyse_Assets_AnimationController(_UnityAssetReference);
                }
            }

            // 4. Make sure to unload the assets.
            EditorUtility.UnloadUnusedAssetsImmediate();

            return true;
        }

        /// <summary>
        /// Analyse model files and search for animation methods.
        /// </summary>
        /// <param name="_UnityAssetReference">The asset reference to process.</param>
        /// <returns>True if the asset was processed successfully; otherwise false.</returns>
        private bool OnAnalyse_Assets_ModelFile(UnityAssetReference _UnityAssetReference)
        {
            // Load all assets.
            UnityEngine.Object[] var_Assets = AssetDatabase.LoadAllAssetsAtPath(_UnityAssetReference.RelativeFilePath);

            // Check if there are assets.
            if (var_Assets == null || var_Assets.Length == 0)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "There is not model file at " + _UnityAssetReference.RelativeFilePath);

                return true;
            }

            // Try to process the assets.
            try
            {
                // A list of all animation methods in the model.
                List<String> var_AnimationMethodList = new List<String>();

                // Iterate all assets and search in found animation clips.
                for (int a = 0; a < var_Assets.Length; a++)
                {
                    if (var_Assets[a] is AnimationClip var_AnimationClip)
                    {
                        // Add the found methods in the animation clip.
                        var_AnimationMethodList.AddRange(this.GetMethodsInAnimationClip(var_AnimationClip));

                        // Unload the asset.
                        Resources.UnloadAsset(var_AnimationClip);
                    }
                }

                // Iterate found methods and add to global list.
                for (int m = 0; m < var_AnimationMethodList.Count; m++)
                {
                    if (!this.ReferencedAnimationMethodHashSet.Contains(var_AnimationMethodList[m]))
                    {
                        this.ReferencedAnimationMethodHashSet.Add(var_AnimationMethodList[m]);
                    }
                }

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Found methods in model file at path '{0}'", _UnityAssetReference.RelativeFilePath), var_AnimationMethodList);
            }
            catch (Exception var_Exception)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to analyse model file at path '{0}': {1}", _UnityAssetReference.RelativeFilePath, var_Exception));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Analyse animation clip files and search for animation methods.
        /// </summary>
        /// <param name="_UnityAssetReference">The asset reference to process.</param>
        /// <returns>True if the asset was processed successfully; otherwise false.</returns>
        private bool OnAnalyse_Assets_AnimationClip(UnityAssetReference _UnityAssetReference)
        {
            // Load Asset as AnimationClip.
            AnimationClip var_AnimationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(_UnityAssetReference.RelativeFilePath);

            // Check if there is an asset.
            if (var_AnimationClip == null)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "There is no AnimationClip at " + _UnityAssetReference.RelativeFilePath);

                return true;
            }

            // Try to process the assets.
            try
            {
                // Find methods in the animation clip.
                List<String> var_AnimationMethodList = this.GetMethodsInAnimationClip(var_AnimationClip);

                // Unload the asset.
                Resources.UnloadAsset(var_AnimationClip);

                // Iterate found methods and add to global list.
                for (int m = 0; m < var_AnimationMethodList.Count; m++)
                {
                    if (!this.ReferencedAnimationMethodHashSet.Contains(var_AnimationMethodList[m]))
                    {
                        this.ReferencedAnimationMethodHashSet.Add(var_AnimationMethodList[m]);
                    }
                }

                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Found methods in animation file at path '{0}'", _UnityAssetReference.RelativeFilePath), var_AnimationMethodList);
            }
            catch (Exception var_Exception)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to analyse animation file at path '{0}': {1}", _UnityAssetReference.RelativeFilePath, var_Exception));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a duplicate free list of methods in _AnimationClip.
        /// </summary>
        /// <param name="_AnimationClip"></param>
        /// <returns></returns>
        private List<String> GetMethodsInAnimationClip(AnimationClip _AnimationClip)
        {
            // Check if there are events.
            if (_AnimationClip.events == null)
            {
                return new List<string>();
            }

            // The result - duplicate free method list.
            HashSet<String> var_Result = new HashSet<string>();

            // Iterate all animation event methods.
            for (int e = 0; e < _AnimationClip.events.Length; e++)
            {
                // Check if event is not null!
                if (_AnimationClip.events[e] == null)
                {
                    continue;
                }

                // Add method to the result list if not already added.
                if (!var_Result.Contains(_AnimationClip.events[e].functionName))
                {
                    var_Result.Add(_AnimationClip.events[e].functionName);
                }
            }

            return var_Result.ToList();
        }

        /// <summary>
        /// Analyse animation controller files and search for StateMachineBehaviours.
        /// </summary>
        /// <param name="_UnityAssetReference">The asset reference to process.</param>
        /// <returns>True if the asset was processed successfully; otherwise false.</returns>
        private bool OnAnalyse_Assets_AnimationController(UnityAssetReference _UnityAssetReference)
        {
            // Load all assets.
            UnityEngine.Object[] var_Assets = AssetDatabase.LoadAllAssetsAtPath(_UnityAssetReference.RelativeFilePath);

            // Check if there are assets.
            if (var_Assets == null || var_Assets.Length == 0)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "There is not controller file at " + _UnityAssetReference.RelativeFilePath);

                return true;
            }

            try 
            { 
                // Iterate all assets and search in found animation controller.
                for (int a = 0; a < var_Assets.Length; a++)
                {
                    try
                    {
                        // If is directly a StateMachineBehaviour, add to list.
                        if (var_Assets[a] is StateMachineBehaviour var_StateMachineBehaviour)
                        {
                            // Get the type and add to list.
                            Type var_Type = var_StateMachineBehaviour.GetType();

                            TypeKey var_TypeKey = new TypeKey(var_Type);

                            if (!this.ReferencedStateMachineBehaviourList.Contains(var_TypeKey))
                            {
                                this.ReferencedStateMachineBehaviourList.Add(var_TypeKey);
                            }

                            // Log - Found StateMachineBehaviour reference.
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found StateMachineBehaviour reference " + var_Type + " in controller file: " + _UnityAssetReference.RelativeFilePath);
                        }

                        // If is a AnimatorStateMachine, search for StateMachineBehaviours.
                        if (var_Assets[a] is AnimatorStateMachine var_AnimatorStateMachine)
                        {
                            // Search the base for behaviours.
                            foreach (var var_Behaviour in var_AnimatorStateMachine.behaviours)
                            {
                                // Get the type and add to list.
                                Type var_Type = var_Behaviour.GetType();

                                TypeKey var_TypeKey = new TypeKey(var_Type);

                                if (!this.ReferencedStateMachineBehaviourList.Contains(var_TypeKey))
                                {
                                    this.ReferencedStateMachineBehaviourList.Add(var_TypeKey);
                                }

                                // Log - Found StateMachineBehaviour reference.
                                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found StateMachineBehaviour reference " + var_Type + " in controller file: " + _UnityAssetReference.RelativeFilePath);
                            }

                            // Search all child states for behaviours.
                            foreach (var var_State in var_AnimatorStateMachine.states)
                            {
                                foreach (var var_Behaviour in var_State.state.behaviours)
                                {
                                    Type var_Type = var_Behaviour.GetType();

                                    TypeKey var_TypeKey = new TypeKey(var_Type);

                                    if (!this.ReferencedStateMachineBehaviourList.Contains(var_TypeKey))
                                    {
                                        this.ReferencedStateMachineBehaviourList.Add(var_TypeKey);
                                    }

                                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, "Found StateMachineBehaviour reference " + var_Type + " in controller file: " + _UnityAssetReference.RelativeFilePath);
                                }
                            }
                        }

                        // Unload the asset.
                        Resources.UnloadAsset(var_Assets[a]);
                    }
                    catch (Exception var_Exception)
                    {
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to analyse controller file at path '{0}': {1}" + _UnityAssetReference.RelativeFilePath, var_Exception));
                    }
                }
            }
            catch (Exception var_Exception)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to analyse controller file at path '{0}': {1}", _UnityAssetReference.RelativeFilePath, var_Exception));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="_UnityAssetReference">The asset reference to process.</param>
        /// <returns>True if the asset was processed successfully; otherwise false.</returns>
        public bool OnProcess_Assets(UnityAssetReference _UnityAssetReference)
        {
            return true;
        }

        #endregion

        // On Post Process Pipeline
        #region On Post Process Pipeline

        /// <summary>
        /// After the pipeline process, add the found animation methods and the found animator classes to the datacontainer.
        /// </summary>
        /// <param name="_StepOutput"></param>
        /// <returns></returns>
        public override bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            bool var_Result = base.OnPostPipelineProcess(_StepOutput);

            // Add found animation methods to datacontainer.
            var_Result = var_Result && _StepOutput.Add(AnalyseAnimationComponent.CAnimation_ReferencedAnimationMethodHashSet, this.ReferencedAnimationMethodHashSet, true);

            // Add found animator clases to datacontainer.
            var_Result = var_Result && _StepOutput.Add(AnalyseAnimationComponent.CAnimation_ReferencedStateMachineBehaviourList, this.ReferencedStateMachineBehaviourList, true);

            return var_Result;
        }

        #endregion
    }
}
