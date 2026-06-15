// System
using System;
using System.Collections.Generic;
using System.Linq;

// OPS - Json
using OPS.Serialization.Json;
using OPS.Serialization.Json.Attributes;

// OPS - Version
using OPS.Editor.IO.Version;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming
{
    /// <summary>
    /// Reprensents the Obfuscator after version 4.0 mapping.
    /// </summary>
    [JsonObject]
    internal class ObfuscatorV4Mapping : IRenameMapping
    {
        // Constructor
        #region Constructor

        public ObfuscatorV4Mapping()
        {
            // Init Mapping.
            this.memberMappingDictionary = new Dictionary<EMemberType, MemberKeyMapping>();

            this.AddMapping(EMemberType.Namespace, new MemberKeyMapping(EMemberType.Namespace));
            this.AddMapping(EMemberType.Type, new MemberKeyMapping(EMemberType.Type));
            this.AddMapping(EMemberType.Method, new MemberKeyMapping(EMemberType.Method));
            this.AddMapping(EMemberType.Field, new MemberKeyMapping(EMemberType.Field));
            this.AddMapping(EMemberType.Property, new MemberKeyMapping(EMemberType.Property));
            this.AddMapping(EMemberType.Event, new MemberKeyMapping(EMemberType.Event));
        }

        #endregion

        // Version
        #region Version

        /// <summary>
        /// Always Version 4.0.
        /// </summary>
        [JsonProperty(Name = "Version")]
        public string Version { get; private set; } = "4.0";

        /// <summary>
        /// Load this version 4.0 mapping from another older version.
        /// </summary>
        /// <typeparam name="TRenameMapping"></typeparam>
        /// <param name="_RenameMapping"></param>
        /// <returns></returns>
        public bool LoadFromVersion<TRenameMapping>(TRenameMapping _RenameMapping)
            where TRenameMapping : IVersionAble
        {
            if (_RenameMapping is IRenameMapping)
            {
                IRenameMapping var_RenameMapping = (IRenameMapping)_RenameMapping;

                this.Add(EMemberType.Namespace, var_RenameMapping.GetMapping(EMemberType.Namespace));
                this.Add(EMemberType.Type, var_RenameMapping.GetMapping(EMemberType.Type));
                this.Add(EMemberType.Method, var_RenameMapping.GetMapping(EMemberType.Method));
                this.Add(EMemberType.Field, var_RenameMapping.GetMapping(EMemberType.Field));
                this.Add(EMemberType.Property, var_RenameMapping.GetMapping(EMemberType.Property));
                this.Add(EMemberType.Event, var_RenameMapping.GetMapping(EMemberType.Event));

                return true;
            }

            return false;
        }

        #endregion

        // Mapping
        #region Mapping

        /// <summary>
        /// Dictionary of a mapping, which maps keys to obfuscated names.
        /// </summary>
        [JsonProperty(Name = "MemberTyp_Mapping")]
        private Dictionary<EMemberType, MemberKeyMapping> memberMappingDictionary;

        /// <summary>
        /// Add a _MemberType to _MemberKeyMapping entry.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKeyMapping"></param>
        private void AddMapping(EMemberType _MemberType, MemberKeyMapping _MemberKeyMapping)
        {
            this.memberMappingDictionary.Add(_MemberType, _MemberKeyMapping);
        }

        /// <summary>
        /// Returns the IMemberKeyMapping belonging to _MemberType.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <returns></returns>
        public IMemberKeyMapping GetMapping(EMemberType _MemberType)
        {
            if (this.memberMappingDictionary.TryGetValue(_MemberType, out MemberKeyMapping _Mapping))
            {
                return _Mapping;
            }

            return null;
        }

        /// <summary>
        /// Add _MemberKey with obfuscated _Name to mapping of _MemberType.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKey"></param>
        /// <param name="_Name"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        public bool Add(EMemberType _MemberType, IMemberKey _MemberKey, string _Name, bool _Override = false)
        {
            // Get mapping.
            IMemberKeyMapping var_MemberKeyMapping = this.GetMapping(_MemberType);

            if (var_MemberKeyMapping == null)
            {
                return false;
            }

            // Add to mapping.
            var_MemberKeyMapping.Add(_MemberKey, _Name);

            return true;
        }

        /// <summary>
        /// Add a whole _MemberKeyMapping to IMemberKeyMapping for _MemberType.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKeyMapping"></param>
        /// <returns></returns>
        private bool Add(EMemberType _MemberType, IMemberKeyMapping _MemberKeyMapping)
        {
            bool var_Failed = false;

            foreach (var var_Pair in _MemberKeyMapping)
            {
                if (!this.Add(_MemberType, var_Pair.Key, var_Pair.Value))
                {
                    var_Failed = true;
                    break;
                }
            }

            return !var_Failed;
        }

        /// <summary>
        /// Returns the obfuscated name for _MemberKey in mapping of _MemberType.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKey"></param>
        /// <returns></returns>
        public string Get(EMemberType _MemberType, IMemberKey _MemberKey)
        {
            // Get mapping.
            IMemberKeyMapping var_MemberKeyMapping = this.GetMapping(_MemberType);

            if (var_MemberKeyMapping == null)
            {
                return null;
            }

            // Return the name for _MemberKey.
            return var_MemberKeyMapping.Get(_MemberKey);
        }

        /// <summary>
        /// Returns all IMemberKey sharing the same _Name in _MemberType mapping.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public List<IMemberKey> GetAllMemberKeys(EMemberType _MemberType, String _Name)
        {
            // Get mapping.
            IMemberKeyMapping var_MemberKeyMapping = this.GetMapping(_MemberType);

            if (var_MemberKeyMapping == null)
            {
                return null;
            }

            // Return the IMemberKey for _Name.
            return var_MemberKeyMapping.GetAllMemberKeys(_Name);
        }

        /// <summary>
        /// Returns all IMemberKey sharing the same _Name in all MemberType to IMemberKeyMapping Mappings.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public List<IMemberKey> GetAllMemberKeys(String _Name)
        {
            List<IMemberKey> var_MemberKeyList = new List<IMemberKey>();

            foreach (var var_Pair in this.memberMappingDictionary)
            {
                var_MemberKeyList = var_MemberKeyList.Union(var_Pair.Value.GetAllMemberKeys(_Name)).ToList();
            }

            return var_MemberKeyList;
        }

        #endregion
    }
}
