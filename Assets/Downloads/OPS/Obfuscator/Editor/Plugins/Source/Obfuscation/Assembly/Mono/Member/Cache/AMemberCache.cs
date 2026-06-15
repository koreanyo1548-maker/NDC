using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;
using OPS.Mono.Cecil.Mdb;
using OPS.Mono.Cecil.Pdb;

// OPS - Obfuscator - Assemby
using OPS.Obfuscator.Editor.Assembly;

// OPS - Obfuscator - Assemby - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key.Helper;

// OPS - Obfuscator - Project
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    /// <summary>
    /// Contains all members in the project and its belonging group.
    /// </summary>
    public abstract class AMemberCache<TMemberDefinition, TMemberKey> : IMemberCache<TMemberDefinition, TMemberKey>
        where TMemberDefinition : IMemberDefinition
        where TMemberKey : IMemberKey
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Set up the AMemberCache using the assemblies from _Step.
        /// </summary>
        /// <param name="_Step"></param>
        public AMemberCache(PostAssemblyBuildStep _Step)
        {
            // Assign Step
            this.Step = _Step;
        }

        #endregion

        // Step
        #region Step

        /// <summary>
        /// The used Step.
        /// </summary>
        protected PostAssemblyBuildStep Step { get; }

        #endregion

        // Member
        #region Member

        /// <summary>
        /// TMemberDefinition, TMemberKey.
        /// </summary>
        private readonly Dictionary<TMemberDefinition, TMemberKey> memberToKeyDictionary = new Dictionary<TMemberDefinition, TMemberKey>();

        /// <summary>
        /// TMemberKey, TMemberDefinition.
        /// </summary>
        private readonly Dictionary<TMemberKey, TMemberDefinition> keyToMemberDictionary = new Dictionary<TMemberKey, TMemberDefinition>();

        /// <summary>
        /// Returns the original key for _MemberDefinition.
        /// If there is non, returns null!
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public TMemberKey GetOriginalMemberKeyByMemberDefinition(TMemberDefinition _MemberDefinition)
        {
            if (this.memberToKeyDictionary.TryGetValue(_MemberDefinition, out TMemberKey var_OriginalKey))
            {
                return var_OriginalKey;
            }

            return default(TMemberKey);
        }

        IMemberKey IMemberCache.GetOriginalMemberKeyByMemberDefinition(IMemberDefinition _MemberDefinition)
        {
            return this.GetOriginalMemberKeyByMemberDefinition((TMemberDefinition)_MemberDefinition);
        }

        /// <summary>
        /// Returns the TMemberDefinition by its original key.
        /// If there is non, returns null!
        /// </summary>
        /// <param name="_OriginalMemberKey"></param>
        /// <returns></returns>
        public TMemberDefinition GetMemberDefinitionByOriginalMemberKey(TMemberKey _OriginalMemberKey)
        {
            if (this.keyToMemberDictionary.TryGetValue(_OriginalMemberKey, out TMemberDefinition var_MemberDefinition))
            {
                return var_MemberDefinition;
            }

            return default(TMemberDefinition);
        }

        IMemberDefinition IMemberCache.GetMemberDefinitionByOriginalMemberKey(IMemberKey _OriginalMemberKey)
        {
            return this.GetMemberDefinitionByOriginalMemberKey((TMemberKey)_OriginalMemberKey);
        }

        /// <summary>
        /// Add _MemberDefinition to the key to member dictionaries.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        private bool AddTo_MemberKey_And_MemberDefinition_Cache(TMemberDefinition _MemberDefinition)
        {
            // Check if already contains the member, if yes return false.
            if (this.memberToKeyDictionary.ContainsKey(_MemberDefinition))
            {
                // Already added, cannot add _MemberDefinition!
                return false;
            }

            // Create member key.
            if (!MemberKeyHelper.TryToCreateMemberKey(_MemberDefinition, out IMemberKey var_MemberKey))
            {
                return false;
            }

            // A member key was created but it is not TMemberKey!
            if (!(var_MemberKey is TMemberKey))
            {
                throw new Exception("The created member key is " + var_MemberKey.GetType().ToString() + " but expected " + typeof(TMemberKey).ToString());
            }

            // Check if already contains the key, if yes return false.
            if (this.keyToMemberDictionary.ContainsKey((TMemberKey)var_MemberKey))
            {
                // Already added, cannot add var_MemberKey!
                return false;
            }

            // Add to member/key dictionaries.
            this.memberToKeyDictionary.Add(_MemberDefinition, (TMemberKey)var_MemberKey);
            this.keyToMemberDictionary.Add((TMemberKey)var_MemberKey, _MemberDefinition);

            // Custom action.
            return this.On_AddTo_MemberKey_And_MemberDefinition_Cache(_MemberDefinition);
        }

        /// <summary>
        /// Override for custom action on add to member key and member cache.
        /// Return false if something went wrong!
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        protected virtual bool On_AddTo_MemberKey_And_MemberDefinition_Cache(TMemberDefinition _MemberDefinition)
        {
            return true;
        }

        #endregion

        // Add
        #region Add

        /// <summary>
        /// Add the _MemberDefinition only to the member cache.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public bool Add_MemberDefinition(TMemberDefinition _MemberDefinition)
        {
            // Check if can add to key to member dictionaries.
            if (!this.AddTo_MemberKey_And_MemberDefinition_Cache(_MemberDefinition))
            {
                // Could not add, something went wrong!
                return false;
            }

            return this.On_Add_MemberDefinition(_MemberDefinition);
        }

        /// <summary>
        /// Override for custom action on add to member.
        /// Return false if something went wrong!
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        protected virtual bool On_Add_MemberDefinition(TMemberDefinition _MemberDefinition)
        {
            return true;
        }

        bool IMemberCache.Add_MemberDefinition(IMemberDefinition _MemberDefinition)
        {
            return this.Add_MemberDefinition((TMemberDefinition)_MemberDefinition);
        }

        #endregion

        // ToString
        #region ToString

        public override string ToString()
        {
            StringBuilder var_Builder = new StringBuilder();
            foreach (var var_Pair in this.memberToKeyDictionary)
            {
                var_Builder.AppendLine(var_Pair.Key.ToString() + " : " + var_Pair.Value.ToString());
            }
            return var_Builder.ToString();
        }

        #endregion
    }
}
