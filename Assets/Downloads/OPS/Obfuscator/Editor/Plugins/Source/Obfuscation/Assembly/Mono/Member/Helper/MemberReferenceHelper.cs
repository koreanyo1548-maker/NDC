using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper
{
    public static class MemberReferenceHelper
    {
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
            // Resolve to IMemberDefinition
            if (!TryToResolve<IMemberDefinition>(_MemberReference, out IMemberDefinition _MemberDefinition))
            {
                return null;
            }

            // Return from IMemberDefinitionExtension.
            return IMemberDefinitionHelper.GetExtendedFullName(_MemberDefinition);
        }

        #endregion

        // Resolve
        #region Resolve

        /// <summary>
        /// Tries to resolve _MemberReference. If could not be resolves, returns false.
        /// </summary>
        /// <typeparam name="TMemberDefinition"></typeparam>
        /// <param name="_MemberReference"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public static bool TryToResolve<TMemberDefinition>(MemberReference _MemberReference, out TMemberDefinition _MemberDefinition) 
            where TMemberDefinition : IMemberDefinition
        {
            if (_MemberReference == null)
            {
                throw new ArgumentNullException("_MemberReference");
            }

            try
            {
                _MemberDefinition = (TMemberDefinition)_MemberReference.Resolve();

                return _MemberDefinition != null;
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                _MemberDefinition = default(TMemberDefinition);

                return false;
            }
        }

        #endregion
    }
}
