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
    /// Helper for mono cecil method references.
    /// </summary>
    public static class MethodReferenceHelper
    {
        // Generic
        #region Generic

        /// <summary>
        /// Make _MethodReference generic with _GenericArguments.
        /// </summary>
        /// <param name="_MethodReference"></param>
        /// <param name="_GenericArguments"></param>
        /// <returns></returns>
        public static MethodReference MakeMethodGeneric(MethodReference _MethodReference, params TypeReference[] _GenericArguments)
        {
            // Amount of Generic Parameters does not match the amount of given Generic Arguments.
            if (_MethodReference.GenericParameters.Count != _GenericArguments.Length)
                throw new ArgumentException();

            // Create a new generic instance of original method reference.
            GenericInstanceMethod var_GenericMethodReference = new GenericInstanceMethod(_MethodReference);

            // Assign generic arguments.
            foreach (TypeReference var_GenericArgument in _GenericArguments)
                var_GenericMethodReference.GenericArguments.Add(var_GenericArgument);

            return var_GenericMethodReference;
        }

        /// <summary>
        /// The _MethodReference DeclaringType has generic parameter.
        /// The MethodReference itself has no custom generic parameter, but is in a generic type. So make TypeReference generic.
        /// </summary>
        /// <param name="_MethodReference"></param>
        /// <param name="_GenericArguments"></param>
        /// <returns></returns>
        public static MethodReference MakeMethodDeclaringTypeGeneric(MethodReference _MethodReference, params TypeReference[] _GenericArguments)
        {
            var reference = new MethodReference(_MethodReference.Name, TypeReferenceHelper.MakeTypeGeneric(_MethodReference.DeclaringType, _GenericArguments), _MethodReference.ReturnType)
            {
                HasThis = _MethodReference.HasThis,
                ExplicitThis = _MethodReference.ExplicitThis,
                CallingConvention = _MethodReference.CallingConvention,
            };

            foreach (var parameter in _MethodReference.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in _MethodReference.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

        #endregion

        // VarArg
        #region VarArg

        public static bool IsVarArg(IMethodSignature _MethodSignature)
        {
            return _MethodSignature.CallingConvention == MethodCallingConvention.VarArg;
        }

        #endregion

        // Parameter
        #region Parameter

        public static int GetSentinelPosition(IMethodSignature _MethodSignature)
        {
            if (!_MethodSignature.HasParameters)
                return -1;

            var parameters = _MethodSignature.Parameters;
            for (int i = 0; i < parameters.Count; i++)
                if (parameters[i].ParameterType.IsSentinel)
                    return i;

            return -1;
        }

        public static int GetParameterAfterSentinel(IMethodSignature _MethodSignature)
        {
            if (!_MethodSignature.HasParameters)
                return -1;

            var parameters = _MethodSignature.Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].ParameterType.IsSentinel)
                {
                    return (parameters.Count - 1) - i;
                }
            }

            return -1;
        }

        #endregion

        // Implicit
        #region Implicit

        public static bool HasImplicitThis(IMethodSignature _MethodSignature)
        {
            return _MethodSignature.HasThis && !_MethodSignature.ExplicitThis;
        }

        #endregion
    }
}
