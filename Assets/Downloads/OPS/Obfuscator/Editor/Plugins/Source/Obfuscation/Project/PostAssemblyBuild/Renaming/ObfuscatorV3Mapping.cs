using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Json
// using OPS.Serialization.Json;
// using OPS.Serialization.Json.Interfaces;

// OPS - Version
using OPS.Editor.IO.Version;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming
{
    /// <summary>
    /// Reprensents the Obfuscator pre version 4.0 mapping.
    /// </summary>
    internal class ObfuscatorV3Mapping : IRenameMapping
    {
        // Constructor
        #region Constructor

        public ObfuscatorV3Mapping()
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
        /// Always Version 3.0.
        /// </summary>
        public string Version { get; private set; } = "3.0";

        /// <summary>
        /// Not needed.
        /// </summary>
        /// <typeparam name="TVersionAble"></typeparam>
        /// <param name="_VersionAble"></param>
        /// <returns></returns>
        public bool LoadFromVersion<TVersionAble>(TVersionAble _VersionAble) where TVersionAble : IVersionAble
        {
            throw new NotImplementedException();
        }

        #endregion

        // Mapping
        #region Mapping

        /// <summary>
        /// Dictionary of a mapping, which maps keys to obfuscated names.
        /// </summary>
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
        /// Not needed.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKey"></param>
        /// <param name="_Name"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        public bool Add(EMemberType _MemberType, IMemberKey _MemberKey, string _Name, bool _Override = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not needed.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKey"></param>
        /// <returns></returns>
        public string Get(EMemberType _MemberType, IMemberKey _MemberKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not needed.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public List<IMemberKey> GetAllMemberKeys(EMemberType _MemberType, String _Name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not needed.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public List<IMemberKey> GetAllMemberKeys(String _Name)
        {
            throw new NotImplementedException();
        }

        #endregion

        // Serialization
        #region Serialization

        /// <summary>
        /// Loads the mapping from a file content.
        /// </summary>
        /// <param name="_FileContent">The content of the file.</param>
        public void Deserialize(String _FileContent)
        {
            try
            {
                // Read mapping from content.
                using (System.IO.StreamReader var_Reader = new System.IO.StreamReader(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(_FileContent))))
                {
                    // Fields
                    int var_FieldIndex = int.Parse(var_Reader.ReadLine());
                    int var_FieldCount = int.Parse(var_Reader.ReadLine());
                    for (int i = 0; i < var_FieldCount; i++)
                    {
                        // Read!
                        String var_Key = var_Reader.ReadLine();
                        String var_Value = var_Reader.ReadLine();

                        // Resolve!
                        this.Helper_Deserialize_3_0_Member(var_Key, out String var_Assembly, out String var_FullName);

                        // Add to mapping!
                        this.GetMapping(EMemberType.Field).Add(new FieldKey(var_Assembly, var_FullName), var_Value);
                    }

                    // Properties
                    int var_PropertyIndex = int.Parse(var_Reader.ReadLine());
                    int var_PropertyCount = int.Parse(var_Reader.ReadLine());
                    for (int i = 0; i < var_PropertyCount; i++)
                    {
                        // Read!
                        String var_Key = var_Reader.ReadLine();
                        String var_Value = var_Reader.ReadLine();

                        // Resolve!
                        this.Helper_Deserialize_3_0_Member(var_Key, out String var_Assembly, out String var_FullName);

                        // Add to mapping!
                        this.GetMapping(EMemberType.Property).Add(new PropertyKey(var_Assembly, var_FullName), var_Value);
                    }

                    // Events
                    int var_EventIndex = int.Parse(var_Reader.ReadLine());
                    int var_EventCount = int.Parse(var_Reader.ReadLine());
                    for (int i = 0; i < var_EventCount; i++)
                    {
                        // Read!
                        String var_Key = var_Reader.ReadLine();
                        String var_Value = var_Reader.ReadLine();

                        // Resolve!
                        this.Helper_Deserialize_3_0_Member(var_Key, out String var_Assembly, out String var_FullName);

                        // Add to mapping!
                        this.GetMapping(EMemberType.Event).Add(new EventKey(var_Assembly, var_FullName), var_Value);
                    }

                    // Methods
                    int var_MethodIndex = int.Parse(var_Reader.ReadLine());
                    int var_MethodCount = int.Parse(var_Reader.ReadLine());
                    for (int i = 0; i < var_MethodCount; i++)
                    {
                        // Read!
                        String var_Key = var_Reader.ReadLine();
                        String var_Value = var_Reader.ReadLine();

                        // Resolve!
                        this.Helper_Deserialize_3_0_Member(var_Key, out String var_Assembly, out String var_FullName);

                        // Add to mapping!
                        this.GetMapping(EMemberType.Method).Add(new MethodKey(var_Assembly, var_FullName), var_Value);
                    }

                    // Types
                    int var_TypeIndex = int.Parse(var_Reader.ReadLine());
                    int var_TypeCount = int.Parse(var_Reader.ReadLine());
                    for (int i = 0; i < var_TypeCount; i++)
                    {
                        // Read!
                        String var_Key = var_Reader.ReadLine();
                        String var_Value = var_Reader.ReadLine();

                        // Resolve!
                        this.Helper_Deserialize_3_0_Type(var_Key, out String var_Assembly, out String var_FullName);

                        // Add to mapping!
                        this.GetMapping(EMemberType.Type).Add(new TypeKey(var_Assembly, var_FullName), var_Value);
                    }

                    // Namespaces
                    int var_NamespaceCount = int.Parse(var_Reader.ReadLine());
                    for (int i = 0; i < var_NamespaceCount; i++)
                    {
                        // Read!
                        String var_Key = var_Reader.ReadLine();
                        String var_Value = var_Reader.ReadLine();

                        // Resolve!
                        this.Helper_Deserialize_3_0_Type(var_Key, out String var_Assembly, out String var_FullName);

                        // Add to mapping!
                        this.GetMapping(EMemberType.Namespace).Add(new TypeKey(var_Assembly, var_FullName), var_Value);
                    }
                }
            }
            catch (Exception e)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Could not read the old mapping, Exception: " + e.ToString(), true);
                return;
            }
        }

        private bool Helper_Deserialize_3_0_Member(String _Value, out String _Assembly, out String _FullName)
        {
            if (String.IsNullOrEmpty(_Value))
            {
                _Assembly = null;
                _FullName = null;

                return false;
            }

            // Always in format Type|Member|FullName
            var var_Split = _Value.Split('|');
            if (var_Split.Length < 3)
            {
                _Assembly = null;
                _FullName = null;

                return false;
            }

            // Type is first.
            String var_Type = var_Split.First();

            this.Helper_Deserialize_3_0_Type(var_Type, out _Assembly, out _);

            // FullName is last.
            _FullName = var_Split.Last();

            return true;
        }

        private bool Helper_Deserialize_3_0_Type(String _Value, out String _Assembly, out String _FullName)
        {
            if (String.IsNullOrEmpty(_Value))
            {
                _Assembly = null;
                _FullName = null;

                return false;
            }

            // Split Type
            var var_Split = _Value.Split('?');
            if (var_Split.Length < 6)
            {
                _Assembly = null;
                _FullName = null;

                return false;
            }

            // Assembly / Scope is first.
            _Assembly = var_Split.First();

            // FullName is last.
            _FullName = var_Split.Last();

            return true;
        }

        #endregion
    }
}
