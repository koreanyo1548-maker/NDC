using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper
{
    public class MethodInfoHelper
    {
        // Parameter
        #region Parameter

        /// <summary>
        /// Removes the method parameter, if has one.
        /// Example: MyMethod(...), returns MyMethod.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public static String RemoveMethodParameter(String _Name)
        {
            if (MethodInfoHelper.TryGetMethodParameterStartIndex(_Name, out int var_ParameterIndex))
            {
                _Name = _Name.Substring(0, var_ParameterIndex - 1);
            }

            return _Name;
        }

        /// <summary>
        /// Returns true if _Name contains parameter.
        /// _ParameterIndex represents the index in _Name, where the parameter after "(" starts.
        /// Example: MyMethod(...), returns true and _ParameterIndex as 9.
        /// </summary>
        /// <param name="_Name"></param>
        /// <param name="_ParameterIndex"></param>
        /// <returns></returns>
        public static bool TryGetMethodParameterStartIndex(String _Name, out int _ParameterIndex)
        {
            int var_LastIndex = _Name.LastIndexOf("(");

            if (var_LastIndex != -1)
            {
                _ParameterIndex = var_LastIndex + 1;
                return true;
            }

            _ParameterIndex = -1;
            return false;
        }

        #endregion
    }
}
