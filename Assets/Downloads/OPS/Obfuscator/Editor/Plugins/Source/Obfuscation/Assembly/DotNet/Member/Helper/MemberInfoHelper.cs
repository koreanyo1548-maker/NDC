using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Obfuscator.Editor.Assembly.DotNet.Member.Helper
{
    /// <summary>
    /// Reflection helper class for member.
    /// </summary>
    public class MemberInfoHelper
    {
        // Name
        #region Name

        /// <summary>
        /// Returns the name of a member from a _FullName.
        /// </summary>
        /// <param name="_FullName"></param>
        /// <returns></returns>
        public static String GetMemberName(String _FullName)
        {
            if (_FullName == null)
            {
                throw new ArgumentNullException("_FullName");
            }

            String var_FullName = _FullName;

            // Remove parameter if method.
            if (var_FullName.Contains("("))
            {
                var_FullName = var_FullName.Substring(0, var_FullName.LastIndexOf("("));
            }

            // Split namespace/type/member.
            String[] var_Dot = var_FullName.Split('.');
            String var_MemberName = var_Dot.Last();

            // Remove generic part, but keep '`' if it is there, just remove annoying '['.
            if (var_MemberName.Contains("["))
            {
                var_MemberName = var_MemberName.Substring(0, var_MemberName.LastIndexOf("["));
            }

            // Remove spaces, if there are some.
            var_MemberName = var_MemberName.Trim();

            return var_MemberName;
        }

        #endregion
    }
}
