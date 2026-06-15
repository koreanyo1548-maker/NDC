using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly - Member
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    /// <summary>
    /// Interface for a MemberCache.
    /// </summary>
    public interface IMemberCache
    {
        /// <summary>
        /// Returns the Key of _MemberDefinition.
        /// If _MemberDefinition is not part of the cache, returns null.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        IMemberKey GetOriginalMemberKeyByMemberDefinition(IMemberDefinition _MemberDefinition);

        /// <summary>
        /// Returns the member by its _OriginalMemberKey.
        /// If there is no member with _OriginalMemberKey, returns null.
        /// </summary>
        /// <param name="_OriginalMemberKey"></param>
        /// <returns></returns>
        IMemberDefinition GetMemberDefinitionByOriginalMemberKey(IMemberKey _OriginalMemberKey);

        /// <summary>
        /// Add a member only to the cache.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        bool Add_MemberDefinition(IMemberDefinition _MemberDefinition);
    }

    /// <summary>
    /// Cache of all members of TMemberDefinition with TMemberKey.
    /// </summary>
    /// <typeparam name="TMemberDefinition"></typeparam>
    /// <typeparam name="TMemberKey"></typeparam>
    public interface IMemberCache<TMemberDefinition, TMemberKey> : IMemberCache
        where TMemberDefinition : IMemberDefinition
        where TMemberKey : IMemberKey 
    {
        /// <summary>
        /// Returns the Key of _MemberDefinition.
        /// If _MemberDefinition is not part of the cache, returns null.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        TMemberKey GetOriginalMemberKeyByMemberDefinition(TMemberDefinition _MemberDefinition);

        /// <summary>
        /// Returns the member by its _OriginalMemberKey.
        /// If there is no member with _OriginalMemberKey, returns null.
        /// </summary>
        /// <param name="_OriginalMemberKey"></param>
        /// <returns></returns>
        TMemberDefinition GetMemberDefinitionByOriginalMemberKey(TMemberKey _OriginalMemberKey);

        /// <summary>
        /// Add a member only to the cache.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        bool Add_MemberDefinition(TMemberDefinition _MemberDefinition);
    }
}
