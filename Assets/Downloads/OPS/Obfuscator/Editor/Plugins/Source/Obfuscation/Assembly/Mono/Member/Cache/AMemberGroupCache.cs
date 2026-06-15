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
    public abstract class AMemberGroupCache<TMemberDefinition, TMemberKey> : AMemberCache<TMemberDefinition, TMemberKey>
        where TMemberDefinition : IMemberDefinition
        where TMemberKey : IMemberKey
    {
        // Constructor
        #region Constructor

        /// <summary>
        /// Set up the AMemberGroupCache using the assemblies from _Step.
        /// </summary>
        /// <param name="_Step"></param>
        public AMemberGroupCache(PostAssemblyBuildStep _Step)
            : base(_Step)
        {
        }

        #endregion

        // Group
        #region Group

        /// <summary>
        /// TMemberDefinition, its belonging Group.
        /// Each virtual TMemberDefinition is added here!
        /// </summary>
        private readonly Dictionary<TMemberDefinition, MemberGroup> memberGroups = new Dictionary<TMemberDefinition, MemberGroup>();

        /// <summary>
        /// Maps a type to all its virtual TMemberDefinition and in its bases and interfaces.
        /// </summary>
        private readonly Dictionary<TypeDefinition, List<TMemberDefinition>> types_Virtual_Member_Cache_Dictionary = new Dictionary<TypeDefinition, List<TMemberDefinition>>();

        /// <summary>
        /// Returns if the _MemberDefinition is virtual.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        protected abstract bool IsVirtual(TMemberDefinition _MemberDefinition);

        /// <summary>
        /// True: Member _Left and _Right match.
        /// </summary>
        /// <param name="_Left"></param>
        /// <param name="_Right"></param>
        /// <returns></returns>
        protected abstract bool MembersMatch(TMemberDefinition _Left, TMemberDefinition _Right);

        /// <summary>
        /// Returns all virtual TMember of _TypeDefinition and its bases and interfaces.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        private List<TMemberDefinition> GetVirtualMembers(TypeDefinition _TypeDefinition)
        {
            // Check if already added to cache.
            if (this.types_Virtual_Member_Cache_Dictionary.TryGetValue(_TypeDefinition, out List<TMemberDefinition> var_Virtual_Members))
            {
            }
            else
            {
                // Find new methods.
                var_Virtual_Members = new List<TMemberDefinition>();

                // Check the interfaces
                foreach (var ifaceRef in _TypeDefinition.Interfaces)
                {
                    // Check the interface unless it isn't in the project.
                    TypeDefinition var_Interface = this.Step.Resolve_TypeDefinition(ifaceRef.InterfaceType);

                    // Search interface
                    if (var_Interface != null)
                    {
                        var_Virtual_Members.AddRange(this.GetVirtualMembers(var_Interface));
                    }
                }

                if (_TypeDefinition.BaseType != null)
                {
                    // Check the base type unless it isn't in the project, or we don't have one.
                    TypeDefinition var_BaseType = this.Step.Resolve_TypeDefinition(_TypeDefinition.BaseType);

                    // Search base
                    if (var_BaseType != null)
                    {
                        var_Virtual_Members.AddRange(this.GetVirtualMembers(var_BaseType));
                    }
                }

                // Now add all virtual members to the list.
                foreach (TMemberDefinition var_MemberDefinition in TypeDefinitionHelper.GetMembers<TMemberDefinition>(_TypeDefinition))
                {
                    if (this.IsVirtual(var_MemberDefinition))
                    {
                        var_Virtual_Members.Add(var_MemberDefinition);
                    }
                }

                // Add to cache.
                this.types_Virtual_Member_Cache_Dictionary[_TypeDefinition] = var_Virtual_Members;
            }

            // Return cached members.
            return var_Virtual_Members;
        }

        /// <summary>
        /// Adds the _MemberDefinition to an already existing group or to a new one.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        private bool AddToMemberGroup(TMemberDefinition _MemberDefinition)
        {
            // Check if already in group, if yes get the group. Else it is null.
            if (this.memberGroups.TryGetValue(_MemberDefinition, out MemberGroup var_MemberGroup))
            {
            }

            // Declaring Type
            TypeDefinition var_TypeDefinition = _MemberDefinition.DeclaringType;

            // Get all virtual members in _TypeDefinition, its base and interfaces.
            List<TMemberDefinition> var_Virtual_Members = this.GetVirtualMembers(var_TypeDefinition);

            // Contains all the matching virtual members and but not the to add _MemberDefinition.
            List<TMemberDefinition> var_Matching_Virtual_Members = new List<TMemberDefinition>();

            // Iterate all found virtual members.
            for (int i = 0; i < var_Virtual_Members.Count; i++)
            {
                // Continue, if iterated is the one to add.
                if (var_Virtual_Members[i].Equals(_MemberDefinition))
                {
                    continue;
                }

                // Continue only matching members.
                if (!this.MembersMatch(var_Virtual_Members[i], _MemberDefinition))
                {
                    continue;
                }

                // Add current iterated, because it matches and is not the to add _MemberDefinition.
                var_Matching_Virtual_Members.Add(var_Virtual_Members[i]);
            }

            // Iterate all found matching virtual members.
            for (int i = 0; i < var_Matching_Virtual_Members.Count; i++)
            {
                TMemberDefinition var_Matching_MemberDefinition = var_Matching_Virtual_Members[i];

                // See if either member is already in a group
                if (var_MemberGroup != null)
                {
                    var_MemberGroup = AddToMemberGroup(var_MemberGroup, var_Matching_MemberDefinition);
                }
                else if (this.memberGroups.TryGetValue(var_Matching_MemberDefinition, out var_MemberGroup))
                {
                    var_MemberGroup = AddToMemberGroup(var_MemberGroup, _MemberDefinition);
                }
                else
                {
                    var_MemberGroup = new MemberGroup();

                    var_MemberGroup = AddToMemberGroup(var_MemberGroup, _MemberDefinition);
                    var_MemberGroup = AddToMemberGroup(var_MemberGroup, var_Matching_MemberDefinition);
                }

                // If the group isn't already external, see if it should be.
                if (!var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly
                    && !this.Step.IsTypeInObfuscateAssemblies(var_Matching_MemberDefinition.DeclaringType))
                {
                    var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly = true;
                }
            }

            // If none matching member is in a group yet, create a new one and add all.
            if (var_MemberGroup == null)
            {
                // Create a new group and add.
                var_MemberGroup = this.AddToMemberGroup(new MemberGroup(), _MemberDefinition);
            }

            // Check if _MemberDefinition is a root member.
            // If the to add _MemberDefinition is either in a interface or in a type implementing it first time, it is a root member.
            if (var_Matching_Virtual_Members.Count - var_Matching_Virtual_Members.Count(m => m.DeclaringType.IsInterface) == 0)
            {
                var_MemberGroup.GroupRootMemberDefinitionHashSet.Add(_MemberDefinition);
            }

            // If the group isn't already external, see if it should be.
            if (!var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly
                && !this.Step.IsTypeInObfuscateAssemblies(_MemberDefinition.DeclaringType))
            {
                var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly = true;
            }

            return true;
        }

        /// <summary>
        /// Adds _MemberDefinition to _Group and returns it.
        /// If _MemberDefinition is already in a group, merge this one and _Group.
        /// </summary>
        /// <param name="_MemberGroup"></param>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        private MemberGroup AddToMemberGroup(MemberGroup _MemberGroup, TMemberDefinition _MemberDefinition)
        {
            // Check if _MemberDefinition is already added to a group.
            if (this.memberGroups.TryGetValue(_MemberDefinition, out MemberGroup var_Already_Added_MemberGroup))
            {
                // Is in a group already.

                // Check if _MemberGroup is the var_Already_Added_MemberGroup.
                if (_MemberGroup == var_Already_Added_MemberGroup)
                {
                    // It is, just return _MemberGroup.
                    return _MemberGroup;
                }
                else
                {
                    // Are not in the same group! 

                    // Merge groups!
                    _MemberGroup.GroupName = _MemberGroup.GroupName ?? var_Already_Added_MemberGroup.GroupName;
                    _MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly = _MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly | var_Already_Added_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly;
                    foreach (TMemberDefinition var_Group_Member in var_Already_Added_MemberGroup.GroupMemberDefinitionList)
                    {
                        this.memberGroups[var_Group_Member] = _MemberGroup;
                        _MemberGroup.AddToGroup(var_Group_Member);
                    }
                    foreach (TMemberDefinition var_Group_Root_Member in var_Already_Added_MemberGroup.GroupRootMemberDefinitionHashSet)
                    {
                        if (!_MemberGroup.GroupRootMemberDefinitionHashSet.Contains(var_Group_Root_Member))
                        {
                            _MemberGroup.GroupRootMemberDefinitionHashSet.Add(var_Group_Root_Member);
                        }
                    }
                    return _MemberGroup;
                }
            }
            else
            {
                // Is in no group yet!

                // Add the member to the _MemberGroup.
                _MemberGroup.AddToGroup(_MemberDefinition);

                // Has no group assigned, assign group.
                this.memberGroups[_MemberDefinition] = _MemberGroup;

                return _MemberGroup;
            }
        }

        /// <summary>
        /// Update all groups of all virtual members in _TypeDefinition.
        /// </summary>
        /// <param name="_TypeDefinition"></param>
        /// <returns></returns>
        public bool UpdateMemberGroup(TypeDefinition _TypeDefinition)
        {
            // Get all virtual methods in _TypeDefinition and bases and interfaces.
            List<TMemberDefinition> var_All_Virtual_Members = this.GetVirtualMembers(_TypeDefinition);

            // Iterate all those found virtual methods and assing to groups or merge them.
            for (int i = 0; i < var_All_Virtual_Members.Count; i++)
            {
                // The member group for current iterated.
                MemberGroup var_MemberGroup;

                // Get current iterated.
                TMemberDefinition var_Left_Member = var_All_Virtual_Members[i];

                // Check if already in a group.
                if (!this.memberGroups.TryGetValue(var_Left_Member, out var_MemberGroup))
                {
                    var_MemberGroup = null;
                }

                // Contains all the matching virtual methods to var_Left_Method but not var_Left_Method itself.
                List<TMemberDefinition> var_Matching_Virtual_Members = new List<TMemberDefinition>();

                // Iterate all found virtual methods.
                for (int j = 0; j < var_All_Virtual_Members.Count; j++)
                {
                    // Continue, if iterated is the one to add.
                    if (var_All_Virtual_Members[j].Equals(var_Left_Member))
                    {
                        continue;
                    }

                    // Continue only matching methods.
                    if (!this.MembersMatch(var_All_Virtual_Members[j], var_Left_Member))
                    {
                        continue;
                    }

                    // Add current iterated, because it matches and is not the to add _MethodDefinition.
                    var_Matching_Virtual_Members.Add(var_All_Virtual_Members[j]);
                }

                // Iterate all matching virtual methods.
                for (int j = 0; j < var_Matching_Virtual_Members.Count; j++)
                {
                    // Iterate matching methods.
                    TMemberDefinition var_Right_Member = var_Matching_Virtual_Members[j];

                    // See if either method is already in a group
                    if (var_MemberGroup != null)
                    {
                        var_MemberGroup = AddToMemberGroup(var_MemberGroup, var_Right_Member);
                    }
                    else if (memberGroups.TryGetValue(var_Right_Member, out var_MemberGroup))
                    {
                        var_MemberGroup = AddToMemberGroup(var_MemberGroup, var_Left_Member);
                    }
                    else
                    {
                        var_MemberGroup = new MemberGroup();

                        var_MemberGroup = AddToMemberGroup(var_MemberGroup, var_Left_Member);
                        var_MemberGroup = AddToMemberGroup(var_MemberGroup, var_Right_Member);
                    }

                    // If the group isn't already external, see if it should be.
                    if (!var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly
                        && !this.Step.IsTypeInObfuscateAssemblies(var_Right_Member.DeclaringType))
                    {
                        var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly = true;
                    }
                }

                // If none matching member is in a group yet, create a new one and add all.
                if (var_MemberGroup == null)
                {
                    // Create a new group and add.
                    var_MemberGroup = this.AddToMemberGroup(new MemberGroup(), var_Left_Member);
                }

                // Check if _MemberDefinition is a root member.
                // If the to add _MemberDefinition is either in a interface or in a type implementing it first time, it is a root member.
                if (var_Matching_Virtual_Members.Count - var_Matching_Virtual_Members.Count(m => m.DeclaringType.IsInterface) == 0)
                {
                    var_MemberGroup.GroupRootMemberDefinitionHashSet.Add(var_Left_Member);
                }

                // If the group isn't already external, see if it should be.
                if (!var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly
                    && !this.Step.IsTypeInObfuscateAssemblies(var_Left_Member.DeclaringType))
                {
                    var_MemberGroup.SomeGroupMemberDefinitionIsInExternalAssembly = true;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the MemberGroup of _MemberDefinition, if it has one.
        /// </summary>
        /// <param name="_MemberDefinition"></param>
        /// <returns></returns>
        public MemberGroup GetMemberGroup(TMemberDefinition _MemberDefinition)
        {
            if (this.memberGroups.TryGetValue(_MemberDefinition, out MemberGroup var_Group))
            {
                return var_Group;
            }
            else
            {
                return null;
            }
        }

        #endregion

        // Add
        #region Add

        protected override bool On_Add_MemberDefinition(TMemberDefinition _MemberDefinition)
        {
            // Continue only virtual members and assign them to a group.
            if (!this.IsVirtual(_MemberDefinition))
            {
                return true;
            }

            // Add to group and return its result.
            return this.AddToMemberGroup(_MemberDefinition);
        }

        #endregion
    }
}
