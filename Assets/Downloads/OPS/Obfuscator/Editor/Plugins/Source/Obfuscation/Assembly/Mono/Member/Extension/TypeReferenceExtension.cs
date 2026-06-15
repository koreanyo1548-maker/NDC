using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Extension
{
    public static class TypeReferenceExtension
    {
        /// <summary>
        /// Returns the simplified name for the assembly where a type can be found,
        /// for example, a type whose module is "Assembly.dll", "Assembly" would be 
        /// returned.
        /// </summary>
        public static string GetScopeName(this TypeReference _Type)
        {
            return TypeReferenceHelper.GetScopeName(_Type);
        }
    }
}
