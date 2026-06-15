using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;

// OPS - Obfuscator
using OPS.Obfuscator.Editor.Extension;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper
{
    /// <summary>
    /// Helper for mono cecil field references.
    /// </summary>
    public static class FieldReferenceHelper
    {
        // Generic
        #region Generic

        /// <summary>
        /// Make _FieldReference generic with _GenericArguments.
        /// </summary>
        /// <param name="_FieldReference"></param>
        /// <param name="_GenericArguments"></param>
        /// <returns></returns>
        public static FieldReference MakeFieldDeclaringTypeGeneric(FieldReference _FieldReference, params TypeReference[] _GenericArguments)
        {
            return new FieldReference(_FieldReference.Name, TypeReferenceHelper.MakeTypeGeneric(_FieldReference.DeclaringType, _GenericArguments))
            {
                FieldType = _FieldReference.FieldType,
            };
        }

        #endregion
    }
}
