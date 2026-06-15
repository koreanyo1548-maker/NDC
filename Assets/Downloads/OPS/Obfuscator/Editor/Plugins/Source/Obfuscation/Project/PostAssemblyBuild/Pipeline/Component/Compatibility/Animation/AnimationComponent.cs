// System
using System;
using System.Collections.Generic;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Project
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
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Compatibility
{
    /// <summary>
    /// Skips the animation methods found in the anaylse pipeline.
    /// </summary>
    public class AnimationComponent : APostAssemblyBuildComponent, IGuiComponent, IAssemblyProcessingComponent
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
                return "Unity Animation Methods - Compatibility";
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
                return "Manages the obfuscation of animation methods set through the inspector.";
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
                return "Manages the obfuscation of inspector set animation methods.";
            }
        }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public const String CSettingsKey = "Default_Obfuscation_Component_Find_Unity_Animation_MethodReferences";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        /// <summary>
        /// The settings key to enable the try find inspector animation methods.
        /// </summary>
        public const String CEnable_Try_Find_Inspector_Animation_Methods = "Enable_Try_Find_Inspector_Animation_Methods";

        /// <summary>
        /// The settings key to enable the try find inspector animation methods in models.
        /// </summary>
        public const String CEnable_Try_Find_Inspector_Animation_Methods_In_Models = "Enable_Try_Find_Inspector_Animation_Methods_In_Models";

        /// <summary>
        /// The settings key to enable the try find inspector animation methods in animations.
        /// </summary>
        public const String CEnable_Try_Find_Inspector_Animation_Methods_In_AnimationClips = "Enable_Try_Find_Inspector_Animation_Methods_In_AnimationClips";

        /// <summary>
        /// The settings key to enable the try find inspector animation methods in animator controllers.
        /// </summary>
        public const String CEnable_Try_Find_Inspector_Animation_Methods_In_AnimatorControllers = "Enable_Try_Find_Inspector_Animation_Methods_In_AnimatorControllers";

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
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CEnable_Try_Find_Inspector_Animation_Methods);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // Explanation
            Row_Text var_ExplanationRow = new Row_Text("");
            var_ExplanationRow.Notification_Info = "Attempts to find all animation methods (set via the Unity Editor inspector) in the assets and skips them. This must be done because Unity calls these methods by name via reflection. To manually trigger skipping, add an 'OPS.Obfuscator.DoNotRename' attribute to these animation methods in your code.";
            var_Content.AddRow(var_ExplanationRow);

            // Search in model files 
            Row_Boolean var_EnableModelSearch = new Row_Boolean("Search model files:", _ComponentSettings, CEnable_Try_Find_Inspector_Animation_Methods_In_Models);
            var_EnableModelSearch.Notification_Info = "Searches for animation methods in model files (.fbx, .dae, .3ds, .dxf, .obj, and .skp formats).";
            var_Content.AddRow(var_EnableModelSearch);

            // Search in animation files
            Row_Boolean var_EnableAnimationSearch = new Row_Boolean("Search animation clip files:", _ComponentSettings, CEnable_Try_Find_Inspector_Animation_Methods_In_AnimationClips);
            var_EnableAnimationSearch.Notification_Info = "Searches for animation methods in animation files (.anim format).";
            var_Content.AddRow(var_EnableAnimationSearch);

            // Search in animator controller files
            Row_Boolean var_EnableAnimatorControllerSearch = new Row_Boolean("Search animator controller files:", _ComponentSettings, CEnable_Try_Find_Inspector_Animation_Methods_In_AnimatorControllers);
            var_EnableAnimatorControllerSearch.Notification_Info = "Searches for animation methods in animator controller files (.controller format).";
            var_Content.AddRow(var_EnableAnimatorControllerSearch);

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
                if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Try_Find_Inspector_Animation_Methods))
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        // Animation Methods
        #region Animation Methods

        /// <summary>
        /// A hashset of all references animation methods.
        /// </summary>
        private HashSet<String> ReferencedAnimationMethodHashSet = new HashSet<string>();

        #endregion

        // StateMachineBehaviour Classes
        #region StateMachineBehaviour Classes

        /// <summary>
        /// A list of all references StateMachineBehaviour classes.
        /// </summary>
        private List<TypeKey> ReferencedStateMachineBehaviourList = new List<TypeKey>();

        #endregion

        // On Pre Process Pipeline
        #region On Pre Process Pipeline

        public override bool OnPrePipelineProcess(IStepInput _StepInput)
        {
            bool var_Result = base.OnPrePipelineProcess(_StepInput);

            // Read ReferencedAnimationMethodHashSet from step input.
            this.ReferencedAnimationMethodHashSet = _StepInput.Get<HashSet<String>>(PreBuild.Pipeline.Component.AnalyseAnimationComponent.CAnimation_ReferencedAnimationMethodHashSet);

            // Read Animation_ReferencedStateMachineBehaviourList from step input.
            this.ReferencedStateMachineBehaviourList = _StepInput.Get<List<TypeKey>>(PreBuild.Pipeline.Component.AnalyseAnimationComponent.CAnimation_ReferencedStateMachineBehaviourList);

            return var_Result;
        }

        #endregion

        // Analyse A
        #region Analyse A

        public bool OnAnalyse_Assemblies(AssemblyInfo _AssemblyInfo)
        {
            // Skip the types based on the found StateMachineBehaviour types.
            this.OnAnalyse_A_Skip_StateMachineBehaviour_Types(_AssemblyInfo);

            // Skip the methods based on the found animation methods.
            this.OnAnalyse_A_Skip_Animiation_Methods(_AssemblyInfo);

            return true;
        }

        /// <summary>
        /// Analyse for referenced StateMachineBehaviour types. Skip those types and serializeable fields.
        /// </summary>
        /// <param name="_AssemblyInfo">The assembly info to analyse.</param>
        private void OnAnalyse_A_Skip_StateMachineBehaviour_Types(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip StateMachineBehaviour Types...");

            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Continue only ScriptableObject!
                if (!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsScriptableObject(var_TypeDefinition, this.Step.GetCache<TypeCache>()))
                {
                    continue;
                }

                // Check if the type is a referenced StateMachineBehaviour, if so do not obfuscate the type and the namespace.
                if (this.ReferencedStateMachineBehaviourList.Contains(new TypeKey(var_TypeDefinition)))
                {
                    // Define the cause.
                    String var_DoNotObuscateCause = "Because the type is a referenced StateMachineBehaviour. If obfuscated, the animations will no longer work correctly.";

                    // Do not obfuscate the namespace and type.
                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Namespace, var_TypeDefinition, var_DoNotObuscateCause);
                    this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Type, var_TypeDefinition, var_DoNotObuscateCause);

                    // Log the cause.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Type [" + var_TypeDefinition.GetExtendedOriginalFullName(this.Step.GetCache<TypeCache>()) + "] " + var_DoNotObuscateCause);

                    // Iterate all fields in the type and do not obfuscate the serializable fields.
                    foreach (FieldDefinition var_FieldDefinition in var_TypeDefinition.Fields)
                    {
                        // Do not obfuscate public fields and serializable fields.
                        if (var_FieldDefinition.IsPublic || OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.FieldDefinitionHelper.IsFieldSerializeAble(var_FieldDefinition))
                        {
                            // Do not obfuscate the field.
                            this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Field, var_FieldDefinition, var_DoNotObuscateCause);

                            // Log the cause.
                            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "Skip Field [" + var_FieldDefinition.GetExtendedOriginalFullName(this.Step.GetCache<FieldCache>()) + "] " + var_DoNotObuscateCause);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Analyse for referenced animation methods. Skip those methods.
        /// </summary>
        /// <param name="_AssemblyInfo">The assembly info to analyse.</param>
        private void OnAnalyse_A_Skip_Animiation_Methods(AssemblyInfo _AssemblyInfo)
        {
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Info, "[" + _AssemblyInfo.Name + "] Skip Animation Methods...");

            // Iterate all types!
            foreach (TypeDefinition var_TypeDefinition in _AssemblyInfo.GetAllTypeDefinitions())
            {
                // Continue only MonoBehaviours!
                if (!OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsMonoBehaviour(var_TypeDefinition, this.Step.GetCache<TypeCache>()))
                {
                    continue;
                }

                // Iterate all methods in the type and do not obfuscate the animation methods.
                foreach (MethodDefinition var_MethodDefinition in var_TypeDefinition.Methods)
                {
                    if (this.ReferencedAnimationMethodHashSet.Contains(var_MethodDefinition.Name))
                    {
                        // Define the cause.
                        String var_DoNotObuscateCause = "Because the method might be a animation method.";

                        // Do not obfuscate the method.
                        this.DataContainer.RenameManager.AddDoNotObfuscate(OPS.Obfuscator.Editor.Assembly.Mono.Member.EMemberType.Method, var_MethodDefinition, var_DoNotObuscateCause);

                        // Log the cause.
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
