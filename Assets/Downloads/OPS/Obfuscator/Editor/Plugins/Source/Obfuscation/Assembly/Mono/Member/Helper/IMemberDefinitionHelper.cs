using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper
{
    /// <summary>
    /// Helper for IMemberDefinition.
    /// </summary>
    public static class IMemberDefinitionHelper
    {
        // Name
        #region Name

        /// <summary>
        /// Returns the name of a member inside a _FullName.
        /// Does not remove generic parameter.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_FullName"></param>
        /// <returns></returns>
        public static String GetMemberName(EMemberType _MemberType, String _FullName)
        {
            if (_FullName == null)
            {
                throw new ArgumentNullException("_FullName");
            }

            // If Type then is only the type fullname.
            // If Member then is return type, space, member fullname.
            String[] var_Space = _FullName.Split(' ');

            // If var_ReturnType_FullName is equals var_FullName, than _FullName is type or namespace.
            String var_ReturnType_FullName = var_Space.First();
            String var_FullName = var_Space.Last();

            // Split the double colon, in front is the type and back is the type member.
            String[] var_DoubleColon = var_FullName.Split(new String[] { "::" }, StringSplitOptions.None);

            // If var_Type_FullName is equals var_Member_FullName, than _FullName is type or namespace.
            String var_Type_FullName = var_DoubleColon.First();
            String var_Member_FullName = var_DoubleColon.Last();

            switch (_MemberType)
            {
                case EMemberType.Namespace:
                    {
                        // Get the namespace.
                        TypeDefinitionHelper.SplitFullName(var_Type_FullName, out String var_Namespace, out String var_Name);
                        return var_Namespace;
                    }
                case EMemberType.Type:
                    {
                        // Get the namespace.
                        TypeDefinitionHelper.SplitFullName(var_Type_FullName, out String var_Namespace, out String var_Name);
                        return var_Name;
                    }
                case EMemberType.Method:
                    {
                        // Remove the methods parameter.
                        return MethodDefinitionHelper.RemoveMethodParameter(var_Member_FullName);
                    }
                case EMemberType.Field:
                case EMemberType.Property:
                case EMemberType.Event:
                    {
                        return var_Member_FullName;
                    }
            }

            return null;
        }

        #endregion

        // FullName
        #region FullName

        /// <summary>
        /// Returns the extended full name of a IMemberDefinition.
        /// Looks like: '[AssemblyName] FullName'.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public static string GetExtendedFullName(IMemberDefinition _MemberDefinition)
        {
            // Scope
            String var_Scope = _MemberDefinition is TypeReference ? TypeReferenceHelper.GetScopeName(_MemberDefinition as TypeReference) : TypeReferenceHelper.GetScopeName(_MemberDefinition.DeclaringType);

            return "[" + var_Scope + "] " + _MemberDefinition.FullName;
        }

        /// <summary>
        /// Returns the extended original full name of a IMemberDefinition.
        /// Looks like: '[AssemblyName] Original FullName'.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <param name="_BelongingCache"></param>
        /// <returns></returns>
        public static string GetExtendedOriginalFullName(IMemberDefinition _MemberDefinition, IMemberCache _BelongingCache)
        {
            // Scope
            String var_Scope = _MemberDefinition is TypeReference ? TypeReferenceHelper.GetScopeName(_MemberDefinition as TypeReference) : TypeReferenceHelper.GetScopeName(_MemberDefinition.DeclaringType);

            // Member
            IMemberKey var_Key = _BelongingCache.GetOriginalMemberKeyByMemberDefinition(_MemberDefinition);

            if (var_Key == null)
            {
                return null;
            }
            else
            {
                return "[" + var_Scope + "] " + var_Key.FullName;
            }
        }

        #endregion

        // GenericParameter
        #region GenericParameter

        /// <summary>
        /// Returns true, if has any generic parameters.
        /// If has so, _GenericParameterCount will contain the count of them.
        /// </summary>
        /// <param name="_FullName"></param>
        /// <param name="_GenericParameterCount"></param>
        /// <returns></returns>
        public static bool TryGetGenericParameter(String _FullName, out int _GenericParameterCount)
        {
            // Stores the generic parameter count of the previous index round.
            int var_Previous_GenericParameterCount = 0;

            // Stores the generic parameter count of the current index round.
            int var_Current_GenericParameterCount = 0;

            // Iterate from end to start.
            for (int var_CurrentIndex = _FullName.Length - 1; var_CurrentIndex >= 0; var_CurrentIndex--)
            {
                // Assign current to previous.
                var_Previous_GenericParameterCount = var_Current_GenericParameterCount;

                // Try to parse and assign to var_Current_GenericParameterCount.
                if (int.TryParse(_FullName.Substring(var_CurrentIndex), out var_Current_GenericParameterCount))
                {
                    // Still a number, continue.
                    continue;
                }

                if (_FullName[var_CurrentIndex] == '`')
                {
                    // Has parameter end found.
                    _GenericParameterCount = var_Previous_GenericParameterCount;
                    return true;
                }
                else
                {
                    // No number at end or other character, so no generic!
                    break;
                }
            }

            _GenericParameterCount = 0;
            return false;
        }

        /// <summary>
        /// Simplifies _Name (Member Name). Simplifies means, if _Name has a generic parameter (`2) it will be removed.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public static String RemoveGenericParameter(String _Name)
        {
            String var_Simplified_Name = _Name;
            if (IMemberDefinitionHelper.TryGetGenericParameter(_Name, out int var_GenericParameterIndex))
            {
                var_Simplified_Name = var_Simplified_Name.Remove(var_Simplified_Name.LastIndexOf("`"));
            }
            return var_Simplified_Name;
        }

        #endregion

        // Return Type
        #region Return Type

        /// <summary>
        /// Get the full return type of a name or fullname. If there is none, returns null.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public static String GetReturnType(String _Name)
        {
            if (_Name == null)
            {
                throw new ArgumentNullException("_Name");
            }

            // Type is only the type fullname.
            // Member is return type, space, member fullname.
            // Note: There are no else spaces! Currently known :D
            String[] var_Space = _Name.Split(' ');

            // Is only the name. Return null. No Return type.
            if (var_Space.Length == 1)
            {
                return null;
            }

            // First is return type, second ist member.
            if (var_Space.Length == 2)
            {
                String var_ReturnType_FullName = var_Space[0];

                return var_ReturnType_FullName;
            }

            // Should not happen!
            if (var_Space.Length > 2)
            {
                throw new Exception("Getting the return type failed! More than 2 spaces in " + _Name + ". This should not happen!");
            }

            return null;
        }

        /// <summary>
        /// Remove the return type from _Name. Either it is a name or fullname.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public static String RemoveReturnType(String _Name)
        {
            if (_Name == null)
            {
                throw new ArgumentNullException("_Name");
            }

            // Type is only the type fullname.
            // Member is return type, space, member fullname.
            // Note: There are no else spaces! Currently known :D
            String[] var_Space = _Name.Split(' ');

            // Either way, the last is the member without return type.
            return var_Space.Last();
        }

        #endregion
    }
}
