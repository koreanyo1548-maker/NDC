using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//OPS
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Match
{
    internal class PropertyMatchHelper
    {
        // Nice to know: https://github.com/mono/mono/blob/master/mcs/tools/linker/Mono.Linker.Steps/TypeMapStep.cs

        public static bool PropertyMatch(PropertyDefinition candidate, PropertyDefinition property)
        {
            if (candidate == null && property == null)
            {
                return true;
            }

            if (candidate == null || property == null)
            {
                return false;
            }

            if (candidate.Name != property.Name)
                return false;

            if (!TypeMatchHelper.TypeMatch(candidate.PropertyType, property.PropertyType))
                return false;

            // Note: Who cares about the methods? They might be different!
            /*if (!(Match.MethodMatchHelper.MethodMatch(candidate.GetMethod, property.GetMethod)
                || Match.MethodMatchHelper.MethodMatch(property.GetMethod, candidate.GetMethod)))
            {
                return false;
            }

            if (!(Match.MethodMatchHelper.MethodMatch(candidate.SetMethod, property.SetMethod)
                || Match.MethodMatchHelper.MethodMatch(property.SetMethod, candidate.SetMethod)))
            {
                return false;
            }*/

            return true;            
        }
    }
}
