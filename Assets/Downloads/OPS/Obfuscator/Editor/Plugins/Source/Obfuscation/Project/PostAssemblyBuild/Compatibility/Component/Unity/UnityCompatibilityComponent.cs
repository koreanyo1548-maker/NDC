using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

//OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component
{
    public class UnityCompatibilityComponent : AObfuscationCompatibilityComponent, INamespaceCompatibility, ITypeCompatibility, IMethodCompatibility, IFieldCompatibility
    {
        //Info
        #region Info

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Unity - Compatibility";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Controls the Obfuscator compatibility to Unity. Without this the obfuscated Assembly would not be compatible with Unity.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Controls the Obfuscator compatibility to Unity.";
            }
        }

        #endregion

        // Helper
        #region Helper

        /// <summary>
        /// False: Either Obfuscator Free or CObfuscate_Class_Serializable is false.
        /// True: CObfuscate_Class_Serializable is true.
        /// </summary>
        /// <param name="_Step"></param>
        /// <returns></returns>
        private static bool Helper_Setting_Obfuscate_Class_Serializable(PostAssemblyBuildStep _Step)
        {
#if Obfuscator_Free

            return false;

#else

            return _Step.Settings.Get_ComponentSettings_As_Bool(PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CSettingsKey, PostAssemblyBuild.Pipeline.Component.Member.Type.TypeObfuscationComponent.CObfuscate_Class_Serializable);

#endif
        }

        /// <summary>
        /// False: Either Obfuscator Free or CObfuscate_Field_Serializable is false.
        /// True: CObfuscate_Field_Serializable is true.
        /// </summary>
        /// <param name="_Step"></param>
        /// <returns></returns>
        private static bool Helper_Setting_Obfuscate_Field_Serializable(PostAssemblyBuildStep _Step)
        {
#if Obfuscator_Free

            return false;

#else

            return _Step.Settings.Get_ComponentSettings_As_Bool(Pipeline.Component.Member.Field.FieldObfuscationComponent.CSettingsKey, Pipeline.Component.Member.Field.FieldObfuscationComponent.CObfuscate_Field_Serializable);

#endif
        }

        /// <summary>
        /// False: Either Obfuscator Free or CObfuscate_Field_Public_Unity is false.
        /// True: CObfuscate_Field_Public_Unity is true.
        /// </summary>
        /// <param name="_Step"></param>
        /// <returns></returns>
        private static bool Helper_Setting_Obfuscate_Field_Public_Unity(PostAssemblyBuildStep _Step)
        {
#if Obfuscator_Free

            return false;

#else

            return _Step.Settings.Get_ComponentSettings_As_Bool(Pipeline.Component.Member.Field.FieldObfuscationComponent.CSettingsKey, Pipeline.Component.Member.Field.FieldObfuscationComponent.CObfuscate_Field_Public_Unity);

#endif
        }

        #endregion

        //Namespace
        #region Namespace

        public bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = "";
            return false;
        }

        public bool IsNamespaceRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            // Serialization - Check if obfuscation of serializeable fields is deactivated by settings.
            if (!Helper_Setting_Obfuscate_Field_Serializable(_Step))
            {
                // Is deactivated, so do not obfuscate type, if some field or base field is serializeable.
                if (OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsSomeFieldInTypeOrBaseSerializable(_TypeDefinition))
                {
                    _Cause = "The class or base class contains some serializeable fields and the 'Obfuscate Serializeable Fields' settings is deactivated.";
                    return false;
                }
            }

            // RuntimeInitializeOnLoadMethodAttribute
            for (int m = 0; m < _TypeDefinition.Methods.Count; m++)
            {
                if (AttributeHelper.HasCustomAttribute(_TypeDefinition.Methods[m], "RuntimeInitializeOnLoadMethodAttribute"))
                {
                    _Cause = "Has RuntimeInitializeOnLoadMethodAttribute.";
                    return false;
                }
            }

            _Cause = null;
            return true;
        }

        public string ApplyNamespaceRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Type
        #region Type

        public bool SkipWholeType(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = null;
            return false;
        }

        public bool IsTypeRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            // Serialization - Check if obfuscation of serializeable fields is deactivated by settings.
            if (!Helper_Setting_Obfuscate_Field_Serializable(_Step))
            {
                // Is deactivated, so do not obfuscate type, if some field or base field is serializeable.
                if (OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsSomeFieldInTypeOrBaseSerializable(_TypeDefinition))
                {
                    _Cause = "The class or base class contains some serializeable fields and the 'Obfuscate Serializeable Fields' settings is deactivated.";
                    return false;
                }
            }

            // RuntimeInitializeOnLoadMethodAttribute
            for (int m = 0; m < _TypeDefinition.Methods.Count; m++)
            {
                if (AttributeHelper.HasCustomAttribute(_TypeDefinition.Methods[m], "RuntimeInitializeOnLoadMethodAttribute"))
                {
                    _Cause = "Has RuntimeInitializeOnLoadMethodAttribute.";
                    return false;
                }
            }

            _Cause = null;
            return true;
        }

        public string ApplyTypeRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Method
        #region Method

        private HashSet<String> editor_AssetModificationProcessor_Methods_ToSkip = new HashSet<string>()
        {
            "IsOpenForEdit",
            "OnWillCreateAsset",
            "OnWillDeleteAsset",
            "OnWillMoveAsset",
            "OnWillSaveAssets"
        };

        private HashSet<String> editor_AssetPostprocessor_Methods_ToSkip = new HashSet<string>()
        {
            "OnAssignMaterialModel",
            "OnPostprocessAllAssets",
            "OnPostprocessAssetbundleNameChanged",
            "OnPostprocessAudio",
            "OnPostprocessGameObjectWithUserProperties",
            "OnPostprocessModel",
            "OnPostprocessSpeedTree",
            "OnPostprocessSprites",
            "OnPostprocessTexture",
            "OnPreprocessAnimation",
            "OnPreprocessAudio",
            "OnPreprocessModel",
            "OnPreprocessSpeedTree",
            "OnPreprocessTexture"
        };

        private HashSet<String> editor_Editor_Methods_ToSkip = new HashSet<string>()
        {
            "OnSceneGUI"
        };

        private HashSet<String> editor_EditorWindow_Methods_ToSkip = new HashSet<string>()
        {
            "Awake",
            "OnDestroy",
            "OnFocus",
            "OnGUI",
            "OnHierarchyChange",
            "OnInspectorUpdate",
            "OnLostFocus",
            "OnProjectChange",
            "OnSelectionChange",
            "Update"
        };

        private HashSet<String> editor_ScriptableWizard_Methods_ToSkip = new HashSet<string>()
        {
            "OnWizardCreate",
            "OnWizardOtherButton",
            "OnWizardUpdate"
        };

        private HashSet<String> game_Collision_Methods_ToSkip = new HashSet<string>()
        {
            "OnCollisionEnter2D",
            "OnCollisionExit2D",
            "OnCollisionStay2D",
            "OnTriggerEnter2D",
            "OnTriggerExit2D",
            "OnTriggerStay2D",
            "OnCollisionEnter",
            "OnCollisionExit",
            "OnCollisionStay",
            "OnTriggerEnter",
            "OnTriggerExit",
            "OnTriggerStay"
        };

        private HashSet<String> game_Joint_Methods_ToSkip = new HashSet<string>()
        {
            "OnJointBreak",
            "OnJointBreak2D"
        };

        private HashSet<String> game_Playable_Methods_ToSkip = new HashSet<string>()
        {
            "OnSetPlayState",
            "OnSetTime",
            "PrepareFrame"
        };

        private HashSet<String> game_Renderer_Methods_ToSkip = new HashSet<string>()
        {
            "OnBecameInvisible",
            "OnBecameVisible"
        };

        private HashSet<String> game_StateMachineBehaviour_Methods_ToSkip = new HashSet<string>()
        {
            "OnStateEnter",
            "OnStateExit",
            "OnStateIK",
            "OnStateMove",
            "OnStateUpdate"
        };

        private HashSet<String> game_ScriptableObject_Methods_ToSkip = new HashSet<string>()
        {
            "Awake",
            "OnDestroy",
            "OnDisable",
            "OnEnable"
        };

        private HashSet<String> game_MonoBehaviour_Methods_ToSkip = new HashSet<string>()
        {
            "Main",
            "Awake",
            "FixedUpdate",
            "LateUpdate",
            "OnAnimatorIK",
            "OnAnimatorMove",
            "OnApplicationFocus",
            "OnApplicationPause",
            "OnApplicationQuit",
            "OnAudioFilterRead",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnCollisionEnter",
            "OnCollisionEnter2D",
            "OnCollisionExit",
            "OnCollisionExit2D",
            "OnCollisionStay",
            "OnCollisionStay2D",
            "OnConnectedToServer",
            "OnControllerColliderHit",
            "OnDestroy",
            "OnDisable",
            "OnDisconnectedFromServer",
            "OnDrawGizmos",
            "OnDrawGizmosSelected",
            "OnEnable",
            "OnFailedToConnect",
            "OnFailedToConnectToMasterServer",
            "OnGUI",
            "OnJointBreak",
            "OnJointBreak2D",
            "OnMasterServerEvent",
            "OnMouseDown",
            "OnMouseDrag",
            "OnMouseEnter",
            "OnMouseExit",
            "OnMouseOver",
            "OnMouseUp",
            "OnMouseUpAsButton",
            "OnNetworkInstantiate",
            "OnParticleCollision",
            "OnParticleSystemStopped",
            "OnParticleTrigger",
            "OnParticleUpdateJobScheduled",
            "OnPlayerConnected",
            "OnPlayerDisconnected",
            "OnPostRender",
            "OnPreCull",
            "OnPreRender",
            "OnRectTransformDimensionsChange",
            "OnRenderImage",
            "OnRenderObject",
            "OnSerializeNetworkView",
            "OnServerInitialized",
            "OnTransformChildrenChanged",
            "OnTransformParentChanged",
            "OnTriggerEnter",
            "OnTriggerEnter2D",
            "OnTriggerExit",
            "OnTriggerExit2D",
            "OnTriggerStay",
            "OnTriggerStay2D",
            "OnValidate",
            "OnWillRenderObject",
            "Reset",
            "Start",
            "Update",
        };

        // TODO:

        /*private List<String> ufpsMethodsStartWithToSkip = new List<string>()
        {
            "OnMessage_",
            "OnValue_",
            "OnAttempt_",
            "CanStart_",
            "CanStop_",
            "OnStart_",
            "OnStop_",
            "OnFailStart_",
            "OnFailStop_"
        };*/

        /*private HashSet<String> nguiMethodsToSkip = new HashSet<string>()
        {
            "OnHover",
            "OnSelect",
            "OnInput",
            "OnScroll",
            "OnKey",
            "OnPress",
            "OnDrag",
            "OnClick",
            "OnDoubleClick",
            "OnDrop",
            "OnTooltip"
        };*/

        public bool IsMethodRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition, out string _Cause)
        {
            //UnityEditor method to check
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEditor.AssetModificationProcessor"))
            {
                if (this.editor_AssetModificationProcessor_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Editor Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEditor.AssetPostprocessor"))
            {
                if (this.editor_AssetPostprocessor_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Editor Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEditor.Editor"))
            {
                if (this.editor_Editor_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Editor Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEditor.EditorWindow"))
            {
                if (this.editor_EditorWindow_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Editor Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEditor.ScriptableWizard"))
            {
                if (this.editor_ScriptableWizard_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Editor Method.";
                    return false;
                }
            }
            //UnityEngine Methods to check
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.Joint")
                || OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.Joint2D"))
            {
                if (this.game_Joint_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Joint Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.Collider")
                || OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.Collider2D"))
            {
                if (this.game_Collision_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Collision Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.Renderer"))
            {
                if (this.game_Renderer_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Renderer Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.StateMachineBehaviour"))
            {
                if (this.game_StateMachineBehaviour_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is StateMachineBehaviour Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.ScriptableObject"))
            {
                if (this.game_ScriptableObject_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is ScriptableObject Method.";
                    return false;
                }
            }
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_MethodDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>(), "UnityEngine.MonoBehaviour"))
            {
                if (this.game_MonoBehaviour_Methods_ToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is MonoBehaviour Method.";
                    return false;
                }
            }

            //Unity Attributes
            if (AttributeHelper.HasCustomAttribute(_MethodDefinition, "RuntimeInitializeOnLoadMethodAttribute"))
            {
                _Cause = "Has RuntimeInitializeOnLoadMethodAttribute.";
                return false;
            }

            _Cause = null;
            return true;
        }

        public string ApplyMethodRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        //Field
        #region Field

        public bool IsFieldRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, out string _Cause)
        {
            // ### Field -> Obfuscate Serializeable Fields ###

            // If the Field has the 'SerializeField' attribute and settings forbidde obfuscation of serializeable fields. Then skip this field.
            if (OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.FieldDefinitionHelper.IsFieldSerializeAble(_FieldDefinition)
                && !Helper_Setting_Obfuscate_Field_Serializable(_Step))
            {
                // Cause
                _Cause = "The field has a 'SerializeField' attribute and therefore is serializeable. The disabled setting 'Obfuscate SerializeAble Fields' deactivates obfuscation of serializeable fields.";
                return false;
            }

            // If the Field is public and declaring type is serializeable and settings forbidde obfuscation of serializeable fields. Then skip this field.
            if (_FieldDefinition.IsPublic
                && OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.IsTypeOrBaseSerializable(_FieldDefinition.DeclaringType)
                && !Helper_Setting_Obfuscate_Field_Serializable(_Step))
            {
                // Cause
                _Cause = "The field is public and in a serializeable class and therefore is serializeable. The disabled setting 'Obfuscate SerializeAble Fields' deactivates obfuscation of serializeable fields.";
                return false;
            }

            // If the Field is serializeable ('SerializeField' attribute or public) and is in a serializeable class and settings forbidde obfuscation of serializeable classes. Then skip this field.
            if (OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper.TypeDefinitionHelper.IsTypeOrBaseSerializable(_FieldDefinition.DeclaringType)
                && !Helper_Setting_Obfuscate_Class_Serializable(_Step))
            {
                if (OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.FieldDefinitionHelper.IsFieldSerializeAble(_FieldDefinition)
                    || _FieldDefinition.IsPublic)
                {
                    // Cause
                    _Cause = "The field is in a serializeable class and serializeable. The disabled setting 'Obfuscate SerializeAble Classes' deactivates obfuscation of serializeable fields.";
                    return false;
                }
            }

            // ### Field -> Obfuscate Public Unity Fields ###

            // If Field is Public and in a Unity Class and the settings disable obfuscation of public unity fields. Then skip this field.
            if (_FieldDefinition.IsPublic)
            {
                // If Fields Type is a Unity Type
                if (OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsMonoBehaviour(_FieldDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>())
                    || OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper.TypeDefinitionHelper.IsScriptableObject(_FieldDefinition.DeclaringType, _Step.GetCache<Assembly.Mono.Member.Cache.TypeCache>()))
                {
                    // Setting disallows obfuscation of unity public fields.
                    if (!Helper_Setting_Obfuscate_Field_Public_Unity(_Step))
                    {
                        //Cause
                        _Cause = "The class is a Unity class (MonoBehaviour/...) and the setting 'Obfuscate Public unity fields' is deactivated.";
                        return false;
                    }

                    // Cannot obfuscate Fields in Unity Classes in extern dlls. Todo: But why?
                    if (!_AssemblyInfo.AssemblyLoadInfo.IsUnityAssembly)
                    {
                        // Cause
                        _Cause = "The class is a Unity class (MonoBehaviour/...) and the field is in a external assembly.";
                        return false;
                    }
                }
            }

            _Cause = null;
            return true;
        }

        public string ApplyFieldRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, FieldDefinition _FieldDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Compatibility_Component_Unity";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        #endregion

        //Gui
        #region Gui

        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            //Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name);

            //Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            //Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            //Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion
    }
}
