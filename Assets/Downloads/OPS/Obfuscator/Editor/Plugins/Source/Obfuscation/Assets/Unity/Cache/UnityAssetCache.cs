// System
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;

namespace OPS.Obfuscator.Editor.Assets.Unity.Cache
{
    /// <summary>
    /// A cache for Unity Assets.
    /// </summary>
    public class UnityAssetCache
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Instantiate a new <see cref="UnityAssetCache"/> with the given extensions.
        /// </summary>
        /// <param name="_ExtensionToIterateArray">The extensions to include in the cache.</param>
        public UnityAssetCache(String[] _ExtensionToIterateArray)
        {
            // Parameters
            this.extensionToIterateArray = _ExtensionToIterateArray;
        }

        #endregion

        // Asset Path
        #region Asset Path

        /// <summary>
        /// Contains all extensions should be iterated.
        /// </summary>
        private String[] extensionToIterateArray;

        /// <summary>
        /// Extension (containing the dot '.prefab' for example), Array of Relative File Paths.
        /// </summary>
        private Dictionary<String, List<UnityAssetReference>> extensionToAssetPathArrayDictionary;

        /// <summary>
        /// Create the asset cache based on the given extensions.
        /// </summary>
        /// <returns>True if the cache was created successfully; otherwise, false.</returns>
        private bool CreateCache()
        {
            // Create Cache as List (easier to add elements).
            this.extensionToAssetPathArrayDictionary = new Dictionary<String, List<UnityAssetReference>>();

            // Try to build the cache.
            try
            {
                // Get all asset paths.
                String[] var_AssetPaths = AssetDatabase.GetAllAssetPaths();

                // Iterate over all asset paths and add their references, if they match the given extensions.
                foreach (String var_AssetPath in var_AssetPaths)
                {
                    // Check if is in Assets or Packages directory!
                    if (!var_AssetPath.StartsWith("Assets")
                        && !var_AssetPath.StartsWith("Packages"))
                    {
                        continue;
                    }

                    // Check Assets for requested extensions.
                    for (int i = 0; i < this.extensionToIterateArray.Length; i++)
                    {
                        if (var_AssetPath.EndsWith(this.extensionToIterateArray[i]))
                        {
                            if (this.extensionToAssetPathArrayDictionary.TryGetValue(this.extensionToIterateArray[i], out List<UnityAssetReference> var_UnityAssetReferenceList))
                            {
                                // Already added extension.
                            }
                            else
                            {
                                // New extension.
                                var_UnityAssetReferenceList = new List<UnityAssetReference>();

                                this.extensionToAssetPathArrayDictionary.Add(this.extensionToIterateArray[i], var_UnityAssetReferenceList);
                            }

                            // Try to create a new UnityAssetReference.
                            try
                            {
                                // Add new UnityAssetReference.
                                var_UnityAssetReferenceList.Add(new UnityAssetReference(var_AssetPath));
                            }
                            catch(Exception var_Exception)
                            {
                                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, String.Format("Failed to create Unity Asset Reference for asset at path '{0}'. Exception: {1}", var_AssetPath, var_Exception));
                            }
                            break;
                        }
                    }
                }
            }
            catch(Exception var_Exception)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Error, String.Format("Failed to create Unity Asset Cache. Exception: {0}", var_Exception));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns an Array of all full file paths for assets with _Extension.
        /// </summary>
        /// <param name="_Extension"></param>
        /// <returns></returns>
        public List<UnityAssetReference> GetUnityAssetReferences(String _Extension)
        {
            if (this.extensionToAssetPathArrayDictionary.TryGetValue(_Extension, out List<UnityAssetReference> var_UnityAssetReferenceArray))
            {
                return var_UnityAssetReferenceArray;
            }
            else
            {
                return new List<UnityAssetReference>();
            }
        }

        /// <summary>
        /// Returns a dictionary with all extensions and UnityAssetReference.
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, List<UnityAssetReference>> GetUnityAssetReferences()
        {
            return this.extensionToAssetPathArrayDictionary;
        }

        #endregion

        // Load
        #region Load

        /// <summary>
        /// Load the UnityAssetCache.
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            // Init Extension to Asset Path Array Dictionary.
            this.extensionToAssetPathArrayDictionary = new Dictionary<string, List<UnityAssetReference>>();

            // Create Cache.
            return this.CreateCache();
        }

        #endregion

        // Unload
        #region Unload

        /// <summary>
        /// Unload the UnityAssetCache.
        /// </summary>
        /// <returns></returns>
        public bool Unload()
        {
            // Clear Extension to Asset Path Array Dictionary.
            this.extensionToAssetPathArrayDictionary = null;

            // Return true.
            return true;
        }

        #endregion
    }
}
