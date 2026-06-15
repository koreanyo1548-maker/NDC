using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//OPS
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Match
{
    public class MethodMatchHelper
    {
        // Nice to know: https://github.com/mono/mono/blob/master/mcs/tools/linker/Mono.Linker.Steps/TypeMapStep.cs

        public static bool MethodMatch(MethodDefinition _Method_A, MethodDefinition _Method_B)
        {
            if (_Method_A == null && _Method_B == null)
            {
                return true;
            }

            if (_Method_A == null || _Method_B == null)
            {
                return false;
            }

            if (_Method_A.Name != _Method_B.Name)
                return false;

            if (!TypeMatchHelper.TypeMatch(_Method_A.ReturnType, _Method_B.ReturnType))
                return false;

            if (_Method_A.Parameters.Count != _Method_B.Parameters.Count)
                return false;

            for (int i = 0; i < _Method_A.Parameters.Count; i++)
                if (!TypeMatchHelper.TypeMatch(_Method_A.Parameters[i].ParameterType, _Method_B.Parameters[i].ParameterType))
                    return false;

            return true;
        }
    }
}
