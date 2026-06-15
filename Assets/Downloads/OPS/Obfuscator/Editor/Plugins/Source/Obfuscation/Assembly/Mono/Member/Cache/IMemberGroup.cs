using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    /// <summary>
    /// Group of Members inside a MemberCache. The Members are related to each other through inheritance/override and sharing the same name.
    /// </summary>
    public interface IMemberGroup
    {
        /// <summary>
        /// The name for the group.
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// All MemberDefinitions in the group.
        /// </summary>
        List<IMemberDefinition> GroupMemberDefinitionList { get; }

        /// <summary>
        /// All root MemberDefinitions in this group. These are either interfaces (and the types implemented the interface) and/or types that root those members.
        /// </summary>
        HashSet<IMemberDefinition> GroupRootMemberDefinitionHashSet { get; }

        /// <summary>
        /// Add _MemberDefinition to the group.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        bool AddToGroup(IMemberDefinition _MemberDefinition);

        /// <summary>
        /// Returns if some of the members is in an external (not obfuscated) assembly.
        /// </summary>
        bool SomeGroupMemberDefinitionIsInExternalAssembly { get; }
    }
}
