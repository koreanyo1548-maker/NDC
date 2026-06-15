// System
using System;
using System.Collections.Generic;
using System.Threading;

// Unity
using UnityEngine;
using UnityEngine.Networking;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly;
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using System.Threading.Tasks;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming
{
    /// <summary>
    /// Contains the mappings, and the shall rename/not rename stuff.
    /// </summary>
    public class RenameManager
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// The rename manager belongs to a PostAssemblyBuildStep.
        /// </summary>
        /// <param name="_Step"></param>
        public RenameManager(PostAssemblyBuildStep _Step)
        {
            this.step = _Step;
        }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// The used project.
        /// </summary>
        private readonly PostAssemblyBuildStep step;

        #endregion

        // Load
        #region Load

        /// <summary>
        /// Load the RenameManager.
        /// </summary>
        public bool Load()
        {
            // Already loaded?
            if (this.doNotObfuscateDictionary != null)
            {
                return true;
            }

            //Init - Do not obfuscate dictionaries.
            this.doNotObfuscateDictionary = new Dictionary<EMemberType, Dictionary<IMemberKey, List<string>>>();

            //Init - The NameGenerator
            this.NameGenerator = new NameGenerator();

            //Init - The Mappings
            this.OriginalMapping = new ObfuscatorV4Mapping();
            this.CurrentMapping = new ObfuscatorV4Mapping();
            this.LoadedMapping = new ObfuscatorV4Mapping();

            // Load Original Mapping
            this.SetupOriginalMapping();

            return true;
        }

        #endregion

        // Unload
        #region Unload

        /// <summary>
        /// Unload the Renamemanager.
        /// </summary>
        public bool Unload()
        {
            // Already unloaded.
            if (this.doNotObfuscateDictionary == null)
            {
                return true;
            }

            //Clear - Do not obfuscate dictionaries.
            this.doNotObfuscateDictionary = null;

            //Clear - The NameGenerator
            this.NameGenerator = null;

            //Clear - The Mappings
            this.OriginalMapping = null;
            this.CurrentMapping = null;
            this.LoadedMapping = null;

            return true;
        }

        #endregion

        //Do not rename
        #region Do not rename

        /// <summary>
        /// Member Type, Dictionary of Original Keys and a List of not obfuscation cause.
        /// </summary>
        private Dictionary<EMemberType, Dictionary<IMemberKey, List<String>>> doNotObfuscateDictionary;

        /// <summary>
        /// Do not obfuscate _MemberDefinition of _MemberType with _Cause of not obfuscation.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberDefinition"></param>
        /// <param name="_Cause"></param>
        public void AddDoNotObfuscate(EMemberType _MemberType, IMemberDefinition _MemberDefinition, String _Cause)
        {
            if (_MemberDefinition == null)
            {
                throw new ArgumentNullException("_MemberDefinition");
            }
            if (String.IsNullOrEmpty(_Cause))
            {
                _Cause = "Non cause given.";
            }

            // Got not loaded yet!
            if (this.doNotObfuscateDictionary == null)
            {
                return;
            }

            IMemberKey var_OriginalKey = null;

            switch (_MemberType)
            {
                case EMemberType.Namespace:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Type:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Method:
                    {
                        var_OriginalKey = this.step.GetCache<MethodCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as MethodDefinition);
                        break;
                    }
                case EMemberType.Field:
                    {
                        var_OriginalKey = this.step.GetCache<FieldCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as FieldDefinition);
                        break;
                    }
                case EMemberType.Property:
                    {
                        var_OriginalKey = this.step.GetCache<PropertyCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as PropertyDefinition);
                        break;
                    }
                case EMemberType.Event:
                    {
                        var_OriginalKey = this.step.GetCache<EventCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as EventDefinition);
                        break;
                    }
            }

            if (var_OriginalKey == null)
            {
                // Is not in cache.
                return;
            }

            // Get belonging fullname to cause list dictionary.
            Dictionary<IMemberKey, List<String>> var_FullNameToCauseListDictionary = null;

            // Or if does not exist, create one.
            if (!this.doNotObfuscateDictionary.TryGetValue(_MemberType, out var_FullNameToCauseListDictionary))
            {
                var_FullNameToCauseListDictionary = new Dictionary<IMemberKey, List<string>>();
                this.doNotObfuscateDictionary.Add(_MemberType, var_FullNameToCauseListDictionary);
            }

            // Add _Cause to the cause list.
            List<String> var_CauseList;
            if (!var_FullNameToCauseListDictionary.TryGetValue(var_OriginalKey, out var_CauseList))
            {
                var_CauseList = new List<string>();
                var_FullNameToCauseListDictionary.Add(var_OriginalKey, var_CauseList);
            }
            var_CauseList.Add(_Cause);
        }

        ////////////////////////////

        /// <summary>
        /// Returns if the _MemberDefinition of _MemberType will be not obfuscated.
        /// True: Getting NOT obfuscated.
        /// False: Getting obfuscated.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public bool GetDoNotObfuscate(EMemberType _MemberType, IMemberDefinition _MemberDefinition)
        {
            if (_MemberDefinition == null)
            {
                throw new ArgumentNullException("_MemberDefinition");
            }

            // Has the ObfuscateAnywayAttribute, so just force obfuscate!
            if (AttributeHelper.HasCustomAttribute(_MemberDefinition, typeof(OPS.Obfuscator.Attribute.ObfuscateAnywayAttribute).Name))
            {
                return false;
            }

            IMemberKey var_OriginalKey = null;

            switch (_MemberType)
            {
                case EMemberType.Namespace:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Type:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Method:
                    {
                        var_OriginalKey = this.step.GetCache<MethodCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as MethodDefinition);
                        break;
                    }
                case EMemberType.Field:
                    {
                        var_OriginalKey = this.step.GetCache<FieldCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as FieldDefinition);
                        break;
                    }
                case EMemberType.Property:
                    {
                        var_OriginalKey = this.step.GetCache<PropertyCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as PropertyDefinition);
                        break;
                    }
                case EMemberType.Event:
                    {
                        var_OriginalKey = this.step.GetCache<EventCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as EventDefinition);
                        break;
                    }
            }

            if (var_OriginalKey == null)
            {
                // Is not in cache.
                return false;
            }

            // Get belonging fullname to cause list dictionary.
            Dictionary<IMemberKey, List<String>> var_FullNameToCauseListDictionary = null;

            // Or if does not exist, return.
            if (!this.doNotObfuscateDictionary.TryGetValue(_MemberType, out var_FullNameToCauseListDictionary))
            {
                return false;
            }

            // Return if contained in do not obfuscate dictionary.
            return var_FullNameToCauseListDictionary.ContainsKey(var_OriginalKey);
        }

        ////////////////////////////

        /// <summary>
        /// Returns the list of not obfuscation cause.
        /// If there is no cause null will be returned.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public List<String> GetDoNotObfuscateCause(EMemberType _MemberType, IMemberDefinition _MemberDefinition)
        {
            if (_MemberDefinition == null)
            {
                throw new ArgumentNullException("_MemberDefinition");
            }

            // Has the ObfuscateAnywayAttribute, so just force obfuscate!
            if (AttributeHelper.HasCustomAttribute(_MemberDefinition, typeof(OPS.Obfuscator.Attribute.ObfuscateAnywayAttribute).Name))
            {
                return null;
            }

            IMemberKey var_OriginalKey = null;

            switch (_MemberType)
            {
                case EMemberType.Namespace:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Type:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Method:
                    {
                        var_OriginalKey = this.step.GetCache<MethodCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as MethodDefinition);
                        break;
                    }
                case EMemberType.Field:
                    {
                        var_OriginalKey = this.step.GetCache<FieldCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as FieldDefinition);
                        break;
                    }
                case EMemberType.Property:
                    {
                        var_OriginalKey = this.step.GetCache<PropertyCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as PropertyDefinition);
                        break;
                    }
                case EMemberType.Event:
                    {
                        var_OriginalKey = this.step.GetCache<EventCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as EventDefinition);
                        break;
                    }
            }

            if (var_OriginalKey == null)
            {
                // Is not in cache.
                return null;
            }

            // Get belonging fullname to cause list dictionary.
            Dictionary<IMemberKey, List<String>> var_FullNameToCauseListDictionary = null;

            // Or if does not exist return.
            if (!this.doNotObfuscateDictionary.TryGetValue(_MemberType, out var_FullNameToCauseListDictionary))
            {
                return null;
            }

            // Get and return the list of causes.
            List<String> var_CauseList;
            if (var_FullNameToCauseListDictionary.TryGetValue(var_OriginalKey, out var_CauseList))
            {
                return var_CauseList;
            }

            return null;
        }

        /// <summary>
        /// Returns the count of the to not obfuscate member per type.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <returns></returns>
        public int GetCountOfDoNotObfuscate(EMemberType _MemberType)
        {
            if (this.doNotObfuscateDictionary.TryGetValue(_MemberType, out var var_Dictionary))
            {
                return var_Dictionary.Count;
            }

            return 0;
        }

        #endregion

        //Renaming Mapping
        #region Renaming Mapping

        /// <summary>
        /// The original mapping (from original key to name) of the assembly members.
        /// </summary>
        public IRenameMapping OriginalMapping { get; private set; }

        /// <summary>
        /// The current obfuscation mapping.
        /// </summary>
        public IRenameMapping CurrentMapping { get; private set; }

        /// <summary>
        /// The loaded obfuscation mapping.
        /// </summary>
        public IRenameMapping LoadedMapping { get; private set; }

        /// <summary>
        /// Setup the original mapping.
        /// </summary>
        private void SetupOriginalMapping()
        {
            //Iterate all assemblies and members and add those to the original mapping.
            foreach (AssemblyInfo var_AssemblyInfo in this.step.DataContainer.ObfuscateAssemblyList)
            {
                foreach (TypeDefinition var_TypeDefinition in var_AssemblyInfo.GetAllTypeDefinitions())
                {
                    this.OriginalMapping.GetMapping(EMemberType.Namespace).Add(new TypeKey(var_TypeDefinition), var_TypeDefinition.Namespace);
                    this.OriginalMapping.GetMapping(EMemberType.Type).Add(new TypeKey(var_TypeDefinition), var_TypeDefinition.Name);

                    foreach (var var_Member in var_TypeDefinition.Methods)
                    {
                        this.OriginalMapping.GetMapping(EMemberType.Method).Add(new MethodKey(var_Member), var_Member.Name);
                    }

                    foreach (var var_Member in var_TypeDefinition.Fields)
                    {
                        this.OriginalMapping.GetMapping(EMemberType.Field).Add(new FieldKey(var_Member), var_Member.Name);
                    }

                    foreach (var var_Member in var_TypeDefinition.Properties)
                    {
                        this.OriginalMapping.GetMapping(EMemberType.Property).Add(new PropertyKey(var_Member), var_Member.Name);
                    }

                    foreach (var var_Member in var_TypeDefinition.Events)
                    {
                        this.OriginalMapping.GetMapping(EMemberType.Event).Add(new EventKey(var_Member), var_Member.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Get a unique name for _MemberDefinition of _MemberType.
        /// Will check the loaded, current or original mapping with _Tries for a unique name.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberDefinition"></param>
        /// <param name="_Tries"></param>
        /// <returns></returns>
        public String GetUniqueObfuscatedName(EMemberType _MemberType, IMemberDefinition _MemberDefinition, int _Tries = Int32.MaxValue)
        {
            String var_ObfuscatedName = null;

            for (int i = 0; i < _Tries; i++)
            {
                var_ObfuscatedName = this.NameGenerator.GetNextName(_MemberType, _MemberDefinition);

                if (!this.IsNameFree(_MemberDefinition, _MemberType, var_ObfuscatedName))
                {
                    var_ObfuscatedName = null;
                    continue;
                }

                break;
            }

            return var_ObfuscatedName;
        }

        /// <summary>
        /// Add _ObfuscatedName for _MemberDefinition of _MemberType to the current mapping. 
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberDefinition"></param>
        /// <param name="_ObfuscatedName"></param>
        public void AddObfuscated(EMemberType _MemberType, IMemberDefinition _MemberDefinition, String _ObfuscatedName)
        {
            if (_MemberDefinition == null)
            {
                throw new ArgumentNullException("_MemberDefinition");
            }

            IMemberKey var_OriginalKey = null;

            switch (_MemberType)
            {
                case EMemberType.Namespace:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Type:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Method:
                    {
                        var_OriginalKey = this.step.GetCache<MethodCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as MethodDefinition);
                        break;
                    }
                case EMemberType.Field:
                    {
                        var_OriginalKey = this.step.GetCache<FieldCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as FieldDefinition);
                        break;
                    }
                case EMemberType.Property:
                    {
                        var_OriginalKey = this.step.GetCache<PropertyCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as PropertyDefinition);
                        break;
                    }
                case EMemberType.Event:
                    {
                        var_OriginalKey = this.step.GetCache<EventCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as EventDefinition);
                        break;
                    }
            }

            if (var_OriginalKey == null)
            {
                // Is not in cache.
                return;
            }

            // Add to current mapping.
            this.CurrentMapping.GetMapping(_MemberType).Add(var_OriginalKey, _ObfuscatedName);
        }

        /// <summary>
        /// Returns the obfuscated name of _MemberDefinition of _MemberType.
        /// The method first checks in the current mapping, if there is a obfuscated name, and returns it.
        /// If there is none, it checks the loaded mapping and returns it.
        /// If non found, returns null.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public String GetObfuscated(EMemberType _MemberType, IMemberDefinition _MemberDefinition)
        {
            if (_MemberDefinition == null)
            {
                throw new ArgumentNullException("_MemberDefinition");
            }

            // Has the ObfuscateAnywayAttribute, so return its value!
            if (AttributeHelper.TryGetCustomAttribute(_MemberDefinition, typeof(OPS.Obfuscator.Attribute.ObfuscateAnywayAttribute).Name, out CustomAttribute var_ObfuscateAnywayAttribute))
            {
                return (String)var_ObfuscateAnywayAttribute.ConstructorArguments[0].Value;
            }

            IMemberKey var_OriginalKey = null;

            switch (_MemberType)
            {
                case EMemberType.Namespace:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Type:
                    {
                        var_OriginalKey = this.step.GetCache<TypeCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as TypeDefinition);
                        break;
                    }
                case EMemberType.Method:
                    {
                        var_OriginalKey = this.step.GetCache<MethodCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as MethodDefinition);
                        break;
                    }
                case EMemberType.Field:
                    {
                        var_OriginalKey = this.step.GetCache<FieldCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as FieldDefinition);
                        break;
                    }
                case EMemberType.Property:
                    {
                        var_OriginalKey = this.step.GetCache<PropertyCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as PropertyDefinition);
                        break;
                    }
                case EMemberType.Event:
                    {
                        var_OriginalKey = this.step.GetCache<EventCache>().GetOriginalMemberKeyByMemberDefinition(_MemberDefinition as EventDefinition);
                        break;
                    }
            }

            if (var_OriginalKey == null)
            {
                // Is not in cache.
                return null;
            }

            // First try to get from current mapping.
            String var_ObfuscatedName = this.CurrentMapping.GetMapping(_MemberType).Get(var_OriginalKey) as String;

            // Second try from loaded mapping.
            if (var_ObfuscatedName == null)
            {
                var_ObfuscatedName = this.LoadedMapping.GetMapping(_MemberType).Get(var_OriginalKey) as String;
            }

            return var_ObfuscatedName;
        }

        /// <summary>
        /// Load a mapping from a file.
        /// </summary>
        /// <param name="_FilePath">The full or relative path to the mapping file.</param>
        public void LoadMappingFromFile(String _FilePath)
        {
            // Check if file exists.
            if (!System.IO.File.Exists(_FilePath))
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "There is no mapping file at path: " + _FilePath, true);
                return;
            }

            // Try to read the file and load the mapping.
            try
            {
                using (System.IO.StreamReader var_Reader = new System.IO.StreamReader(new System.IO.FileStream(_FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read)))
                {
                    String var_FileContent = var_Reader.ReadToEnd();

                    this.LoadMapping(var_FileContent);
                }
            }
            catch (Exception e)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to read mapping file at path: " + _FilePath + ". Exception: " + e.ToString(), true);
            }
        }

        /// <summary>
        /// When loading the mapping from an url, the version parameter in the url is replaced with the current version of the app.
        /// </summary>
        private const String CVersionParameter = "{version}";

        /// <summary>
        /// Load a mapping from an url. The url must return the mapping as json.
        /// </summary>
        /// <param name="_Url">The url to the mapping file.</param>
        /// <remarks>
        /// You can use optional the {version} parameter in the url to replace it with the current version of the app (Application.version).
        /// </remarks>
        public void LoadMappingFromUrl(String _Url)
        {
            // The endpoint to request the mapping from.
            String var_Endpoint = _Url;

            // Replace the version parameter in the url with the current version (Application.version) of the app. Application.version returns the current version of
            // the Application. To set the version number in Unity, go to Edit > Project Settings > Player. This is the same as PlayerSettings.bundleVersion.
            var_Endpoint = var_Endpoint.ToLower().Replace(CVersionParameter, Application.version);

            // Send a request to the server to get the hash as hex-string.
            using (UnityWebRequest var_Request = UnityWebRequest.Get(var_Endpoint))
            {
                // Send the request and wait for the response.
                var var_RequestWaiter = var_Request.SendWebRequest();

                // Wait for the request to finish.
                while (!var_RequestWaiter.isDone)
                {
                    // Wait for the request to finish.
                    Thread.Sleep(100);
                }

#if UNITY_2021_2_OR_NEWER
                // If the request was not successful, log the error and return.
                if (var_Request.result != UnityWebRequest.Result.Success)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to read mapping file at url: " + var_Endpoint + ". Exception: " + var_Request.error, true);
                    return;
                }
#else
                // If the request was not successful, log the error and return.
                if (var_Request.isNetworkError || var_Request.isHttpError)
                {
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to read mapping file at url: " + var_Endpoint + ". Exception: " + var_Request.error, true);
                    return;
                }
#endif

                // Get the content of the response.
                String var_DownloadedContent = var_Request.downloadHandler.text;

                // Load the mapping from the downloaded content.
                this.LoadMapping(var_DownloadedContent);
            }
        }

        /// <summary>
        /// Load a mapping from a string.
        /// </summary>
        /// <param name="_FileContent">The content of the mapping file.</param>
        private void LoadMapping(String _FileContent)
        {
            // Check Version
            bool var_Pre_4_0 = false;

            // Get the first line of the file content.
            String var_First_Line = _FileContent.Split(new String[] { Environment.NewLine }, StringSplitOptions.None)[0];

            // If is a json object, is past 4.0.
            // If not a json object, is pre 4.0.
            if (!OPS.Serialization.Json.JsonSerializer.IsJson(var_First_Line))
            {
                var_Pre_4_0 = true;
            }

            if (var_Pre_4_0)
            {
                // Create a v3 mapping.
                ObfuscatorV3Mapping var_ObfuscatorV3Mapping = new ObfuscatorV3Mapping();

                // Deserialize from file.
                var_ObfuscatorV3Mapping.Deserialize(_FileContent);

                // Parse to v4.
                this.LoadedMapping.LoadFromVersion(var_ObfuscatorV3Mapping);
            }
            else
            {
                // Read mapping.
                this.LoadedMapping = OPS.Serialization.Json.JsonSerializer.ParseStringToObject<ObfuscatorV4Mapping>(_FileContent);
            }
        }

        /// <summary>
        /// Save the current mapping to a file.
        /// </summary>
        /// <param name="_FilePath">The full or relative path to the mapping file.</param>
        public void SaveMappingToFile(String _FilePath)
        {
            // Create the directory if it does not exist.
            if(!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(_FilePath)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_FilePath));
            }

            // Saves the mapping.
            using (System.IO.StreamWriter var_Writer = new System.IO.StreamWriter(new System.IO.FileStream(_FilePath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite)))
            {
                var_Writer.Write(OPS.Serialization.Json.JsonSerializer.ParseObjectToJson(this.CurrentMapping, OPS.Serialization.Json.EJsonTextMode.Compact));
            }
        }

        /// <summary>
        /// Save the current mapping to an url. The url must accept a post request with the mapping as json.
        /// </summary>
        /// <param name="_Url">The url to the mapping file.</param>
        /// <exception cref="Exception">Throws an exception if the request was not successful.</exception>
        public void SaveMappingToUrl(String _Url)
        {
            // The endpoint to store the mapping to.
            String var_Endpoint = _Url;

            // Replace the version parameter in the url with the current version (Application.version) of the app. Application.version returns the current version of
            // the Application. To set the version number in Unity, go to Edit > Project Settings > Player. This is the same as PlayerSettings.bundleVersion.
            var_Endpoint = var_Endpoint.ToLower().Replace(CVersionParameter, Application.version);

            // Serialize the current mapping to json.
            String var_Json = OPS.Serialization.Json.JsonSerializer.ParseObjectToJson(this.CurrentMapping, OPS.Serialization.Json.EJsonTextMode.Compact);

            // Send a request to the server to post the mapping.
#if UNITY_2023_1_OR_NEWER
            using (UnityWebRequest var_Request = UnityWebRequest.Post(var_Endpoint, var_Json, "application/json"))
            {
                // Send the request and wait for the response.
                var var_RequestWaiter = var_Request.SendWebRequest();

                while (!var_RequestWaiter.isDone)
                {
                    // Wait for the request to finish.
                    Thread.Sleep(100);
                }

                // If the request was not successful, throw an exception.
                if (var_Request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(var_Request.error);
                }
            }
#elif UNITY_2021_2_OR_NEWER
            using (UnityWebRequest var_Request = UnityWebRequest.PostWwwForm(var_Endpoint, var_Json))
            {
                // Send the request and wait for the response.
                var var_RequestWaiter = var_Request.SendWebRequest();

                while (!var_RequestWaiter.isDone)
                {
                    // Wait for the request to finish.
                    Thread.Sleep(100);
                }

                // If the request was not successful, throw an exception.
                if (var_Request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(var_Request.error);
                }
            }
#else
            using (UnityWebRequest var_Request = UnityWebRequest.Post(var_Endpoint, var_Json))
            {
                // Send the request and wait for the response.
                var var_RequestWaiter = var_Request.SendWebRequest();

                while (!var_RequestWaiter.isDone)
                {
                    // Wait for the request to finish.
                    Thread.Sleep(100);
                }

                // If the request was not successful, throw an exception.
                if (var_Request.isNetworkError || var_Request.isHttpError)
                {
                    throw new Exception(var_Request.error);
                }
            }
#endif
        }

        /// <summary>
        /// Save the current mapping to the temp path.
        /// </summary>
        internal void SaveMappingToTempPath()
        {
            // Get the temp path.
            String var_TempPath = System.IO.Path.Combine(OPS.Obfuscator.Editor.IO.PathHelper.Get_Obfuscator_Editor_Temp_Directory(), DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_Mapping.json");

            // Save the mapping to the temp path.
            this.SaveMappingToFile(var_TempPath);
        }

        #endregion

        // Name Free
        #region Name Free

        /// <summary>
        /// Returns true if the _Name is free and not used either in the loaded, current or original mapping.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <param name="_MemberType"></param>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public bool IsNameFree(IMemberDefinition _MemberDefinition, EMemberType _MemberType, String _Name)
        {
            if (this.NameGenerator.UseGlobalIndexer)
            {
                // Any loaded has the same obfuscated name as _Name.
                if (this.LoadedMapping.GetAllMemberKeys(_Name).Count > 0)
                {
                    return false;
                }

                // Any current assigned has the same obfuscated name as _Name.
                if (this.CurrentMapping.GetAllMemberKeys(_Name).Count > 0)
                {
                    return false;
                }

                // Any has the original name of _Name.
                if (this.OriginalMapping.GetAllMemberKeys(_Name).Count > 0)
                {
                    return false;
                }

                return true;
            }
            else
            {
                // Any loaded has the same obfuscated name as _Name.
                if (this.LoadedMapping.GetMapping(_MemberType).GetAllMemberKeys(_Name).Count > 0)
                {
                    return false;
                }

                // Any current assigned has the same obfuscated name as _Name.
                if (this.CurrentMapping.GetMapping(_MemberType).GetAllMemberKeys(_Name).Count > 0)
                {
                    return false;
                }

                // Any has the original name of _Name.
                if (this.OriginalMapping.GetMapping(_MemberType).GetAllMemberKeys(_Name).Count > 0)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        //Name Generator
        #region Name Generator

        /// <summary>
        /// Used to generate obfuscated names.
        /// </summary>
        public NameGenerator NameGenerator { get; private set; }

        #endregion
    }
}
