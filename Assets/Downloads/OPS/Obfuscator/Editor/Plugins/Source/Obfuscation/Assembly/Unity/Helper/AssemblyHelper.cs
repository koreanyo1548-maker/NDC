using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEditor;

namespace OPS.Obfuscator.Editor.Assembly.Unity.Helper
{
    /// <summary>
    /// Helper class for Unitys AssemblyDefinition Assets.
    /// </summary>
    internal class AssemblyHelper
    {
        // Find Assemblies
        #region Find Assemblies

        /// <summary>
        /// Returns a list of assembly definition files.
        /// </summary>
        /// <returns></returns>
        public static List<UnityEditor.Compilation.Assembly> FindAllAssemblyDefinitionFiles()
        {
            List<UnityEditor.Compilation.Assembly> var_AssemblyDefinitionFileList = new List<UnityEditor.Compilation.Assembly>();

            foreach (UnityEditor.Compilation.Assembly var_Assembly in UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player))
            {
                var_AssemblyDefinitionFileList.Add(var_Assembly);
            }

            return var_AssemblyDefinitionFileList;
        }

        #endregion

        // Setting
        #region Setting

        /// <summary>
        /// True: Is a test assembly! Applies only for Unity 2019.2 or later, else Test Assemblies are not included in UnityEditor.Compilation.
        /// </summary>
        /// <param name="_AssemblyDefinitionFile"></param>
        /// <returns></returns>
        public static bool IsTestAssembly(UnityEditor.Compilation.Assembly _AssemblyDefinitionFile)
        {
            // Note: Including Tests is not a good idea :D because some are in normal Library/ScriptAssemblies directory and will be overriden again and again :D
            // Note: Does always have define 'UNITY_INCLUDE_TESTS' somehow :D, so not an indicator!
            if (/*_AssemblyDefinitionFile.defines.Contains("UNITY_INCLUDE_TESTS")
                || */_AssemblyDefinitionFile.assemblyReferences.Where(a => a.name == "UnityEngine.TestRunner").Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// True: Is a unity assembly defintion file assembly.
        /// </summary>
        /// <param name="_AssemblyName"></param>
        /// <returns></returns>
        public static bool IsAssemblyDefinitionFileAssembly(String _AssemblyName)
        {
            // Direct entered Path.
            if (System.IO.File.Exists(_AssemblyName))
            {
                // Remove .dll
                if (_AssemblyName.EndsWith(".dll"))
                {
                    _AssemblyName = System.IO.Path.GetFileNameWithoutExtension(_AssemblyName);
                }
            }

            // Remove .dll
            if (_AssemblyName.EndsWith(".dll"))
            {
                _AssemblyName = System.IO.Path.GetFileNameWithoutExtension(_AssemblyName);
            }

            // Get all AssemblyDefinitionFiles
            List<UnityEditor.Compilation.Assembly> var_AssemblyDefinitionFileList = FindAllAssemblyDefinitionFiles();

            // Check if one of them is equals _AssemblyName
            for (int i = 0; i < var_AssemblyDefinitionFileList.Count; i++)
            {
                if(var_AssemblyDefinitionFileList[i].name == _AssemblyName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True: Is a pre compiled assembly. (Extern assembly)
        /// </summary>
        /// <param name="_AssemblyName"></param>
        /// <returns></returns>
        public static bool IsPreCompiledAssembly(String _AssemblyName)
        {
            //Is not a assembly definition file assembly, so it is a precompiled assembly.
            return !IsAssemblyDefinitionFileAssembly(_AssemblyName);
        }

        #endregion
    }
}
