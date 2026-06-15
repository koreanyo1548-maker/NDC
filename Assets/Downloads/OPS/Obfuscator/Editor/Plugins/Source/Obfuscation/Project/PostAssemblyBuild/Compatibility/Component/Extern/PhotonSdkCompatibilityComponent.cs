// System
using System;
using System.Collections.Generic;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Compatibility.Component
{
    /// <summary>
    /// The compatibility component for Photon Sdks.
    /// </summary>
    public class PhotonSdkCompatibilityComponent : AObfuscationCompatibilityComponent, INamespaceCompatibility, ITypeCompatibility, IMethodCompatibility
    {
        // Info
        #region Info

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Photon - Compatibility";
            }
        }

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override String Description
        {
            get
            {
                return "Controls the Obfuscator compatibility to Photon Sdks.";
            }
        }

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Controls the Obfuscator compatibility to Photon Sdks.";
            }
        }

        #endregion

        // Namespace
        #region Namespace

        /// <summary>
        /// The namespaces to skip.
        /// </summary>
        private List<String> namespaceToSkip = new List<string>()
        {
            "Photon",
        };

        /// <summary>
        /// Returns if the whole namespace should be skipped.
        /// </summary>
        public bool SkipWholeNamespace(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipPhotonActivated(_Step))
            {
                // Iterate over all namespaces to skip.
                for (int i = 0; i < this.namespaceToSkip.Count; i++)
                {
                    if (_TypeDefinition.Namespace.StartsWith(this.namespaceToSkip[i]))
                    {
                        _Cause = "Inside Photon Namespace";
                        return true;
                    }
                }
            }

            _Cause = "";
            return false;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public bool IsNamespaceRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = "";
            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string ApplyNamespaceRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Type
        #region Type

        /// <summary>
        /// The types to skip.
        /// </summary>
        private HashSet<String> typesToSkip = new HashSet<string>()
        {
            "Photon.MonoBehaviour",
            "Photon.PunBehaviour",
            "SupportLogging"
        };

        /// <summary>
        /// Does nothing.
        /// </summary>
        public bool SkipWholeType(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            _Cause = "";
            return false;
        }

        /// <summary>
        /// Returns true if the type should be renamed.
        /// </summary>
        public bool IsTypeRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipPhotonActivated(_Step))
            {
                // Iterate over all types to skip.
                if (this.typesToSkip.Contains(_TypeDefinition.Namespace + "." + _TypeDefinition.Name))
                {
                    _Cause = "Is Photon Class.";
                    return true;
                }

                if (this.typesToSkip.Contains(_TypeDefinition.Name))
                {
                    _Cause = "Is Photon Class.";
                    return true;
                }
            }

            _Cause = null;
            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string ApplyTypeRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, TypeDefinition _TypeDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Method
        #region Method

        /// <summary>
        /// The methods to skip.
        /// </summary>
        private HashSet<String> methodsToSkip = new HashSet<string>()
        {
            "OnConnectedToPhoton",
            "OnLeftRoom",
            "OnMasterClientSwitched",
            "OnPhotonCreateRoomFailed",
            "OnPhotonJoinRoomFailed",
            "OnCreatedRoom",
            "OnJoinedLobby",
            "OnLeftLobby",
            "OnDisconnectedFromPhoton",
            "OnConnectionFail",
            "OnFailedToConnectToPhoton",
            "OnReceivedRoomListUpdate",
            "OnJoinedRoom",
            "OnPhotonPlayerConnected",
            "OnPhotonPlayerDisconnected",
            "OnPhotonRandomJoinFailed",
            "OnConnectedToMaster",
            "OnPhotonSerializeView",
            "OnPhotonInstantiate",
            "OnPhotonMaxCccuReached",
            "OnPhotonCustomRoomPropertiesChanged",
            "OnPhotonPlayerPropertiesChanged",
            "OnUpdatedFriendList",
            "OnCustomAuthenticationFailed",
            "OnCustomAuthenticationResponse",
            "OnWebRpcResponse",
            "OnOwnershipRequest",
            "OnLobbyStatisticsUpdate",
        };

        /// <summary>
        /// Returns true if the method should be renamed.
        /// </summary>
        public bool IsMethodRenamingAllowed(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition, out string _Cause)
        {
            if (this.Helper_Has_SkipPhotonActivated(_Step))
            {
                // Iterate over all methods to skip.
                if (AttributeHelper.HasCustomAttribute(_MethodDefinition, "PunRPC"))
                {
                    _Cause = "Has PunRPCAttribute.";
                    return false;
                }

                if (this.methodsToSkip.Contains(_MethodDefinition.Name))
                {
                    _Cause = "Is Photon Method.";
                    return false;
                }
            }

            _Cause = null;
            return true;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public string ApplyMethodRenamingFilter(PostAssemblyBuildStep _Step, AssemblyInfo _AssemblyInfo, MethodDefinition _MethodDefinition, string _OriginalName, string _CurrentName)
        {
            return _CurrentName;
        }

        #endregion

        // Settings
        #region Settings

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public const String CSettingsKey = "Default_Compatibility_Component_Photon_Sdk";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public override String SettingsKey { get; } = CSettingsKey;

        /// <summary>
        /// The settings key for the skip Photon setting.
        /// </summary>
        public const String CSkip_Photon = "Skip_Photon";

        /// <summary>
        /// Helper method to check if the user has activated the skip Photon setting.
        /// </summary>
        private bool Helper_Has_SkipPhotonActivated(PostAssemblyBuildStep _Step)
        {
            return _Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CSkip_Photon, true);
        }

        #endregion

        // Gui
        #region Gui

        /// <summary>
        /// Get the GUI container for this component.
        /// </summary>
        /// <param name="_ComponentSettings">The settings for this component.</param>
        /// <returns>The GUI container for this component.</returns>
        public override ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name, _ComponentSettings, CSkip_Photon);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        #endregion
    }
}
