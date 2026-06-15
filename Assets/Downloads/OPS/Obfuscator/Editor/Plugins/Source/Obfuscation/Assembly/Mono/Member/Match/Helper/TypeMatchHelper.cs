using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//OPS
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Match
{
    public class TypeMatchHelper
    {
        public static bool TypeMatch(IModifierType a, IModifierType b)
        {
            if (!TypeMatch(a.ModifierType, b.ModifierType))
            {
                return false;
            }

            return TypeMatch(a.ElementType, b.ElementType);
        }

        public static bool TypeMatch(TypeSpecification a, TypeSpecification b)
        {
            if (a is GenericInstanceType)
            {
                return TypeMatch((GenericInstanceType)a, (GenericInstanceType)b);
            }

            if (a is IModifierType)
            {
                return TypeMatch((IModifierType)a, (IModifierType)b);
            }

            return TypeMatch(a.ElementType, b.ElementType);
        }

        public static bool TypeMatch(GenericInstanceType a, GenericInstanceType b)
        {
            if (!TypeMatch(a.ElementType, b.ElementType))
            {
                return false;
            }

            if (a.GenericArguments.Count != b.GenericArguments.Count)
            {
                return false;
            }

            if (a.GenericArguments.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < a.GenericArguments.Count; i++)
            {
                if (!TypeMatch(a.GenericArguments[i], b.GenericArguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TypeMatch(TypeReference a, TypeReference b)
        {
            if (a is GenericParameter)
            {
                return true;
            }

            if (a is TypeSpecification || b is TypeSpecification)
            {
                if (a.GetType() != b.GetType())
                {
                    return false;
                }

                return TypeMatch((TypeSpecification)a, (TypeSpecification)b);
            }

            return a.FullName == b.FullName;
        }
    }
}
