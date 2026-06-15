using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// OPS - Json
using OPS.Serialization.Json.Interfaces;

// OPS - Version
using OPS.Editor.IO.Version;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Key;
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Mapping;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming
{
    /// <summary>
    /// Represents a collection of IMemberKeyMapping for each EMemberType, to map IMemberKeys to obfuscated names.
    /// </summary>
    public interface IRenameMapping : IVersionAble
    {
        // Mapping
        #region Mapping

        /// <summary>
        /// Get the IMemberKeyMapping by _MemberType.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <returns></returns>
        IMemberKeyMapping GetMapping(EMemberType _MemberType);

        /// <summary>
        /// Add a _MemberKey with obfuscated _Name to the mapping of _MemberType. 
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKey"></param>
        /// <param name="_Name"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        bool Add(EMemberType _MemberType, IMemberKey _MemberKey, String _Name, bool _Override = false);

        /// <summary>
        /// Get the obfuscated name of _MemberKey in the mapping of _MemberType. 
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_MemberKey"></param>
        /// <returns></returns>
        String Get(EMemberType _MemberType, IMemberKey _MemberKey);

        /// <summary>
        /// Returns all IMemberKey sharing the same obfsucated _Name in the mapping of _MemberType.
        /// </summary>
        /// <param name="_MemberType"></param>
        /// <param name="_Name"></param>
        /// <returns></returns>
        List<IMemberKey> GetAllMemberKeys(EMemberType _MemberType, String _Name);

        /// <summary>
        /// Returns all IMemberKey sharing the same obfsucated _Name in all member types.
        /// </summary>
        /// <param name="_Name"></param>
        /// <returns></returns>
        List<IMemberKey> GetAllMemberKeys(String _Name);

        #endregion
    }
}
