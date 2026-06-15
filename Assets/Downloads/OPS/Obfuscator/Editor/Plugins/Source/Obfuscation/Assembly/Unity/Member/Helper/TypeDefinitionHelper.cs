using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly - Mono
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;

namespace OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper
{
    public class TypeDefinitionHelper
    {
        // Base
        #region Base

        /// <summary>
        /// True: Some base class is a MonoBehaviour.
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <returns></returns>
        public static bool IsMonoBehaviour(TypeDefinition _Type, TypeCache _TypeCache)
        {
            return Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _TypeCache, "UnityEngine.MonoBehaviour");
        }

        /// <summary>
        /// True: Some base class is a NetworkBehaviour.
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <returns></returns>
        public static bool IsNetworkBehaviour(TypeDefinition _Type, TypeCache _TypeCache)
        {
            return Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _TypeCache, "UnityEngine.Networking.NetworkBehaviour");
        }

        /// <summary>
        /// True: Some base class is a ScriptableObject.
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <returns></returns>
        public static bool IsScriptableObject(TypeDefinition _Type, TypeCache _TypeCache)
        {
            return Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _TypeCache, "UnityEngine.ScriptableObject");
        }

        /// <summary>
        /// True: Some base class is a Playable. 
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <returns></returns>
        public static bool IsPlayable(TypeDefinition _Type, TypeCache _TypeCache)
        {
            return Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _TypeCache, "UnityEngine.Playables.Playable");
        }

        /// <summary>
        /// True: Some base class is a PlayableAsset. 
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <returns></returns>
        public static bool IsPlayableAsset(TypeDefinition _Type, TypeCache _TypeCache)
        {
            return Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _TypeCache, "UnityEngine.Playables.PlayableAsset");
        }

        /// <summary>
        /// True: Some base class is a PlayableBehaviour. 
        /// </summary>
        /// <param name="_Type"></param>
        /// <param name="_TypeCache"></param>
        /// <returns></returns>
        public static bool IsPlayableBehaviour(TypeDefinition _Type, TypeCache _TypeCache)
        {
            return Mono.Member.Helper.TypeDefinitionHelper.InheritsFrom(_Type, _TypeCache, "UnityEngine.Playables.PlayableBehaviour");
        }

        #endregion

        // Serialization
        #region Serialization

        /// <summary>
        /// True: The _TypeDefinition has a field that is serializable.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsSomeFieldInTypeSerializable(TypeDefinition _TypeDefinition)
        {
            if(_TypeDefinition == null)
            {
                throw new ArgumentNullException("_TypeDefinition is null!");
            }

            try
            {
                foreach (FieldDefinition var_FieldDefinition in _TypeDefinition.Fields)
                {
                    if (FieldDefinitionHelper.IsFieldSerializeAble(var_FieldDefinition))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Exception: " + e.ToString());
            }

            return false;
        }

        /// <summary>
        /// True: The _TypeDefinition or any base has a field that is serializable.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public static bool IsSomeFieldInTypeOrBaseSerializable(TypeDefinition _TypeDefinition)
        {
            if (_TypeDefinition == null)
            {
                throw new ArgumentNullException("_TypeDefinition is null!");
            }

            try
            {
                TypeDefinition var_CurrentTypeDefinition = _TypeDefinition;

                while (var_CurrentTypeDefinition != null)
                {
                    // Check some field is Serializeable.
                    if (IsSomeFieldInTypeSerializable(var_CurrentTypeDefinition))
                    {
                        return true;
                    }

                    // Has no base type.
                    if (var_CurrentTypeDefinition.BaseType == null)
                    {
                        return false;
                    }

                    // Cannot resolve base type!
                    if (!Mono.Member.Helper.MemberReferenceHelper.TryToResolve(var_CurrentTypeDefinition.BaseType, out var_CurrentTypeDefinition))
                    {
                        // Log
                        Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Cannot resolve base: " + Mono.Member.Helper.MemberReferenceHelper.GetExtendedFullName(var_CurrentTypeDefinition.BaseType));

                        // Cannot resolve!!
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                // Log
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Exception: " + e.ToString());
            }

            return false;
        }

        #endregion
    }
}
