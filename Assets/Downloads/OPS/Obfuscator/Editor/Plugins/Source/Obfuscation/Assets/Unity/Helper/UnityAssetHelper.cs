// System
using System;
using System.Collections.Generic;
using System.IO;

// GUPS - Assets
using GUPS.Assets.Editor.Files;
using GUPS.Assets.Editor.Files.Bundle;
using GUPS.Assets.Editor.Files.Serialized;
using GUPS.Assets.Editor.IO;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assets.Unity
{
#if Obfuscator_Free

#else

    /// <summary>
    /// Helper for Unity Assets, Serialized and Bundle Files.
    /// </summary>
    internal static class UnityAssetHelper
    {
        // Refresh
        #region Refresh

        /// <summary>
        /// Refresh the AssetDatabase. If failed returns false.
        /// </summary>
        public static bool TryToRefreshAssetDatabase()
        {
            try
            {
                UnityEditor.AssetDatabase.Refresh();

                return true;
            }
            catch (Exception e)
            {
                // Log as debug.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                return false;
            }
        }

        #endregion

        // Serialized File
        #region Serialized File

        /// <summary>
        /// Reads and returns an unity assets file at _FilePath.
        /// </summary>
        /// <param name="_FilePath">The path to the unity assets file.</param>
        /// <returns>The read assets file.</returns>
        /// <exception cref="ArgumentNullException">Thrown if _FilePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if there is no file at _FilePath.</exception>
        public static AssetsFile LoadAssetsFile(String _FilePath)
        {
            // Check if _FilePath is null or empty.
            if (String.IsNullOrEmpty(_FilePath))
            {
                throw new ArgumentNullException("_FilePath");
            }

            // Check if there is a file.
            if (!File.Exists(_FilePath))
            {
                throw new FileNotFoundException("There is no file at: " + _FilePath);
            }

            // Create reader.
            UnityFileReader var_Reader = new UnityFileReader(_FilePath);

            // Read assets file.
            AssetsFile var_AssetsFile = new AssetsFile(var_Reader);
            var_AssetsFile.Read(var_Reader);

            // Close the reader.
            var_Reader.Close();

            // Return read file.
            return var_AssetsFile;
        }

        /// <summary>
        /// Loads an unity assets file with the passed _Name and _Binary.
        /// </summary>
        /// <param name="_Name">The name of the assets file.</param>
        /// <param name="_Binary">The binary data of the assets file.</param>
        /// <returns>The read assets file.</returns>
        public static AssetsFile LoadAssetsFile(String _Name, byte[] _Binary)
        {
            // Create reader.
            UnityFileReader var_Reader = new UnityFileReader(_Name, new MemoryStream(_Binary));

            // Read assets file.
            AssetsFile var_AssetsFile = new AssetsFile(var_Reader);
            var_AssetsFile.Read(var_Reader);

            // Close the reader.
            var_Reader.Close();

            // Return read file.
            return var_AssetsFile;
        }

        /// <summary>
        /// Updates all the MonoScripts in _AssetsFile with the passed one in _OldToNewDictionary.
        /// </summary>
        /// <param name="_AssetsFile">The assets file to update.</param>
        /// <param name="_OldToNewDictionary">The dictionary to update the MonoScripts.</param>
        /// <returns>The count of updated MonoScripts.</returns>
        public static int UpdateMonoScriptsInAssetsFile(AssetsFile _AssetsFile, Dictionary<TypeKey, TypeKey> _OldToNewDictionary)
        {
            // Check if _AssetsFile is null.
            if (_AssetsFile == null)
            {
                throw new ArgumentNullException("_AssetsFile");
            }

            // Check if _OldToNewDictionary is null.
            if (_OldToNewDictionary == null)
            {
                throw new ArgumentNullException("_OldToNewDictionary");
            }

            // Get all MonoScripts in the assets file.
            List<GUPS.Assets.Editor.Files.Serialized.Objects.Classes.MonoScript> var_MonoScriptList = _AssetsFile.GetObjects<GUPS.Assets.Editor.Files.Serialized.Objects.Classes.MonoScript>();

            // Count of updated MonoScripts.
            int var_UpdatedCount = 0;

            // Iterate all MonoScripts and find obfuscated.
            for (int i = 0; i < var_MonoScriptList.Count; i++)
            {
                // TypeKey Assembly name does not contain the '.dll' at the end, which might be at the end of unity MonoScripts.
                String var_Assembly = var_MonoScriptList[i].AssemblyName;

                if (var_Assembly.EndsWith(".dll"))
                {
                    var_Assembly = var_Assembly.Substring(0, var_Assembly.Length - ".dll".Length);
                }

                // Get the Namespace and Name of the MonoScript.
                String var_Namespace = var_MonoScriptList[i].Namespace;
                String var_Name = var_MonoScriptList[i].Name;

                // Check if the MonoScript has an assembly name. It is required. Else log a warning and continue.
                if(String.IsNullOrEmpty(var_Assembly))
                {
                    // Log - No Assembly Name.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("MonoBehaviour '{0}' in assets file '{1}' has no assembly name! It will be skipped. In most cases the belonging file has no content or the file name differs from the class name.", String.IsNullOrEmpty(var_Namespace) ? var_Name : var_Namespace + "." + var_Name, _AssetsFile.FileName));

                    continue;
                }

                // Create a TypeKey from the iterated MonoScript.
                TypeKey var_TypeKey = new TypeKey(var_Assembly, var_Namespace, var_Name);

                // Check if the obfuscated assembles contain the iterated MonoScript.
                if (!_OldToNewDictionary.ContainsKey(var_TypeKey))
                {
                    continue;
                }

                // Get the Obfuscated, if there is some.
                if (_OldToNewDictionary.TryGetValue(var_TypeKey, out TypeKey var_ValueTypeKey))
                {
                    // Update the MonoScript.
                    var_MonoScriptList[i].UpdateType(var_MonoScriptList[i].AssemblyName, String.IsNullOrEmpty(var_ValueTypeKey.Namespace) ? "" : var_ValueTypeKey.Namespace, String.IsNullOrEmpty(var_ValueTypeKey.Name) ? "" : var_ValueTypeKey.Name);
                
                    // Increase the count.
                    var_UpdatedCount++;
                }
            }

            // Log - Obfuscated Assets File.
            Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Obfuscated '{0}' MonoScripts in assets file '{1}'.", var_UpdatedCount, _AssetsFile.FileName));

            // Return the count of updated MonoScripts.
            return var_UpdatedCount;
        }

        /// <summary>
        /// Saves the _AssetsFile at _FilePath and _Override it if, there is already a file.
        /// </summary>
        /// <param name="_AssetsFile">The assets file to save.</param>
        /// <param name="_FilePath">The path to save the assets file.</param>
        /// <param name="_Override">True if the file should be overridden, false otherwise.</param>
        /// <exception cref="ArgumentNullException">Thrown if _AssetsFile is null or _FilePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if there is already a file at _FilePath and _Override is false.</exception>
        public static void SaveAssetsFile(AssetsFile _AssetsFile, String _FilePath, bool _Override = true)
        {
            // Check if _AssetsFile is null.
            if (_AssetsFile == null)
            {
                throw new ArgumentNullException("_AssetsFile");
            }

            // Check if _FilePath is null or empty.
            if (String.IsNullOrEmpty(_FilePath))
            {
                throw new ArgumentNullException("_FilePath");
            }

            // Check if there is a file.
            if (File.Exists(_FilePath) && !_Override)
            {
                throw new FileNotFoundException("There is already a file at: " + _FilePath);
            }

            // Open the filestram.
            using (FileStream var_FileStream = new FileStream(_FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                // Discard the contents of the file by setting the length to 0.
                var_FileStream.SetLength(0);

                // Write assets data.
                using (UnityFileWriter var_Writer = new UnityFileWriter(_FilePath, var_FileStream))
                {
                    _AssetsFile.Write(var_Writer);
                }
            }
        }

        /// <summary>
        /// Saves the _AssetsFile and returns the written data.
        /// </summary>
        /// <param name="_AssetsFile">The assets file to save.</param>
        /// <returns>The written data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if _AssetsFile is null.</exception>
        public static byte[] SaveAssetsFile(AssetsFile _AssetsFile)
        {
            // Check if _AssetsFile is null.
            if (_AssetsFile == null)
            {
                throw new ArgumentNullException("_AssetsFile");
            }

            // Open the filestram.
            using (MemoryStream var_Stream = new MemoryStream())
            {
                // Write assets data.
                using (EndianBinaryWriter var_Writer = new EndianBinaryWriter(var_Stream))
                {
                    _AssetsFile.Write(var_Writer);
                }

                // Return the written data.
                return var_Stream.ToArray();
            }
        }

        #endregion

        // Bundle File
        #region Bundle File

        /// <summary>
        /// Reads and returns an unity bundle file at _FilePath.
        /// </summary>
        /// <param name="_FilePath">The path to the unity bundle file.</param>
        /// <returns>The read bundle file.</returns>
        /// <exception cref="ArgumentNullException">Thrown if _FilePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if there is no file at _FilePath.</exception>
        public static BundleFile LoadBundleFile(String _FilePath)
        {
            // Check if _FilePath is null or empty.
            if (String.IsNullOrEmpty(_FilePath))
            {
                throw new ArgumentNullException("_FilePath");
            }

            // Check if there is a file.
            if (!File.Exists(_FilePath))
            {
                throw new FileNotFoundException("There is no file at: " + _FilePath);
            }

            // Create reader.
            UnityFileReader var_Reader = new UnityFileReader(_FilePath);

            // Read bundle file.
            BundleFile var_BundleFile = new BundleFile(var_Reader);
            var_BundleFile.Read(var_Reader);

            // Close the reader.
            var_Reader.Close();

            // Return read file.
            return var_BundleFile;
        }

        /// <summary>
        /// Update the mono scripts in all serialized files inside the _BundleFile with the passed _OldToNewDictionary.
        /// </summary>
        /// <param name="_BundleFile">The bundle file to update.</param>
        /// <param name="_OldToNewDictionary">The dictionary to update the MonoScripts.</param>
        /// <returns>Returns the count of updated MonoScripts.</returns>
        /// <exception cref="ArgumentNullException">Thrown if _BundleFile is null or _OldToNewDictionary is null.</exception>
        public static int UpdateMonoScriptsInBundleFile(BundleFile _BundleFile, Dictionary<TypeKey, TypeKey> _OldToNewDictionary)
        {
            // Check if _BundleFile is null.
            if (_BundleFile == null)
            {
                throw new ArgumentNullException("_BundleFile");
            }

            // Check if _OldToNewDictionary is null.
            if (_OldToNewDictionary == null)
            {
                throw new ArgumentNullException("_OldToNewDictionary");
            }

            // Get all serialized files in the bundle file.
            var var_ResourceInfos = _BundleFile.GetAllResourceInfos(GUPS.Assets.Editor.Files.Bundle.Node.ResourceInfoFlag.SerializedFile);

            // Count of updated MonoScripts.
            int var_UpdatedCount = 0;

            // Iterate all serialized files and update MonoScripts.
            foreach (var var_ResourceInfo in var_ResourceInfos)
            {
                // Log - Obfuscating Resource.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, String.Format("Obfuscating resource '{0}' in bundle file.", var_ResourceInfo.Path));

                // Get the binary for the serialized file.
                var var_Binary = _BundleFile.GetResource(var_ResourceInfo.Path);

                // Load the serialized file.
                AssetsFile var_AssetsFile = LoadAssetsFile(var_ResourceInfo.Path, var_Binary);

                // Update the MonoScripts in the serialized file.
                int var_InFileCount = UpdateMonoScriptsInAssetsFile(var_AssetsFile, _OldToNewDictionary);

                // If there is no MonoScript upadated, the file is not updated.
                if(var_InFileCount == 0)
                {
                    continue;
                }

                // Save the serialized file.
                var var_NewBinary = SaveAssetsFile(var_AssetsFile);

                // Update the resource in the bundle file.
                _BundleFile.ReplaceResource(var_ResourceInfo.Path, var_NewBinary);

                // Increase updated count.
                var_UpdatedCount += var_InFileCount;
            }

            return var_UpdatedCount;
        }

        /// <summary>
        /// Saves the _BundleFile at _FilePath and _Override it if, there is already a file.
        /// </summary>
        /// <param name="_BundleFile">The bundle file to save.</param>
        /// <param name="_FilePath">The path to save the bundle file.</param>
        /// <param name="_Override">True if the file should be overridden, false otherwise.</param>
        /// <exception cref="ArgumentNullException">Thrown if _BundleFile is null or _FilePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if there is already a file at _FilePath and _Override is false.</exception>
        public static void SaveBundleFile(BundleFile _BundleFile, String _FilePath, bool _Override = true)
        {
            // Check if _BundleFile is null.
            if (_BundleFile == null)
            {
                throw new ArgumentNullException("_BundleFile");
            }

            // Check if _FilePath is null or empty.
            if (String.IsNullOrEmpty(_FilePath))
            {
                throw new ArgumentNullException("_FilePath");
            }

            // Check if there is a file.
            if (File.Exists(_FilePath) && !_Override)
            {
                throw new FileNotFoundException("There is already a file at: " + _FilePath);
            }

            // Open the filestram.
            using (FileStream var_FileStream = new FileStream(_FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                // Discard the contents of the file by setting the length to 0.
                var_FileStream.SetLength(0);

                // Write bundle data.
                using (UnityFileWriter var_Writer = new UnityFileWriter(_FilePath, var_FileStream))
                {
                    _BundleFile.Write(var_Writer);
                }
            }
        }

        #endregion
    }

#endif
}
