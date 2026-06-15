using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper
{
    /// <summary>
    /// Helper class for Types.
    /// </summary>
    public class TypeHelper
    {
        /// <summary>
        /// Removes the member name and following parameters.
        /// Example: System.String MyNamespace.MyType.MyMethod(...), returns System.String MyNamespace.MyType.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public static String RemoveMemberName(String _Name)
        {
            if (TypeHelper.TryGetMemberNameStartIndex(_Name, out int var_MemberIndex))
            {
                _Name = _Name.Substring(0, var_MemberIndex - 1);
            }

            return _Name;
        }

        /// <summary>
        /// Returns true if _Name contains a member (method/field/property/event).
        /// _MemberIndex represents the index in _Name, where the member name after "." starts.
        /// Example: System.String MyNamespace.MyType.MyMethod(...), returns true and _MemberIndex as 34.
        /// </summary>
        /// <param name="_Name"></param>
        /// <param name="_MemberIndex"></param>
        /// <returns></returns>
        public static bool TryGetMemberNameStartIndex(String _Name, out int _MemberIndex)
        {
            // Remove method parameter if exists.
            String var_Name = MethodInfoHelper.RemoveMethodParameter(_Name);

            // Find last index of '.', is in dotnet the type and member seperator...
            int var_LastIndex = var_Name.LastIndexOf(".");

            if (var_LastIndex != -1)
            {
                _MemberIndex = var_LastIndex + 1;
                return true;
            }

            _MemberIndex = -1;
            return false;
        }
    }
}
