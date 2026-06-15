using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// OPS - Json
using OPS.Serialization.Json;
using OPS.Serialization.Json.Attributes;
using OPS.Serialization.Json.Interfaces;

// OPS - Obfuscator - Assembly - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping
{
    /// <summary>
    /// Mapping from the original key of a member to its obfuscated name.
    /// </summary>
    [JsonObject]
    public class MemberKeyMapping : IMemberKeyMapping, ICustomJsonSerializeable
    {
        // Constructor
        #region Constructor

        public MemberKeyMapping(EMemberType _MemberType)
        {
            this.MemberType = _MemberType;
        }

        #endregion

        // Member
        #region Member

        /// <summary>
        /// The Member Type this mapping is for.
        /// </summary>
        [JsonProperty(Name = "MemberType")]
        public EMemberType MemberType { get; private set; }

        /// <summary>
        /// (Original) Member Full Name, Obfuscated Name.
        /// </summary>

        [JsonProperty(Name = "Mapping")]
        private Dictionary<IMemberKey, String> memberKeyToObfuscatedNameDictionary = new Dictionary<IMemberKey, string>();

        /// <summary>
        /// Obfuscated Name without generic suffix (simplified), List of (Original) Member Full Name.
        /// </summary>
        private Dictionary<String, List<IMemberKey>> obfuscatedSimplifiedNameToMemberKeyListDictionary = new Dictionary<string, List<IMemberKey>>();

        #endregion

        // Add
        #region Add

        /// <summary>
        /// Add or override an obfuscated name for a extended full name.
        /// For example: System.Void Method().
        /// </summary>
        /// <param name="_MemberKey"></param>
        /// <param name="_Name"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        public bool Add(IMemberKey _MemberKey, String _Name, bool _Override = false)
        {
            // Get Simplified Name
            String var_Simplified_Name = Helper.IMemberDefinitionHelper.RemoveGenericParameter(_Name);

            if (this.memberKeyToObfuscatedNameDictionary.TryGetValue(_MemberKey, out String var_AlreadySetName))
            {
                //Already added!

                if (_Override)
                {
                    // Get already added simplified name
                    String var_Simplified_AlreadySetName = Helper.IMemberDefinitionHelper.RemoveGenericParameter(var_AlreadySetName);

                    if (this.obfuscatedSimplifiedNameToMemberKeyListDictionary.TryGetValue(var_Simplified_AlreadySetName, out List<IMemberKey> var_Old_Simplified_MemberSharingSameObfuscatedNameList))
                    {
                        var_Old_Simplified_MemberSharingSameObfuscatedNameList.Remove(_MemberKey);
                    }

                    this.memberKeyToObfuscatedNameDictionary[_MemberKey] = _Name;

                    if (this.obfuscatedSimplifiedNameToMemberKeyListDictionary.TryGetValue(var_Simplified_Name, out List<IMemberKey> var_New_Simplified_MemberSharingSameObfuscatedNameList))
                    {
                        var_New_Simplified_MemberSharingSameObfuscatedNameList.Add(_MemberKey);
                    }
                    else
                    {
                        this.obfuscatedSimplifiedNameToMemberKeyListDictionary.Add(var_Simplified_Name, new List<IMemberKey>() { _MemberKey });
                    }
                }
            }
            else
            {
                //Add new!

                // Does not contain _FullName, so just add it.
                this.memberKeyToObfuscatedNameDictionary.Add(_MemberKey, _Name);

                if (this.obfuscatedSimplifiedNameToMemberKeyListDictionary.TryGetValue(var_Simplified_Name, out List<IMemberKey> var_Simplified_MemberSharingSameObfuscatedNameList))
                {
                    var_Simplified_MemberSharingSameObfuscatedNameList.Add(_MemberKey);
                }
                else
                {
                    this.obfuscatedSimplifiedNameToMemberKeyListDictionary.Add(var_Simplified_Name, new List<IMemberKey>() { _MemberKey });
                }
            }

            return true;
        }

        #endregion

        // Get
        #region Get

        /// <summary>
        /// Get the obfuscated name for a member based on its original full name.
        /// For example: System.Void Method().
        /// </summary>
        /// <param name="_MemberKey"></param>
        /// <returns></returns>
        public String Get(IMemberKey _MemberKey)
        {
            String var_ObfuscatedName;
            if (this.memberKeyToObfuscatedNameDictionary.TryGetValue(_MemberKey, out var_ObfuscatedName))
            {
                return var_ObfuscatedName;
            }
            return null;
        }

        /// <summary>
        /// Returns a list of all members original keys by its obfuscated name or simplified obfuscated name.
        /// If there is none, returns an empty list.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public List<IMemberKey> GetAllMemberKeys(String _Name)
        {
            HashSet<IMemberKey> var_MemberKeyHashSet = new HashSet<IMemberKey>();

            String var_Simplified_ObfuscatedName = Helper.IMemberDefinitionHelper.RemoveGenericParameter(_Name);

            if (this.obfuscatedSimplifiedNameToMemberKeyListDictionary.TryGetValue(var_Simplified_ObfuscatedName, out List<IMemberKey> var_MemberSharingSameSimplifiedObfuscatedNameList))
            {
                for (int i = 0; i < var_MemberSharingSameSimplifiedObfuscatedNameList.Count; i++)
                {
                    if (!var_MemberKeyHashSet.Contains(var_MemberSharingSameSimplifiedObfuscatedNameList[i]))
                    {
                        var_MemberKeyHashSet.Add(var_MemberSharingSameSimplifiedObfuscatedNameList[i]);
                    }
                }
            }

            return var_MemberKeyHashSet.ToList();
        }

        /// <summary>
        /// Returns a list of all members original keys as TMemberKey by its obfuscated name or simplified obfuscated name.
        /// If there is none, returns an empty list.
        /// </summary>
        /// <typeparam name="TMemberKey"></typeparam>
        /// <param name="_Name"></param>
        /// <returns></returns>
        public List<TMemberKey> GetAllMemberKeys<TMemberKey>(String _Name)
        {
            return this.GetAllMemberKeys(_Name).Cast<TMemberKey>().ToList();
        }

        #endregion

        // Enumerate
        #region Enumerate

        /// <summary>
        /// Enumerator for mapping.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<IMemberKey, string>> GetEnumerator()
        {
            return this.memberKeyToObfuscatedNameDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        // Serialization
        #region Serialization

        /// <summary>
        /// Serialize to custom json object.
        /// </summary>
        /// <returns></returns>
        public JsonObject OnCustomSerialization()
        {
            JsonObject var_JsonObject = new JsonObject();

            // Member Type
            var_JsonObject.Add("MemberType", new JsonString(this.MemberType.ToString()));

            // Member Mapping
            Dictionary<String, String> var_Mapping_Dictionary = new Dictionary<string, string>();

            foreach (var var_Pair in this.memberKeyToObfuscatedNameDictionary)
            {
                if (!var_Pair.Key.Serialize(out String var_Serialization_Key))
                {
                    continue;
                }

                var_Mapping_Dictionary[var_Serialization_Key] = var_Pair.Value;
            }

            JsonNode var_Mapping_Dictionary_As_JsonNode = JsonSerializer.ParseObjectToJsonNode(var_Mapping_Dictionary, EJsonTextMode.Compact);

            var_JsonObject.Add("Mapping", var_Mapping_Dictionary_As_JsonNode);

            return var_JsonObject;
        }

        /// <summary>
        /// Deserialize from custom json object.
        /// </summary>
        /// <param name="_JsonObject"></param>
        /// <returns></returns>
        public bool OnCustomDeserialization(JsonObject _JsonObject)
        {
            // Init Object
            this.memberKeyToObfuscatedNameDictionary = new Dictionary<IMemberKey, string>();
            this.obfuscatedSimplifiedNameToMemberKeyListDictionary = new Dictionary<string, List<IMemberKey>>();

            // Member Type
            this.MemberType = (EMemberType)Enum.Parse(typeof(EMemberType), _JsonObject["MemberType"].Value, false);

            // Member Mapping
            JsonNode var_Mapping_Dictionary_As_JsonNode = _JsonObject["Mapping"];

            Dictionary<String, String> var_Mapping_Dictionary = (Dictionary<String, String>)JsonSerializer.ParseJsonNodeToObject(var_Mapping_Dictionary_As_JsonNode, typeof(Dictionary<String, String>));

            foreach (var var_Pair in var_Mapping_Dictionary)
            {
                String var_Serialization_Key = var_Pair.Key;
                String var_Value = var_Pair.Value;

                try
                {
                    if (Key.Helper.MemberKeyHelper.TryToCreateMemberKey(this.MemberType, out IMemberKey var_MemberKey))
                    {
                        if (!var_MemberKey.Deserialize(var_Serialization_Key))
                        {
                            continue;
                        }

                        this.Add(var_MemberKey, var_Value);
                    }
                }
                catch (Exception e)
                {
                    // Log as debug.
                    Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Debug, e.ToString());

                    continue;
                }
            }

            return true;
        }

        #endregion
    }
}
