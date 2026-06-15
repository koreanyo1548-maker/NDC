using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OPS.Obfuscator.Editor.Assembly.DotNet.Helper
{
    /// <summary>
    /// Helper class for dotnet assemblies.
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        /// Returns all loadable types in _Assembly.
        /// </summary>
        /// <param name="_Assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetLoadableTypes(System.Reflection.Assembly _Assembly)
        {
            if (_Assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            try
            {
                return _Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        /// <summary>
        /// Returns all loadable types in the domain.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetLoadableTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => GetLoadableTypes(a));
        }
    }
}
