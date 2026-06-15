using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Extension
{
    /// <summary>
    /// Extension class for IMemberDefinition.
    /// </summary>
    public static class MemberReferenceExtension
    {
        // Resolve
        #region Resolve

        /// <summary>
        /// Tries to resolve _MemberReference. If failed, returns false.
        /// </summary>
        /// <param name="_MemberReference"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public static bool TryToResolve(this MemberReference _MemberReference, out IMemberDefinition _MemberDefinition)
        {
            return MemberReferenceHelper.TryToResolve<IMemberDefinition>(_MemberReference, out _MemberDefinition);
        }

        /// <summary>
        /// Tries to resolve _MemberReference. If failed, returns false.
        /// </summary>
        /// <param name="_MemberReference"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public static bool TryToResolve<TMemberDefinition>(this MemberReference _MemberReference, out TMemberDefinition _MemberDefinition)
            where TMemberDefinition : IMemberDefinition
        {
            return MemberReferenceHelper.TryToResolve<TMemberDefinition>(_MemberReference, out _MemberDefinition);
        }

        #endregion

            // FullName
            #region FullName

            /// <summary>
            /// Returns the extended full name of a MemberReference.
            /// Looks like: '[AssemblyName] FullName'.
            /// </summary>
            /// <param name="_MemberReference"></param>
            /// <returns></returns>
        public static string GetExtendedFullName(this MemberReference _MemberReference)
        {
            // Return from MemberReferenceHelper.
            return MemberReferenceHelper.GetExtendedFullName(_MemberReference);
        }

        /// <summary>
        /// Returns the extended original full name of a MemberReference.
        /// Looks like: '[AssemblyName] Original FullName'.
        /// </summary>
        /// <param name="_MemberReference"></param>
        /// <param name="_BelongingCache"></param>
        /// <returns></returns>
        public static string GetExtendedOriginalFullName(this MemberReference _MemberReference, IMemberCache _BelongingCache)
        {
            // Resolve to IMemberDefinition
            if(!_MemberReference.TryToResolve(out IMemberDefinition var_MemberDefinition))
            {
                UnityEngine.Debug.Log("Could not resolve");

                return null;
            }

            // Return from IMemberDefinitionExtension.
            return IMemberDefinitionHelper.GetExtendedOriginalFullName(var_MemberDefinition, _BelongingCache);
        }

        #endregion
    }
}
