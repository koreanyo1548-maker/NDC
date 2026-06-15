using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Cil;
using OPS.Mono.Cecil.Mdb;
using OPS.Mono.Cecil.Pdb;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Helper
{
    /// <summary>
    /// Helper class for mono assemblies.
    /// </summary>
    public static class AssemblyHelper
    {
        // Location
        #region Location

        /// <summary>
        /// Returns the full file path of a _Assembly name.
        /// </summary>
        /// <param name="_Assembly"></param>
        /// <returns></returns>
        public static string FindAssemblyLocation(String _Assembly)
        {
            // Direct entered Path.
            if (System.IO.File.Exists(_Assembly))
            {
                if (_Assembly.EndsWith(".dll"))
                {
                    return _Assembly;
                }
            }

            // Check for ending
            if (!_Assembly.EndsWith(".dll"))
            {
                _Assembly += ".dll";
            }

            String var_AssemblyLocation = null;

            // Load from Temp/StagingArea/Data/Managed
            if (GetLocationInTempStagingArea(_Assembly, out var_AssemblyLocation))
            {
                return var_AssemblyLocation;
            }

            // Load from Library/Bee/PlayerScriptAssemblies
            if (GetLocationInBeeStagingArea(_Assembly, out var_AssemblyLocation))
            {
                return var_AssemblyLocation;
            }

            // Notice: Do not get Location in files!

            return var_AssemblyLocation;
        }

        private static bool GetLocationInTempStagingArea(String _Assembly, out String _AssemblyLocation)
        {
            // Temp/StagingArea/Data/Managed
            String var_TempStagingAreaDirectory = System.IO.Path.GetDirectoryName(Application.dataPath);
            var_TempStagingAreaDirectory = System.IO.Path.Combine(var_TempStagingAreaDirectory, "Temp");
            var_TempStagingAreaDirectory = System.IO.Path.Combine(var_TempStagingAreaDirectory, "StagingArea");
            var_TempStagingAreaDirectory = System.IO.Path.Combine(var_TempStagingAreaDirectory, "Data");
            var_TempStagingAreaDirectory = System.IO.Path.Combine(var_TempStagingAreaDirectory, "Managed");
            String var_AssemblyFile = System.IO.Path.Combine(var_TempStagingAreaDirectory, _Assembly);
            if (System.IO.File.Exists(var_AssemblyFile))
            {
                _AssemblyLocation = var_AssemblyFile;
                return true;
            }

            _AssemblyLocation = null;
            return false;
        }

        private static bool GetLocationInBeeStagingArea(String _Assembly, out String _AssemblyLocation)
        {
            // Temp/StagingArea/Data/Managed
            String var_TempStagingAreaDirectory = System.IO.Path.GetDirectoryName(Application.dataPath);
            var_TempStagingAreaDirectory = System.IO.Path.Combine(var_TempStagingAreaDirectory, "Library");
            var_TempStagingAreaDirectory = System.IO.Path.Combine(var_TempStagingAreaDirectory, "Bee");
            var_TempStagingAreaDirectory = System.IO.Path.Combine(var_TempStagingAreaDirectory, "PlayerScriptAssemblies");
            String var_AssemblyFile = System.IO.Path.Combine(var_TempStagingAreaDirectory, _Assembly);
            if (System.IO.File.Exists(var_AssemblyFile))
            {
                _AssemblyLocation = var_AssemblyFile;
                return true;
            }

            _AssemblyLocation = null;
            return false;
        }

        #endregion

        // Save and Load
        #region Save and Load

        /// <summary>
        /// Returns the ReaderParameters for a assembly.
        /// </summary>
        /// <param name="_AssemblyFilePath"></param>
        /// <returns></returns>
        private static ReaderParameters GetReaderParameters(String _AssemblyFilePath)
        {
            if (System.IO.File.Exists(System.IO.Path.ChangeExtension(_AssemblyFilePath, ".dll.mdb")))
            {
                return new ReaderParameters()
                {
                    ReadSymbols = true,
                    SymbolReaderProvider = new MdbReaderProvider()
                };
            }
            if (System.IO.File.Exists(System.IO.Path.ChangeExtension(_AssemblyFilePath, ".pdb")))
            {
                return new ReaderParameters()
                {
                    ReadSymbols = true,
                    SymbolReaderProvider = new PdbReaderProvider()
                };
            }
            return new ReaderParameters()
            {
                ReadSymbols = false,
                SymbolReaderProvider = null
            };
        }

        /// <summary>
        /// Returns the writer parameter for a assembly.
        /// </summary>
        /// <param name="_AssemblyFilePath"></param>
        /// <returns></returns>
        private static WriterParameters GetWriterParameters(String _AssemblyFilePath)
        {
            if (System.IO.File.Exists(System.IO.Path.ChangeExtension(_AssemblyFilePath, ".dll.mdb")))
            {
                return new WriterParameters()
                {
                    WriteSymbols = true,
                    SymbolWriterProvider = new MdbWriterProvider()
                };
            }
            if (System.IO.File.Exists(System.IO.Path.ChangeExtension(_AssemblyFilePath, ".pdb")))
            {
                return new WriterParameters()
                {
                    WriteSymbols = true,
                    SymbolWriterProvider = new PortablePdbWriterProvider()
                };
            }
            return new WriterParameters();
        }

        /// <summary>
        /// Loads an AssemblyDefinition at _AssemblyFilePath.
        /// </summary>
        /// <param name="_AssemblyFilePath"></param>
        /// <param name="_AssemblyResolver"></param>
        /// <param name="_BuildTarget"></param>
        /// <returns></returns>
        internal static AssemblyDefinition LoadAssembly(String _AssemblyFilePath, AssemblyResolver _AssemblyResolver, UnityEditor.BuildTarget _BuildTarget)
        {
            try
            {
                // Windows Stores does not like pdb files....
                if (_BuildTarget == UnityEditor.BuildTarget.WSAPlayer)
                {
                    System.IO.File.Delete(System.IO.Path.ChangeExtension(_AssemblyFilePath, ".pdb"));
                }

                ReaderParameters var_ReaderParameters = GetReaderParameters(_AssemblyFilePath);
                var_ReaderParameters.AssemblyResolver = _AssemblyResolver;
                var_ReaderParameters.ReadWrite = true;
                var_ReaderParameters.InMemory = true;

                AssemblyDefinition var_AssemblyDefinition = AssemblyDefinition.ReadAssembly(_AssemblyFilePath, var_ReaderParameters);

                _AssemblyResolver.RegisterAssembly(var_AssemblyDefinition);

                return var_AssemblyDefinition;
            }
            catch (System.IO.IOException e)
            {
                throw new Exception("Unable to find assembly:  " + _AssemblyFilePath, e);
            }
        }

        /// <summary>
        /// Save an _AssemblyDefinition at _AssemblyFilePath.
        /// </summary>
        /// <param name="_AssemblyDefinition"></param>
        /// <param name="_AssemblyFilePath"></param>
        /// <returns></returns>
        internal static bool SaveAssembly(AssemblyDefinition _AssemblyDefinition, String _AssemblyFilePath)
        {
            // Create writer parameters
            WriterParameters writerParameters = GetWriterParameters(_AssemblyFilePath);

            // Write Assembly.
            try
            {
                _AssemblyDefinition.Write(_AssemblyFilePath, writerParameters);
                return true;
            }
            catch (Exception e1)
            {
                // Failed to write, so write again without parameter!
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e1.ToString());

                try
                {
                    _AssemblyDefinition.Write(_AssemblyFilePath, new WriterParameters());
                    return true;
                }
                catch (Exception e2)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, e2.ToString());
                    return false;
                }
            }
        }

        #endregion

        // Remove
        #region Remove

        /// <summary>
        /// Removes the _Assembly from the build. Deletes it from the staging area. So can only be callen after the assembly got build.
        /// </summary>
        /// <param name="_Assembly"></param>
        /// <returns></returns>
        public static bool RemoveAssemblyFromBuild(String _Assembly)
        {
            // Check for ending
            if (!_Assembly.EndsWith(".dll"))
            {
                _Assembly += ".dll";
            }

            // Load from Temp/StagingArea/Data/Managed
            if (!GetLocationInTempStagingArea(_Assembly, out String var_AssemblyLocation))
            {
                return false;
            }

            // Delete it.
            System.IO.File.Delete(var_AssemblyLocation);

            return true;
        }

        #endregion

        // Types
        #region Types

        /// <summary>
        /// Get all Types sorted, nested included.
        /// </summary>
        /// <param name="_AssemblyDefinition"></param>
        /// <returns></returns>
        public static List<TypeDefinition> GetAllTypes(AssemblyDefinition _AssemblyDefinition)
        {
            List<TypeDefinition> var_Result = new List<TypeDefinition>();

            for(int m = 0; m < _AssemblyDefinition.Modules.Count; m++)
            {
                ModuleDefinition var_ModuleDefinition = _AssemblyDefinition.Modules[m];

                for(int t = 0; t < var_ModuleDefinition.Types.Count; t++)
                {
                    TypeDefinition var_TypeDefinition = var_ModuleDefinition.Types[t];

                    if (var_TypeDefinition.HasNestedTypes)
                    {
                        //Get all nested types.
                        List<TypeDefinition> var_NestedTypeDefinitionList = GetAllNestedTypes(var_TypeDefinition);

                        for (int n = 0; n < var_NestedTypeDefinitionList.Count; n++)
                        {
                            var_Result.Add(var_NestedTypeDefinitionList[n]);
                        }
                    }

                    var_Result.Add(var_TypeDefinition);
                }
            }

            return var_Result;
        }

        /// <summary>
        /// Returns a ordered list of all nested types (iterates through their nested types too) for a _TypeDefinition.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private static List<TypeDefinition> GetAllNestedTypes(TypeDefinition _TypeDefinition)
        {
            List<TypeDefinition> var_Result = new List<TypeDefinition>();
            
            if (_TypeDefinition.HasNestedTypes)
            {
                for(int n = 0; n < _TypeDefinition.NestedTypes.Count; n++)
                {
                    //Add nested types of nested Type.
                    var_Result.AddRange(GetAllNestedTypes(_TypeDefinition.NestedTypes[n]));

                    //Add nested type
                    var_Result.Add(_TypeDefinition.NestedTypes[n]);
                }
            }

            return var_Result;
        }

        #endregion

        // Count
        #region Count

        /// <summary>
        /// Returns the count of all types. Include _Nested in count via true/false.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <param name="_Nested"></param>
        /// <returns></returns>
        public static int GetCountOfAllTypes(AssemblyInfo _AssemblyInfo, bool _Nested = true)
        {
            return _AssemblyInfo.GetAllTypeDefinitions().Where(t => t.IsNested ? _Nested : true).Count();
        }

        /// <summary>
        /// Returns the count of all types. Include _Nested in count via true/false.
        /// </summary>
        /// <param name="_AssemblyInfo_Enumerable"></param>
        /// <param name="_Nested"></param>
        /// <returns></returns>
        public static int GetCountOfAllTypes(IEnumerable<AssemblyInfo> _AssemblyInfo_Enumerable, bool _Nested = true)
        {
            return _AssemblyInfo_Enumerable.Select(a => GetCountOfAllTypes(a, _Nested)).Sum();
        }

        /// <summary>
        /// Returns the count of all methods in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public static int GetCountOfAllMethods(AssemblyInfo _AssemblyInfo)
        {
            return _AssemblyInfo.GetAllTypeDefinitions().SelectMany(t => t.Methods).Count();
        }

        /// <summary>
        /// Returns the count of all methods in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo_Enumerable"></param>
        /// <returns></returns>
        public static int GetCountOfAllMethods(IEnumerable<AssemblyInfo> _AssemblyInfo_Enumerable)
        {
            return _AssemblyInfo_Enumerable.Select(a => GetCountOfAllMethods(a)).Sum();
        }

        /// <summary>
        /// Returns the count of all fields in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public static int GetCountOfAllFields(AssemblyInfo _AssemblyInfo)
        {
            return _AssemblyInfo.GetAllTypeDefinitions().SelectMany(t => t.Fields).Count();
        }

        /// <summary>
        /// Returns the count of all fields in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo_Enumerable"></param>
        /// <returns></returns>
        public static int GetCountOfAllFields(IEnumerable<AssemblyInfo> _AssemblyInfo_Enumerable)
        {
            return _AssemblyInfo_Enumerable.Select(a => GetCountOfAllFields(a)).Sum();
        }

        /// <summary>
        /// Returns the count of all properties in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public static int GetCountOfAllProperties(AssemblyInfo _AssemblyInfo)
        {
            return _AssemblyInfo.GetAllTypeDefinitions().SelectMany(t => t.Properties).Count();
        }

        /// <summary>
        /// Returns the count of all properties in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo_Enumerable"></param>
        /// <returns></returns>
        public static int GetCountOfAllProperties(IEnumerable<AssemblyInfo> _AssemblyInfo_Enumerable)
        {
            return _AssemblyInfo_Enumerable.Select(a => GetCountOfAllProperties(a)).Sum();
        }

        /// <summary>
        /// Returns the count of all events in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo"></param>
        /// <returns></returns>
        public static int GetCountOfAllEvents(AssemblyInfo _AssemblyInfo)
        {
            return _AssemblyInfo.GetAllTypeDefinitions().SelectMany(t => t.Events).Count();
        }

        /// <summary>
        /// Returns the count of all events in all types including nested ones.
        /// </summary>
        /// <param name="_AssemblyInfo_Enumerable"></param>
        /// <returns></returns>
        public static int GetCountOfAllEvents(IEnumerable<AssemblyInfo> _AssemblyInfo_Enumerable)
        {
            return _AssemblyInfo_Enumerable.Select(a => GetCountOfAllEvents(a)).Sum();
        }

        #endregion
    }
}
