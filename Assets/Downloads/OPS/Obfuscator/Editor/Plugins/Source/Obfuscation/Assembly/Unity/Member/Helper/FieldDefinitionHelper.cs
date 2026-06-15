using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Assembly.Unity.Member.Helper
{
    /// <summary>
    /// Helper for FieldDefinitions.
    /// </summary>
    public static class FieldDefinitionHelper
    {
        /// <summary>
        /// True: The field has the Unity SerializeField attribute.
        /// </summary>
        /// <param name="_Field"></param>
        /// <returns></returns>
        public static bool IsFieldSerializeAble(FieldDefinition _Field)
        {
            return Mono.Member.Attribute.Helper.AttributeHelper.HasCustomAttribute(_Field, "SerializeField");
        }
    }
}
