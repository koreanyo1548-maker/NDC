using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Cache
{
    public class MemberGroup : IMemberGroup
    {
        // Name
        #region Name

        /// <summary>
        /// The obfuscated name of the group.
        /// </summary>
        public string GroupName { get; set; } = null;

        #endregion

        // Member
        #region Member

        /// <summary>
        /// All members in the group.
        /// </summary>
        public List<IMemberDefinition> GroupMemberDefinitionList { get; private set; } = new List<IMemberDefinition>();

        /// <summary>
        /// All root members in this groups. Those are interfaces and types declaring those member.
        /// </summary>
        public HashSet<IMemberDefinition> GroupRootMemberDefinitionHashSet { get; private set; } = new HashSet<IMemberDefinition>();

        /// <summary>
        /// Add _MemberDefinition to group.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public bool AddToGroup(IMemberDefinition _MemberDefinition)
        {
            this.GroupMemberDefinitionList.Add(_MemberDefinition);

            return true;
        }

        #endregion

        // External
        #region External

        /// <summary>
        /// Returns if some of the members is in an external (not obfuscated) assembly.
        /// </summary>
        public bool SomeGroupMemberDefinitionIsInExternalAssembly { get; set; } = false;

        #endregion

        // ToString
        #region ToString

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.GroupName);
            if (this.SomeGroupMemberDefinitionIsInExternalAssembly)
                sb.Append("(External)");
            else
                sb.Append("(Internal)");
            sb.Append(": ");
            foreach (IMemberDefinition m in this.GroupMemberDefinitionList)
            {
                sb.Append(m.ToString());
                sb.Append(" ");
            }
            return sb.ToString();
        }

        #endregion
    }
}
