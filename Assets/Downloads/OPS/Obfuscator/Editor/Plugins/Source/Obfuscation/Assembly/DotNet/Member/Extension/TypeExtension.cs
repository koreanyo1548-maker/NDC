using System;
using System.Collections;
using System.Collections.Generic;

namespace OPS.Obfuscator.Editor.Assembly.DotNet.Member.Extension
{
    /// <summary>
    /// Extension for Type.
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// True: Type is an Array or List.
        /// </summary>
        /// <param name="_Type"></param>
        /// <returns></returns>
        public static bool IsArrayOrList(this Type _Type)
        {
            if (_Type.IsArray)
            {
                return true;
            }
            if (_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the ElementType of the _Type, if the _Type is a Array or List, else returns null.
        /// </summary>
        /// <param name="_Type"></param>
        /// <returns></returns>
        public static Type GetArrayOrListElementType(this Type _Type)
        {
            if (_Type.IsArray)
            {
                return _Type.GetElementType();
            }
            if (_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return _Type.GetGenericArguments()[0];
            }
            return null;
        }

        /// <summary>
        /// True: Type is an IEnumerable.
        /// </summary>
        /// <param name="_Type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type _Type)
        {
            return typeof(IEnumerable).IsAssignableFrom(_Type);
        }

        /// <summary>
        /// True: Type is an ICollection.
        /// </summary>
        /// <param name="_Type"></param>
        /// <returns></returns>
        public static bool IsCollection(this Type _Type)
        {
            return typeof(ICollection).IsAssignableFrom(_Type);
        }
    }
}
