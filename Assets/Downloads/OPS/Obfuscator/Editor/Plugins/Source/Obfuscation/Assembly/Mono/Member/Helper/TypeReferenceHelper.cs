using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//OPS Mono
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;

//OPS Obfuscator
using OPS.Obfuscator.Editor.Extension;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper
{
    /// <summary>
    /// Helper for type references.
    /// </summary>
    public static class TypeReferenceHelper
    {
        // Scope
        #region Scope

        /// <summary>
        /// Returns the Assembly name of _TypeReference.
        /// If the scope is null, returns null.
        /// </summary>
        /// <param name="_TypeReference"></param>
        /// <returns></returns>
        public static string GetScopeName(TypeReference _TypeReference)
        {
            if (_TypeReference == null)
            {
                throw new ArgumentNullException("_TypeReference");
            }

            IMetadataScope var_Scope = _TypeReference.Scope;

            if (var_Scope == null)
            {
                return null;
            }

            return ParseScopeName(var_Scope);
        }

        /// <summary>
        /// Returns the name of the assembly of an metadataScope.
        /// </summary>
        /// <param name="metadataScope"></param>
        /// <returns></returns>
        private static String ParseScopeName(IMetadataScope metadataScope)
        {
	        int num = metadataScope.Name.LastIndexOf(".dll");
	        if (num > 0)
	        {
		        return metadataScope.Name.Substring(0, num);
	        }
	        return metadataScope.Name;
        }

        #endregion

        // Generic
        #region Generic

        /// <summary>
        /// Make _TypeReference generic with _Arguments.
        /// </summary>
        /// <param name="_TypeReference"></param>
        /// <param name="_Arguments"></param>
        /// <returns></returns>
        public static TypeReference MakeTypeGeneric(TypeReference _TypeReference, params TypeReference[] _Arguments)
        {
            // Amount of Generic Parameters does not match the amount of given Generic Arguments.
            if (_TypeReference.GenericParameters.Count != _Arguments.Length)
                throw new ArgumentException();

            // Create a new generic instance of original type reference.
            GenericInstanceType var_GenericInstanceType = new GenericInstanceType(_TypeReference);

            // Assign generic arguments.
            foreach (var argument in _Arguments)
                var_GenericInstanceType.GenericArguments.Add(argument);

            return var_GenericInstanceType;
        }

        #endregion
    }
}
